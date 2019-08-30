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
        TRADING
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

    }
    
    // during dice roll phase the player can activate a development card from earlier
    protected Phase currentPhase = Phase.FIRST_SETTLEMENT_PLACEMENT;

    protected EventTextController eventTextController;

    protected bool setUpPhase = true;

    protected string username;
    protected string colourHex;

    protected bool myTurn = false;
    protected int currentTurn;

    protected List<Intersection> selectableIntersections = new List<Intersection>();
    protected List<Intersection> selectableSettlements = new List<Intersection>();
    protected List<WorldPath> selectablePaths = new List<WorldPath>();

    protected GameObject[] intersections;
   
    // Every player has their own inventory of cards, resources, and buildings.
    protected Inventory inventory;

    [SerializeField]
    protected GameObject inventoryPrefab;

    [SerializeField]
    protected GameObject turnManagerPrefab;

    protected PhotonView photonView;

    protected TurnManager turnManager;

    protected bool busy = false;

    protected List<Intersection> myIntersections = new List<Intersection>();
    protected List<WorldPath> myPaths = new List<WorldPath>();

    protected Player currentPlayer = null;

    protected Card selectedConstructionCard = null;
    
    protected TradeController tradeController;

    protected int acknowledged;
    protected bool[] haveToDiscard;
    protected int waitingForDiscardCount;

    // UI

    protected Button rollDiceButton;

    // Start is called before the first frame update
    void Start()
    {
       
    }
    
    protected void Init()
    {

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


    #region IPunTurnManagerCallbacks

    // Called from TurnManager - Each player gets this called by their local turn manager
    public void OnTurnBegins(int turn)
    {
        Debug.Log("Turn " + turn + " beginning globally.");
        
        currentTurn = turn;
        
        if (turn == 1)
        {

            

            currentPlayer = PhotonNetwork.CurrentRoom.GetPlayer(1);

            setUpPhase = true;
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {

                // Init trade.
                tradeController = GameObject.Find("TradeController").GetComponent<TradeController>();
                tradeController.Init(inventory);
                
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);

                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);

                myTurn = true;
            }
        }
        else if (turn == 2)
        {

            currentPlayer = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.PlayerCount);

            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                Debug.Log("In turn2 the player to play is: " + PhotonNetwork.LocalPlayer.ActorNumber + ", name = " + PhotonNetwork.LocalPlayer.NickName);
                myTurn = true;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }

        } else if (turn == 3)
        {
            
            currentPlayer = PhotonNetwork.CurrentRoom.GetPlayer(1);
            
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                myTurn = true;
                setUpPhase = false;
                currentPhase = Phase.ROLL_DICE;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }

        } else
        {
            currentPlayer = PhotonNetwork.CurrentRoom.GetPlayer(1);
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                Debug.Log("In turn" + turn + " the player to play is: " + PhotonNetwork.LocalPlayer.ActorNumber + ", name = " + PhotonNetwork.LocalPlayer.NickName);
                myTurn = true;
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
                    haveToDiscard[acknowledged++] = moveMessage[3] == 1 ? true : false;

                    if (acknowledged == PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        
                        waitingForDiscardCount = 0;
                        List<Player> playerList = new List<Player>();
                        for (int i = 0; i < 4; i++)
                        {
                            if (haveToDiscard[i])
                            {
                                playerList.Add(PhotonNetwork.CurrentRoom.GetPlayer(i));
                                waitingForDiscardCount++;
                            }
                        }
                        if (waitingForDiscardCount != 0)
                        {
                            eventTextController.SetText(EventTextController.TextCode.SHOULD_DISCARD, null, playerList);
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

                    // Notify event text to update.
                    eventTextController.SetText(EventTextController.TextCode.SHOULD_DISCARD, PhotonNetwork.CurrentRoom.GetPlayer(senderId));

                    // Check if it was the last person who was being waited on to send the discard complete message.

                    waitingForDiscardCount--;

                    if (waitingForDiscardCount == 0)
                    {
                        // Last person to discard. Continue on to the moving the bandit phase.
                        MoveBandit();
                    }
                }
                break;
        }

    }


    // Connectors calls - 
    public void OnPlayerFinished(Player player, int turn, object move)
    {
        // Player <player> has ended their turn on their machine. End it locally as well.

        // Start the turn for the next player only if there's players who haven't yet played it.

        if (turnManager.IsCompletedByAll) { return; }


        Player nextPlayer;

        if (currentTurn != 2)
        {
            nextPlayer = player.GetNext();
        }
        else
        {
            // LIFO setup.
            nextPlayer = player.Get(player.ActorNumber - 1);
        }

        if (nextPlayer == null)
        {
            return;
        }

        this.currentPlayer = nextPlayer;
        if (PhotonNetwork.LocalPlayer == nextPlayer)
        {
            if (turn == 1)
            {
                // Init trade.
                tradeController = GameObject.Find("TradeController").GetComponent<TradeController>();
                tradeController.Init(inventory);
                
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
                setUpPhase = true;
                myTurn = true;
            }
            else if (turn == 2)
            {
                myTurn = true;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }
            else if (turn == 3)
            {
                setUpPhase = false;
                currentPhase = Phase.ROLL_DICE;
                myTurn = true;
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
            }
            else
            {
                myTurn = true;
                currentPhase = Phase.ROLL_DICE;
                GameObject.Find("DiceController").GetComponent<DiceController>().SetDiceOwner(PhotonNetwork.LocalPlayer);
                eventTextController.SetCurrentPlayer(PhotonNetwork.LocalPlayer);
            }
        }
    }

    public void OnTurnTimeEnds(int turn)
    {
        throw new System.NotImplementedException();
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
            eventTextController.SetText(EventTextController.TextCode.FIRST_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

        }
        else if (currentTurn == 2)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            eventTextController.SetText(EventTextController.TextCode.SECOND_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

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
            
            foreach (WorldPath road in myPaths)
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
            eventTextController.SetText(EventTextController.TextCode.FIRST_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);

        }
        else if (currentTurn == 2)
        {
            eventTextController.SetText(EventTextController.TextCode.SECOND_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);
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
                    if ((currentTurn == 1 && i == myIntersections[0]) || (currentTurn == 2 && i == myIntersections[1]) || currentTurn >= 3)
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

            // 2) Paths connected to the player's roads. Avoid duplicates.

            foreach (WorldPath road in myPaths)
            {
                List<WorldPath> connectedPaths = road.GetAvailablePaths();

                foreach (WorldPath p in connectedPaths)
                {
                    if (!selectablePaths.Contains(p))
                    {
                        selectablePaths.Add(p);
                    }
                }
            }

            // Toggle blinks for the player.

            foreach (WorldPath path in selectablePaths)
            {
                path.ToggleBlink();
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

        eventTextController.SetText(EventTextController.TextCode.PRE_DICE_ROLL, PhotonNetwork.LocalPlayer);
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
    protected void DisableRollDiceButton()
    {
        BottomPanel bottomPanel = GameObject.Find("BottomPanel").GetComponent<BottomPanel>();
        bottomPanel.DisableRollDiceButton();
    }
    
    public void WaitForDiceResult(int diceValue)
    {
        busy = false;
        // inform everyone
        Debug.Log("Dice rolled - " + diceValue);

        eventTextController.SetText(EventTextController.TextCode.DICE_ROLLED, PhotonNetwork.LocalPlayer, diceValue);

        if (diceValue != 7)
        {
            ResourceIncome(diceValue);

            // move on to action phase
            DisableRollDiceButton();
            EnableEndingTurn();

            currentPhase = Phase.TRADE_BUILD_IDLE;

        } else
        {
            // Notify players that a 7 has been rolled.

            int[] sevenRolledMessage = new int[2];
            sevenRolledMessage[0] = (int)MessageCode.SEVEN_ROLLED_ANNOUNCEMENT;
            sevenRolledMessage[1] = PhotonNetwork.LocalPlayer.ActorNumber;

            acknowledged = 0;
            haveToDiscard = new bool[4];


            turnManager.SendMove(sevenRolledMessage, false);

            // Logic continues once the player receives the sufficient amount of DISCARD_COMPLETE messages.
            currentPhase = Phase.TRADE_BUILD_IDLE;

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

        DisableEndingTurn();
        EndLocalTurn();
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
                    resourceIncomeMessage[1] = i + 1;
                    resourceIncomeMessage[2] = (int)resourceType;
                    resourceIncomeMessage[3] = incomeByPlayer[i];

                    turnManager.SendMove(resourceIncomeMessage, false);
                    eventTextController.AddToQueue(EventTextController.TextCode.RESOURCE_EARNED, PhotonNetwork.CurrentRoom.GetPlayer(i + 1), resourceIncomeMessage[2], resourceIncomeMessage[3]);
                }
            }
            
        }

        if (!incomeAnnounced)
        {
            eventTextController.SetText(EventTextController.TextCode.NO_RESOURCE_EARNED, PhotonNetwork.LocalPlayer);
        }

    }

    private bool ShouldDiscard()
    {
        return inventory.GetResourceCardCount() >= 8;
    }

    private void MoveBandit()
    {
        Debug.Log("Moving bandit!");

        eventTextController.SetText(EventTextController.TextCode.BANDIT_MOVE, PhotonNetwork.LocalPlayer);

        currentPhase = Phase.BANDIT_MOVE;
    }


}
