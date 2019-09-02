using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceController : MonoBehaviour
{

    int fallenDiceCount = 0;
    int value = 0;

    GamePlayer player;

    [SerializeField]
    private Dice[] dice;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetPlayer(GamePlayer p)
    {
        player = p;
    }

    public void SetDiceOwner(Player p)
    {
        foreach (Dice die in dice)
        {
            die.SetOwner(p);
        }
    }
    public void RollDice()
    {

        // Only roll the die if the event controller isn't busy.
        //if (GameObject.Find("EventTextController").GetComponent<EventTextController>().IsBusy())
       // {
        //    return;
        //}

        foreach (Dice die in dice)
        {
            die.RollDice();
        }

        // Disable rolling for the current player.
        player.DisableRollDiceButton();
    }

    public void DiceFallen(int diceValue)
    {
        if (fallenDiceCount == 0)
        {
            fallenDiceCount++;
            value += diceValue;
        }
        else if (fallenDiceCount == 1)
        {
            value += diceValue;

            // Inform player.
            player.WaitForDiceResult(value);

            fallenDiceCount = 0;
            value = 0;
        }
    }
}
