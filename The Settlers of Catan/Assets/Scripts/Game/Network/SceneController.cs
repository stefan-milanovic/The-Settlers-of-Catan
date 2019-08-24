using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.Experimental.UIElements;

public class SceneController : MonoBehaviour
{

    private ChatController chatController;
    private EventTextController eventTextController;
    
    // Start is called before the first frame update
    void Start()
    {

        eventTextController = GameObject.Find("EventTextController").GetComponent<EventTextController>();
        chatController = GameObject.Find("ChatController").GetComponent<ChatController>();
            
        eventTextController.Init();
        CreatePlayer();
        chatController.JoinChat(PhotonNetwork.CurrentRoom.Name);


        // Create leaderboard controller.


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
