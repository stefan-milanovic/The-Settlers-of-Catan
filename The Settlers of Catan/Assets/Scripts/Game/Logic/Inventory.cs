using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour

{


    public enum UnitCode
    {
        BRICK,
        GRAIN,
        LUMBER,
        ORE,
        WOOL,
        KNIGHT,
        EXPANSION,
        YEAR_OF_PLENTY,
        MONOPOLY,
        VICTORY_CARD,
        ROAD,
        SETTLEMENT,
        CITY,
        LARGEST_ARMY,
        LONGEST_ROAD
    };

    private const int UNIT_ARRAY_SIZE = 15;

    private int[] stock =
    {
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        15, 5, 4, 0, 0
    };

    private InventoryUIController inventoryUIController;

    public void Start()
    {
        inventoryUIController = GameObject.Find("InventoryUIController").GetComponent<InventoryUIController>();

        for (int i = 0; i < UNIT_ARRAY_SIZE; i++)
        {
            inventoryUIController.UpdateInventoryUIText((UnitCode)i, stock[i]);
        }
    }

    public InventoryUIController GetInventoryUIController()
    {
        return inventoryUIController;
    }

    public void TakeFromPlayer(UnitCode unit, int amount)
    {
        if (amount > stock[(int)unit])
        {
            // code in case this happens
            return;
        }

        stock[(int)unit] -= amount;

        inventoryUIController.UpdateInventoryUIText(unit, stock[(int)unit]);
    }

    public void GiveToPlayer(UnitCode unit, int amount)
    {
        if (amount > stock[(int)unit])
        {
            // code in case this happens
            return;
        }

        stock[(int)unit] += amount;
        inventoryUIController.UpdateInventoryUIText(unit, stock[(int)unit]);
    }
}
