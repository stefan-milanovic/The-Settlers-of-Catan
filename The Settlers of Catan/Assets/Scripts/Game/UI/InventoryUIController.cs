using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI[] stockCounts;

    [SerializeField]
    private Image[] stockImages;
    
    private const float transparencyFactor = 0.2f;

    [SerializeField]
    private Button rollDiceButton;

    [SerializeField]
    private Button endTurnButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void UpdateInventoryUIText(Inventory.UnitCode unit, int newAmount)
    {
        stockCounts[(int)unit].text = "x" + newAmount;
        if (newAmount == 0)
        {
            UpdateInventoryUIImage(unit, true);
        } else
        {
            // can be avoided if we check whether or not the old amount was zero (if oldAmount == 0)
            UpdateInventoryUIImage(unit, false);
        }
    }
    
    private void UpdateInventoryUIImage(Inventory.UnitCode unit, bool transparent)
    {
        Image image = stockImages[(int)unit];
        
        if (image == null)
        {
            Debug.Log("Image in UpdateInventoryUIImage() was null.");
            return;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, transparent ? transparencyFactor : 1f);
    }

    

    public void DisplayRollDiceButton()
    {
        rollDiceButton.gameObject.SetActive(true);
    }

    public void HideRollDiceButton()
    {
        rollDiceButton.gameObject.SetActive(false);
    }

    public void EnableRollDiceButton()
    {
        rollDiceButton.interactable = true;
    }

    public void DisableRollDiceButton()
    {
        rollDiceButton.interactable = false;
    }

    public void DisplayEndTurnButton()
    {
        endTurnButton.gameObject.SetActive(true);
    }

    public void HideEndTurnButton()
    {
        endTurnButton.gameObject.SetActive(false);
    }

    public void EnableEndTurnButton()
    {
        endTurnButton.interactable = true;
    }

    public void DisableEndTurnButton()
    {
        endTurnButton.interactable = false;
    }

    public void EnableRolling()
    {

        if (endTurnButton.IsActive())
        {
            HideEndTurnButton();
        }

        if (!rollDiceButton.IsActive())
        {
            DisplayRollDiceButton();
        }

        EnableRollDiceButton();

       
    }

    public void EnableEndingTurn()
    {
        if (rollDiceButton.IsActive())
        {
            HideRollDiceButton();
        }

        if (!endTurnButton.IsActive())
        {
            DisplayEndTurnButton();
        }

        EnableEndTurnButton();
    }

    public void DisableEndingTurn()
    {
        DisableEndTurnButton();
    }
}
