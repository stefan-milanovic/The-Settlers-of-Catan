using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
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

    
    public void Init()
    {

    }

    // Called when the local player earns some points.
    public void UpdateLeaderboard(int localPlayerId, int newPoints)
    {
         photonView.RPC("RPCUpdateLeaderboard", RpcTarget.All, localPlayerId, newPoints);
    }

    [PunRPC]
    private void RPCUpdateLeaderboard(int playerId, int newPoints)
    {
        // Calculate new standings.

        int[] playersSorted = new int [PhotonNetwork.CurrentRoom.PlayerCount];
        int[] pointsSorted = new int[PhotonNetwork.CurrentRoom.PlayerCount];

        for (int l = 0; l < playersSorted.Length; l++)
        {
            playersSorted[l] = l + 1;
        }
        pointsSorted[playerId - 1] = newPoints;
        foreach (PlayerSlot slot in playerSlots)
        {
            if (!slot.Initialised) { continue; }

            if (slot.PlayerId != playerId)
            {
                pointsSorted[slot.PlayerId - 1] = slot.Score;
            }
        }

        // Sort descending.

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount - 1; i++)
        {
            for (int j = i + 1; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
            {
                if (pointsSorted[i] < pointsSorted[j])
                {
                    int playerTemp = playersSorted[i];
                    int pointTemp = pointsSorted[i];

                    playersSorted[i] = playersSorted[j];
                    pointsSorted[i] = pointsSorted[j];

                    playersSorted[j] = playerTemp;
                    pointsSorted[j] = playerTemp;
                }
            }
        }

        // Assign to slots.

        int r = 0;
        foreach (PlayerSlot slot in playerSlots)
        {
            if (!slot.Initialised) continue;

            slot.SetPlayer(playersSorted[r], pointsSorted[r]);
            r++;
        }
    }

    public void TakeSlot(int slotId, int playerId)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPCTakeSlot", RpcTarget.All, slotId, playerId);
        }
    }

    [PunRPC]
    private void RPCTakeSlot(int slotId, int playerId)
    {
        PlayerSlot slot = playerSlots[slotId];

        // When a slot is taken, the score for that player is 1.
        slot.Init();
        slot.SetPlayer(playerId, 1);
    }
}
