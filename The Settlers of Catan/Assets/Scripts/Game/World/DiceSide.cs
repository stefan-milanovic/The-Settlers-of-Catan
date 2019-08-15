using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    private bool onGround;

    [SerializeField]
    private int sideValue; 

    public void OnTriggerStay(Collider col)
    {
        if (col.tag == "DiceGround")
        {
            onGround = true;
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.tag == "DiceGround")
        {
            onGround = false;
        }
    }

    public bool OnGround()
    {
        return onGround;
    }

    public int GetSideValue()
    {
        return sideValue;
    }
    
}
