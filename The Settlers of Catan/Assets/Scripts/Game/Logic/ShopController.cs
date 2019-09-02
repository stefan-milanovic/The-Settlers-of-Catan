using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{

    [SerializeField]
    private GameObject purchasePanel;

    [SerializeField]
    private Button cancelButton;

    [SerializeField]
    private Button confirmButton;

    private DevelopmentCardDeck deck;

    private Inventory inventory;

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


        // Set up the status panel.
        TextMeshProUGUI popupText = purchasePanel.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

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
        purchasePanel.SetActive(true);
    }

    public void PurchasePanelCancelPress()
    {
        // Close panel.
        purchasePanel.SetActive(false);
    }


    // If this is called a card surely exists in the development card deck.
    public void PurchasePanelConfirmPress()
    {
        // Close panel. Get a development card from the deck.
        purchasePanel.SetActive(false);
        
        // Pay the card cost.
        inventory.TakeFromPlayer(Inventory.UnitCode.GRAIN, 1);
        inventory.TakeFromPlayer(Inventory.UnitCode.ORE, 1);
        inventory.TakeFromPlayer(Inventory.UnitCode.WOOL, 1);

        // Receive card.

        Inventory.UnitCode receivedCard = deck.TakeCard();

        inventory.GiveToPlayer(receivedCard, 1);

        // Event text.
        GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.DEVELOPMENT_CARD_PURCHASED, PhotonNetwork.LocalPlayer);
    }
}
