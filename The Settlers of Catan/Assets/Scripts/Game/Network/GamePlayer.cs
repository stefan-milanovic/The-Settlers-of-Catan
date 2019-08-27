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
        TRADING
    }

    protected enum MessageCode
    {
        RESOURCE_INCOME,
        TRADE_REQUEST
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

    protected LeaderboardController leaderboardController;
    protected TradeController tradeController;

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

    public void ClaimLeaderboardSlot()
    {
        for (int i = 0; i < 4; i++)
        {
            string key = "leaderboardSlot" + (i + 1);

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
                // This slot will be taken -- leave the loop.
                Debug.Log("Player " + PhotonNetwork.LocalPlayer.ActorNumber + " got slot: " + key);
                break;
            }
        }
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
                tradeController.Init();

                // Claim an empty leaderboard slot.
                ClaimLeaderboardSlot();

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
    public void OnPlayerMove(Player player, int turn, object move)
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
                // Check if I am the recepient.
                int senderId = moveMessage[1];

                if (senderId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Display Accept/Decline message locally.
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
                tradeController.Init();

                // Claim an empty leaderboard slot.
                ClaimLeaderboardSlot();
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

    protected void FindSelectablePaths()
    {
        if (busy) return;

        busy = true;

        selectablePaths = new List<WorldPath>();

        if (currentTurn == 1)
        {
            eventTextController.SetText(EventTextController.TextCode.FIRST_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);

        }
        else if (currentTurn == 2)
        {
            eventTextController.SetText(EventTextController.TextCode.SECOND_TURN_PHASE_TWO, PhotonNetwork.LocalPlayer);
        }

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
        foreach (GameObject intersection in intersections)
        {
            Intersection i = intersection.GetComponent<Intersection>();
            if (i.IsAvailable())
            {
                i.ToggleRipple();
            }
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

        ResourceIncome(diceValue);

        // move on to action phase

        DisableRollDiceButton();
        EnableEndingTurn();

        currentPhase = Phase.TRADE_BUILD_IDLE;
    }

    public void EndTurnButtonPress()
    {
        Debug.Log("end turn button pressed");
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

    public void SendTradeRequest(Player recepient)
    {
        // send request via
        int[] tradeRequestMessage = new int[2];
        tradeRequestMessage[0] = (int)MessageCode.TRADE_REQUEST;
        tradeRequestMessage[1] = recepient.ActorNumber;
        turnManager.SendMove(tradeRequestMessage, false);
    }
}
