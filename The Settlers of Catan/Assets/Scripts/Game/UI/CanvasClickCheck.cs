using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasClickCheck : MonoBehaviour
{

    GraphicRaycaster raycaster;

    // Start is called before the first frame update
    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check if the left Mouse button is clicked
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Set up the new Pointer Event
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            pointerData.position = Input.mousePosition;
            this.raycaster.Raycast(pointerData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<Card>() != null)
                {
                    Card card = result.gameObject.GetComponent<Card>();
                    if (card.IsVisible())
                    {
                        card.OnCardClicked();
                    }
                    
                }
            }
        }
    }
}
