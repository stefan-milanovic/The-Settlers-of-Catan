using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{

    // Phases
    public enum Phase
    {
        FIRST_SETTLEMENT_PLACEMENT,
        FIRST_ROAD_PLACEMENT,
        SECOND_SETTLEMENT_PLACEMENT,
        SECOND_ROAD_PLACEMENT,
        ROLL_DICE,
        TRADE_BUILD_IDLE,
        BUILDING,
        STOP_BUILDING,
        BANDIT_MOVE,
        TRADING,
        STEAL,
        PLAYED_EXPANSION_CARD
    }

    public enum MessageCode
    {
        RESOURCE_INCOME,
        TRADE_REQUEST,
        TRADE_ACCEPTED,
        TRADE_DECLINED,
        TRADE_CANCELLED,
        TRADE_PERFORMED,
        TRADE_LOCK,
        TRADE_UNLOCK,
        TRADE_EXECUTE,
        RESOURCE_OFFERED,
        RESOURCE_RETRACTED,
        SEVEN_ROLLED_ANNOUNCEMENT,
        SEVEN_ROLLED_ACKNOWLEDGEMENT,
        SEVEN_ROLLED_DISCARD_COMPLETE,
        STEAL_RESOURCE_REQUEST,
        STEAL_RESOURCE_REPLY,
        MONOPOLY_PLAYED,
        MONOPOLY_REPLY,
        LARGEST_ARMY_OVERTAKE,
        LONGEST_ROAD_OVERTAKE,
        LONGEST_ROAD_RETURNED,
        GAME_OVER
    }

    // during dice roll phase the player can activate a development card from earlier
    protected Phase currentPhase = Phase.TRADE_BUILD_IDLE;

    protected EventTextController eventTextController;

    protected bool setUpPhase = true;
    
    protected bool myTurn = false;
    protected bool gameOver = false;

    protected int currentTurn;

    public int CurrentTurn {
        get { return currentTurn; }
    }

    protected List<Intersection> selectableIntersections = new List<Intersection>();
    protected List<Intersection> selectableSettlements = new List<Intersection>();
    protected List<WorldPath> selectablePaths = new List<WorldPath>();

    protected GameObject[] intersections;
   
    // Every player has their own inventory of cards, resources, and buildings.
    protected Inventory inventory;

    [SerializeField]
    protected GameObject inventoryPrefab;

    protected PhotonView photonView;

    protected TurnManager turnManager;

    protected bool busy = false;

    protected List<Intersection> myIntersections = new List<Intersection>();
    protected List<WorldPath> myRoads = new List<WorldPath>();


    
    //protected List<RoadChainLink> roadNetwork = new List<RoadChainLink>();
    
    //public List<RoadChainLink> RoadNetwork {
    //    get { return roadNetwork; }
    //}

    protected Player currentPlayer = null;

    protected Card selectedConstructionCard = null;
    
    protected TradeController tradeController;

    protected int acknowledged;
    protected bool[] haveToDiscard;
    protected int waitingForDiscardCount;

    protected int freeRoadsPlaced = 0;

    // UI

    protected Button rollDiceButton;

    protected AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    protected void Init()
    {
        audioSource = GameObject.Find("AudioSources").GetComponent<AudioSource>();
        rollDiceButton = GameObject.Find("RollDiceButton").GetComponent<Button>();
    }
    
    public static GamePlayer FindLocalPlayer()
    {
        GamePlayer[] playerList = FindObjectsOfType<HumanPlayer>();
        foreach (GamePlayer player in playerList)
        {
            if (player.GetPhotonPlayer() == PhotonNetwork.LocalPlayer)
            {
                return player;
            }
        }

        return null;
    }

    public bool IsMyTurn() { return myTurn; }

    public WorldPath GetLastAddedRoad()
    {
        return myRoads[myRoads.Count - 1];
    }

    public override void OnEnable()
    {
        base.OnEnable();

        photonView = GetComponent<PhotonView>();
    }

    public Player GetPhotonPlayer() { return photonView.Owner; }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Phase GetPhase() { return currentPhase;  }
    public void SetPhase(Phase phase) { currentPhase = phase; }

    public void SetSelectedConstructionCard(Card card) { selectedConstructionCard = card; }
    public Card GetSelectedConstructionCard() { return selectedConstructionCard; }

    public void TurnOffIndicators()
    {
        switch (selectedConstructionCard.GetUnitCode())
        {
            case Inventory.UnitCode.ROAD:
                TogglePathBlink();
                break;
            case Inventory.UnitCode.SETTLEMENT:
                ToggleIntersectionRipples();
                break;
            case Inventory.UnitCode.CITY:
                ToggleSettlementRipples();
                break;
        }

    }

    protected void ConnectToTurnManager()
    {
        this.turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.RegisterPlayer();
    }
    
    public Inventory GetInventory()
    {
        return inventory;
    }

    private void ClaimColour()
    {
        for (int i = 0; i < 4; i++)
        {
            string key = "colour" + (i + 1) + "Owner";

            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                [key] = PhotonNetwork.LocalPlayer.ActorNumber
            },
            new ExitGames.Client.Photon.Hashtable
            {
                [key] = 0
            });

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties[key] == 0)
            {
                // This colour will be taken -- leave the loop.
                break;
            }
        }
    }
    
    #region IPunTurnManagerCallbacks

    // Called from TurnManager - Each player gets this called by their local turn manager
    public void OnTurnBegins(int turn)
    {
        Debug.Log("Turn " + turn + " beginning globally.");
        
        currentTurn = turn;

        
        
        if (turn == 1)
        {

            currentPlayer = PhotonNetwork.PlayerList[0];

            setUpPhase = true;
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {

                // Claim colour slot.
                ClaimColour();

                myTurn = true;
                

                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
                
                // Turns starts once a colour slot is claimed.
            }
        }
        else if (turn == 2)
        {

            currentPlayer = PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length - 1];

            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {

                

                // Init trade.
                tradeController = GameObject.Find("TradeController").GetComponent<TradeController>();
                tradeController.Init(inventory);

                Debug.Log("In turn2 the player to play is: " + PhotonNetwork.LocalPlayer.ActorNumber + ", name = " + PhotonNetwork.LocalPlayer.NickName);
                myTurn = true;
                audioSource.Play();
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }

        } else if (turn == 3)
        {

            currentPlayer = PhotonNetwork.PlayerList[0];

            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                

                myTurn = true;
                audioSource.Play();
                setUpPhase = false;
                currentPhase = Phase.ROLL_DICE;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }

        } else
        {
            currentPlayer = PhotonNetwork.PlayerList[0];
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {

                

                Debug.Log("In turn" + turn + " the player to play is: " + PhotonNetwork.LocalPlayer.ActorNumber + ", name = " + PhotonNetwork.LocalPlayer.NickName);
                myTurn = true;
                audioSource.Play();
                currentPhase = Phase.ROLL_DICE;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }
        }
        
    }

    public void EndLocalTurn()
    {
        
        Debug.Log("Player id = " + PhotonNetwork.LocalPlayer.ActorNumber + " ending their turn.");
        myTurn = false;
        
        eventTextController.SendEvent(EventTextController.EventCode.END_TURN, PhotonNetwork.LocalPlayer);
        turnManager.SendMove(null, true);
        
    }


    public void OnTurnCompleted(int turn)
    {
        
        Debug.Log("Turn " + turn + " completed globally.");
        // check for gameover

        // start a new turn
        if (PhotonNetwork.IsMasterClient)
        {
            turnManager.BeginTurn();
        }
       
    }

    // Gets called when another player makes a move.
    public void OnPlayerMove(Player sender, int turn, object move)
    {

        int[] moveMessage = (int[])move;

        MessageCode moveCode = (MessageCode) moveMessage[0];

        switch (moveCode)
        {
            
            case MessageCode.RESOURCE_INCOME:
                // If I am the player who should receive the resources, update my inventory. In this case, turn and sender don't matter.
                int playerId = moveMessage[1];
                Inventory.UnitCode resourceType = (Inventory.UnitCode)moveMessage[2];
                int amount = moveMessage[3];

                if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    inventory.GiveToPlayer(resourceType, amount);
                }
                break;
            case MessageCode.TRADE_REQUEST:
                // Check if I am the recepient of the trade request.
                int recepientId = moveMessage[1];

                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Display Accept/Decline message locally.
                    tradeController.ReceiveTradeRequest(sender.ActorNumber, recepientId);
                }
                break;
            case MessageCode.TRADE_ACCEPTED:

                int senderId = moveMessage[1];

                // If I am the initial sender of the trade request, accept this message.
                if (senderId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeAccepted();
                }
                break;
            case MessageCode.TRADE_DECLINED:

                senderId = moveMessage[1];

                // If I am the initial sender of the trade request, accept this message.
                if (senderId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeDeclined();
                }
                break;
            case MessageCode.TRADE_CANCELLED:

                int receiverId = moveMessage[1];
                // Check if I am the recepient of this message.
                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeCancelled();
                }
                break;

            case MessageCode.TRADE_LOCK:

                receiverId = moveMessage[1];

                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeLocked();
                }
                break;
            case MessageCode.TRADE_UNLOCK:

                receiverId = moveMessage[1];

                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeUnlocked();
                }
                break;
            case MessageCode.TRADE_EXECUTE:

                receiverId = moveMessage[1];

                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.RemoteTradeExecuted();
                }
                break;
            case MessageCode.RESOURCE_OFFERED:
                
                receiverId = moveMessage[1];

                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber) {
                    tradeController.ResourceOffered((Inventory.UnitCode)moveMessage[2], moveMessage[3]);
                }
                break;
            case MessageCode.RESOURCE_RETRACTED:
                // Check if I am the recepient of this message.
                receiverId = moveMessage[1];

                if (receiverId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    tradeController.ResourceOfferRetracted((Inventory.UnitCode)moveMessage[2], moveMessage[3]);
                }
                break;
            case MessageCode.SEVEN_ROLLED_ANNOUNCEMENT:

                // Every player does this.
                senderId = moveMessage[1];
                
                int[] sevenRolledAcknowledgementMessage = new int[4];

                sevenRolledAcknowledgementMessage[0] = (int)MessageCode.SEVEN_ROLLED_ACKNOWLEDGEMENT;
                sevenRolledAcknowledgementMessage[1] = senderId;
                sevenRolledAcknowledgementMessage[2] = PhotonNetwork.LocalPlayer.ActorNumber;
                if (ShouldDiscard())
                {
                    sevenRolledAcknowledgementMessage[3] = 1;
                    turnManager.SendMove(sevenRolledAcknowledgementMessage, false);

                    GameObject.Find("DiscardController").GetComponent<DiscardController>().DiscardHalf(senderId);
                }
                else
                {
                    sevenRolledAcknowledgementMessage[3] = 0;
                    turnManager.SendMove(sevenRolledAcknowledgementMessage, false);
                }
                    
                break;
            case MessageCode.SEVEN_ROLLED_ACKNOWLEDGEMENT:

                recepientId = moveMessage[1];

                // Only the player who rolled the 7 does this.
                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    haveToDiscard[FindPosition(moveMessage[2])] = moveMessage[3] == 1 ? true : false;
                    acknowledged++;
                    if (acknowledged == PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        
                        waitingForDiscardCount = 0;
                        List<int> playerList = new List<int>();
                        for (int i = 0; i < 4; i++)
                        {
                            if (haveToDiscard[i])
                            {
                                playerList.Add(PhotonNetwork.PlayerList[i].ActorNumber);
                                waitingForDiscardCount++;
                            }
                        }
                        if (waitingForDiscardCount != 0)
                        {
                            eventTextController.SendEvent(EventTextController.EventCode.SHOULD_DISCARD, null, playerList.ToArray());
                        } else
                        {
                            // No one is discarding. Move bandit.
                            MoveBandit();
                        }
                    }
                }
                break;

            case MessageCode.SEVEN_ROLLED_DISCARD_COMPLETE:

                recepientId = moveMessage[1];

                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Find out who sent the message.
                    senderId = moveMessage[2];

                    
                    // Check if it was the last person who was being waited on to send the discard complete message.
                    waitingForDiscardCount--;

                    if (waitingForDiscardCount == 0)
                    {
                        // Last person to discard. Continue on to the moving the bandit phase.
                        MoveBandit();
                    }
                    else
                    {
                        // Notify event text to update.
                        eventTextController.SendEvent(EventTextController.EventCode.SHOULD_DISCARD, PhotonNetwork.CurrentRoom.GetPlayer(senderId));
                    }
                }
                break;

            case MessageCode.STEAL_RESOURCE_REQUEST:

                recepientId = moveMessage[1];

                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // I am the person who should give my opponent a random resource card.
                    inventory.GiveRandomResourceCard(moveMessage[2]);
                }

                break;

            case MessageCode.STEAL_RESOURCE_REPLY:

                recepientId = moveMessage[1];
                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Check which resource the player sent you.

                    inventory.ReceiveStolenCard(sender.ActorNumber, moveMessage[2]);
                    DisableRollDiceButton();
                    EnableEndingTurn();

                    eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
                    currentPhase = Phase.TRADE_BUILD_IDLE;
                }
                break;
            case MessageCode.MONOPOLY_PLAYED:

                senderId = moveMessage[1];

                if (PhotonNetwork.LocalPlayer.ActorNumber != senderId)
                {
                    // Give ALL of my resources to the sender.
                    inventory.GiveAllOfResourceToPlayer((Inventory.UnitCode)moveMessage[2], senderId);
                }
                break;
            case MessageCode.MONOPOLY_REPLY:

                recepientId = moveMessage[1];

                if (recepientId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // A player has given me all their resource of the adequate type.
                    GameObject.Find("DiscardController").GetComponent<DiscardController>().MonopolyReplyReceived(moveMessage[2]);
                }
                break;

            case MessageCode.LARGEST_ARMY_OVERTAKE:

                senderId = moveMessage[1];
                if (PhotonNetwork.LocalPlayer.ActorNumber != senderId)
                {
                    inventory.SetLargestArmyOwner(senderId, moveMessage[2]);
                }

                break;
            case MessageCode.LONGEST_ROAD_OVERTAKE:

                senderId = moveMessage[1];
                if (PhotonNetwork.LocalPlayer.ActorNumber != senderId)
                {
                    inventory.SetLongestRoadOwner(senderId, moveMessage[2]);
                }
                else
                {
                    if (moveMessage[3] == 0)
                    {
                        // Calling from the settlement overtake.
                        inventory.GiveToPlayer(Inventory.UnitCode.LONGEST_ROAD, 1);
                        inventory.AddVictoryPoint(Inventory.UnitCode.LONGEST_ROAD);
                    }
                    
                }

                break;
            case MessageCode.LONGEST_ROAD_RETURNED:
                inventory.SetLongestRoadOwner(-1, -1);
                
                break;
            case MessageCode.GAME_OVER:

                // A player has signalled that the game is over. Allow the local player to go back to the main menu by pressing ESC.
                this.gameOver = true;

                break;
        }

    }


    // Connectors calls - 
    public void OnPlayerFinished(Player sender, int turn, object move)
    {
        // Player <player> has ended their turn on their machine. End it locally as well.

        // Start the turn for the next player only if there's players who haven't yet played it.

        if (turnManager.IsCompletedByAll) { return; }


        Player nextPlayer;

        int prevPlayerPosition = -1;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (sender.ActorNumber == PhotonNetwork.PlayerList[i].ActorNumber)
            {
                prevPlayerPosition = i;
                break;
            }
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            if (currentTurn != 2)
            {
                nextPlayer = PhotonNetwork.PlayerList[prevPlayerPosition + 1];
            }
            else
            {
                // LIFO setup.
                nextPlayer = PhotonNetwork.PlayerList[prevPlayerPosition - 1];
            }

        }
        else
        {
            nextPlayer = null;
        }

        if (nextPlayer == null)
        {
            return;
        }

        this.currentPlayer = nextPlayer;
        if (PhotonNetwork.LocalPlayer == nextPlayer)
        {
            currentTurn = turn;

            if (turn == 1)
            {
                ClaimColour();
                myTurn = true;
                
                

                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
                setUpPhase = true;

                // Turns starts once a colour slot is claimed.
            }
            else if (turn == 2)
            {

                

                // Init trade.
                tradeController = GameObject.Find("TradeController").GetComponent<TradeController>();
                tradeController.Init(inventory);

                myTurn = true;
                audioSource.Play();
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }
            else if (turn == 3)
            {
                

                setUpPhase = false;
                currentPhase = Phase.ROLL_DICE;
                myTurn = true;
                audioSource.Play();
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }
            else
            {
                
                myTurn = true;
                audioSource.Play();
                currentPhase = Phase.ROLL_DICE;
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
            }
        }
    }

    public void OnTurnTimeEnds(int turn)
    {
        //throw new System.NotImplementedException();
    }


    #endregion


    #region Player Abilities

    protected void FindSelectableIntersections()
    {

        if (busy) return;

        busy = true;

        if (currentTurn == 1)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            eventTextController.SendEvent(EventTextController.EventCode.FIRST_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

        }
        else if (currentTurn == 2)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            eventTextController.SendEvent(EventTextController.EventCode.SECOND_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

        }

        selectableIntersections = new List<Intersection>();
        if (currentTurn < 3)
        {
            // This logic only applies when placing starting settlements.
            
            foreach (GameObject intersection in intersections)
            {
                Intersection i = intersection.GetComponent<Intersection>();
                if (i.IsAvailable())
                {
                    selectableIntersections.Add(i);
                    i.ToggleRipple();
                }
            }
        }
        else
        {
            // Locate reachable, available intersections. Avoid duplicates.
            
            foreach (WorldPath road in myRoads)
            {
                foreach (Intersection i in road.GetIntersections())
                {
                    if (i.IsAvailable())
                    {
                        if (!selectableIntersections.Contains(i))
                        {
                            selectableIntersections.Add(i);
                            i.ToggleRipple();
                        }
                        
                    }
                }
            }
        }
        
    }

    protected void FindSelectablePaths()
    {
        if (busy) return;

        busy = true;

        

        if (currentTurn == 1)
        {
            eventTextController.SendEvent(EventTextController.EventCode.FIRST_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);

        }
        else if (currentTurn == 2)
        {
            eventTextController.SendEvent(EventTextController.EventCode.SECOND_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);
        }


        selectablePaths = new List<WorldPath>();

        if (currentTurn < 3)
        {
            // This logic only applies when placing starting roads.
            foreach (Intersection i in myIntersections)
            {
                List<WorldPath> availablePaths = i.GetAvailablePaths();

                foreach (var path in availablePaths)
                {
                    if ((currentTurn == 1 && i == myIntersections[0]) || (currentTurn == 2 && i == myIntersections[1]))
                    {
                        selectablePaths.Add(path);
                        path.ToggleBlink();
                    }

                }
            }
        }
        else
        {

            

            // 1) Paths adjacent to the player's settlements that are available.

            foreach (Intersection i in myIntersections)
            {
                selectablePaths.AddRange(i.GetAvailablePaths());
            }

            // 2) Paths connected to the player's roads that do NOT cross an opponent's settement. Avoid duplicates.

            foreach (WorldPath road in myRoads)
            {
                bool invalid = false;

                Intersection[] roadIntersections = road.GetIntersections();

                foreach (Intersection i in roadIntersections)
                {
                    if (i.GetOwnerId() != 0 && i.GetOwnerId() != PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        // We cannot construct roads through other players' settlements.
                        invalid = true;
                        break;
                    }
                }

                if (!invalid)
                {
                    List<WorldPath> connectedPaths = road.GetAvailablePaths(); // returns 6 results

                    foreach (WorldPath p in connectedPaths)
                    {
                        if (!selectablePaths.Contains(p))
                        {
                            selectablePaths.Add(p);
                        }
                    }
                }
                
            }

            // Toggle blinks for the player.

            foreach (WorldPath path in selectablePaths)
            {
                path.ToggleBlink();
            }
        }

        // If the player is placing free roads (Expansion Development card), adjust counters.
        if (currentPhase == Phase.PLAYED_EXPANSION_CARD)
        {
            // Reset if required.
            if (freeRoadsPlaced == 2)
            {
                freeRoadsPlaced = 0;
            }
            
        }

    }

    protected void FindSelectableSettlements()
    {
        if (busy) return;

        busy = true;

        selectableSettlements = new List<Intersection>();

        foreach (Intersection i in myIntersections)
        {
            if (i.HasSettlement())
            {
                selectableSettlements.Add(i);
                i.ToggleRipple();
            }
        }
        
    }
    
    protected void ToggleIntersectionRipples()
    {
        foreach (Intersection intersection in selectableIntersections)
        {
            intersection.ToggleRipple();
        }
        busy = false;
    }

    protected void TogglePathBlink()
    {
        foreach (WorldPath path in selectablePaths)
        {
            path.ToggleBlink();
        }
        busy = false;
    }

    protected void ToggleSettlementRipples()
    {
        Debug.Log("toggling settlement ripples, selectablesettlementscount = " + selectableSettlements.Count);
        foreach (Intersection i in selectableSettlements)
        {
            i.ToggleRipple();
        }
        busy = false;
    }

    protected void EnableRolling()
    {

        if (busy) { return; }

        busy = true;

        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        bottomPanel.EnableRolling();

        eventTextController.SendEvent(EventTextController.EventCode.PRE_DICE_ROLL, PhotonNetwork.LocalPlayer);
    }

    protected void EnableEndingTurn()
    {
        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        bottomPanel.EnableEndingTurn();
    }

    protected void DisableEndingTurn()
    {
        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        bottomPanel.DisableEndingTurn();

    }
    public void DisableRollDiceButton()
    {
        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        bottomPanel.DisableRollDiceButton();
    }
    
    public void WaitForDiceResult(int diceValue)
    {
        busy = false;
        // inform everyone
        Debug.Log("Dice rolled - " + diceValue);

        eventTextController.SendEvent(EventTextController.EventCode.DICE_ROLLED, PhotonNetwork.LocalPlayer, diceValue);

        if (diceValue != 7)
        {
            ResourceIncome(diceValue);

            // move on to action phase
            DisableRollDiceButton();
            EnableEndingTurn();

            eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
            currentPhase = Phase.TRADE_BUILD_IDLE;

        } else
        {
            // Notify players that a 7 has been rolled.

            DisableRollDiceButton();
            DisableEndingTurn();

            int[] sevenRolledMessage = new int[2];
            sevenRolledMessage[0] = (int)MessageCode.SEVEN_ROLLED_ANNOUNCEMENT;
            sevenRolledMessage[1] = PhotonNetwork.LocalPlayer.ActorNumber;

            acknowledged = 0;
            haveToDiscard = new bool[4];
            
            // Logic continues once the player receives the sufficient amount of DISCARD_COMPLETE messages.
            currentPhase = Phase.TRADE_BUILD_IDLE;

            turnManager.SendMove(sevenRolledMessage, false);
        }

        
    }

    public void EndTurnButtonPress()
    {
        Debug.Log("end turn button pressed");

        // Close trade window if it was open.
        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        if (bottomPanel.TradePanelOpen())
        {
            bottomPanel.OpenTradeTab();
        }

        if (currentPhase == Phase.TRADE_BUILD_IDLE)
        {
            DisableEndingTurn();
            EndLocalTurn();
        }
            
        
    }

    #endregion

    private void ResourceIncome(int diceValue)
    {
        // find out for eveyrone who gets a resource, broadcast info, wait 3s, repeat

        // find hexes with that number
        GameObject[] hexGOs = GameObject.FindGameObjectsWithTag("Hex");

        List<Hex> validHexes = new List<Hex>();

        foreach (GameObject hexGO in hexGOs)
        {
            Hex hex = hexGO.GetComponent<Hex>();
            
            if (hex.GetNumber() == diceValue)
            {
                validHexes.Add(hex);
            }
        }

        // Indeces in array range from 0-3. Player id's range from 1-4.
        bool incomeAnnounced = false;

        foreach (Hex hex in validHexes)
        {
            

            Hex.Resource resourceType = hex.GetResource();

            // Deserts do not generate income.
            if (resourceType == Hex.Resource.NO_RESOURCE) { continue; }


            // calculate how much each player has earned
            int[] incomeByPlayer = hex.GenerateIncome();
            for (int i = 0; i < 4; i++)
            {
                if (incomeByPlayer[i] != 0)
                {

                    incomeAnnounced = true;
                    // announce income - prepare resource income message

                    int[] resourceIncomeMessage = new int[4];
                    resourceIncomeMessage[0] = (int)MessageCode.RESOURCE_INCOME;
                    resourceIncomeMessage[1] = PhotonNetwork.PlayerList[i].ActorNumber;
                    resourceIncomeMessage[2] = (int)resourceType;
                    resourceIncomeMessage[3] = incomeByPlayer[i];

                    turnManager.SendMove(resourceIncomeMessage, false);
                    eventTextController.SendEvent(EventTextController.EventCode.RESOURCE_EARNED, PhotonNetwork.PlayerList[i], resourceIncomeMessage[2], resourceIncomeMessage[3]);
                }
            }
            
        }

        if (!incomeAnnounced)
        {
            eventTextController.SendEvent(EventTextController.EventCode.NO_RESOURCE_EARNED, PhotonNetwork.LocalPlayer);
        }

    }

    private bool ShouldDiscard()
    {
        return inventory.GetResourceCardCount() >= 8;
    }

    public void MoveBandit()
    {
        Debug.Log("Moving bandit!");

        eventTextController.SendEvent(EventTextController.EventCode.BANDIT_MOVE, PhotonNetwork.LocalPlayer);

        currentPhase = Phase.BANDIT_MOVE;
    }

    protected void StealFromPlayer(Hex banditHex)
    {
        // Find all the players that have a settlement or a city on this hex (and they are not this player).

        currentPhase = Phase.STEAL;

        Intersection[] hexIntersections = banditHex.GetIntersections();
        List<int> playerIdList = new List<int>();

        foreach (Intersection i in hexIntersections)
        {
            if (i.HasSettlement() || i.HasCity())
            {
                if (i.GetOwnerId() != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Only add a player once.
                    if (!playerIdList.Contains(i.GetOwnerId()))
                    {
                        playerIdList.Add(i.GetOwnerId());
                    }
                }
            }
        }

        // If the player list is empty, just move on to the TRADE_BUILD phase.
        if (playerIdList.Count == 0)
        {
            eventTextController.SendEvent(EventTextController.EventCode.STEAL_NO_ADJACENT_PLAYER, PhotonNetwork.LocalPlayer);
            DisableRollDiceButton();
            EnableEndingTurn();

            eventTextController.SendEvent(EventTextController.EventCode.PLAYER_IDLE, PhotonNetwork.LocalPlayer);
            currentPhase = Phase.TRADE_BUILD_IDLE;
            EnableEndingTurn();
        }
        else
        {

            eventTextController.SendEvent(EventTextController.EventCode.SELECTING_STEAL_VICTIM, PhotonNetwork.LocalPlayer);
            // Notify the discard controller to open the steal panel and allow the player to select someone to steal from.
            GameObject.Find("DiscardController").GetComponent<DiscardController>().PrepareStealing(playerIdList);
        }
    }
    
    public void GameOver()
    {
        // Player won.
        eventTextController.SendEvent(EventTextController.EventCode.GAME_OVER, PhotonNetwork.LocalPlayer);

        // End the local player's turn.
        myTurn = false;

        // Notify everybody to set their game over flag.

        int[] gameOverMessage = new int[1];
        gameOverMessage[0] = (int)MessageCode.GAME_OVER;
        turnManager.SendMove(gameOverMessage, false);
    }

    public static int FindPosition(int ownerId)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == ownerId)
            {
                return i;
            }
        }
        return -1;
    }
}
