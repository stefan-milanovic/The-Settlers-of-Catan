using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class PlayerSlot : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI position;

    [SerializeField]
    private TextMeshProUGUI player;

    [SerializeField]
    private TextMeshProUGUI score;


    private PhotonView photonView;

    private int leaderboardPosition;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerText(string playerName)
    {
        Debug.Log("setting playerName " + playerName);
        player.text = playerName;
    }

    public void SetPlayerScore(int newScore)
    {
        score.text = "" + newScore;
    }

    public void SetPlayerPosition(int newPosition)
    {
        position.text = "#" + (newPosition + 1);
    }

    public void SetPlayer(int playerActorId, int position)
    {
        photonView.RPC("RPCSetPlayer", RpcTarget.All, playerActorId, position);
    }

    

    [PunRPC]
    public void RPCSetPlayer(int playerActorId, int position)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorId);

        string username = player.CustomProperties["username"] as string;
        string colour = player.CustomProperties["colour"] as string;

        SetPlayerText("<color=" + colour + ">" + username + "</color>");

        SetPlayerScore(0);
        
        SetPlayerPosition(leaderboardPosition = position);

        // set slot owner to player
        
    }
}
