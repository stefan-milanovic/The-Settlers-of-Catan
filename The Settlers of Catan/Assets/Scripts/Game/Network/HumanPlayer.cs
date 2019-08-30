using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HumanPlayer : GamePlayer
{
   
    // Start is called before the first frame update
    void Start()
    {
        
        Init();

        eventTextController = GameObject.Find("EventTextController").GetComponent<EventTextController>();
        
        if (photonView.IsMine)
        {

            // Connect to chat.
            GameObject.Find("ChatController").GetComponent<ChatController>().JoinChat(PhotonNetwork.CurrentRoom.Name);

            // Connect to dice controller.
            GameObject.Find("DiceController").GetComponent<DiceController>().SetPlayer(this);

            // Instantiate local inventory.
            inventory = Instantiate(inventoryPrefab).GetComponent<Inventory>();
            inventory.SetPlayer(this);

            // Fill in local intersection list.
            intersections = GameObject.FindGameObjectsWithTag("Intersection");

            // If master client create board generator and leaderboard 
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(Path.Combine("Prefabs/Network", "BoardGenerator"), Vector3.zero, Quaternion.identity);
            }

            // Get preferred colour and username.
            this.username = PlayerPrefs.GetString("Username");
            this.colourHex = PlayerPrefs.GetString("Colour");

            // delete when color is implemented
            if (colourHex == "")
            {
                Color myColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                string hex = "#" + ColorUtility.ToHtmlStringRGB(myColor);
                
                colourHex = hex;
            }

            // set them room-wide
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                ["username"] = this.username,
                ["colour"] = this.colourHex
            });

            // Claim an empty leadeboard slot.

            // Connect to Turn Manager - ONLY LOCAL PLAYER CALLS THIS.
            ConnectToTurnManager();
            
        }
    }
    
    
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

        if (photonView.IsMine)
        {
            Debug.Log("properties that changed: " + propertiesThatChanged);

            // When a player claims a slot.

            if (propertiesThatChanged.Count == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    string key = "leaderboardSlot" + (i + 1);

                    if (propertiesThatChanged.ContainsKey(key) && (int)propertiesThatChanged[key] != 0)
                    {

                        GameObject.Find("LeaderboardController").GetComponent<LeaderboardController>().TakeSlot(i, (int)propertiesThatChanged[key]);

                    }
                }
            }
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // Selects only the player controlled locally
        if (!photonView.IsMine)
        {
            return;
        }

        // Disables actions while it isn't the local player's turn
        if (!myTurn)
        {
            return;
        }
        else
        {

            // It is my turn to play.

            if (setUpPhase)
            {

                if (currentPhase == Phase.FIRST_SETTLEMENT_PLACEMENT || currentPhase == Phase.SECOND_SETTLEMENT_PLACEMENT)
                {
                    FindSelectableIntersections();
                    SelectSettlementLocation();
                }

                if (currentPhase == Phase.FIRST_ROAD_PLACEMENT || currentPhase == Phase.SECOND_ROAD_PLACEMENT)
                {
                    FindSelectablePaths();
                    SelectRoadLocation();
                }
            }

            if (currentPhase == Phase.ROLL_DICE)
            {
                EnableRolling();
                // Waiting for dice to be rolled. When they're rolled the WaitForDiceResult() method of GamePlayer will be called.
            }

            if (currentPhase == Phase.TRADE_BUILD_IDLE)
            {
                // Wait for action.
                
            }
            
            if (currentPhase == Phase.BUILDING)
            {
                switch (selectedConstructionCard.GetUnitCode())
                {
                    case Inventory.UnitCode.ROAD:
                        FindSelectablePaths();
                        SelectRoadLocation();
                        break;
                    case Inventory.UnitCode.SETTLEMENT:
                        FindSelectableIntersections();
                        SelectSettlementLocation();
                        break;
                    case Inventory.UnitCode.CITY:
                        FindSelectableSettlements();
                        SelectCityLocation();
                        break;
                }
            }

            if (currentPhase == Phase.STOP_BUILDING)
            {

                TurnOffIndicators();
                selectedConstructionCard = null;
                currentPhase = Phase.TRADE_BUILD_IDLE;
            }

            if (currentPhase == Phase.BANDIT_MOVE)
            {
                SelectBanditHex();
            }
        }
        
    }


    

    protected void SelectSettlementLocation()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Debug.Log(hit.collider.name);
                if (hit.collider.tag == "IntersectionCollider") // click on another player, enemy, building
                {
                    Intersection i = hit.collider.transform.parent.GetComponent<Intersection>();
                    
                    if (i.IsAvailable() && selectableIntersections.Contains(i))
                    {

                        ToggleIntersectionRipples();
                        i.ConstructSettlement(PhotonNetwork.LocalPlayer.ActorNumber);
                        
                        inventory.TakeFromPlayer(Inventory.UnitCode.SETTLEMENT, 1);
                        inventory.AddVictoryPoint(Inventory.UnitCode.SETTLEMENT);

                        if (i.OnHarbour(out HarbourPath.HarbourBonus? bonus))
                        {
                            inventory.AddHarbourBonus((HarbourPath.HarbourBonus)bonus);
                        }

                        myIntersections.Add(i);

                        
                        // Move on to next phase.
                        if (currentTurn == 1)
                        {
                            currentPhase = Phase.FIRST_ROAD_PLACEMENT;
                        }
                        else if (currentTurn == 2)
                        {
                            // The player now gets his starting resources.
                            inventory.GrantStartingResources(i);

                            currentPhase = Phase.SECOND_ROAD_PLACEMENT;
                        }
                        else
                        {
                            // Pay resources
                            inventory.PaySettlementConstruction();
                            currentPhase = Phase.TRADE_BUILD_IDLE;
                        }
                    }
                }
            }

        }
    }

    protected void SelectRoadLocation()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.tag == "Path") // click on another player, enemy, building
                {

                    WorldPath p = hit.collider.transform.GetComponent<WorldPath>();

                    if (p.IsAvailable() && selectablePaths.Contains(p))
                    {
                        TogglePathBlink();

                        if (currentTurn == 1)
                        {
                            currentPhase = Phase.SECOND_SETTLEMENT_PLACEMENT;

                            p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                            inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                            myPaths.Add(p);

                            EndLocalTurn();
                        } else if (currentTurn == 2)
                        {

                            p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                            inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                            myPaths.Add(p);

                            EndLocalTurn();
                            
                        }
                        else
                        {
                            // Pay resources
                            inventory.PayRoadConstruction();
                            currentPhase = Phase.TRADE_BUILD_IDLE;

                            p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                            inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                            myPaths.Add(p);
                        }
                    }
                    
                }
            }

        }
    }

    protected void SelectCityLocation()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.tag == "IntersectionCollider") // click on another player, enemy, building
                {
                    Intersection i = hit.collider.transform.parent.GetComponent<Intersection>();

                    if (selectableSettlements.Contains(i))
                    {
                        Debug.Log("Creating city");

                        ToggleSettlementRipples();

                        i.ConstructCity();

                        inventory.GiveToPlayer(Inventory.UnitCode.SETTLEMENT, 1); // Return 1 settlement to the player's stock.
                        inventory.TakeFromPlayer(Inventory.UnitCode.CITY, 1); // Take 1 city from the player's stock.
                        inventory.AddVictoryPoint(Inventory.UnitCode.CITY);

                        inventory.PayCityConstruction();
                        currentPhase = Phase.TRADE_BUILD_IDLE;
                    }
                }
            }

        }
    }

    protected void SelectBanditHex()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.tag == "Hex") // click on another player, enemy, building
                {
                    Hex hex = hit.collider.transform.GetComponent<Hex>();

                    if (!hex.OccupiedByBandit())
                    {
                        // Remove the bandit from the previous hex.
                        Hex banditHex = Hex.GetBanditHex();
                        banditHex.RemoveBandit();

                        // Move the bandit to this hex.

                        hex.OccupyByBandit();

                        // Move to steal phase.

                        StealFromPlayer(hex);
                    }
                   
                }
            }
        }
    }
}
