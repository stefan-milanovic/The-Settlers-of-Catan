using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

                    string keyColour = "colour" + (i + 1) + "Owner";

                    if (propertiesThatChanged.ContainsKey(key) && (int)propertiesThatChanged[key] != 0)
                    {
                        GameObject.Find("LeaderboardController").GetComponent<LeaderboardController>().TakeSlot(i, (int)propertiesThatChanged[key]);
                    }
                    else if (propertiesThatChanged.ContainsKey(keyColour) && (int)propertiesThatChanged[keyColour] != 0)
                    {
                        // Set the player's colour.
                        string colourHex = "#FFFFFF";

                        switch (i)
                        {
                            case 0:
                                colourHex = "#00FF00";
                                break;
                            case 1:
                                colourHex = "#FF0000";
                                break;
                            case 2:
                                colourHex = "#1F00FF";
                                break;
                            case 3:
                                colourHex = "#00EDFF";
                                break;
                        }

                        if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
                        {

                            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                            {
                                ["username"] = PlayerPrefs.GetString("Username"),
                                ["colour"] = colourHex
                            });

                            eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);

                            myTurn = true;
                            audioSource.Play();
                        }
                        
                        
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

        if (gameOver)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {

                Debug.Log("A key or mouse click has been detected after game over");

                // Leave the room.
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene(0);
            }
        }

        // Disables actions while it isn't the local player's turn
        if (!myTurn)
        {
            return;
        }
        else
        {

            if (gameOver) { return; }

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

            if (currentPhase == Phase.PLAYED_EXPANSION_CARD)
            {
                FindSelectablePaths();
                SelectRoadLocation();
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

                            // Inform event text controller.
                            eventTextController.SendEvent(EventTextController.EventCode.SETTLEMENT_CONSTRUCTED, PhotonNetwork.LocalPlayer);

                            eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
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

                            

                            myRoads.Add(p);

                            inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                            EndLocalTurn();
                        } else if (currentTurn == 2)
                        {

                            p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                            

                            myRoads.Add(p);

                            inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                            EndLocalTurn();
                            
                        }
                        else
                        {

                            if (currentPhase == Phase.PLAYED_EXPANSION_CARD)
                            {
                                // Free road placement.
                                
                                p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);
                                
                                myRoads.Add(p);

                                inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                                freeRoadsPlaced++;
                                // If second one is placed, put up event text.

                                if (freeRoadsPlaced == 2)
                                {
                                    eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
                                    currentPhase = Phase.TRADE_BUILD_IDLE;
                                }
                                
                            }
                            else
                            {
                                // Pay resources
                                inventory.PayRoadConstruction();


                                p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                                myRoads.Add(p);

                                inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                                // Inform event text.

                                eventTextController.SendEvent(EventTextController.EventCode.ROAD_CONSTRUCTED, PhotonNetwork.LocalPlayer);

                                eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
                                currentPhase = Phase.TRADE_BUILD_IDLE;

                                
                            }
                            
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

                        eventTextController.SendEvent(EventTextController.EventCode.CITY_CONSTRUCTED, PhotonNetwork.LocalPlayer);

                        eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
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
                Debug.Log(hit.collider.name);
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
