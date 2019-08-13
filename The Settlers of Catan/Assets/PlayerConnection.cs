using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    // Start is called before the first frame update


    // Player's name (used in the lobby and in game).
    [SyncVar]
    string playerName = "Anonymous";

    public GameObject playerSlot;

    void Start()
    {
        if (isLocalPlayer == false)
        {

            // fill in other lobby slots
            return;
        }

        Debug.Log("here!");

        // Instantiate(playerSlot); - only this for local effects like fireworks
        // NetworkManager.Spawn() - for things that have to be on the network on everyone's pc


        // find panel to adjust with player info

        CmdFillLobbySlot();


        // give authority to main menu controls
        GameObject.Find("MainMenuControls").GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

    }

    // Update is called once per frame
    void Update()
    {
     
        // runs on everyone's pc, whether or not they own this player object.
    }



    // Wrapper function for the Create Lobby command sent to the server.
    public void CreateLobby()
    {
        CmdCreateLobby();
    }


    // COMMANDS
    // Special functions that ONLY get executed on the server.

    [Command]
    public void CmdFillLobbySlot()
    {
        // We are on the server right now.
    }

    [Command]
    public void CmdChangePlayerName()
    {

    }

    [Command]
    public void CmdCreateLobby()
    {
        Debug.Log("CmdCreateLobby() called");
    }


    // RPCs.
    // Special functions that ONLY get executed on the clients.
    [ClientRpc]
    public void RpcChangePlayerName()
    {

    }
}
