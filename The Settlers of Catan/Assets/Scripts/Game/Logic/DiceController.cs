using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceController : MonoBehaviour
{

    int fallenDiceCount = 0;
    int value = 0;

    GamePlayer player;

    [SerializeField]
    private Dice[] die;

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

    public void RollDice()
    {
        foreach (Dice dice in die)
        {
            dice.RollDice();
        }
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
