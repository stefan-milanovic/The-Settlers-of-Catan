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
    private TextMeshProUGUI scoreText;

    private int playerId;

    private int score;

    private bool initialised = false;

    public bool Initialised {
        get { return initialised; }
        set { initialised = value; }
    }

    public int Score {
        get { return score; }
        set { score = value; }
    }

    public int PlayerId {
        get { return playerId; }
        set { playerId = value; }
    }

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

    public void Init()
    {
        initialised = true;
    }

    public void SetPlayerScore(int newScore)
    {
        this.score = newScore;
        scoreText.text = "" + newScore;
    }

    public void DisplayPosition()
    {
        position.gameObject.SetActive(true);
    }

    public void SetPlayer(int playerId, int score)
    {
        if (initialised)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);

            string username = player.CustomProperties["username"] as string;
            string colour = player.CustomProperties["colour"] as string;

            this.playerId = playerId;

            DisplayPosition();
            SetPlayerText("<color=" + colour + ">" + username + "</color>");
            SetPlayerScore(score);
        }
    }
    
}
