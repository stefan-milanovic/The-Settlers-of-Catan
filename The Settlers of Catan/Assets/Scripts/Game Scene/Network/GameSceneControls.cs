using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameSceneControls : MonoBehaviour
{

    private ChatController chatController;

    // Start is called before the first frame update
    void Start()
    {
        CreatePlayer();

        chatController = GameObject.Find("ChatController").GetComponent<ChatController>();
        chatController.JoinChat(PhotonNetwork.CurrentRoom.Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating player");
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer"), Vector3.zero, Quaternion.identity);
    }
}
