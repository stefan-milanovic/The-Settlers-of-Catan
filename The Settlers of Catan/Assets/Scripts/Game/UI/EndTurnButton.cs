using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    // Start is called before the first frame update

    private GamePlayer myPlayer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayer(GamePlayer p)
    {
        myPlayer = p;
    }
    public void EndTurnButtonPress()
    {
        myPlayer.EndTurnButtonPress();
    }
}
