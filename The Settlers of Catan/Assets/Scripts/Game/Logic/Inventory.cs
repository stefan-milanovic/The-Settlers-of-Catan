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
    private const int WIN_POINTS = 10;
    
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
    private int hiddenPlayerScore = 0;

    private int knightCardsPlayed = 0;

    private int settlementsOnBoard = 0;
    private int citiesOnBoard = 0;

    private List<HarbourPath.HarbourBonus> harbourBonuses = new List<HarbourPath.HarbourBonus>();

    private GamePlayer myPlayer;

    private OverviewController overviewController;

    // For development cards we have to remember on which turn the card was added.
    private struct DevelopmentCardInfo
    {
        public UnitCode card;
        public int turn;

        public DevelopmentCardInfo(UnitCode card, int turn)
        {
            this.card = card;
            this.turn = turn;
        }
    }

    private List<DevelopmentCardInfo> developmentCards = new List<DevelopmentCardInfo>();
    
    public void Start()
    {

        this.overviewController = GameObject.Find("OverviewController").GetComponent<OverviewController>();

        GameObject.Find("ShopController").GetComponent<ShopController>().SetInventory(this);

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

    public bool CanBuyDevelopmentCard()
    {
        return (stock[(int)UnitCode.GRAIN] > 0 && stock[(int)UnitCode.ORE] > 0 && stock[(int)UnitCode.WOOL] > 0);
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

        if (unit >= UnitCode.BRICK && unit <= UnitCode.WOOL)
        {
            UpdateResourceTotal();
        }

        UpdateConstructionCards();
        
    }

    public void GiveToPlayer(UnitCode unit, int amount)
    {

        stock[(int)unit] += amount;

        cards[(int)unit].UpdateCard(stock[(int)unit]);

        if (unit >= UnitCode.BRICK && unit <= UnitCode.WOOL)
        {
            UpdateResourceTotal();
        }

        UpdateConstructionCards();
        
    }
    
    private void UpdateConstructionCards()
    {
        for (UnitCode i = UnitCode.ROAD; i <= UnitCode.CITY; i++)
        {
            cards[(int)i].UpdateCard(stock[(int)i]);
        }
    }

    private void UpdateResourceTotal()
    {
        
        overviewController.SetTotalCardsText(GetResourceCardCount());
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
                hiddenPlayerScore++;

                settlementsOnBoard++;
                overviewController.SetVictoryPoint(UnitCode.SETTLEMENT, settlementsOnBoard);
                break;
            case UnitCode.CITY:
                // Also increase the score by only 1 because a settlement was removed (-1 score) and a city was added (+2 score).

                settlementsOnBoard--;
                citiesOnBoard++;

                overviewController.SetVictoryPoint(UnitCode.SETTLEMENT, settlementsOnBoard);
                overviewController.SetVictoryPoint(UnitCode.CITY, citiesOnBoard * 2);

                playerScore++;
                hiddenPlayerScore++;
                break;
            case UnitCode.VICTORY_CARD:

                overviewController.SetVictoryPoint(UnitCode.VICTORY_CARD, stock[(int)UnitCode.VICTORY_CARD]);

                playerScore++; // remove later
                hiddenPlayerScore++;
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

        if (CheckWinCondition() == true)
        {
            // Hide end turn button, show Claim Victory button.

            GameObject.Find("BottomPanel").GetComponent<BottomPanel>().EnableClaimVictory();
        }
    }

    private bool CheckWinCondition()
    {
        return hiddenPlayerScore >= WIN_POINTS;
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

    public void GiveRandomResourceCard(int recepientId)
    {
        // If I don't have any cards, return nothing.
        if (GetResourceCardCount() == 0)
        {
            int[] stealResourceReplyMessage = new int[3];

            stealResourceReplyMessage[0] = (int)GamePlayer.MessageCode.STEAL_RESOURCE_REPLY; // Message code.
            stealResourceReplyMessage[1] = recepientId; // Who should read this message?
            stealResourceReplyMessage[2] = -1; // What resource am I sending?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(stealResourceReplyMessage, false);
        }
        else
        {
            // Choose a random card.

            UnitCode selectedCard;

            do
            {
                selectedCard = (UnitCode) Random.Range((int)UnitCode.BRICK, (int)UnitCode.WOOL);
            } while (stock[(int)selectedCard] == 0);

            // Send the message.

            this.TakeFromPlayer(selectedCard, 1);

            int[] stealResourceReplyMessage = new int[3];

            stealResourceReplyMessage[0] = (int)GamePlayer.MessageCode.STEAL_RESOURCE_REPLY; // Message code.
            stealResourceReplyMessage[1] = recepientId; // Who should read this message?
            stealResourceReplyMessage[2] = (int)selectedCard; // What resource am I sending?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(stealResourceReplyMessage, false);
        }
    }

    public void ReceiveStolenCard(int senderId, int receivedResourceId)
    {
        // If the resource id is -1, the selected player had no card to give.

        if (receivedResourceId == -1)
        {
            GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.NO_RESOURCE_STOLEN, PhotonNetwork.LocalPlayer, senderId);
        }
        else
        {
            UnitCode receivedResource = (UnitCode)receivedResourceId;
            this.GiveToPlayer(receivedResource, 1);
            GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.RESOURCE_STOLEN, PhotonNetwork.LocalPlayer, senderId, ColourUtility.GetResourceText(receivedResource));
        }
        
    }

    public void ReceiveDevelopmentCard(UnitCode receivedCard)
    {

        GiveToPlayer(receivedCard, 1);
        developmentCards.Add(new DevelopmentCardInfo(receivedCard, myPlayer.CurrentTurn));

        if (receivedCard == UnitCode.VICTORY_CARD)
        {
            AddVictoryPoint(UnitCode.VICTORY_CARD);
        }
    }

    public bool CanPlayDevelopmentCard(UnitCode card)
    {
        foreach (DevelopmentCardInfo info in developmentCards)
        {
            if (info.turn < myPlayer.CurrentTurn)
            {
                if (info.card == card)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Found a card that can be played. Remove it from the development cards.
    public void PlayDevelopmentCard(UnitCode card)
    {
        // Remove the first instance of the DevelopmentCardInfo from the list. 

        int index = 0;
        foreach (DevelopmentCardInfo info in developmentCards)
        {
            if (info.card == card && info.turn < myPlayer.CurrentTurn)
            {
                break;
            }
            index++;
        }

        developmentCards.RemoveAt(index);
        TakeFromPlayer(card, 1);

        // Announce the playing to the event text.
        GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.DEVELOPMENT_CARD_PLAYED, PhotonNetwork.LocalPlayer, (int)card);

        switch (card)
        {
            case UnitCode.KNIGHT:
                knightCardsPlayed++;
                LargestArmyCheck();

                // Let the local player move the bandit.
                myPlayer.MoveBandit();
                break;
            case UnitCode.EXPANSION:

                // The player may immediately place 2 free roads on the board.

                myPlayer.SetPhase(GamePlayer.Phase.PLAYED_EXPANSION_CARD);
                break;
            case UnitCode.YEAR_OF_PLENTY:

                // The player may immediately take 2 Resource cards from the supply.
                GameObject.Find("TradeController").GetComponent<TradeController>().YearOfPlentyInit();

                break;
            case UnitCode.MONOPOLY:

                // The player selects a resource type. Other players give this player ALL of their cards of this resource type.
                break;
            case UnitCode.VICTORY_CARD:

                // Clicking this card has no effect.
                break;
        }

        
    }

    private void LargestArmyCheck()
    {
        if (knightCardsPlayed < 3)
        {
            return;
        }

        // If we overtook the maximum, take the Largest Army card.
    }
}
