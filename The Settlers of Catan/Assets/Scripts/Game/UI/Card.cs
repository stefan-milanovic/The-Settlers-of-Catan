using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    [SerializeField]
    private Inventory.UnitCode unitCode;

    private bool enabled = true;
    // private int count;

    private Inventory inventory;

    private TextMeshProUGUI stockCount;
    private Image image;

    private const float transparencyFactor = 0.2f;

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

    public void UpdateCard(int newAmount)
    {
        stockCount.text = "x" + newAmount;

        if (newAmount == 0)
        {
            DisplayGrayCard();
        } else
        {
            if (unitCode >= Inventory.UnitCode.ROAD && unitCode <= Inventory.UnitCode.CITY)
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
        stockCount.color = new Color(1, 1, 1, transparencyFactor);
        image.color = new Color(1, 1, 1, transparencyFactor);
    }

    private void DisplayNormalCard()
    {
        stockCount.color = new Color(1, 1, 1);
        image.color = new Color(1, 1, 1, 1f);
    }

    private void DisplayRedCard()
    {
        stockCount.color = new Color(1, 0, 0);
        image.color = new Color(1, 0, 0);
    }

    public void OnCardClick()
    {

    }
}
