using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour, IPunTurnManagerCallbacks
{

    protected EventTextController eventTextController;

    protected bool setUpPhase = true;

    protected string username;
    protected string colourHex;

    protected bool myTurn = false;
    protected int currentTurn;

    protected List<Intersection> selectableIntersections = new List<Intersection>();
    protected List<WorldPath> selectablePaths = new List<WorldPath>();

    protected GameObject[] intersections;

    // TEMP
    public ChatController chat;

    // Every player has their own inventory of cards, resources, and buildings.
    protected Inventory inventory;

    [SerializeField]
    protected GameObject inventoryPrefab;

    [SerializeField]
    protected GameObject turnManagerPrefab;

    protected PhotonView photonView;

    protected TurnManager turnManager;

    protected bool selectingIntersection = false;
    protected bool selectingPath = false;

    protected List<Intersection> myIntersections = new List<Intersection>();
    protected List<WorldPath> myPaths = new List<WorldPath>();


    // UI

    protected Button rollDiceButton;

    // Start is called before the first frame update
    void Start()
    {
       
    }
    
    protected void Init()
    {

        rollDiceButton = GameObject.Find("RollDiceButton").GetComponent<Button>();
        // Claim an empty leaderboard slot.
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void ConnectToTurnManager()
    {
        this.turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.RegisterPlayer();
    }
 
    public void EndTurn()
    {
        Debug.Log("Player id = " + PhotonNetwork.LocalPlayer.ActorNumber + " ending their turn.");
        chat.AddMessageToSelectedChannel(PhotonNetwork.LocalPlayer.NickName + "ending turn " + currentTurn);
        myTurn = false;

        turnManager.SendMove(null, true);
    }

    protected void FindSelectableIntersections()
    {
        if (selectingIntersection) return;

        if (currentTurn == 1)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            eventTextController.SetText(EventTextController.TextCode.FIRST_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

        } else if (currentTurn == 2)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            eventTextController.SetText(EventTextController.TextCode.SECOND_TURN_PHASE_ONE, PhotonNetwork.LocalPlayer);

        }
        
        selectingIntersection = true;

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
        if (selectingPath) return;

        selectingPath = true;
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
    }

    protected void TogglePathBlink()
    {
        foreach (WorldPath path in selectablePaths)
        {
            path.ToggleBlink();
        }
    }

    public Inventory GetInventory()
    {
        return inventory;
    }


    #region IPunTurnManagerCallbacks

    // Called from TurnManager - Connectors call
    public void OnTurnBegins(int turn)
    {
        
        Debug.Log("Turn " + turn + " beginning globally.");

        chat.AddMessageToSelectedChannel(PhotonNetwork.LocalPlayer.NickName + "(gameobject locally: " + gameObject.name + ") start turn globally.");

        currentTurn = turn;
        if (turn == 1)
        {
            setUpPhase = true;
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                chat.AddMessageToSelectedChannel(PhotonNetwork.LocalPlayer.NickName + "starting turn in turn1");
                myTurn = true;
            }
        }
        else if (turn == 2)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Debug.Log("In turn2 the player to play is: " + PhotonNetwork.LocalPlayer.ActorNumber + ", name = " + PhotonNetwork.LocalPlayer.NickName);
                chat.AddMessageToSelectedChannel(PhotonNetwork.LocalPlayer.NickName + "starting turn in turn2");
                myTurn = true;
            }

        } else if (turn == 3)
        {
            setUpPhase = false;
        } else
        {

        }
        
    }

    public void OnTurnCompleted(int turn)
    {
        
        Debug.Log("Turn " + turn + " completed globally.");

        chat.AddMessageToSelectedChannel(PhotonNetwork.LocalPlayer.NickName + "acknowledges global completion");
        // check for gameover

        // start a new turn
        turnManager.BeginTurn();
    }

    // Gets called when another player makes a move.
    public void OnPlayerMove(Player player, int turn, object move)
    {
        
    }


    // Connectors calls - 
    public void OnPlayerFinished(Player player, int turn, object move)
    {
        // end turn


        // start the turn for the next player
        Player nextPlayer;

        if (currentTurn != 2)
        {
            nextPlayer = player.GetNext();
        }
        else
        {
            // LIFO setup.
            nextPlayer = player.Get(PhotonNetwork.LocalPlayer.ActorNumber - 1);
        }

        Debug.Log("Player id = " + PhotonNetwork.LocalPlayer.ActorNumber + " starting turn");
        if (PhotonNetwork.LocalPlayer.ActorNumber == nextPlayer.ActorNumber)
        {
            myTurn = true;

            if (turn == 3)
            {
                setUpPhase = false;
            }

        }
    }

    public void OnTurnTimeEnds(int turn)
    {
        throw new System.NotImplementedException();
    }


    #endregion


    #region Player Abilities
    

    protected void EnableRollDiceButton()
    {
        InventoryUIController inventoryUIController = inventory.GetInventoryUIController();
        inventoryUIController.EnableRollDiceButton();
    }

    protected void DisableRollDiceButton()
    {
        InventoryUIController inventoryUIController = inventory.GetInventoryUIController();
        inventoryUIController.DisableRollDiceButton();
    }

    public void WaitForDiceResult(int diceValue)
    {
        // inform everyone
    }

    #endregion
}
