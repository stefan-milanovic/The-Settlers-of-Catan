using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    private readonly string[] terrainPool =
    {
        "Forest", "Forest", "Forest", "Forest",
        "Pasture","Pasture","Pasture","Pasture",
        "Field", "Field", "Field", "Field",
        "Desert",
        "Hill", "Hill", "Hill",
        "Mountain", "Mountain","Mountain"
    };

    private readonly int[] numberPool =
    {
        2,
        3, 3,
        4, 4,
        5, 5,
        6, 6,
        8, 8,
        9, 9,
        10, 10,
        11, 11,
        12
    };

    private List<Hex> hexes = new List<Hex>();
    private PhotonView photonView;

    void Start()
    {

        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            return;
        }

        GameObject[] hexGOs = GameObject.FindGameObjectsWithTag("Hex");
        foreach (GameObject go in hexGOs)
        {
            hexes.Add(go.GetComponent<Hex>());
        }
        GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateBoard()
    {
        // Start hex generation.

        List<string> terrainPoolCopy = terrainPool.OfType<string>().ToList();
        List<int> numberPoolCopy = numberPool.OfType<int>().ToList();

        int terrainPoolSize = terrainPool.Length;
        int numberPoolSize = numberPool.Length;

        foreach (Hex hex in hexes)
        {
            // Generate random number in range.
            int terrainIndex = Random.Range(0, terrainPoolSize);
            string chosenTerrainName = terrainPoolCopy[terrainIndex];
            terrainPoolCopy.RemoveAt(terrainIndex);
            hex.SetMaterial(chosenTerrainName);
            terrainPoolSize--;


            if (chosenTerrainName == "Desert")
            {
                hex.SetNumber(7);
            }
            else
            {
                int numberIndex = Random.Range(0, numberPoolSize);
                int chosenNumber = numberPoolCopy[numberIndex];
                numberPoolCopy.RemoveAt(numberIndex);
                hex.SetNumber(chosenNumber);
                numberPoolSize--;
            }
            

        }
    }


}
