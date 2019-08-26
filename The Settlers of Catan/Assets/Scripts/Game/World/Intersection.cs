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
        RIPPLE_SYSTEM
    }
    
    private bool available = true;
    private bool rippleActive = false;
    private bool hasSettlement = false;
    private bool hasCity = false;

    // private GamePlayer owner = null;

    private GameObject rippleSystem;
    private GameObject settlement;
    private GameObject city;

    private PhotonView photonView;

    [SerializeField]
    private WorldPath[] surroundingPaths;

    private int surroundingPathsLength;

    private int ownerId = 0; // at the start, no one owns this intersection

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        surroundingPathsLength = surroundingPaths.Length;

        settlement = gameObject.transform.GetChild((int)ChildId.SETTLEMENT).gameObject;
        city = gameObject.transform.GetChild((int)ChildId.CITY).gameObject;
        rippleSystem = gameObject.transform.GetChild((int) ChildId.RIPPLE_SYSTEM).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsAvailable()
    {
        return available;
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
        hasSettlement = true;
        
    }

    [PunRPC]
    private void RPCConstructCity()
    {

        settlement.SetActive(false);
        city.SetActive(true);

        hasSettlement = false;
        hasCity = true;
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

}
