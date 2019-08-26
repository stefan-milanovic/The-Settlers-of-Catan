using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPanel : MonoBehaviour
{

    [SerializeField]
    private Card[] cardsInPanel;

    private bool visible = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVisible(bool visible)
    {
        this.visible = visible;

        foreach(Card card in cardsInPanel)
        {
            card.SetVisible(visible);
        }
    }
}
