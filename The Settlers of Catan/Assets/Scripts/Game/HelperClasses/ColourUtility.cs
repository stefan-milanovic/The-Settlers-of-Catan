using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourUtility 
{

    //private static Color[] resourceColours =
    //{
    //    new Color(0.913f, 0.470f, 0.411f),
    //    new Color(0.701f, 0.439f, 0.125f),
    //    new Color(0, 0.329f, 0),
    //    new Color(0.474f, 0.278f, 0.290f),
    //    new Color(0.533f, 0.615f, 0)
    //};

    //public static Color GetResourceColour(Inventory.UnitCode resource)
    //{

    //     if (resource > Inventory.UnitCode.WOOL)
    //    {
    //        // Only happens if this function gets called for a non-resource parameter.
    //        return new Color(1f, 1f, 1f, 0f);
    //    }

    //    return resourceColours[(int)resource];
    //}

    private static string[] resourceColours =
    {
        "#E97869",
        "#B37020",
        "#005400",
        "#79474A",
        "#889D00"
    };

    private static string[] resourceNames = {
        "Brick",
        "Grain",
        "Lumber",
        "Ore",
        "Wool"
    };

    private static string[] developmentCardNames = {
        "Knight",
        "Expansion",
        "Year of Plenty",
        "Monopoly",
        "Victory Card"
    };


    public static string GetResourceText(Inventory.UnitCode resourceCode) {
        
        if (resourceCode > Inventory.UnitCode.WOOL)
        {
            // Only happens if this function gets called for a non-resource parameter.
            return "<invalid_resource_code>";
        }

        return "<color=" + resourceColours[(int) resourceCode] + ">" + resourceNames[(int)resourceCode] + "</color>";
    }
    public static string GetPlayerDisplayName(Player player)
    {
        return "<color=" + player.CustomProperties["colour"] + ">" + player.CustomProperties["username"] + "</color>";
    }

    public static string GetPlayerDisplayNameFromId(int playerId)
    {
        return "<color=" + PhotonNetwork.CurrentRoom.GetPlayer(playerId).CustomProperties["colour"] + ">" + PhotonNetwork.CurrentRoom.GetPlayer(playerId).CustomProperties["username"] + "</color>";
    }

    public static string GetDevelopmentText(Inventory.UnitCode cardCode)
    {
        return "<color=" + "black" + ">" + developmentCardNames[(int)cardCode - 5] + "</color>";
    }
}
