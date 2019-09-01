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
        9, 9,
        10, 10,
        11, 11,
        12
    };

    // Separate from the other numbers.
    private int sixes = 2;
    private int eights = 2;

    public readonly HarbourPath.HarbourBonus[] harbourBonusPool =
    {
        HarbourPath.HarbourBonus.THREE_TO_ONE,
        HarbourPath.HarbourBonus.TWO_TO_ONE_BRICK,
        HarbourPath.HarbourBonus.THREE_TO_ONE,
        HarbourPath.HarbourBonus.TWO_TO_ONE_GRAIN,
        HarbourPath.HarbourBonus.THREE_TO_ONE,
        HarbourPath.HarbourBonus.TWO_TO_ONE_LUMBER,
        HarbourPath.HarbourBonus.THREE_TO_ONE,
        HarbourPath.HarbourBonus.TWO_TO_ONE_ORE,
        HarbourPath.HarbourBonus.TWO_TO_ONE_WOOL
    };

    private List<Hex> hexes = new List<Hex>();
    private HarbourPath[] harbourPaths;
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

        harbourPaths = FindObjectsOfType<HarbourPath>();

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

        // Generate terrain.
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
        }

        // Exhaust sixes.
        while (sixes != 0)
        {
            do
            {
                Hex hex = RandomHexWithNoNumber();
                if (hex.NeighbourHasRedNumber() == false)
                {
                    hex.SetNumber(6);
                    sixes--;
                    break;
                }
            } while (true);
        }
        
        // Exhaust eights.
        while (eights != 0)
        {
            do
            {
                Hex hex = RandomHexWithNoNumber();
                if (hex.NeighbourHasRedNumber() == false)
                {
                    hex.SetNumber(8);

                    eights--;
                    break;
                }
            } while (true);

            
        }

        // Now exhaust the rest of the number pool randomly.
        foreach (Hex hex in hexes)
        {
            if (hex.GetNumber() == 0)
            {
                int numberIndex = Random.Range(0, numberPoolSize);
                int chosenNumber = numberPoolCopy[numberIndex];

                // Get first hex without a number.
                numberPoolCopy.RemoveAt(numberIndex);
                hex.SetNumber(chosenNumber);
                numberPoolSize--;
            }
        }
        

        // Generate harbour path bonuses.

        List<HarbourPath.HarbourBonus> harbourBonusPoolCopy = harbourBonusPool.OfType<HarbourPath.HarbourBonus>().ToList();

        int harbourPoolSize = harbourBonusPool.Length;

        foreach (HarbourPath harbourPath in harbourPaths)
        {
            int bonusIndex = Random.Range(0, harbourPoolSize);
            HarbourPath.HarbourBonus chosenBonus = harbourBonusPoolCopy[bonusIndex];

            harbourBonusPoolCopy.RemoveAt(bonusIndex);

            harbourPath.SetHarbourBonus(chosenBonus);

            harbourPoolSize--;
        }

        // Shuffle the development deck.

        GameObject.Find("DevelopmentCardDeck").GetComponent<DevelopmentCardDeck>().Init();
        
    }

    private Hex RandomHexWithNoNumber()
    {
        do
        {
            int randomIndex = Random.Range(0, hexes.Count - 1);

            if (hexes[randomIndex].GetNumber() == 0)
            {
                return hexes[randomIndex];
            }
        } while (true);
        
    }
}
