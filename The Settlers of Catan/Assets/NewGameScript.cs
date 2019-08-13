using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewGameScript : MonoBehaviour
{

    

    // Start is called before the first frame update
    void Start()
    {
        // obtain reference to local player
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateGameButtonPress()
    {
        // switch to CreateGamePanel


    }


    public void CreateGameOpenRoomButtonPress()
    {
        // create a lobby, place player in lobby (tell player to ask server for id), switch to lobby panel

        Debug.Log("Ping");

       // localPlayer.CreateLobby();

        // switch to lobby panel
    }

    public void CreateGameBackButtonPress()
    {

    }
}
