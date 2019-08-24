using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HumanPlayer : GamePlayer
{
   
    // Phases
    private enum Phase
    {
        FIRST_SETTLEMENT_PLACEMENT,
        FIRST_ROAD_PLACEMENT,
        SECOND_SETTLEMENT_PLACEMENT,
        SECOND_ROAD_PLACEMENT,
        ROLL_DICE
    }

    // during dice roll phase the player can activate a development card from earlier

    private Phase currentPhase = Phase.FIRST_SETTLEMENT_PLACEMENT;
    
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
            EnableRollDiceButton();
            // Waiting for dice to be rolled.
        }
    }


    protected void SelectSettlementLocation()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit.collider.name);
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
                        selectingIntersection = false;

                        if (currentTurn == 1)
                        {
                            currentPhase = Phase.FIRST_ROAD_PLACEMENT;
                        }
                        else if (currentTurn == 2)
                        {
                            currentPhase = Phase.SECOND_ROAD_PLACEMENT;
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
                Debug.Log(hit.collider.name);
                if (hit.collider.tag == "Path") // click on another player, enemy, building
                {

                    WorldPath p = hit.collider.transform.GetComponent<WorldPath>();

                    if (p.IsAvailable())
                    {
                        TogglePathBlink();

                        p.ConstructRoad(PhotonNetwork.LocalPlayer.ActorNumber);

                        inventory.TakeFromPlayer(Inventory.UnitCode.ROAD, 1);

                        myPaths.Add(p);

                        selectingPath = false;

                        if (currentTurn == 1)
                        {
                            currentPhase = Phase.SECOND_SETTLEMENT_PLACEMENT;
                        } else if (currentTurn == 2)
                        {

                        }
                        

                        EndTurn();
                    }
                    
                }
            }

        }
    }
}
