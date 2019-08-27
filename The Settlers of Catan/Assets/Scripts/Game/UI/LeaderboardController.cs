using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{

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

    
    public void Init()
    {
        //for (int i = 0; i < 4; i++)
        //{
        //    string key = "leaderboardSlot" + (i + 1);
            
        //    if ((int)PhotonNetwork.CurrentRoom.CustomProperties[key] != 0)
        //    {
        //        // leaderboard manager.setslot(playerid)
        //        TakeSlot(i, (int)PhotonNetwork.CurrentRoom.CustomProperties[key]);

        //    }
        //}
    }

    public void UpdateLeaderboard()
    {

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

        slot.SetPlayer(playerId);
    }
}
