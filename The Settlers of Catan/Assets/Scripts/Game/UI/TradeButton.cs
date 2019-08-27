using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class TradeButton : MonoBehaviour
{

    private bool interactable = false;
    private Player playerToTradeWith;

    private bool maritime = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaritime(bool maritime)
    {
        this.maritime = maritime;
    }

    private void SetButtonText()
    {
        gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetPlayerToTradeWithDisplayName();
    }

    private string GetPlayerToTradeWithDisplayName()
    {
        return "<color=" + playerToTradeWith.CustomProperties["colour"] + ">" + playerToTradeWith.CustomProperties["username"] + "</color>";
    }

    public void SetPlayer(Player p)
    {

        playerToTradeWith = p;
        interactable = true;
        // set button interactable to true

        this.interactable = true;
        gameObject.GetComponent<Button>().interactable = true;

        SetButtonText();
    }

    private GamePlayer FindLocalPlayer()
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
    public void SendTradeRequest()
    {
        

        if (!interactable) return;

        // If the player is not in idle trade phase, decline request (also place in event text).

        GamePlayer p = FindLocalPlayer();
        // Send trade request to player bound to this button. If the maritime flag is set, open local trade.

        if (p.GetPhase() != GamePlayer.Phase.TRADE_BUILD_IDLE)
        {
            return;
        }

        if (maritime)
        {

        }
        else
        {
            // Trading with another player. Send request.
            GameObject.Find("TradeController").GetComponent<TradeController>().SendTradeRequest(p, playerToTradeWith, GetPlayerToTradeWithDisplayName());

        }


    }

    
}
