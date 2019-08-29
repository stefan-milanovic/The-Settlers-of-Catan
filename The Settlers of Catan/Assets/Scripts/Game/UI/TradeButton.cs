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

    private bool supply = false;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSupplyFlag(bool supplyFlag)
    {
        this.supply = supplyFlag;
        SetInteractable();
    }

    public void SetInteractable()
    {
        this.interactable = true;
        gameObject.GetComponent<Button>().interactable = true;
        
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

        // set button interactable to true
        SetInteractable();
       
        SetButtonText();
    }

    
    public void SendTradeRequest()
    {


        if (!interactable) return;

        // If the player is not in idle trade phase, decline request (also place in event text).

        GamePlayer p = GamePlayer.FindLocalPlayer();

        if (p.GetPhase() != GamePlayer.Phase.TRADE_BUILD_IDLE)
        {
            return;
        }

        if (supply)
        {
            // Trading with the supply. Handle all trading logic locally.
            GameObject.Find("TradeController").GetComponent<TradeController>().SupplyTradeInit();
        }
        else
        {
            // Send trade request to player bound to this button.
            GameObject.Find("TradeController").GetComponent<TradeController>().SendTradeRequest(p, playerToTradeWith);
        }


    }

    
}
