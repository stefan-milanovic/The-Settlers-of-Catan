using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{

    private int freeSlot = 0;

    private readonly double[] yMins = { 0.6, 0.4, 0.2, 0 };
    private readonly double[] yMaxs = { 0.8, 0.6, 0.4, 0.2 };

    [SerializeField]
    private PlayerSlot[] playerSlots;

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLeaderboard()
    {

    }

    public void RegisterPlayer(Player player)
    {

        int freeSlot = GetFreeSlot();
        photonView.RPC("RPCRegisterPlayer", RpcTarget.AllBufferedViaServer, player.ActorNumber, freeSlot);
    }

    // Every leaderboardcontroller should call this 4 times.
    [PunRPC]
    private void RPCRegisterPlayer(int playerActorNumber, int freeSlot)
    {
        Debug.Log("registering player: " + playerActorNumber + ", free slot: " + freeSlot);

        playerSlots[freeSlot].SetPlayer(playerActorNumber, freeSlot);
    }
    
    private int GetFreeSlot()
    {
        bool llock = (bool) PhotonNetwork.CurrentRoom.CustomProperties["leaderboardLock"];

        while (llock)
        {
            llock = (bool)PhotonNetwork.CurrentRoom.CustomProperties["leaderboardLock"];
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            ["leaderboardLock"] = true
        });

        int freeSlot = (int) PhotonNetwork.CurrentRoom.CustomProperties["leaderboardFreeSlot"];

        Debug.Log("In getfreeslot() receieved freeslot: " + freeSlot);

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            ["leaderboardFreeSlot"] = freeSlot + 1,
            ["leaderboardLock"] = false
        });

        return freeSlot;
    }
}
