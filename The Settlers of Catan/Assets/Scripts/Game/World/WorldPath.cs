using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPath : MonoBehaviour
{

    private const int ROAD_CHILD_ID = 0;
    private const int BLINK_CHILD_ID = 1;

    private bool available = true;
    private bool blinkActive = false;

    private int ownerId;

    private GameObject road;
    private GameObject emissionObject;

    private PhotonView photonView;
    
    [SerializeField]
    private Intersection[] intersections;
    

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    protected void Init()
    {
        photonView = GetComponent<PhotonView>();
        road = gameObject.transform.GetChild(ROAD_CHILD_ID).gameObject;
        emissionObject = gameObject.transform.GetChild(BLINK_CHILD_ID).gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public Intersection[] GetIntersections() { return intersections; }

    // We return all paths connected to this path that are available. We do not count the 
    public List<WorldPath> GetAvailablePaths()
    {
        List<WorldPath> list = new List<WorldPath>();
        foreach (Intersection i in intersections)
        {
            list.AddRange(i.GetAvailablePaths());
        }

        return list;
    }

    public bool IsAvailable() { return available;  }

    public void ToggleBlink()
    {
        if (!blinkActive)
        {
            emissionObject.SetActive(blinkActive = true);
        }
        else
        {
            emissionObject.SetActive(blinkActive = false);
        }
    }

    public void ConstructRoad(int ownerId)
    {
        photonView.RPC("RPCConstructRoad", RpcTarget.All, ownerId);
    }

    [PunRPC]
    public void RPCConstructRoad(int ownerId)
    {
        if (blinkActive)
        {
            emissionObject.SetActive(blinkActive = false);
        }

        // Colour the road according to the owner's colour.
        string materialPath = "Materials/Paths/Player" + ownerId + "WoodPath";

        ColorUtility.TryParseHtmlString(PhotonNetwork.CurrentRoom.GetPlayer(ownerId).CustomProperties["colour"] as string, out Color playerColour);

        road.GetComponent<MeshRenderer>().material = Resources.Load(materialPath) as Material;
        road.GetComponent<MeshRenderer>().material.SetColor("_Color", playerColour);

        road.SetActive(true);
        this.ownerId = ownerId;

        available = false;
    }
}
