using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviourPunCallbacks
{
    
    private EventTextController eventTextController;

    [SerializeField]
    private EndTurnButton endTurnButton;

    private bool playerCreated = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void OnEnable()
    {
        base.OnEnable();

        eventTextController = GameObject.Find("EventTextController").GetComponent<EventTextController>();

        eventTextController.Init();


        if (PhotonNetwork.CurrentRoom.CustomProperties["leaderboardSlot1"] == null)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                ["leaderboardSlot1"] = 0,
                ["leaderboardSlot2"] = 0,
                ["leaderboardSlot3"] = 0,
                ["leaderboardSlot4"] = 0,
                ["colour1Owner"] = 0,
                ["colour2Owner"] = 0,
                ["colour3Owner"] = 0,
                ["colour4Owner"] = 0
            }
           );
            
        }
        else
        {
            CreatePlayer();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("test" + propertiesThatChanged);
        if (propertiesThatChanged.Count == 8)
        {
            bool leaderboardInit = true;
            for (int i = 0; i < 4; i++)
            {
                string key = "leaderboardSlot" + (i + 1);

                if (!propertiesThatChanged.ContainsKey(key))
                {
                    leaderboardInit = false;
                    break;
                }
            }

            if (leaderboardInit)
            {
                CreatePlayer();
            }
        }

    }

    private void CreatePlayer()
    {

        if (playerCreated) return;

        playerCreated = true;
        Debug.Log("Creating player");
        GameObject playerObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer"), Vector3.zero, Quaternion.identity);

        // attach to end turn button
        endTurnButton.SetPlayer(playerObject.GetComponent<GamePlayer>());
    }
}
