using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscardController : MonoBehaviour
{

    [SerializeField]
    private GameObject discardPanel;

    [SerializeField]
    private GameObject stealPanel;

    [SerializeField]
    private Button[] stealButtons;

    [SerializeField]
    private Card[] discardCards;

    [SerializeField]
    private TextMeshProUGUI statusText;

    [SerializeField]
    private Button discardButton;

    private int remaining;

    private bool discarding = false;

    private int notifierId;

    private const int MAX_STEAL_PLAYER_COUNT = 3;

    private int[] playerStealIds = new int[MAX_STEAL_PLAYER_COUNT];

    private int Remaining {
        get { return remaining; }
        set {
            remaining = value;
            statusText.text = remaining == 0 ?
                "<color=green>Press the Discard button to finish.</color>" :
                "<color=red>You must discard " + remaining + " more card(s).";
        }
    }

    private Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsDiscarding() { return discarding; }

    public void SetInventory(Inventory inv)
    {
        this.inventory = inv;
    }
    
    public void DiscardHalf(int notifierId)
    {

        this.discarding = true;
        this.notifierId = notifierId;

        discardPanel.SetActive(true);
        discardButton.interactable = false;

        // Set up status text.

        Remaining = inventory.GetResourceCardCount() / 2;
        
        // Set up discard cards.

        foreach (Card card in discardCards)
        {
            card.Init();
            card.SetVisible(true);
            card.SetInventory(inventory);
            card.UpdateCard(0);
        }
        
    }

    public void DiscardResource(Inventory.UnitCode unitCode, int amount)
    {

        if (Remaining != 0)
        {
            int oldAmount = discardCards[(int)unitCode].getAmount();
            discardCards[(int)unitCode].UpdateCard(oldAmount + amount);

            Remaining--;

            if (Remaining == 0)
            {
                discardButton.interactable = true;
            }
        }
        else
        {
            inventory.GiveToPlayer(unitCode, amount);
        }
    }

    public void RetractResourceDiscard(Inventory.UnitCode unitCode, int amount)
    {

        // Remove a unit from the card panel.
        int oldAmount = discardCards[(int)unitCode].getAmount();
        discardCards[(int)unitCode].UpdateCard(oldAmount - amount);

        if (Remaining == 0)
        {
            // Disable the button again.
            discardButton.interactable = false;
        }

        Remaining++;
    }

    public void DiscardButtonPressed()
    {
        // Just close the window and notify the player on turn.

        int[] discardCompleteMessage = new int[3];

        discardCompleteMessage[0] = (int)GamePlayer.MessageCode.SEVEN_ROLLED_DISCARD_COMPLETE;
        discardCompleteMessage[1] = this.notifierId; // Who should read this message?
        discardCompleteMessage[2] = PhotonNetwork.LocalPlayer.ActorNumber; // Who is sending this message.

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(discardCompleteMessage, false);


        // Reset.
        this.discarding = false;
        discardPanel.SetActive(false);
        foreach (Card card in discardCards)
        {
            card.SetVisible(false);
            card.UpdateCard(0);
        }
    }

    public void PrepareStealing(List<int> playerIdList)
    {
        // The playerIdList size can be maximum 3.

        stealPanel.SetActive(true);

        // Reset buttons. Fill with player info top down.
        for (int i = 0; i < stealButtons.Length; i++)
        {
            if (playerIdList.Count == 0)
            {
                stealButtons[i].interactable = false;
                stealButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

                playerStealIds[i] = -1;
            }
            else
            {
                int playerId = playerIdList[0];

                playerIdList.RemoveAt(0);
                stealButtons[i].interactable = true;
                stealButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ColourUtility.GetPlayerDisplayNameFromId(playerId);

                playerStealIds[i] = playerId;
            }
        }
        

    }

    public void StealButtonPressed(int index)
    {
        // Get adequate player.

        int playerId = playerStealIds[index];

        // Disable buttons.
        foreach(Button b in stealButtons)
        {
            b.interactable = false;
        }
        // Close panel.
        stealPanel.SetActive(false);

        // Send a steal resource card message to them.
        int[] resourceCardStealMessage = new int[3];

        resourceCardStealMessage[0] = (int)GamePlayer.MessageCode.STEAL_RESOURCE_REQUEST; // Message code.
        resourceCardStealMessage[1] = playerId; // Who should read the message?
        resourceCardStealMessage[2] = PhotonNetwork.LocalPlayer.ActorNumber; // Who should the player reply to.

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(resourceCardStealMessage, false);
        
        // Wait for the reply in GamePlayer.OnPlayerMove().
    }
}
