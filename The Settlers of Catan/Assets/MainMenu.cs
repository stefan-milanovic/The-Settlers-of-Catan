using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Photon.Pun;

public class MainMenu : MonoBehaviour
{
    
    public enum WindowCode
    {
        START_NEW_GAME_WINDOW,
        TUTORIAL_WINDOW,
        ABOUT_WINDOW,
        ROOM_CREATION_WINDOW,
        LOBBY_WINDOW,
        ROOM_WINDOW
    }
    

    WindowCode? currentOpenWindow = null;
    
    private LobbyController lobbyController;
    private RoomController roomController;

    // Start is called before the first frame update
    void Start()
    {
        // localPlayer = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>().playerPrefab.GetComponent<PlayerConnection>();
        lobbyController = GameObject.Find("LobbyController").GetComponent<LobbyController>();
        roomController = GameObject.Find("RoomController").GetComponent<RoomController>();

        // Connect to Photon servers.

        lobbyController.ConnectToServer();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Window toggle functions.

    public void ToggleWindow(WindowCode windowToOpen)
    {

        string windowPrefix = FindPrefix(windowToOpen);

        string panelTag = windowPrefix + "Panel";
        string buttonTag = windowPrefix + "Button";

        if (currentOpenWindow == null)
        {

            DisplayPanel(panelTag, true);
            ToggleButton(buttonTag);

            currentOpenWindow = windowToOpen;
        }
        else if (currentOpenWindow.Equals(windowToOpen))
        {
            // only close

            DisplayPanel(panelTag, false);
            ToggleButton(buttonTag);

            currentOpenWindow = null;
        }
        else
        {
            // else -> close old, open new

            // if the old window was the room window, close the chat window as well

            
            string oldWindowPrefix = FindPrefix(currentOpenWindow);

            if (currentOpenWindow == WindowCode.ROOM_WINDOW)
            {
                DisplayPanel("ChatPanel", false);
            }

            string oldPanelTag = oldWindowPrefix + "Panel";
            string oldButtonTag = oldWindowPrefix + "Button";

            DisplayPanel(oldPanelTag, false);
            ToggleButton(oldButtonTag);

            DisplayPanel(panelTag, true);
            ToggleButton(buttonTag);

            currentOpenWindow = windowToOpen;

            if (currentOpenWindow == WindowCode.ROOM_WINDOW)
            {
                DisplayPanel("ChatPanel", true);
            }

            // if the current window is the Room window, open the Chat window as well
        }
    }

    // utility function
    private string FindPrefix(WindowCode? windowToOpen)
    {
        /*
         * 
         * START_NEW_GAME_WINDOW,
        TUTORIAL_WINDOW,
        ABOUT_WINDOW,
        ROOM_CREATION_WINDOW,
        LOBBY_WINDOW
        */
        switch (windowToOpen)
        {
            case WindowCode.START_NEW_GAME_WINDOW: return "StartNewGame";
            case WindowCode.TUTORIAL_WINDOW: return "Tutorial";
            case WindowCode.ABOUT_WINDOW: return "About";
            case WindowCode.ROOM_CREATION_WINDOW: return "CreateGame";
            case WindowCode.LOBBY_WINDOW: return "JoinGame";
            case WindowCode.ROOM_WINDOW: return "Room";
        }

        // In case of invalid WindowCode - should never occur
        return "";
    }

    private void DisplayPanel(string panelTag, bool visibility)
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (obj.CompareTag(panelTag))
            {
                Debug.Log("setting " + panelTag + "to " + visibility);
                obj.SetActive(visibility);
                break;
            }
        }
    }

    private void ToggleButton(string buttonTag)
    {

        // Only toggles if it's a button from the main menu panel at the bottom of the screen.

        string[] validButtons = { "StartNewGameButton", "TutorialButton", "AboutButton" };
        if (!validButtons.Contains(buttonTag))
        {
            return;
        }

        Button button = GameObject.FindGameObjectWithTag(buttonTag).GetComponent<Button>();

        ColorBlock colourBlock = button.colors;

        Color oldNormal = colourBlock.normalColor;
        colourBlock.normalColor = colourBlock.pressedColor;
        colourBlock.highlightedColor = oldNormal;
        colourBlock.pressedColor = oldNormal;


        button.colors = colourBlock;

    }

    public void OnRoomNameChange(string newName)
    {
        if (newName == "")
        {
            // disable Create button
            DisableButton(GameObject.FindGameObjectWithTag("ConfirmRoomCreationButton").GetComponent<Button>());
        }
        else if (newName.Length == 1)
        {
            EnableButton(GameObject.FindGameObjectWithTag("ConfirmRoomCreationButton").GetComponent<Button>());
            
        }
    }
    // Button presses.

    public void StartButtonPress()
    {

        ToggleWindow(WindowCode.START_NEW_GAME_WINDOW);
    }

    public void TutorialButtonPress()
    {
        ToggleWindow(WindowCode.TUTORIAL_WINDOW);
    }

    public void AboutButtonPress()
    {
        ToggleWindow(WindowCode.ABOUT_WINDOW);
    }

    public void CreateRoomButtonPress()
    {
        ToggleWindow(WindowCode.ROOM_CREATION_WINDOW);

        lobbyController.JoinPhotonLobby();
    }

    public void RoomCreationBackPress()
    {
        ToggleWindow(WindowCode.START_NEW_GAME_WINDOW);
    }

    public void RoomCreationConfirmationPress()
    {

        string roomName = GameObject.FindGameObjectWithTag("RoomNameField").GetComponent<InputField>().text;
        string password = GameObject.FindGameObjectWithTag("RoomPasswordField").GetComponent<InputField>().text;

        lobbyController.CreateRoom(roomName, password);
        
    }


    public void JoinLobbyButtonPress()
    {
        ToggleWindow(WindowCode.LOBBY_WINDOW);

        lobbyController.JoinPhotonLobby();
    }

    public void LobbyBackPress()
    {
        ToggleWindow(WindowCode.START_NEW_GAME_WINDOW);

        lobbyController.MatchmakingCancel();
    }
    
    public void RoomLeavePress()
    {
        ToggleWindow(WindowCode.LOBBY_WINDOW);

        roomController.LeaveRoom();
    }

    public void ExitButtonPress()
    {
        // Disconnect from Photon servers and exit the application.

        lobbyController.DisconnectFromServer();
        Application.Quit();
    }

    public static void DisableButton(Button button)
    {
        button.interactable = false;
    }

    public static void EnableButton(Button button)
    {
        button.interactable = true;
    }


}
