using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenResourcesTab()
    {
        CanvasGroup resourcesPanel = GameObject.Find("ResourcePanel").GetComponent<CanvasGroup>();
    }

    public void OpenConstructionTab()
    {

    }
}
