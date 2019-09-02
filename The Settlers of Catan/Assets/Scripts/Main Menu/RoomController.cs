using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomController : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private int multiplayerSceneIndex;

    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject roomPanel;

    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private Transform playerList;
    

    [SerializeField]
    private TextMeshProUGUI roomTitle; //display for the name of the room

    [SerializeField]
    private TextMeshProUGUI[] roomPlayerSlots;

    private MainMenu mainMenu;
    private ChatController chatController;

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = GameObject.Find("MainMenuControls").GetComponent<MainMenu>();
        chatController = GameObject.Find("ChatController").GetComponent<ChatController>();
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {


    }
    
    private void ListPlayers()
    {
        for (int i = 0; i < roomPlayerSlots.Length; i++)
        {
            Player player;
            if (i >= PhotonNetwork.PlayerList.Length)
            {
                player = null;
            }
            else
            {
                player = PhotonNetwork.PlayerList[i];
            }
            if (player != null)
            {
                roomPlayerSlots[i].text = player.NickName;
            }
            else
            {
                roomPlayerSlots[i].text = "<color=#A6A6A6>Empty</color>";
            }
        }
    }

    public void RefreshPlayerList()
    {
        photonView.RPC("RPCRefreshPlayerList", RpcTarget.All);
    }

    [PunRPC]
    private void RPCRefreshPlayerList()
    {
        ListPlayers();
    }

    public override void OnJoinedRoom()
    {
        // Switch to room panel

        mainMenu.ToggleWindow(MainMenu.WindowCode.ROOM_WINDOW);
        roomTitle.text = PhotonNetwork.CurrentRoom.Name;

        // Join room chat as well.
        chatController.JoinChat(PhotonNetwork.CurrentRoom.Name);

        
        if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
        {

        }
        // do this code only when it's (4/4)
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }

        //ClearPlayerList();
        ListPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //ClearPlayerList();
        ListPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //ClearPlayerList();
        ListPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(multiplayerSceneIndex);
        }
    }
   

    public void LeaveRoom()
    {
        // go back to lobby panel

        // PhotonNetwork.LeaveRoom();
        // PhotonNetwork.LeaveLobby();
        // StartCoroutine(rejoinLobby());

        chatController.SendChatMessage("has left the room.");
        chatController.LeaveChat();
        PhotonNetwork.LeaveRoom();
    }
}