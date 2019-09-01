using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopmentCardDeck : MonoBehaviour
{

    private readonly Inventory.UnitCode[] developmentCards =
    {
        Inventory.UnitCode.KNIGHT,
        Inventory.UnitCode.EXPANSION,
        Inventory.UnitCode.YEAR_OF_PLENTY,
        Inventory.UnitCode.MONOPOLY,
        Inventory.UnitCode.VICTORY_CARD
    };

    private readonly int[] cardInitialStock = {
        14,
        2,
        2,
        2,
        5
    };

    private const int DEVELOPMENT_CARD_COUNT = 25;
    private const int ITERATION_NUMBER = 5;
    
    private Queue<Inventory.UnitCode> deck;

    private PhotonView photonView;
        
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init()
    {

        photonView = GetComponent<PhotonView>();

        // Shuffle the deck.
        ShuffleDeck();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Only called by the master client when the board is generated.
    private void ShuffleDeck()
    {

        // Create an unsorted deck.
        deck = new Queue<Inventory.UnitCode>();
        int[] cardStockCopy = new int[cardInitialStock.Length];
        cardInitialStock.CopyTo(cardStockCopy, 0);
        Inventory.UnitCode[] unsortedDeck = new Inventory.UnitCode[DEVELOPMENT_CARD_COUNT];

        for (int i = 0, j = 0; i < cardInitialStock.Length;)
        {
            if (cardStockCopy[i] != 0)
            {
                unsortedDeck[j++] = developmentCards[i];
                cardStockCopy[i]--;
            }
            else
            {
                i++;
            }
        }

        // Shuffle for 5 iterations.
        for (int i = 0; i < ITERATION_NUMBER; i++)
        {
            RecursiveShuffle(deck, unsortedDeck, 0, DEVELOPMENT_CARD_COUNT - 1, i);

            if (i != ITERATION_NUMBER - 1)
            {
                // Flush deck queue into unsortedDeck.
                int j = 0;
                while (deck.Count > 0)
                {
                    unsortedDeck[j++] = deck.Dequeue();
                }
            }
        }

        int[] sortedDeck = new int[DEVELOPMENT_CARD_COUNT];
        int r = 0;
        foreach(Inventory.UnitCode card in deck)
        {
            sortedDeck[r++] = (int)card;
        }

        GameObject.Find("ShopController").GetComponent<ShopController>().SetDeck(this);

        // Set this deck room-wide.    
        photonView.RPC("RPCSetDeck", RpcTarget.OthersBuffered, sortedDeck);
    }

    [PunRPC]
    private void RPCSetDeck(int[] sortedDeck)
    {
        this.deck = new Queue<Inventory.UnitCode>();
       
        foreach(int card in sortedDeck)
        {
            deck.Enqueue((Inventory.UnitCode)card);
        }

        GameObject.Find("ShopController").GetComponent<ShopController>().SetDeck(this);

        //Debug.Log("received this deck: ");

        //foreach (Inventory.UnitCode card in deck)
        //{
        //    Debug.Log(card);
        //}
    }

    private void RecursiveShuffle(Queue<Inventory.UnitCode> deck, Inventory.UnitCode[] unsortedDeck, int lower, int upper, int iteration)
    {

        if (lower <= upper)
        {
            int index = Random.Range(lower, upper);
            
             deck.Enqueue(unsortedDeck[index]);
            

            if (Random.Range(0, 1) == 1)
            {
                RecursiveShuffle(deck, unsortedDeck, lower, index - 1, iteration);
                RecursiveShuffle(deck, unsortedDeck, index + 1, upper, iteration);
            }
            else
            {
                RecursiveShuffle(deck, unsortedDeck, index + 1, upper, iteration);
                RecursiveShuffle(deck, unsortedDeck, lower, index - 1, iteration);
            }

        }
    }

    public Inventory.UnitCode TakeCard()
    {
        // Take a card from the top of the deck.
        Inventory.UnitCode card = deck.Peek();

        // Notify every client to discard the top card.
        photonView.RPC("RPCTakeCard", RpcTarget.All);

        return card;
    }

    [PunRPC]
    private void RPCTakeCard()
    {
        deck.Dequeue();

        // Update deck graphics.
    }
}
