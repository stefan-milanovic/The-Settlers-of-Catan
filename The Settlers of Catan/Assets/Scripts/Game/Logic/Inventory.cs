using Photon.Pun;
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
        START_ROAD_COUNT, START_SETTLEMENT_COUNT, START_CITY_COUNT, 0, 0
    };

    private Card[] cards = new Card[UNIT_ARRAY_SIZE];

    private const int START_ROAD_COUNT = 15;
    private const int START_SETTLEMENT_COUNT = 5;
    private const int START_CITY_COUNT = 4;
    
    private int playerScore = 0;
    private int playerHiddenScore = 0;
    
    public void Start()
    {
        // Connect to cards.
        Card[] cardList = FindObjectsOfType<Card>();
        foreach (Card card in cardList)
        {
            card.SetInventory(this);
            card.UpdateCard(stock[(int)card.GetUnitCode()]);
            cards[(int)card.GetUnitCode()] = card;
        }

        //inventoryUIController = GameObject.Find("InventoryUIController").GetComponent<InventoryUIController>();

        //inventoryUIController.SetInventory(this);

        //for (int i = 0; i < UNIT_ARRAY_SIZE; i++)
        //{
        //    inventoryUIController.UpdateInventoryUIText((UnitCode)i, stock[i]);
        //}
    }
    
    public int[] getStock() { return stock; }

    public void TakeFromPlayer(UnitCode unit, int amount)
    {
        if (amount > stock[(int)unit])
        {
            // code in case this happens
            return;
        }

        stock[(int)unit] -= amount;

        cards[(int)unit].UpdateCard(stock[(int)unit]);

        UpdateConstructionCards();

        UpdatePlayerScore();
    }

    public void GiveToPlayer(UnitCode unit, int amount)
    {

        stock[(int)unit] += amount;

        cards[(int)unit].UpdateCard(stock[(int)unit]);

        UpdateConstructionCards();

        UpdatePlayerScore();
    }

    private void UpdatePlayerScore()
    {
        int constructedSettlements = START_SETTLEMENT_COUNT - stock[(int)UnitCode.SETTLEMENT];
        int constructedCities = START_CITY_COUNT - stock[(int)UnitCode.CITY];

        playerScore = constructedSettlements + constructedCities * 2 + stock[(int)UnitCode.LARGEST_ARMY] * 2 + stock[(int)UnitCode.LONGEST_ROAD] * 2;

        playerHiddenScore = playerScore + stock[(int)UnitCode.VICTORY_CARD];
        

    }   

    private void UpdateConstructionCards()
    {
        for (UnitCode i = UnitCode.ROAD; i <= UnitCode.CITY; i++)
        {
            cards[(int)i].UpdateCard(stock[(int)i]);
        }
    }
}
