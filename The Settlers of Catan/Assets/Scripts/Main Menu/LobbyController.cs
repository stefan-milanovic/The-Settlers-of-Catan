using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;

public class LobbyController : MonoBehaviourPunCallbacks
{

    //[SerializeField]
    //private GameObject lobbyConnectButton;

    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject mainMenuPanel;


    private string roomName;
    private List<RoomInfo> localRoomsList = new List<RoomInfo>();

    [SerializeField]
    private Transform roomsContainer;

    [SerializeField]
    private GameObject roomSlotPrefab;

    private const byte MAX_NUMBER_OF_PLAYERS = 4;
    private const string START_NAME = "Anonymous";

    private MainMenu mainMenu;

    private bool firstTime = true;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = GameObject.Find("MainMenuControls").GetComponent<MainMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {

        if (firstTime)
        {
            firstTime = false;
            PhotonNetwork.AutomaticallySyncScene = true;
            
            mainMenu.EnableBottomButtons();

            if (PlayerPrefs.GetString("Username") != null)
            {
                PlayerNameUpdate(PlayerPrefs.GetString("Username"));
            }
            else
            {
                PlayerNameUpdate(START_NAME);
                mainMenu.SetPlayerName(START_NAME);
            }
        }

        PhotonNetwork.JoinLobby();
    }


    public void PlayerNameUpdate(string newName)
    {
        PhotonNetwork.NickName = newName;
        PlayerPrefs.SetString("Username", newName);


        // If the player is in a room, change room display name.
        if (PhotonNetwork.InRoom)
        {
            GameObject.Find("RoomController").GetComponent<RoomController>().RefreshPlayerList();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomIndex;
        foreach (RoomInfo room in roomList)
        {
            if (localRoomsList != null)
            {
                roomIndex = localRoomsList.FindIndex(FindByName(room.Name));
            }
            else
            {
                roomIndex = -1;
            }

            if (roomIndex != -1)
            {
                localRoomsList.RemoveAt(roomIndex);
                Destroy(roomsContainer.GetChild(roomIndex).gameObject);
            }
            
            if (room.PlayerCount > 0)
            {
                localRoomsList.Add(room);
                AddRoomToPanelList(room);
            }
        }
    }


    // Utility predicate function for finding lobby rooms by name.
    static System.Predicate<RoomInfo> FindByName(string name)
    {
        return delegate (RoomInfo room)
        {
            return room.Name == name;
        };
    }

    void AddRoomToPanelList(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject newListing = Instantiate(roomSlotPrefab, roomsContainer, true);
            RoomButton roomButton = newListing.GetComponent<RoomButton>();
            roomButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }
    
    public void CreateRoom(string roomName)
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = MAX_NUMBER_OF_PLAYERS, PlayerTtl = 0 };

        PhotonNetwork.CreateRoom(roomName, roomOps);
    }

    public override void OnCreatedRoom()
    {
        //PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        //{
        //    ["playerSlot2"] = 0,
        //    ["playerSlot3"] = 0,
        //    ["playerSlot4"] = 0
        //}
        //);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // show error message
        Debug.Log("ERROR: Room with same name exists");

        // signal to main menu to show error message

        mainMenu.DisplayRoomCreationErrorMessage();

    }
    
    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }
}
