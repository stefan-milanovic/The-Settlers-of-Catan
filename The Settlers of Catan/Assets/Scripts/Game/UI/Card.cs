using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    [SerializeField]
    private Inventory.UnitCode unitCode;

    private bool enabled;
    private bool visible = false;
    private bool selected = false;
    
    // private int count;

    private Inventory inventory;

    private TextMeshProUGUI stockCount;
    private Image image;

    private const float transparencyFactor = 0.2f;

    private int amount;

    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        stockCount = gameObject.transform.parent.parent.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Inventory.UnitCode GetUnitCode() { return unitCode; }

    public void SetInventory(Inventory inv)
    {
        inventory = inv;
    }

    public bool IsVisible() { return visible; }

    public void SetVisible(bool visible) { this.visible = visible; }

    private bool IsConstructionCard() { return unitCode >= Inventory.UnitCode.ROAD && unitCode <= Inventory.UnitCode.CITY; }

    private bool IsResourceCard() { return unitCode >= Inventory.UnitCode.BRICK && unitCode <= Inventory.UnitCode.WOOL; }

    public void UpdateCard(int newAmount)
    {
        stockCount.text = "x" + newAmount;
        this.amount = newAmount;

        if (newAmount == 0)
        {
            DisplayGrayCard();
        } else
        {
            
            if (IsConstructionCard())
            {
                // check cost
                int[] stock = inventory.getStock();
                switch (unitCode)
                {
                    case Inventory.UnitCode.ROAD:
                        if (stock[(int)Inventory.UnitCode.BRICK] < 1 || stock[(int)Inventory.UnitCode.LUMBER] < 1)
                        {
                            DisplayRedCard();
                        }
                        else
                        {
                            DisplayNormalCard();
                        }
                        break;
                    case Inventory.UnitCode.SETTLEMENT:
                        if (stock[(int)Inventory.UnitCode.BRICK] < 1 || stock[(int)Inventory.UnitCode.LUMBER] < 1 || stock[(int)Inventory.UnitCode.GRAIN] < 1 || stock[(int)Inventory.UnitCode.WOOL] < 1)
                        {
                            DisplayRedCard();
                        }
                        else
                        {
                            DisplayNormalCard();
                        }
                        break;
                    case Inventory.UnitCode.CITY:
                        if (stock[(int)Inventory.UnitCode.GRAIN] < 2 || stock[(int)Inventory.UnitCode.ORE] < 3)
                        {
                            DisplayRedCard();
                        }
                        else
                        {
                            DisplayNormalCard();
                        }
                        break;
                }
            }
            else
            {
                DisplayNormalCard();
            }
        }
    }

    private void DisplayGrayCard()
    {
        enabled = false;
        stockCount.color = new Color(1, 1, 1, transparencyFactor);
        image.color = new Color(1, 1, 1, transparencyFactor);
    }

    private void DisplayNormalCard()
    {
        enabled = true;
        stockCount.color = new Color(1, 1, 1);
        image.color = new Color(1, 1, 1, 1f);
    }

    private void DisplayRedCard()
    {
        enabled = false;
        stockCount.color = new Color(1, 0, 0);
        image.color = new Color(1, 0, 0);
    }

    private void DisplaySelectedCard()
    {
        image.color = new Color(1, 1, 0);
    }
    

    public void OnCardClicked()
    {
        
        if (!enabled || !visible) return;

        Debug.Log("Card " + unitCode + " clicked!");
        // Construction cards -- check if the player is in BUILD
        if (IsConstructionCard())
        {
            if (inventory.GetPlayer().GetPhase() == GamePlayer.Phase.TRADE_BUILD_IDLE)
            {
                // Select
                Debug.Log("Selecting " + unitCode);
                DisplaySelectedCard();
                inventory.GetPlayer().SetPhase(GamePlayer.Phase.BUILDING);
                inventory.GetPlayer().SetSelectedConstructionCard(this);
            }
            else if (inventory.GetPlayer().GetPhase() == GamePlayer.Phase.BUILDING)
            {

                // If I am the selected card -- deselect. Otherwise, deselect other card and select this card.
                if (inventory.GetPlayer().GetSelectedConstructionCard() == this)
                {
                    // Deselect
                    Debug.Log("Deselecting " + unitCode);
                    DisplayNormalCard();
                    inventory.GetPlayer().SetPhase(GamePlayer.Phase.STOP_BUILDING);
                }
                else
                {
                    Debug.Log("Switching selection to: " + unitCode);
                    DisplaySelectedCard();

                    Card previousSelectedCard = inventory.GetPlayer().GetSelectedConstructionCard();
                    
                    previousSelectedCard.DisplayNormalCard();

                    inventory.GetPlayer().TurnOffIndicators();

                    inventory.GetPlayer().SetSelectedConstructionCard(this);
                }
                
            }
        }


        // Resource cards -- check if the player is in trade, if it is drop into trade menu and reduce current cost
    }
    
}
