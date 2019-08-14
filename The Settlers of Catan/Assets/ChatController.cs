using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ChatController : MonoBehaviour, IChatClientListener
{

    // private static string WelcomeText = "Welcome to chat. Type \\help to list commands.";
    private static string HelpText = "\n    -- HELP --\n" +
        "To subscribe to channel(s):\n" +
            "\t<color=#E07B00>\\subscribe</color> <color=green><list of channelnames></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\s</color> <color=green><list of channelnames></color>\n" +
            "\n" +
            "To leave channel(s):\n" +
            "\t<color=#E07B00>\\unsubscribe</color> <color=green><list of channelnames></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\u</color> <color=green><list of channelnames></color>\n" +
            "\n" +
            "To switch the active channel\n" +
            "\t<color=#E07B00>\\join</color> <color=green><channelname></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\j</color> <color=green><channelname></color>\n" +
            "\n" +
            "To send a private message:\n" +
            "\t\\<color=#E07B00>msg</color> <color=green><username></color> <color=green><message></color>\n" +
            "\n" +
            "To change status:\n" +
            "\t\\<color=#E07B00>state</color> <color=green><stateIndex></color> <color=green><message></color>\n" +
            "<color=green>0</color> = Offline " +
            "<color=green>1</color> = Invisible " +
            "<color=green>2</color> = Online " +
            "<color=green>3</color> = Away \n" +
            "<color=green>4</color> = Do not disturb " +
            "<color=green>5</color> = Looking For Group " +
            "<color=green>6</color> = Playing" +
            "\n\n" +
            "To clear the current chat tab (private chats get closed):\n" +
            "\t<color=#E07B00>\\clear</color>";

    private const int HISTORY_LENGTH = 5;
    
    public ChatClient chatClient;

    public string UserName { get; set; }

    protected internal AppSettings chatAppSettings;

    private string selectedChannelName;

    [SerializeField]
    private InputField InputFieldChat;   

    [SerializeField]
    private TextMeshProUGUI channelTextbox;     

    // Start is called before the first frame update
    void Start()
    {
        chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings;

        if (string.IsNullOrEmpty(UserName))
        {
            this.UserName = "Player" + Random.Range(1, 100);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }
        
    }

    public void Connect()
    {
        chatClient = new ChatClient(this);

        chatClient.Connect(chatAppSettings.AppIdChat, "1.0", new Photon.Chat.AuthenticationValues(UserName));
        
        Debug.Log("Connecting as: " + this.UserName);
    }

    public void JoinChat(string roomName)
    {
        Connect();
        selectedChannelName = roomName;
    }

    public void LeaveChat()
    {
        chatClient.Disconnect();
    }

    public void OnDestroy()
    {
        if (chatClient != null)
        {
            chatClient.Disconnect();
        }
    }

    
    public void OnApplicationQuit()
    {
        if (chatClient != null)
        {
            chatClient.Disconnect();
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
        {
            Debug.LogError(message);
        }
        else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
        {
            Debug.LogWarning(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void PostHelpToCurrentChannel()
    {
        this.channelTextbox.text += HelpText;
    }

    public void OnEnterSend()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            SendChatMessage(InputFieldChat.text);
            InputFieldChat.text = "";
        }
    }

    public void OnClickSend()
    {
        if (InputFieldChat != null)
        {
            SendChatMessage(this.InputFieldChat.text);
            InputFieldChat.text = "";
        }
    }

    private void SendChatMessage(string inputLine)
    {
        if (string.IsNullOrEmpty(inputLine))
        {
            return;
        }

        //if ("test".Equals(inputLine))
        //{
        //    if (this.TestLength != this.testBytes.Length)
        //    {
        //        this.testBytes = new byte[this.TestLength];
        //    }

        //    this.chatClient.SendPrivateMessage(this.chatClient.AuthValues.UserId, this.testBytes, true);
        //}

        UnityEngine.Debug.Log("selectedChannelName: " + selectedChannelName);


        if (inputLine[0].Equals('\\'))
        {
            string[] tokens = inputLine.Split(new char[] { ' ' }, 2);
            if (tokens[0].Equals("\\help"))
            {
                this.PostHelpToCurrentChannel();
            }
            else if (tokens[0].Equals("\\clear"))
            {
                if (chatClient.TryGetChannel(selectedChannelName, false, out ChatChannel channel))
                {
                    channel.ClearMessages();
                }
            }
            else if (tokens[0].Equals("\\msg") && !string.IsNullOrEmpty(tokens[1]))
            {
                string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);
                if (subtokens.Length < 2) return;

                string targetUser = subtokens[0];
                string message = subtokens[1];
                this.chatClient.SendPrivateMessage(targetUser, message);
            }
            else
            {
                Debug.Log("The command '" + tokens[0] + "' is invalid.");
            }
        }
        else
        {
            this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
        }
    }

    public void AddMessageToSelectedChannel(string msg)
    {
        bool found = this.chatClient.TryGetChannel(this.selectedChannelName, out ChatChannel channel);
        if (!found)
        {
            Debug.Log("AddMessageToSelectedChannel failed to find channel: " + this.selectedChannelName);
            return;
        }

        if (channel != null)
        {
            channel.Add("Bot", msg, 0); //TODO: how to use msgID?
        }
    }

    public void OnDisconnected()
    {
        
    }

    public void OnConnected()
    {
        chatClient.Subscribe(selectedChannelName, HISTORY_LENGTH);
        chatClient.SetOnlineStatus(ChatUserStatus.Online);
 
    }

    public void OnChatStateChange(ChatState state)
    {
        
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals(selectedChannelName))
        {
            // update text
            ShowChannel(selectedChannelName);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        foreach (string channel in channels)
        {
            this.chatClient.PublishMessage(channel, "has joined the chatroom."); // you don't HAVE to send a msg on join but you could.
            
        }

        Debug.Log("OnSubscribed: " + string.Join(", ", channels));

        ShowChannel(channels[0]);
    }

    public void OnUnsubscribed(string[] channels)
    {
       
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }

    private void ShowChannel(string channelName)
    {
        if (string.IsNullOrEmpty(channelName))
        {
            return;
        }
        
        bool found = chatClient.TryGetChannel(channelName, out ChatChannel channel);
        if (!found)
        {
            Debug.Log("ShowChannel failed to find channel: " + channelName);
            return;
        }

        channelTextbox.text = channel.ToStringMessages();

        Debug.Log("ShowChannel: " + selectedChannelName);


    }
}
