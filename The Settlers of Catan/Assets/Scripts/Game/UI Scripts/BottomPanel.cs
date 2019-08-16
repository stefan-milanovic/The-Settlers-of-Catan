using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanel : MonoBehaviour
{

    private enum PanelCode
    {
        RESOURCES,
        CONSTRUCTION
    };

    PanelCode currentOpenPanel = PanelCode.RESOURCES;

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
        TogglePanel(PanelCode.RESOURCES);
    }

    public void OpenConstructionTab()
    {
        TogglePanel(PanelCode.CONSTRUCTION);
    }

    private void TogglePanel(PanelCode panelToOpen)
    {

        string prefix = FindPrefix(panelToOpen);

        string panelName = prefix + "Panel";
        string buttonTag = prefix + "Button";
        
        
        string oldPrefix = FindPrefix(currentOpenPanel);
        
        string oldPanelName = oldPrefix + "Panel";
        string oldButtonTag = oldPrefix + "Button";

        DisplayPanel(oldPanelName, false);
        ToggleButton(oldButtonTag, true);

        DisplayPanel(panelName, true);
        ToggleButton(buttonTag, false);

        currentOpenPanel = panelToOpen;
      
    }

    private string FindPrefix(PanelCode? panelToOpen)
    {
        switch (panelToOpen)
        {
            case PanelCode.RESOURCES: return "Resource";
            case PanelCode.CONSTRUCTION: return "Construction";
        }

        // In case of invalid PanelCode - should never occur
        return "";
    }

    private void DisplayPanel(string panelName, bool visibility)
    {
        GameObject panel = GameObject.Find(panelName);

        panel.GetComponent<CanvasGroup>().alpha = (visibility) ? 1 : 0;
    }

    private void ToggleButton(string buttonTag, bool interactable)
    {

        // Only toggles if it's a button from the main menu panel at the bottom of the screen.
        
        Button button = GameObject.FindGameObjectWithTag(buttonTag).GetComponent<Button>();
        
        ColorBlock colourBlock = button.colors;

        Color oldNormal = colourBlock.normalColor;
        colourBlock.normalColor = colourBlock.pressedColor;
        colourBlock.highlightedColor = oldNormal;
        colourBlock.pressedColor = oldNormal;
        
        button.colors = colourBlock;

        GameObject.FindGameObjectWithTag(buttonTag).GetComponent<Image>().color = colourBlock.normalColor;

        // Disable the button.
        button.interactable = interactable;

    }
}
