using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeController : MonoBehaviour
{

    [SerializeField]
    private TradeButton[] tradeButtons;


    [SerializeField]
    private GameObject tradeRequestStatusPanel;


    private bool playerResponded;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init()
    {
        // Fill in button info with other players' usernames.

        for (int i = 0, j = 0; i < 4; i++)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(i + 1);

            if (player != PhotonNetwork.LocalPlayer && player != null)
            {
                tradeButtons[j++].SetPlayer(player);
            }
        }

        tradeButtons[3].SetMaritime(true);

    }

    public void SendTradeRequest(GamePlayer sender, Player receiver, string receiverName)
    {
        sender.SendTradeRequest(receiver);

        // Hide trade panel and show waiting panel.

        GameObject.Find("BottomPanel").GetComponent<BottomPanel>().OpenTradeTab(); // toggles the trade tab

        DisplayWaitingPanel(receiverName);
    }

    private void DisplayWaitingPanel(string receiverName)
    {
        // Set panel text.
        tradeRequestStatusPanel.SetActive(true);
        StartCoroutine(DisplayTick(receiverName));


    }

    IEnumerator DisplayTick(string receiverName)
    {

        TextMeshProUGUI panelText = tradeRequestStatusPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        for (int i = 10; i > 0; i--)
        {
            panelText.text = "Waiting for " + receiverName + "to accept your trade request... (" + i + ")";
            yield return new WaitForSeconds(1);
        }

        // check if player accepted in the meantime 
        if (playerResponded)
        {
            // Panel will be set inactive at another place if this occurs.
            yield return null;
        }

        panelText.text = "The trade request timed out.";

        yield return new WaitForSeconds(2);

        tradeRequestStatusPanel.SetActive(false);
    }

    
    // message received callback


}
