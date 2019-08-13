using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
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
    private GameObject playerSlotPrefab; //Instantiate to display each player in the room

    [SerializeField]
    private TextMeshProUGUI roomTitle; //display for the name of the room


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

    private void ClearPlayerList()
    {
        for (int i = playerList.childCount - 1; i >= 0; i--)
        {
            Destroy(playerList.GetChild(i).gameObject);
        }
    }

    private void ListPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject slot = Instantiate(playerSlotPrefab, playerList);
            Text slotText = slot.transform.GetChild(0).GetComponent<Text>();
            // slotText.text = player.NickName;
        }
    }


    public override void OnJoinedRoom()
    {
        // switch to room panel

        mainMenu.ToggleWindow(MainMenu.WindowCode.ROOM_WINDOW);

        roomTitle.text = PhotonNetwork.CurrentRoom.Name;


        // do this code only when it's (4/4)
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }

        ClearPlayerList();
        ListPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ClearPlayerList();
        ListPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ClearPlayerList();
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

    IEnumerator RejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    public void LeaveRoom()
    {
        // go back to lobby panel

        // PhotonNetwork.LeaveRoom();
        // PhotonNetwork.LeaveLobby();
        // StartCoroutine(rejoinLobby());

        PhotonNetwork.LeaveRoom();
    }
}