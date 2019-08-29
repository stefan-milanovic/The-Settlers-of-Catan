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
        10, 10, 10, 10, 10,
        0, 0, 0, 0, 0,
        START_ROAD_COUNT, START_SETTLEMENT_COUNT, START_CITY_COUNT, 0, 0
    };

    private Card[] cards = new Card[UNIT_ARRAY_SIZE];

    private const int START_ROAD_COUNT = 15;
    private const int START_SETTLEMENT_COUNT = 5;
    private const int START_CITY_COUNT = 4;
    
    private int playerScore = 0;
    private int playerHiddenScore = 0;

    private List<HarbourPath.HarbourBonus> harbourBonuses = new List<HarbourPath.HarbourBonus>();

    private GamePlayer myPlayer;
    
    public void Start()
    {
        // Connect to inventory cards (do not initialise trade cards).
        Card[] cardList = FindObjectsOfType<Card>();
        foreach (Card card in cardList)
        {
            if (card.gameObject.tag != "InventoryCard") { return; }

            card.Init();
            card.SetInventory(this);
            card.UpdateCard(stock[(int)card.GetUnitCode()]);
            cards[(int)card.GetUnitCode()] = card;
        }
        
    }
    
    public void SetPlayer(GamePlayer p)
    {
        myPlayer = p;
    }

    public GamePlayer GetPlayer() { return myPlayer; }


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

    public void PayRoadConstruction()
    {

        TakeFromPlayer(UnitCode.BRICK, 1);
        TakeFromPlayer(UnitCode.LUMBER, 1);

        // cards[(int)UnitCode.ROAD].UpdateCard()
    }

    public void PaySettlementConstruction()
    {
        TakeFromPlayer(UnitCode.BRICK, 1);
        TakeFromPlayer(UnitCode.LUMBER, 1);
        TakeFromPlayer(UnitCode.GRAIN, 1);
        TakeFromPlayer(UnitCode.WOOL, 1);
    }

    public void PayCityConstruction()
    {
        TakeFromPlayer(UnitCode.GRAIN, 2);
        TakeFromPlayer(UnitCode.ORE, 3);
    }

    public void AddHarbourBonus(HarbourPath.HarbourBonus bonus)
    {
        harbourBonuses.Add(bonus);
    }

    public List<HarbourPath.HarbourBonus> GetHarbourBonuses() { return this.harbourBonuses; }
}
