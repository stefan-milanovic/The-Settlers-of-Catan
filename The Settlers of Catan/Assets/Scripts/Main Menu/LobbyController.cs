﻿using System.Collections;
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
    private List<RoomInfo> localRoomsList;

    [SerializeField]
    private Transform roomsContainer;

    [SerializeField]
    private GameObject roomSlotPrefab;

    private const byte MAX_NUMBER_OF_PLAYERS = 4;
    private const string START_NAME = "Anonymous";

    private MainMenu mainMenu;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = GameObject.Find("MainMenuControls").GetComponent<MainMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // when we first connect to the Photon servers

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        //lobbyConnectButton.SetActive(true);
        localRoomsList = new List<RoomInfo>();

        // player name : 9min in https://www.youtube.com/watch?v=onDorc3Qfn0

        // Initialize player name.
        PlayerNameUpdate(START_NAME);
    }

    public void PlayerNameUpdate(string newName)
    {
        PhotonNetwork.NickName = newName;
        PlayerPrefs.SetString("Username", newName);
    }

    public void JoinPhotonLobby()
    {
        PhotonNetwork.JoinLobby();
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
    
    public void CreateRoom(string roomName, string password)
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = MAX_NUMBER_OF_PLAYERS };

        PhotonNetwork.CreateRoom(roomName, roomOps);
    }

    public override void OnCreatedRoom()
    {
        //PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        //{
        //    ["leaderboardSlot1"] = 0,
        //    ["leaderboardSlot2"] = 0,
        //    ["leaderboardSlot3"] = 0,
        //    ["leaderboardSlot4"] = 0
        //},
        //new ExitGames.Client.Photon.Hashtable
        //{
        //    ["leaderboardSlot1"] = 0,
        //    ["leaderboardSlot2"] = 0,
        //    ["leaderboardSlot3"] = 0,
        //    ["leaderboardSlot4"] = 0
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


    public void MatchmakingCancel()
    {
        PhotonNetwork.LeaveLobby();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }
}
