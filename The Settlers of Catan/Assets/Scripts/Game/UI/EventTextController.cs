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
    private TextMeshProUGUI currentPlayerText;

    private PhotonView photonView;

    private readonly string[] resourceColours =
    {
        "red",
        "yellow",
        "green",
        "blue",
        "orange"
    };

    private readonly string[] resourceNames =
    {
        "Brick",
        "Grain",
        "Lumber",
        "Ore",
        "Wool"
    }; 

    public enum TextCode
    {
        FIRST_TURN_PHASE_ONE,
        FIRST_TURN_PHASE_TWO,
        SECOND_TURN_PHASE_ONE,
        SECOND_TURN_PHASE_TWO,
        PRE_DICE_ROLL,
        DICE_ROLLED,
        RESOURCE_EARNED,
        NO_RESOURCE_EARNED,
        SHOULD_DISCARD,
        BANDIT_MOVE,
        STEAL_NO_ADJACENT_PLAYER,
        NO_RESOURCE_STOLEN,
        RESOURCE_STOLEN
    };
    
    private bool busy = false;

    private List<Player> discardList;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {

        eventText.text = "Waiting for all players to connect...";

    }

    [PunRPC]
    private void RPCInit()
    {
        
    }
    

    public void SetCurrentPlayer(Player player)
    {
        photonView.RPC("RPCSetCurrentPlayer", RpcTarget.All, player.ActorNumber);
    }

    [PunRPC]
    private void RPCSetCurrentPlayer(int playerId)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);
        currentPlayerText.text = "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>";
    }

    public void AddToQueue(TextCode code, Player player, params object[] additionalParams)
    {
        string message = GetMessage(code, player.ActorNumber, additionalParams);
        StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        while (busy) { yield return null;  }
        busy = true;
        SetText(message);
        yield return new WaitForSeconds(3);
        busy = false;
    }

    public void SetText(TextCode code, Player player, params object[] additionalParams)
    {
        string message;

        if (player != null)
        {
            message = GetMessage(code, player.ActorNumber, additionalParams);
        }
        else
        {
            message = GetMessage(code, -1, additionalParams);
        }
        
        photonView.RPC("RPCSetText", RpcTarget.All, message);
    }
    
    private void SetText(string message)
    {
        photonView.RPC("RPCSetText", RpcTarget.All, message);
    }

    [PunRPC]
    public void RPCSetText(string newText)
    {
        Debug.Log("Setting text - " + newText);
        eventText.text = newText;
    }

    private string GetMessage(TextCode code, int actorNumber, params object[] additionalParams)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        switch (code)
        {
            case TextCode.FIRST_TURN_PHASE_ONE:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their first settlement.";
                
            case TextCode.FIRST_TURN_PHASE_TWO:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their first road.";
               
            case TextCode.SECOND_TURN_PHASE_ONE:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their second settlement.";
                
            case TextCode.SECOND_TURN_PHASE_TWO:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their second road.";
               
            case TextCode.PRE_DICE_ROLL:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is rolling the dice.";
                
            case TextCode.DICE_ROLLED:
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

                diceValueString += diceValue + "</color>";
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " has rolled a " + diceValueString + ".";

            case TextCode.RESOURCE_EARNED:

                int resourceType = (int)additionalParams[0];
                int amount = (int)additionalParams[1];

                Debug.Log("resourceType = " + resourceType + ", amount = " + amount);

                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " has earned " + amount + "x" + "<color=" + resourceColours[resourceType] + ">" + resourceNames[resourceType] + "</color>.";

            case TextCode.NO_RESOURCE_EARNED:

                return "No player has earned any resources from this roll.";
            case TextCode.SHOULD_DISCARD:
                
                string resultText = "Waiting for ";

                if (actorNumber == -1)
                {
                    // Initial set.
                    discardList = (List<Player>)additionalParams[0];

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
                        resultText += "<color=" + discardList[i].CustomProperties["colour"] + ">" + discardList[i].CustomProperties["username"] + "</color>";
                       
                    }
                    resultText += " to discard half of their resource cards.";
                }
                else
                {
                    // Update. Remove the player from the discard list and then repeat.
                    Player foundPlayer = discardList.Find(p => p.ActorNumber == actorNumber);
                    if (foundPlayer != null)
                    {
                        discardList.Remove(foundPlayer);
                    }

                    bool first = true;
                    for (int i = 0; i < discardList.Count; i++)
                    {
                        if (discardList[i] != null)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                resultText += ", ";
                            }
                            resultText += "<color=" + discardList[i].CustomProperties["colour"] + ">" + discardList[i].CustomProperties["username"] + "</color>";
                        }
                    }
                    resultText += " to discard half of their resource cards!";

                }

                Debug.Log(resultText);
                return resultText;

            case TextCode.BANDIT_MOVE:
                return "Waiting for " + "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " to move the bandit to another hex.";
            case TextCode.STEAL_NO_ADJACENT_PLAYER:
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color> had nobody to steal from.";
            case TextCode.NO_RESOURCE_STOLEN:
                Player stealPlayer = PhotonNetwork.CurrentRoom.GetPlayer((int)additionalParams[0]);
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color> had no cards to steal from " + "<color=" + stealPlayer.CustomProperties["colour"] + ">" + stealPlayer.CustomProperties["username"] + ".";
            case TextCode.RESOURCE_STOLEN:
                stealPlayer = PhotonNetwork.CurrentRoom.GetPlayer((int)additionalParams[0]);
                string resourceText = (string)additionalParams[1];
                return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color> stole 1x" + resourceText + " from <color=" + stealPlayer.CustomProperties["colour"] + ">" + stealPlayer.CustomProperties["username"] + ".";
        }
        

        return "<invalid_text_code>";
    }
}
