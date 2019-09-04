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
    private int roadLength = 0;

    private int settlementsOnBoard = 0;
    private int citiesOnBoard = 0;

    private int turnDevelopmentCardPlayed = 0;

    private int largestArmyOwnerId = -1;
    private int largestArmyKnightCount = -1;

    private int longestRoadOwnerId = -1;
    private int longestRoadLength = -1;

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

        // If the card is a road card, that means that a road was placed on the board by this player.
        // Calculate his longest road, and take the Longest Road card if all the conditions are met.
        if (unit == UnitCode.ROAD)
        {
            CalculatePlayerRoadLength();
            LongestRoadCheck();
        }
        
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
                
                hiddenPlayerScore++;
                break;
            case UnitCode.LARGEST_ARMY:

                overviewController.SetVictoryPoint(UnitCode.LARGEST_ARMY, 2);

                playerScore += 2;
                hiddenPlayerScore += 2;

                break;

            case UnitCode.LONGEST_ROAD:

                overviewController.SetVictoryPoint(UnitCode.LONGEST_ROAD, 2);

                playerScore += 2;
                hiddenPlayerScore += 2;

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

    private void TakeVictoryPoint(UnitCode pointSource)
    {
        switch (pointSource)
        {
            case UnitCode.LARGEST_ARMY:
                overviewController.SetVictoryPoint(UnitCode.LARGEST_ARMY, 0);
                playerScore -= 2;
                hiddenPlayerScore -= 2;
                break;
            case UnitCode.LONGEST_ROAD:
                overviewController.SetVictoryPoint(UnitCode.LONGEST_ROAD, 0);
                playerScore -= 2;
                hiddenPlayerScore -= 2;
                break;
        }

        GameObject.Find("LeaderboardController").GetComponent<LeaderboardController>().UpdateLeaderboard(PhotonNetwork.LocalPlayer.ActorNumber, playerScore);
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

    public bool HasPlayedDevelopmentCardThisTurn()
    {
        
        return turnDevelopmentCardPlayed == myPlayer.CurrentTurn;
        
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

        turnDevelopmentCardPlayed = myPlayer.CurrentTurn;

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

                // TODO: Add new phase for HumanPlayer to prevent ending turn while doing this
                GameObject.Find("TradeController").GetComponent<TradeController>().YearOfPlentyInit();

                break;
            case UnitCode.MONOPOLY:

                // The player selects a resource type. Other players give this player ALL of their cards of this resource type.

                // TODO: Add new phase for HumanPlayer to prevent ending turn while doing this
                GameObject.Find("DiscardController").GetComponent<DiscardController>().MonopolyActivated();
                break;
            case UnitCode.VICTORY_CARD:

                // Clicking this card has no effect.
                break;
        }

        
    }

    public void GiveAllOfResourceToPlayer(UnitCode resourceCode, int recepientId)
    {
        int amount = stock[(int)resourceCode];

        // Take all cards of this resource type from this player.
        TakeFromPlayer(resourceCode, amount);

        // Send the resources to the player that played the Monopoly card.

        int[] monopolyReplyMessage = new int[3];

        monopolyReplyMessage[0] = (int)GamePlayer.MessageCode.MONOPOLY_REPLY; // Message code.
        monopolyReplyMessage[1] = recepientId; // Who should read the message?
        monopolyReplyMessage[2] = amount; // How much of the resource am I sending? (the Monopoly card player knows which resource is being sent)

        GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(monopolyReplyMessage, false);
    }

    private void CalculatePlayerRoadLength()
    {

        //// TODO: Creating a settlement on an open intersection can break a longest road.

        //// Find the last added road.
        //WorldPath lastRoad = myPlayer.GetLastAddedRoad();


        //// 0 -- left side, 1 -- right side
        //bool[] connectedOnSide = { false, false };

        //// Check both sides.

        //Intersection[] intersections = lastRoad.GetIntersections();
        //for (int i = 0; i < intersections.Length; i++)
        //{
        //    Intersection intersection = intersections[i];

        //    WorldPath[] paths = intersection.GetSurroundingPaths();

        //    int pathIndex = -1;
        //    foreach (WorldPath path in paths)
        //    {
        //        if (path == lastRoad) { continue; }

        //        // If one of the paths belongs to me, remember its path index. If both paths belong to me, remember the path with the higher index.
        //        if (path.OwnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        //        {
        //            if (path.RoadChainIndex > pathIndex)
        //            {
        //                pathIndex = path.RoadChainIndex;
        //                connectedOnSide[i] = true;
        //            }
        //        }
        //    }
        //}


        // LIST OF LISTS IDEA
    }

    private void LargestArmyCheck()
    {
        if (knightCardsPlayed < 3)
        {
            return;
        }

        // If we overtook the maximum, take the Largest Army card.
        if (knightCardsPlayed > largestArmyKnightCount)
        {
            // Overtake. Send a message so every player updates the current Largest Army owner.
            
            if (largestArmyOwnerId == -1)
            {
                GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.LARGEST_ARMY_TAKE_FIRST_TIME, PhotonNetwork.LocalPlayer);
            }
            else
            {
                GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.LARGEST_ARMY_STEAL, PhotonNetwork.LocalPlayer, largestArmyOwnerId);
            }

            largestArmyKnightCount = knightCardsPlayed;
            largestArmyOwnerId = PhotonNetwork.LocalPlayer.ActorNumber;

            GiveToPlayer(UnitCode.LARGEST_ARMY, 1);
            AddVictoryPoint(UnitCode.LARGEST_ARMY);

            int[] largestArmyOvertakeMessage = new int[3];

            largestArmyOvertakeMessage[0] = (int)GamePlayer.MessageCode.LARGEST_ARMY_OVERTAKE; // Message code.
            largestArmyOvertakeMessage[1] = PhotonNetwork.LocalPlayer.ActorNumber; // Who sent the message and who has the card.
            largestArmyOvertakeMessage[2] = knightCardsPlayed; // How many cards must the next player to contest for this card pass?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(largestArmyOvertakeMessage, false);
        }
    }

    private void LongestRoadCheck()
    {
        if (roadLength < 5)
        {
            return;
        }

        if (roadLength > longestRoadLength)
        {
            if (longestRoadOwnerId == -1)
            {
                GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.LONGEST_ROAD_TAKE_FIRST_TIME, PhotonNetwork.LocalPlayer);
            }
            else
            {
                GameObject.Find("EventTextController").GetComponent<EventTextController>().SendEvent(EventTextController.EventCode.LONGEST_ROAD_STEAL, PhotonNetwork.LocalPlayer, longestRoadOwnerId);
            }

            longestRoadLength = roadLength;
            longestRoadOwnerId = PhotonNetwork.LocalPlayer.ActorNumber;

            GiveToPlayer(UnitCode.LONGEST_ROAD, 1);
            AddVictoryPoint(UnitCode.LONGEST_ROAD);

            int[] longestRoadOvertakeMessage = new int[3];

            longestRoadOvertakeMessage[0] = (int)GamePlayer.MessageCode.LONGEST_ROAD_OVERTAKE; // Message code.
            longestRoadOvertakeMessage[1] = PhotonNetwork.LocalPlayer.ActorNumber; // Who sent the message and who has the card.
            longestRoadOvertakeMessage[2] = longestRoadLength; // What road length must be surpassed for the Longest Road card to be acquired?

            GameObject.Find("TurnManager").GetComponent<TurnManager>().SendMove(longestRoadOvertakeMessage, false);
        }
    }

    public void SetLargestArmyOwner(int ownerId, int amount)
    {
        // If I previously held this card, remove it from my inventory (and the points).

        if (largestArmyOwnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            TakeFromPlayer(UnitCode.LARGEST_ARMY, 1);
            TakeVictoryPoint(UnitCode.LARGEST_ARMY);
        }

        largestArmyKnightCount = amount;
        largestArmyOwnerId = ownerId;
    }

    public void SetLongestRoadOwner(int ownerId, int roadLength)
    {
        if (longestRoadOwnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            TakeFromPlayer(UnitCode.LONGEST_ROAD, 1);
            TakeVictoryPoint(UnitCode.LONGEST_ROAD);
        }

        longestRoadLength = roadLength;
        longestRoadOwnerId = ownerId;
    }
}
