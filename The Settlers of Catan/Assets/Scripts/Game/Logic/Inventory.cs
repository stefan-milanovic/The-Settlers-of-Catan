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

    public int GetResourceCardCount()
    {
        return stock[(int)UnitCode.BRICK] + stock[(int)UnitCode.GRAIN] + stock[(int)UnitCode.LUMBER] + stock[(int)UnitCode.ORE] + stock[(int)UnitCode.WOOL];
    }


    public void SetPlayer(GamePlayer p)
    {
        myPlayer = p;
    }

    public GamePlayer GetPlayer() { return myPlayer; }


    public int[] getStock() { return stock; }


    // The player receives one resource card for each hex adjacent to their second-placed settlement (i).
    public void GrantStartingResources(Intersection i)
    {
        Hex[] allHexes = FindObjectsOfType<Hex>();
        List<Hex> adjacentHexes = new List<Hex>();
        foreach (Hex hex in allHexes)
        {
            if (hex.HasIntersection(i))
            {
                if (hex.GetResource() != Hex.Resource.NO_RESOURCE)
                {
                    GiveToPlayer((UnitCode)hex.GetResource(), 1);
                }
                
            }
        }
    }
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
        
    }

    public void GiveToPlayer(UnitCode unit, int amount)
    {

        stock[(int)unit] += amount;

        cards[(int)unit].UpdateCard(stock[(int)unit]);

        UpdateConstructionCards();
        
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
    
    public void AddVictoryPoint(UnitCode pointSource)
    {
        
        switch (pointSource)
        {
            case UnitCode.SETTLEMENT:
                playerScore++;
                break;
            case UnitCode.CITY:
                // Also increase the score by only 1 because a settlement was removed (-1 score) and a city was added (+2 score).
                playerScore++;
                break;
        }

        if (playerScore == 1)
        {
            // Claim an empty leaderboard slot.
            ClaimLeaderboardSlot();
        }
        else
        {
            // Update the leaderboard.
            GameObject.Find("LeaderboardController").GetComponent<LeaderboardController>().UpdateLeaderboard(PhotonNetwork.LocalPlayer.ActorNumber, playerScore);
        }

        
    }

    private void ClaimLeaderboardSlot()
    {
        for (int i = 0; i < 4; i++)
        {
            string key = "leaderboardSlot" + (i + 1);

            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                [key] = PhotonNetwork.LocalPlayer.ActorNumber
            },
            new ExitGames.Client.Photon.Hashtable
            {
                [key] = 0
            });

            if ((int)PhotonNetwork.CurrentRoom.CustomProperties[key] == 0)
            {
                // This slot will be taken -- leave the loop.
                Debug.Log("Player " + PhotonNetwork.LocalPlayer.ActorNumber + " got slot: " + key);
                break;
            }
        }
    }


}
