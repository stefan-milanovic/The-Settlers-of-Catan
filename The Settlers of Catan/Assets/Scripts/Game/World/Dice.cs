using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{

    private Rigidbody rigidBody;

    private bool hasLanded;
    private bool thrown;

    private Vector3 initPosition;

    [SerializeField]
    private Vector3 throwPosition;

    private int diceValue = 0;

    [SerializeField]
    private DiceSide[] diceSides;

    public int getDiceValue()
    {
        return diceValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rigidBody.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
        }

        if (rigidBody.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rigidBody.useGravity = false;
            SideValueCheck();
        }
        else if (rigidBody.IsSleeping() && hasLanded && diceValue == 0)
        {
            RollAgain();
        }
    }


    private void RollDice()
    {

        // place dice into it's throw position
        
        if (!thrown && !hasLanded)
        {
            transform.localPosition += throwPosition;
            thrown = true;
            rigidBody.useGravity = true;
            rigidBody.AddTorque(Random.Range(500, 1000), Random.Range(300, 500), Random.Range(500, 1000));
        } else if (thrown && hasLanded)
        {
            Reset();
        }
    }

    private void Reset()
    {
        transform.position = initPosition;
        thrown = false;
        hasLanded = false;
        rigidBody.useGravity = false;
    }

    private void RollAgain()
    {
        Reset();
        thrown = true;
        rigidBody.useGravity = true;
        rigidBody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    private void SideValueCheck()
    {
        diceValue = 0;

        foreach (DiceSide diceSide in diceSides)
        {
            if (diceSide.OnGround())
            {
                diceValue = diceSide.GetSideValue();
                Debug.Log("Dice value: " + diceValue);
            }
        }
    }
}
