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

    [SerializeField]
    private GameObject tradeRequestPopupPanel;

    #region Offers Objects
    [SerializeField]
    private GameObject offersPanel;

    [SerializeField]
    private TextMeshProUGUI localPlayerText;

    [SerializeField]
    private TextMeshProUGUI remotePlayerText;

    [SerializeField]
    private Card[] localCards;

    [SerializeField]
    private Card[] remoteCards;

    [SerializeField]
    private GameObject[] localPanels;

    [SerializeField]
    private GameObject[] remotePanels;
    #endregion

    private Inventory inventory;

    private readonly Color confirmationColour = new Color(0.4745098f, 0.8431373f, 0.1568628f);

    private bool trading = false;

    private bool recepientResponded;
    private bool tradeDeclined;

    private bool amRecepient;

    private bool confirmed, otherConfirmed;

    private int senderId;
    private int recepientId;

    private bool supplyTrading;
    private const int SUPPLY_ID = -1;
    [SerializeField]
    private GameObject clearRemoteButton;

    private int availableCards = 0;
    private int AvailableCards {
        get {
            return availableCards;
        }
        set {

            // update text
            availableCards = value;
            remotePlayerText.text = "Available cards: " + value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(Inventory inventory)
    {
        // Set inventory reference.

        this.inventory = inventory;

        // Initialize the discard controller.

        GameObject.Find("DiscardController").GetComponent<DiscardController>().SetInventory(inventory);

        // Fill in button info with other players' usernames.

        for (int i = 0, j = 0; i < 4; i++)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(i + 1);

            if (player != PhotonNetwork.LocalPlayer && player != null)
            {
                tradeButtons[j++].SetPlayer(player);
            }
        }

        tradeButtons[3].SetSupplyFlag(true);
    }

    public bool IsTrading() { return trading; }


    #region Sender Methods (This player is the trade request sender)

    public void SendTradeRequest(GamePlayer sender, Player recepient)
    {

        this.senderId = sender.GetPhotonPlayer().ActorNumber; // should always be local player
        this.recepientId = recepient.ActorNumber;

        this.amRecepient = false;
        this.supplyTrading = false;

        int[] tradeRequestMessage = new int[2];
        tradeRequestMessage[0] = (int)GamePlayer.MessageCode.TRADE_REQUEST;
        tradeRequestMessage[1] = recepient.ActorNumber;
        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeRequestMessage, false);
        
        // Hide trade panel and show waiting panel.

        GameObject.Find("BottomPanel").GetComponent<BottomPanel>().OpenTradeTab(); // toggles the trade tab

        DisplayWaitingPanel();
    }

    private void DisplayWaitingPanel()
    {
        // Set panel text.
        tradeRequestStatusPanel.SetActive(true);
        StartCoroutine(TradeSenderTick());

    }

    // This is run on the sender of the trade request.
    private IEnumerator TradeSenderTick()
    {
        recepientResponded = false;
        TextMeshProUGUI panelText = tradeRequestStatusPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        string recepientName = ColourUtility.GetPlayerDisplayNameFromId(recepientId);

        for (int i = 10; i > 0; i--)
        {
            if (recepientResponded)
            {
                break;
            }
            panelText.text = "Waiting for " + recepientName + " to accept your trade request... (" + i + ")";
            yield return new WaitForSeconds(1);
        }
        
        if (recepientResponded)
        {

            if (tradeDeclined)
            {
                panelText.text = ColourUtility.GetPlayerDisplayNameFromId(recepientId) + " has declined your trade request.";
                yield return new WaitForSeconds(3);
                tradeRequestStatusPanel.SetActive(false);
                yield break;
            }
            else
            {
                // Trade accepted.
                tradeRequestStatusPanel.SetActive(false);
                InitOffersPanel();
                yield break;
            }

        }
        else
        {
            panelText.text = "The trade request timed out.";
            yield return new WaitForSeconds(3);
            tradeRequestStatusPanel.SetActive(false);
            yield break;
        }
        
    }

    // Run by the sender when they're notified by the receiver that their trade was accepted.
    public void RemoteTradeAccepted()
    {
        AcceptTradeOffer();
    }

    // Run by the sender when they're notified by the receiver that their trade was declined.
    public void RemoteTradeDeclined()
    {
        DeclineTradeOffer();
    }

    public void RemoteTradeCancelled()
    {
        CancelTrade();
    }
    
    public void RemoteTradeLocked()
    {
        LockRemotePanel();
    }

    public void RemoteTradeUnlocked()
    {
        UnlockRemotePanel();
    }

    public void RemoteTradeExecuted()
    {
        PerformResourceExchange();
    }

    #endregion

    #region Recepient Methods (This player is the trade request recepient)

    public void ReceiveTradeRequest(int senderId, int recepientId)
    {

        // DISABLE OPENING TRADE BUTTON HERE (CAUSE YOU HAVE A REQUEST) AND ENDING TURN (IF IT'S YOUR TURN)

        // if the trade panel was open, close it

        if (GameObject.Find("BottomPanel").GetComponent<BottomPanel>().TradePanelOpen())
        {
            GameObject.Find("BottomPanel").GetComponent<BottomPanel>().OpenTradeTab(); // toggle
        }

        this.senderId = senderId;
        this.recepientId = recepientId; // should always be local player

        this.amRecepient = true;
        this.supplyTrading = false;
        
        StartCoroutine(TradeOfferTick());

    }

    // This is run on the recepient of the trade request.
    private IEnumerator TradeOfferTick()
    {
        recepientResponded = false;
        tradeRequestPopupPanel.SetActive(true);
        TextMeshProUGUI popupText = tradeRequestPopupPanel.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        for (int i = 10; i > 0; i--)
        {
            if (recepientResponded)
            {
                break;
            }

            popupText.text = ColourUtility.GetPlayerDisplayNameFromId(senderId) + " sent you a trade request (" + i + ").";
            yield return new WaitForSeconds(1);
        }

        if (recepientResponded)
        {
            // Close trade request popup panel.

            if (tradeDeclined)
            {
                tradeRequestPopupPanel.SetActive(false);
                yield break;
            }
            else
            {
                tradeRequestPopupPanel.SetActive(false);
                InitOffersPanel();
                yield break;
            }

        }
        else
        {
            tradeRequestPopupPanel.SetActive(false);
            yield break;
        }
    }


    #endregion

    #region Supply Trading

    public bool IsSupplyTrading() { return supplyTrading; }

    public void SupplyTradeInit()
    {
        // Hide trade panel and show trade panel.
        GameObject.Find("BottomPanel").GetComponent<BottomPanel>().OpenTradeTab(); // toggles the trade tab
        offersPanel.SetActive(true);

        confirmed = false;
        foreach (GameObject panel in localPanels)
        {
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
        LockRemotePanel();

        trading = true;
        clearRemoteButton.SetActive(true);

        supplyTrading = true;
        this.AvailableCards = 0;

        this.senderId = PhotonNetwork.LocalPlayer.ActorNumber;
        this.recepientId = SUPPLY_ID;

        // Set up player and supply name.
        localPlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(senderId) + "'s offer";
        remotePlayerText.text = "Available cards: 0";

        // Set up cards. Left ones are local, right ones are remote.

        foreach (Card card in localCards)
        {
            card.Init();
            card.SetVisible(true);
            card.SetInventory(inventory);
            card.UpdateCard(0);
        }

        foreach (Card card in remoteCards)
        {
            card.Init();
            card.SetVisible(true);
            card.SetInventory(inventory);
            card.UpdateCard(0);

            // Specifically for supply trading.
            card.SetEnabled(true);
        }
    }

    private void SupplyOnOfferChanged()
    {
        // Check the player's entire offer and place adequate resources.

        int[] playerOffer = new int[localCards.Length];
        
        int i = 0;
        foreach (Card localCard in localCards)
        {
            playerOffer[i++] = localCard.getAmount();
        }

        AvailableCards = 0;
        // Check order bonuses: 2:1 (Specific), 3:1 (Generic), 4:1 (No bonus)
        List<HarbourPath.HarbourBonus> playerHarbourBonuses = inventory.GetHarbourBonuses();
        for (i = 0; i < playerOffer.Length; i++)
        {
            if (playerHarbourBonuses.Contains((HarbourPath.HarbourBonus)i))
            {
                // 2:1 Specific bonus.
                AvailableCards += playerOffer[i] / 2;
            }
            else if (playerHarbourBonuses.Contains(HarbourPath.HarbourBonus.THREE_TO_ONE))
            {
                // 3:1 Generic bonus.

                AvailableCards += playerOffer[i] / 3;
            }
            else
            {
                // 4:1 No bonus.
                AvailableCards += playerOffer[i] / 4;
            }
        }
        
        // Reset remote cards.
        foreach (Card remoteCard in remoteCards)
        {
            remoteCard.UpdateCard(0);
            remoteCard.SetEnabled(true);
        }

        // Allow player to choose which remote cards he would like.
    }

    public void SupplyCardChosen(Inventory.UnitCode unitCode, int amount)
    {
        // Always adds to the selected cards. Use the Clear button in the top right hand corner to clear the remote cards.

        if (AvailableCards > 0)
        {
            int oldAmount = remoteCards[(int)unitCode].getAmount();
            remoteCards[(int)unitCode].UpdateCard(oldAmount + amount);

            AvailableCards -= amount;
        }

        //if (remoteCards[(int)unitCode].getAmount() == 0)
        //{
            

        //}
        //else
        //{
        //    int oldAmount = remoteCards[(int)unitCode].getAmount();
        //    remoteCards[(int)unitCode].UpdateCard(oldAmount - amount);

        //    AvailableCards += amount;
        //}
        
    }

    private IEnumerator SupplyTradeError()
    {
        remotePlayerText.text = "<color=red>You must choose " + AvailableCards + " more card(s) from the supply.</color>";
        yield return new WaitForSeconds(3);
        remotePlayerText.text = "Available cards: " + AvailableCards;
    }

    #endregion

    #region Mutual methods

    public void DeclineTradeOffer()
    {
        this.tradeDeclined = true;
        HaltCountdowns();
    }

    private void HaltCountdowns()
    {
        recepientResponded = true;
    }

    public void AcceptTradeOffer()
    {
        this.tradeDeclined = false;
        HaltCountdowns();
    }

    // Should be run only from the Tick functions.
    private void InitOffersPanel()
    {
        offersPanel.SetActive(true);


        confirmed = false;
        foreach (GameObject panel in localPanels)
        {
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
        UnlockRemotePanel();

        trading = true;
        clearRemoteButton.SetActive(false);

        // Set up player names.

        if (amRecepient)
        {
            localPlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(recepientId) + "'s offer";
            remotePlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(senderId) + "'s offer";
        }
        else
        {
            localPlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(senderId) + "'s offer";
            remotePlayerText.text = ColourUtility.GetPlayerDisplayNameFromId(recepientId) + "'s offer";
        }

        // Set up cards. Left ones are local, right ones are remote.

        foreach (Card card in localCards)
        {
            card.Init();
            card.SetVisible(true);
            card.SetInventory(inventory);
            card.UpdateCard(0);
        }

        foreach (Card card in remoteCards)
        {
            card.Init();
            card.SetVisible(true);
            card.SetInventory(inventory);
            card.UpdateCard(0);
        }
    }

    public bool IsLocalCard(Card card)
    {
        foreach (Card localCard in localCards)
        {
            if (localCard == card)
            {
                return true;
            }
        }
        return false;
    }

    #endregion
    

    #region Button Methods
    // The recepient can use this method to accept the trade.
    public void TradeAcceptedButtonPress()
    {

        // Notify sender to halt the countdown and open offers panel. Do so yourself.

        if (!tradeRequestPopupPanel.activeSelf) { return; }

        // Send the initial sender a TradeAccepted message.

        int[] tradeAcceptedMessage = new int[2];
        tradeAcceptedMessage[0] = (int)GamePlayer.MessageCode.TRADE_ACCEPTED;
        tradeAcceptedMessage[1] = senderId;

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeAcceptedMessage, false);

        // Halt countdown and open offers panel yourself.
        AcceptTradeOffer();

    }

    // The recepient can use this method to decline the trade.
    public void TradeDeclinedButtonPress()
    {
        if (!tradeRequestPopupPanel.activeSelf) { return; }

        // Notify the sender to halt the countdown and display the declined message. Remove the trade popup locally.

        int[] tradeDeclinedMessage = new int[2];
        tradeDeclinedMessage[0] = (int)GamePlayer.MessageCode.TRADE_DECLINED;
        tradeDeclinedMessage[1] = senderId;

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeDeclinedMessage, false);

        // Decline the trade message locally.
        DeclineTradeOffer();

    }

    public void TradeCancelledButtonPress()
    {
        // Notify the other player that the trade has been cancelled. Cancel it locally.

        int[] tradeCancelledMessage = new int[2];
        tradeCancelledMessage[0] = (int)GamePlayer.MessageCode.TRADE_CANCELLED;
        tradeCancelledMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeCancelledMessage, false);

        // Cancel trade locally.

        CancelTrade();
    }

    public void TradeOfferConfirmationButtonPress()
    {

        // We can be either the first or second to press.
        
        if (confirmed)
        {
            return;
        }


        // Check if other player confirmed

        if (otherConfirmed)
        {
            // Execute trade.
            ExecuteTrade();
        } else
        {
            // If not, confirm locally and inform the other player.
            LockLocalPanel();
        }
        
    }

    public void ClearButtonClicked()
    {
        // Reset remote cards.
        if (supplyTrading)
        {
            foreach (Card remoteCard in remoteCards)
            {
                AvailableCards += remoteCard.getAmount();
                remoteCard.UpdateCard(0);
                remoteCard.SetEnabled(true);
            }
        }
    }

    #endregion

    #region Offers Functions

    public void OfferResource(Inventory.UnitCode unitCode, int amount)
    {
        // Place this resource into the local offers slot. Send a message to the other player to update their remote card slot.

        int oldAmount = localCards[(int)unitCode].getAmount();
        localCards[(int)unitCode].UpdateCard(oldAmount + amount);

        if (!supplyTrading)
        {
            int[] offerResourceMessage = new int[4];

            offerResourceMessage[0] = (int)GamePlayer.MessageCode.RESOURCE_OFFERED; // Message code.
            offerResourceMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?
            offerResourceMessage[2] = (int)unitCode;       // What resource am I sending?
            offerResourceMessage[3] = amount;   // How much of it am I sending?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(offerResourceMessage, false);

            // If the player confirmed locally and then attempts to make a change, unconfirm.
            if (confirmed)
            {
                UnlockLocalPanel();
            }
            if (otherConfirmed)
            {
                UnlockRemotePanel();
            }
        } else
        {
            SupplyOnOfferChanged();
        }
        
    }


    public void RetractResourceOffer(Inventory.UnitCode unitCode, int amount)
    {
        // Remove this resource from the local offers slot. Send a message to the other player to update their remote card slot.
        int oldAmount = localCards[(int)unitCode].getAmount();
        localCards[(int)unitCode].UpdateCard(oldAmount - amount);

        if (!supplyTrading)
        {
            int[] retractResourceOfferMessage = new int[4];

            retractResourceOfferMessage[0] = (int)GamePlayer.MessageCode.RESOURCE_RETRACTED; // Message code.
            retractResourceOfferMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?
            retractResourceOfferMessage[2] = (int)unitCode;       // What resource am I retracting?
            retractResourceOfferMessage[3] = amount;   // How much of it am I retracting?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(retractResourceOfferMessage, false);

            // If the player confirmed locally and then attempts to make a change, unconfirm.
            if (confirmed)
            {
                UnlockLocalPanel();
            }
            if (otherConfirmed)
            {
                UnlockRemotePanel();
            }
        }
        else
        {
            SupplyOnOfferChanged();
        }

    }
    #endregion 

    #region Offers Callbacks

    public void ResourceOffered(Inventory.UnitCode unitCode, int amount)
    {
        // Update remote card slot.

        int oldAmount = remoteCards[(int)unitCode].getAmount();
        remoteCards[(int)unitCode].UpdateCard(oldAmount + amount);

        // If the remote player had confirmed the trade beforehand, unlock both mine and his.

        if (confirmed)
        {
            UnlockLocalPanel();
        }
        if (otherConfirmed)
        {
            UnlockRemotePanel();
        }
    }

    public void ResourceOfferRetracted(Inventory.UnitCode unitCode, int amount)
    {
        // Update remote card slot.

        int oldAmount = remoteCards[(int)unitCode].getAmount();
        remoteCards[(int)unitCode].UpdateCard(oldAmount - amount);

        // If the remote player had confirmed the trade beforehand, unlock both mine and his.
        if (confirmed)
        {
            UnlockLocalPanel();
        }
        if (otherConfirmed)
        {
            UnlockRemotePanel();
        }
    }
    #endregion 

    private void CancelTrade()
    {

        trading = false;

        // Return resources from the local cards to the stock.

        foreach (Card localCard in localCards)
        {
            inventory.GiveToPlayer(localCard.GetUnitCode(), localCard.getAmount());
        }
        
        // Show notification to both players.
        StartCoroutine(ShowTradeCancelledMessage());
    }

    private void ExecuteTrade()
    {
        
        // If the player is trading with the supply, process trade locally. 

        if (!supplyTrading)
        {
            // The local player now receives the resources from the Remote cards slots. Inform the other player to do the same.
            int[] tradeExecuteMessage = new int[2];
            tradeExecuteMessage[0] = (int)GamePlayer.MessageCode.TRADE_EXECUTE;
            tradeExecuteMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeExecuteMessage, false);

            PerformResourceExchange();
        } else
        {
            if (AvailableCards == 0)
            {
                PerformResourceExchange();
            }
            else
            {
                StartCoroutine(SupplyTradeError());
            }
        }
        
    }

    private IEnumerator ShowTradeCancelledMessage()
    {
        offersPanel.SetActive(false);
        tradeRequestStatusPanel.SetActive(true);
        TextMeshProUGUI panelText = tradeRequestStatusPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        panelText.text = "The trade has been cancelled.";
        yield return new WaitForSeconds(3);

        tradeRequestStatusPanel.SetActive(false);
    }

    private void LockLocalPanel()
    {
        confirmed = true;

        foreach (GameObject panel in localPanels)
        {
            panel.GetComponent<Image>().color = confirmationColour;
        }
        
        int[] tradeLockMessage = new int[2];
        tradeLockMessage[0] = (int)GamePlayer.MessageCode.TRADE_LOCK;
        tradeLockMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeLockMessage, false);

    }

    private void LockRemotePanel()
    {
        otherConfirmed = true;

        foreach (GameObject panel in remotePanels)
        {
            panel.GetComponent<Image>().color = confirmationColour;
        }
    }

    private void UnlockLocalPanel()
    {
        confirmed = false;
        foreach (GameObject panel in localPanels)
        {
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }

        int[] tradeUnlockMessage = new int[2];
        tradeUnlockMessage[0] = (int)GamePlayer.MessageCode.TRADE_UNLOCK;
        tradeUnlockMessage[1] = (amRecepient) ? senderId : recepientId; // Who should read the message?

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(tradeUnlockMessage, false);
    }

    private void UnlockRemotePanel()
    {
        otherConfirmed = false;

        foreach (GameObject panel in remotePanels)
        {
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
    }

    private void PerformResourceExchange()
    {
        foreach(Card remoteCard in remoteCards)
        {
            inventory.GiveToPlayer(remoteCard.GetUnitCode(), remoteCard.getAmount());
            remoteCard.UpdateCard(0);
        }

        StartCoroutine(ShowTradeCompletedMessage());
    }

    private IEnumerator ShowTradeCompletedMessage()
    {

        trading = false;

        offersPanel.SetActive(false);
        tradeRequestStatusPanel.SetActive(true);
        TextMeshProUGUI panelText = tradeRequestStatusPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        panelText.text = "Trade has been completed successfully.";
        yield return new WaitForSeconds(3);

        tradeRequestStatusPanel.SetActive(false);
    }
}
