using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventTextController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI eventText;

    private PhotonView photonView;

    public enum TextCode
    {
        FIRST_TURN_PHASE_ONE,
        FIRST_TURN_PHASE_TWO,
        SECOND_TURN_PHASE_ONE,
        SECOND_TURN_PHASE_TWO
    };

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {

        eventText.text = "Waiting for all players to connect...";

    }

    [PunRPC]
    private void RPCInit()
    {
        
    }
    

    public void SetText(TextCode code, Player player)
    {
        photonView.RPC("RPCSetText", RpcTarget.All, code, player.ActorNumber);
        
    }

    [PunRPC]
    public void RPCSetText(TextCode code, int actorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        switch (code)
        {
            case TextCode.FIRST_TURN_PHASE_ONE:
                eventText.text = "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their first settlement.";
                break;
            case TextCode.FIRST_TURN_PHASE_TWO:
                eventText.text = "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their first road.";
                break;
            case TextCode.SECOND_TURN_PHASE_ONE:
                eventText.text = "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their second settlement.";
                break;
            case TextCode.SECOND_TURN_PHASE_TWO:
                eventText.text = "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>" + " is placing their second road.";
                break;
        }
    }
}
