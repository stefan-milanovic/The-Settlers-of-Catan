using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverviewController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] scoreText;
    
    [SerializeField]
    private TextMeshProUGUI totalCardsText;

    [SerializeField]
    private TextMeshProUGUI cardWarningText;

    private static int DISCARD_WARNING_AMOUNT_THRESHOLD = 8;

    // Start is called before the first frame update
    void Start()
    {
        scoreText[0].text = "Settlements: +0";
        scoreText[1].text = "Cities: +0";
        scoreText[2].text = "Largest Army: +0";
        scoreText[3].text = "Longest Road: +0";
        scoreText[4].text = "Victory Cards: +0";

        totalCardsText.text = "Total number of cards: <color=orange>0</color>";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVictoryPoint(Inventory.UnitCode source, int amount)
    {
        switch (source)
        {
            case Inventory.UnitCode.SETTLEMENT:
                scoreText[0].text = "Settlements: +" + amount;
                break;
            case Inventory.UnitCode.CITY:
                scoreText[1].text = "Cities: +" + amount;
                break;
            case Inventory.UnitCode.LARGEST_ARMY:
                scoreText[2].text = "Largest Army: +" + amount;
                break;
            case Inventory.UnitCode.LONGEST_ROAD:
                scoreText[3].text = "Longest Road: +" + amount;
                break;
            case Inventory.UnitCode.VICTORY_CARD:
                scoreText[4].text = "Victory Cards: +" + amount;
                break;
        }
    }

    public void SetTotalCardsText(int amount)
    {
        totalCardsText.text = "Total number of cards: " + ((amount >= DISCARD_WARNING_AMOUNT_THRESHOLD) ? ("<color=red>" + amount + "</color>") : ("<color=orange>" + amount + "</color>"));

        if (amount >= DISCARD_WARNING_AMOUNT_THRESHOLD)
        {
            cardWarningText.text = "<color=red>If any player rolls a <color=black>7</color> you will have to discard <color=orange>" + amount / 2 + "</color> Resource cards.</color>";
        }
        else
        {
            cardWarningText.text = "";
        }
    }
        
}
