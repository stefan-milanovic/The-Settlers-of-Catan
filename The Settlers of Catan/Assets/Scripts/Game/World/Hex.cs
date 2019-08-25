using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hex : MonoBehaviour
{

    public enum Resource
    {
        BRICK,
        GRAIN,
        LUMBER,
        ORE,
        WOOL,
        NO_RESOURCE
    }


    private PhotonView photonView;

    // No resource set means it's a desert hex.
    private Resource resource;
    private int number;

    [SerializeField]
    private Intersection[] intersections;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Resource GetResource() { return resource; }
    public int GetNumber() { return number; }

    public void SetMaterial(string materialPrefix)
    {
        photonView.RPC("RPCSetMaterial", RpcTarget.All, materialPrefix);
    }

    public void SetNumber(int number)
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

        // Set resource

        resource = FindResource(materialPrefix);
    }

    [PunRPC]
    private void RPCSetNumber(int number)
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

        this.number = number;

    }

    private Resource FindResource(string materialPrefix)
    {
        Resource resource = Resource.NO_RESOURCE;

        switch (materialPrefix)
        {
            case "Hill": return Resource.BRICK;
            case "Field": return Resource.GRAIN;
            case "Forest": return Resource.LUMBER;
            case "Mountain": return Resource.ORE;
            case "Pasture": return Resource.WOOL;
        }

        return resource;
    }
    
    public int[] GenerateIncome()
    {
        int[] income = new int[4];

        // Deserts do not generate income.
        if (number == 7) return income;

        foreach (Intersection i in intersections)
        {
            if (i.HasSettlement() || i.HasCity())
            {
                int ownerId = i.GetOwnerId();

                income[ownerId - 1] += (i.HasSettlement() ? 1 : 2);
            }
        }

        return income;
    }
}
