using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    
    private enum ChildId
    {
        SETTLEMENT,
        CITY,
        RIPPLE_SYSTEM,
        PLAYER_FLAG_SHORT = 4,
        PLAYER_FLAG_LONG
    }
    
    private bool available = true;

    private bool rippleActive = false;
    private bool hasSettlement = false;
    private bool hasCity = false;

    // private GamePlayer owner = null;

    private GameObject rippleSystem;
    private GameObject settlement;
    private GameObject city;

    private GameObject shortFlagObject;
    private GameObject longFlagObject;

    private GameObject shortFlag;
    private GameObject longFlag;

    private PhotonView photonView;

    [SerializeField]
    private WorldPath[] surroundingPaths;

    private List<Intersection> neighbouringIntersections;

    private int surroundingPathsLength;

    private int ownerId = 0; // at the start, no one owns this intersection

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        surroundingPathsLength = surroundingPaths.Length;

        settlement = transform.GetChild((int)ChildId.SETTLEMENT).gameObject;
        city = transform.GetChild((int)ChildId.CITY).gameObject;
        rippleSystem = transform.GetChild((int) ChildId.RIPPLE_SYSTEM).gameObject;

        shortFlagObject = transform.GetChild((int)ChildId.PLAYER_FLAG_SHORT).gameObject;
        longFlagObject = transform.GetChild((int)ChildId.PLAYER_FLAG_LONG).gameObject;

        shortFlag = shortFlagObject.transform.GetChild(0).gameObject;
        longFlag = longFlagObject.transform.GetChild(0).gameObject;

        // Set up the neighbouring intersections list.
        neighbouringIntersections = new List<Intersection>();

        foreach (WorldPath path in surroundingPaths)
        {
            Intersection[] pathIntersections = path.GetIntersections();
            foreach (Intersection i in pathIntersections)
            {
                if (i != this)
                {
                    neighbouringIntersections.Add(i);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsAvailable()
    {
        return available;
    }

    public void SetAvailable(bool avail)
    {
        this.available = avail;
    }

    public bool HasSettlement()
    {
        return hasSettlement;
    }

    public bool HasCity()
    {
        return hasCity;
    }

    public int GetOwnerId()
    {
        return ownerId;
    }

    public List<Intersection> GetNeighbouringIntersections()
    {
        return neighbouringIntersections;
    }

    public void ToggleRipple()
    {
        if (!rippleActive)
        {
            rippleSystem.SetActive(rippleActive = true);
        }
        else
        {
            rippleSystem.SetActive(rippleActive = false);
        }
        
    }

    public void ConstructSettlement(int ownerId)
    {
       
        photonView.RPC("RPCConstructSettlement", RpcTarget.All, ownerId);
        
    }

    public void ConstructCity()
    {
        photonView.RPC("RPCConstructCity", RpcTarget.All);
    }

    [PunRPC]
    private void RPCConstructSettlement(int ownerId)
    {

        if (rippleActive)
        {
            rippleSystem.SetActive(rippleActive = false);
        }

        settlement.SetActive(true);
        
        this.ownerId = ownerId;

        available = false; // chain this to neighbouring intersections

        foreach (Intersection neighbour in neighbouringIntersections)
        {
            neighbour.SetAvailable(false);
        }

        hasSettlement = true;

        ShowShortFlag();
    }

    [PunRPC]
    private void RPCConstructCity()
    {

        settlement.SetActive(false);
        city.SetActive(true);

        hasSettlement = false;
        hasCity = true;

        HideShortFlag();
        ShowLongFlag();
    }

    public List<WorldPath> GetAvailablePaths()
    {
        List<WorldPath> list = new List<WorldPath>();
        foreach (WorldPath path in surroundingPaths)
        {
            if (path.IsAvailable())
            {
                list.Add(path);
            }
        }

        return list;
    }

    private void ShowShortFlag()
    {
        string materialPath = "Materials/PlayerMaterials/Player" + ownerId + "Material";

        ColorUtility.TryParseHtmlString(PhotonNetwork.CurrentRoom.GetPlayer(ownerId).CustomProperties["colour"] as string, out Color playerColour);

        shortFlag.GetComponent<SkinnedMeshRenderer>().material = Resources.Load(materialPath) as Material;
        shortFlag.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", playerColour);
        

        shortFlagObject.SetActive(true);
    }
    private void HideShortFlag()
    {
        shortFlagObject.SetActive(false);
    }

    private void ShowLongFlag()
    {
        string materialPath = "Materials/PlayerMaterials/Player" + ownerId + "Material";
        ColorUtility.TryParseHtmlString(PhotonNetwork.LocalPlayer.CustomProperties["colour"] as string, out Color playerColour);

        longFlag.GetComponent<SkinnedMeshRenderer>().material = Resources.Load(materialPath) as Material;
        longFlag.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", playerColour);

        longFlagObject.SetActive(true);
    }

    public bool OnHarbour(out HarbourPath.HarbourBonus? bonus)
    {
        bonus = null;

        foreach (WorldPath path in surroundingPaths)
        {
            if (path is HarbourPath)
            {
                bonus = ((HarbourPath)path).GetHarbourBonus();
                return true;
            }
        }

        return false;
    }
}
