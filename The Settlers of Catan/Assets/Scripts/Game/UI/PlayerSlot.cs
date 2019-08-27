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
   

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerText(string playerName)
    {
        player.text = playerName;
    }

    public void SetPlayerScore(int newScore)
    {
        score.text = "" + newScore;
    }

    public void DisplayPosition()
    {
        position.gameObject.SetActive(true);
    }

    public void SetPlayer(int playerId)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);

        Debug.Log(player.ActorNumber);
        string username = player.CustomProperties["username"] as string;
        string colour = player.CustomProperties["colour"] as string;


        DisplayPosition();
        SetPlayerText("<color=" + colour + ">" + username + "</color>");
        SetPlayerScore(0);
        
    }
    
}
