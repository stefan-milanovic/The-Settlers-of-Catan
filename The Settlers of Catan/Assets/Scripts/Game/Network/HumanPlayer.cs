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
        photonView = GetComponent<PhotonView>();
        Init();

        eventTextController = GameObject.Find("EventTextController").GetComponent<EventTextController>();
        
        if (photonView.IsMine)
        {

            chat = GameObject.Find("ChatController").GetComponent<ChatController>();


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
                colourHex = "#123123";
            }

            // set them room-wide
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                ["username"] = this.username,
                ["colour"] = this.colourHex
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

            // Connect to Turn Manager - ONLY LOCAL PLAYER CALLS THIS.
            ConnectToTurnManager();

            // Claim an empty leaderboard slot.

            GameObject.Find("LeaderboardController").GetComponent<LeaderboardController>().RegisterPlayer(PhotonNetwork.LocalPlayer);

            // Connect to end turn button.


        }
        else
        {
            this.username = photonView.Owner.CustomProperties["username"] as string;
            this.colourHex = photonView.Owner.CustomProperties["colour"] as string;

            // delete when color is implemented
            if (colourHex == "")
            {
                colourHex = "#123123";
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
                
                //if (roadCardSelected)
                //{
                
                //}

                //if (settlementCardSelected)
                //{
                
                //}

                //if (cityCardSelected)
                //{
                //    // city logic
                //}
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

                    if (i.IsAvailable())
                    {

                        ToggleIntersectionRipples();
                        i.ConstructSettlement(PhotonNetwork.LocalPlayer.ActorNumber);
                        
                        inventory.TakeFromPlayer(Inventory.UnitCode.SETTLEMENT, 1);

                        myIntersections.Add(i);

                        // Move on to next phase.
                        if (currentTurn == 1)
                        {
                            currentPhase = Phase.FIRST_ROAD_PLACEMENT;
                        }
                        else if (currentTurn == 2)
                        {
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

                    if (p.IsAvailable())
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
                            currentPhase = Phase.ROLL_DICE;

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

                    foreach (Intersection mySettlement in selectableSettlements)
                    {
                        if (i == mySettlement)
                        {

                            Debug.Log("Creating city");

                            ToggleSettlementRipples();

                            i.ConstructCity();

                            inventory.GiveToPlayer(Inventory.UnitCode.SETTLEMENT, 1); // Return 1 settlement to the player's stock.
                            inventory.TakeFromPlayer(Inventory.UnitCode.CITY, 1); // Take 1 city from the player's stock.
                            
                            inventory.PayCityConstruction();
                            currentPhase = Phase.TRADE_BUILD_IDLE;
                        }
                    }
                }
            }

        }
    }
}
