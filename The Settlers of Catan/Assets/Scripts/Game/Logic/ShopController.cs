using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{

    [SerializeField]
    private GameObject statusPanel;

    [SerializeField]
    private Button cancelButton;

    [SerializeField]
    private Button confirmButton;

    private DevelopmentCardDeck deck;

    private Inventory inventory;


    private Inventory.UnitCode cardToPlay;

    public enum ActionCode
    {
        BUY_DEVELOPMENT_CARD,
        PLAY_DEVELOPMENT_CARD
    }

    private ActionCode action;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInventory(Inventory inv)
    {
        this.inventory = inv;
    }

    public void SetDeck(DevelopmentCardDeck deck)
    {
        this.deck = deck;
    }

    public void BuyDevelopmentCardsButtonPress()
    {

        action = ActionCode.BUY_DEVELOPMENT_CARD;

        // Set up the status panel.
        TextMeshProUGUI popupText = statusPanel.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        popupText.text = "One development card costs 1x" + ColourUtility.GetResourceText(Inventory.UnitCode.GRAIN) + ", 1x" + ColourUtility.GetResourceText(Inventory.UnitCode.ORE) + ", and 1x" + ColourUtility.GetResourceText(Inventory.UnitCode.WOOL) + ".\n";

        if (!deck.IsEmpty())
        {
            if (inventory.CanBuyDevelopmentCard())
            {
                popupText.text += "<color=green>You have enough resources to buy a development card.</color>";
                confirmButton.interactable = true;
            }
            else
            {
                popupText.text += "<color=red>You do not have enough resources to buy a development card.</color>";
                confirmButton.interactable = false;
            }
        }
        else
        {
            popupText.text += "<color=red>The development card deck is empty.</color>";
            confirmButton.interactable = false;
        }

        // Open panel.
        statusPanel.SetActive(true);
    }

    public void CancelPress()
    {
        // Close panel.
        statusPanel.SetActive(false);
    }


    // If this is called a card surely exists in the development card deck.
    public void ConfirmPress()
    {
        if (action == ActionCode.BUY_DEVELOPMENT_CARD)
        {
            // Close panel. Get a development card from the deck.
            statusPanel.SetActive(false);

            // Pay the card cost.
            inventory.TakeFromPlayer(Inventory.UnitCode.GRAIN, 1);
            inventory.TakeFromPlayer(Inventory.UnitCode.ORE, 1);
            inventory.TakeFromPlayer(Inventory.UnitCode.WOOL, 1);

            // Receive card.

            Inventory.UnitCode receivedCard = deck.TakeCard();

            inventory.ReceiveDevelopmentCard(receivedCard);

            // Event text.
            GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.DEVELOPMENT_CARD_PURCHASED, PhotonNetwork.LocalPlayer);
        }
        else if (action == ActionCode.PLAY_DEVELOPMENT_CARD)
        {
            statusPanel.SetActive(false);

            // Play the card.
            inventory.PlayDevelopmentCard(cardToPlay);
        }
    }

    public void PlayDevelopmentCard(Inventory.UnitCode unitCode)
    {
        if (statusPanel.activeSelf == true)
        {

            // A card is opened already.
            return;
        }

        cardToPlay = unitCode;
        action = ActionCode.PLAY_DEVELOPMENT_CARD;

        TextMeshProUGUI popupText = statusPanel.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (inventory.CanPlayDevelopmentCard(unitCode))
        {
            popupText.text = "<color=green>Are you sure you want to play this Development card?</color>";
            confirmButton.interactable = true;

        }
        else
        {
            popupText.text = "<color=red>Development cards can first be played a turn after their purchase.</color>";
            confirmButton.interactable = false;
        }

        // Open panel.
        statusPanel.SetActive(true);
    }
}
