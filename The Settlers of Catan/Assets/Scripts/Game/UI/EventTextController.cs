using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EventTextController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI eventText;

    [SerializeField]
    private GameObject eventPanel;

    [SerializeField]
    private TextMeshProUGUI currentPlayerText;

    [SerializeField]
    private AudioSource audioSource;

    private PhotonView photonView;
    public enum EventCode
    {
        FIRST_TURN_PHASE_ONE,
        FIRST_TURN_PHASE_TWO,
        SECOND_TURN_PHASE_ONE,
        SECOND_TURN_PHASE_TWO,
        SETTLEMENT_CONSTRUCTED,
        ROAD_CONSTRUCTED,
        CITY_CONSTRUCTED,
        PRE_DICE_ROLL,
        DICE_ROLLED,
        PLAYER_IDLE,
        RESOURCE_EARNED,
        NO_RESOURCE_EARNED,
        TRADE_SUPPLY_INITIATED,
        TRADE_PLAYERS_INITIATED,
        TRADE_CANCELLED,
        TRADE_SUPPLY_COMPLETED,
        TRADE_PLAYERS_COMPLETED,
        SHOULD_DISCARD,
        BANDIT_MOVE,
        SELECTING_STEAL_VICTIM,
        STEAL_NO_ADJACENT_PLAYER,
        NO_RESOURCE_STOLEN,
        RESOURCE_STOLEN,
        DEVELOPMENT_CARD_PURCHASED,
        END_TURN,
        GAME_OVER,
    };
    
    private bool busy = false;

    private List<int> discardList;


    private Queue<string> messageQueue = new Queue<string>();
    private bool incomingNewMessage = true;

    private bool firstPass;

    private string currentMessage;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsBusy()
    {
        return incomingNewMessage == true;
    } 

    public void Init()
    {
        messageQueue.Enqueue("Waiting for all players to connect...");
        StartCoroutine(AcceptNewMessage());
    }
    
    public void SetCurrentPlayer(Player player)
    {
        photonView.RPC("RPCSetCurrentPlayer", RpcTarget.All, player.ActorNumber);
    }

    [PunRPC]
    private void RPCSetCurrentPlayer(int playerId)
    {
        currentPlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(playerId);
    }

    public void SendEvent(EventCode messageCode, Player sender, params object[] additionalParams)
    {
        photonView.RPC("RPCEventReceived", RpcTarget.All, messageCode, sender != null ? sender.ActorNumber : -1, additionalParams);
    }

    [PunRPC]
    private void RPCEventReceived(EventCode messageCode, int senderId, params object[] additionalParams)
    {
        
        string message = GetMessage(messageCode, senderId, additionalParams);
        messageQueue.Enqueue(message);
        incomingNewMessage = true;

    }
    
    private IEnumerator FadeOutPanel()
    {
        eventPanel.GetComponent<CanvasGroup>().alpha = 1;
        
        if (firstPass)
        {
            yield return new WaitForSeconds(1);
        }
        

        for (float f = 1f; f >= -0.025f; f -= 0.025f)
        {
            if (incomingNewMessage && !firstPass)
            {
                StartCoroutine(AcceptNewMessage());
                yield break;
            }

            eventPanel.GetComponent<CanvasGroup>().alpha -= 0.025f;
            yield return new WaitForSeconds(0.025f);
        }

        if (incomingNewMessage)
        {
            StartCoroutine(AcceptNewMessage());
            yield break;
        }
        else
        {
            firstPass = false;
            StartCoroutine(FadeInPanel());
        }
        
       
    }
   
    private IEnumerator FadeInPanel()
    {
        
        for (float f = 1f; f >= -0.025f; f -= 0.025f)
        {
            if (incomingNewMessage)
            {
                StartCoroutine(AcceptNewMessage());
                yield break;
            }

            eventPanel.GetComponent<CanvasGroup>().alpha += 0.025f;
            yield return new WaitForSeconds(0.025f);
        }
        
        StartCoroutine(FadeOutPanel());
    }

    private IEnumerator AcceptNewMessage()
    {
        SetText(currentMessage = messageQueue.Dequeue());

        if (messageQueue.Count == 0)
        {
            incomingNewMessage = false;
        }
        
        firstPass = true;

        StartCoroutine(FadeOutPanel());

        yield break;
    }
    
    private void SetText(string message)
    {
        eventText.text = message;
    }
    
    private string GetMessage(EventCode code, int actorNumber, params object[] additionalParams)
    {
        switch (code)
        {
            case EventCode.FIRST_TURN_PHASE_ONE:
                
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + "</color>" + " is placing their first settlement.";
                
            case EventCode.FIRST_TURN_PHASE_TWO:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is placing their first road.";
               
            case EventCode.SECOND_TURN_PHASE_ONE:
                
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is placing their second settlement.";
                
            case EventCode.SECOND_TURN_PHASE_TWO:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is placing their second road.";
            case EventCode.ROAD_CONSTRUCTED:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " constructed a road.";
            case EventCode.SETTLEMENT_CONSTRUCTED:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " constructed a settlement.";
            case EventCode.CITY_CONSTRUCTED:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " upgraded a settlement into a city.";
            case EventCode.PRE_DICE_ROLL:
                
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is rolling the dice.";
                
            case EventCode.DICE_ROLLED:
                int diceValue = (int)additionalParams[0];
                string diceValueString = "";
                if (diceValue == 7)
                {
                    diceValueString = "<color=black>";
                }
                else if (diceValue == 6 || diceValue == 8)
                {
                    diceValueString = "<color=red>";
                }

                diceValueString += diceValue;
                if (diceValue >= 6 && diceValue <= 8) {
                    diceValueString += "</color>";
                }
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has rolled a " + diceValueString + ".";
            case EventCode.PLAYER_IDLE:

                return "Waiting for an action from " + ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + ".";
            case EventCode.RESOURCE_EARNED:

                int resourceType = (int)additionalParams[0];
                int amount = (int)additionalParams[1];

                Debug.Log("resourceType = " + resourceType + ", amount = " + amount);

                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has earned " + amount + "x" + ColourUtility.GetResourceText((Inventory.UnitCode)resourceType) + ".";

            case EventCode.NO_RESOURCE_EARNED:

                return "No player has earned any resources from this roll.";
            case EventCode.TRADE_SUPPLY_INITIATED:

                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is trading with the supply.";
            case EventCode.TRADE_PLAYERS_INITIATED:
                int secondPlayerId = (int)additionalParams[0];
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " is trading with " + ColourUtility.GetPlayerDisplayNameFromId(secondPlayerId) + ".";
            case EventCode.TRADE_CANCELLED:

                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has cancelled the trade.";
            case EventCode.TRADE_SUPPLY_COMPLETED:

                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has finished trading with the supply.";
            case EventCode.TRADE_PLAYERS_COMPLETED:
                secondPlayerId = (int)additionalParams[0];
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has finished trading with " + ColourUtility.GetPlayerDisplayNameFromId(secondPlayerId) + ".";

            case EventCode.SHOULD_DISCARD:
                
                string resultText = "Waiting for ";

                if (actorNumber == -1)
                {
                    // Initial set.
                    discardList = ((int[])additionalParams[0]).OfType<int>().ToList();

                    bool first = true;
                    for (int i = 0; i < discardList.Count; i++)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            resultText += ", ";
                        }
                        //discardlist[i] is null for some reason
                        resultText += ColourUtility.GetPlayerDisplayNameFromId(discardList[i]);
                       
                    }
                    resultText += " to discard half of their resource cards.";
                }
                else
                {
                    // Update. Remove the player from the discard list and then repeat.
                    if (discardList.IndexOf(actorNumber) != -1)
                    {
                        discardList.Remove(actorNumber);
                    }

                    bool first = true;
                    for (int i = 0; i < discardList.Count; i++)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            resultText += ", ";
                        }
                        resultText += ColourUtility.GetPlayerDisplayNameFromId(discardList[i]); 
                        
                    }
                    resultText += " to discard half of their resource cards.";

                }

                Debug.Log(resultText);
                return resultText;

            case EventCode.BANDIT_MOVE:
                return "Waiting for " + ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " to move the bandit to another hex.";
            case EventCode.SELECTING_STEAL_VICTIM:
                return "Waiting for " + ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " to select a player to steal from.";
            case EventCode.STEAL_NO_ADJACENT_PLAYER:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " had nobody to steal from.";
            case EventCode.NO_RESOURCE_STOLEN:
                Player stealPlayer = PhotonNetwork.CurrentRoom.GetPlayer((int)additionalParams[0]);
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " had no cards to steal from " + ColourUtility.GetPlayerDisplayName(stealPlayer) + ".";
            case EventCode.RESOURCE_STOLEN:
                stealPlayer = PhotonNetwork.CurrentRoom.GetPlayer((int)additionalParams[0]);
                string resourceText = (string)additionalParams[1];
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " stole 1x" + resourceText + " from " + ColourUtility.GetPlayerDisplayName(stealPlayer) + ".";
            case EventCode.DEVELOPMENT_CARD_PURCHASED:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " purchased a development card from the supply.";
            case EventCode.GAME_OVER:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has won the game! Press ESC to return to the main menu.";
            case EventCode.END_TURN:
                return ColourUtility.GetPlayerDisplayNameFromId(actorNumber) + " has ended their turn.";
        }
        

        return "<invalid_text_code>";
    }
}
