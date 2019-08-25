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

    [SerializeField]
    private EndTurnButton endTurnButton;
    // Start is called before the first frame update
    void Start()
    {

        eventTextController = GameObject.Find("EventTextController").GetComponent<EventTextController>();
        chatController = GameObject.Find("ChatController").GetComponent<ChatController>();
            
        eventTextController.Init();
        CreatePlayer();
        chatController.JoinChat(PhotonNetwork.CurrentRoom.Name);


        // Setup leaderboard controller.
        if (PhotonNetwork.IsMasterClient)
        {

            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                ["leaderboardFreeSlot"] = 0,
                ["leaderboardLock"] = false
            });
        }
        

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating player");
        GameObject playerObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer"), Vector3.zero, Quaternion.identity);

        // attach to end turn button
        endTurnButton.SetPlayer(playerObject.GetComponent<GamePlayer>());
    }
}
