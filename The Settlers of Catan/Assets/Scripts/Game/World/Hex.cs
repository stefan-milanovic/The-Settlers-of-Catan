using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hex : MonoBehaviour
{

    private bool blinkFlag = false;

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetMaterial(string materialPrefix)
    {
        photonView.RPC("RPCSetMaterial", RpcTarget.All, materialPrefix);
    }

    public void SetNumber(short number)
    {
        photonView.RPC("RPCSetNumber", RpcTarget.All, number);
    }

    [PunRPC]
    private void RPCSetMaterial(string materialPrefix)
    {

        string materialName = materialPrefix + "Material";
        string materialPath = "Materials/BoardResources/" + materialPrefix + "/" + materialName;

        Material material = Resources.Load(materialPath) as Material;

        gameObject.GetComponent<MeshRenderer>().material = material;
    }

    [PunRPC]
    private void RPCSetNumber(short number)
    {
        TextMeshPro childText = this.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();

        if (number == 6 || number == 8)
        {
            childText.text = "<color=red>" + number + "</color>";
        }
        else
        {
            childText.text = "" + number;
        }

    }
    
}
