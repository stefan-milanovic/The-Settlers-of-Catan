using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class RoomButton : MonoBehaviour
{
    
    [SerializeField]
    private TextMeshProUGUI roomText;

    [SerializeField]
    private TextMeshProUGUI countText;

    private string roomName;
    private int roomSize;
    private int playerCount;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void JoinRoomOnClick()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void SetRoom(string name, int size, int count)
    {
        roomName = name;
        roomSize = size;
        playerCount = count;

        roomText.text = name;
        countText.text = "(" + count + "/" + size + ")";
    }
}
