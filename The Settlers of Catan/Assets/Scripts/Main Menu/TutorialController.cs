using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{

    private const int PAGE_COUNT = 10;

    [SerializeField]
    private TextMeshProUGUI tutorialText;

    [SerializeField]
    private TextMeshProUGUI currentPageText;

    [SerializeField]
    private Button backButton;

    [SerializeField]
    private Button forwardButton;

    private readonly string[] tutorialPages =
    {
        "The island of Catan lies before you. The isle consists of 19 terrain hexes surrounded by ocean. Your goal is to settle the island and expand your territory to become the largest and most glorious in Catan.",
        "There are five productive terrain types and one desert on Catan. Each terrain type produces a different type of resource (card). The desert produces nothing.",
        "You begin the game with two settlements and two roads. Each settlement is worth 1 victory point. You therefore start the game with 2 victory points! The first player to acquire 10 victory points on his/her turn wins the game.",
        "To gain more victory points, you must build new roads and settlements or upgrade settlements into cities. Each city is worth 2 victory points. To build or upgrade, you need to acquire resources.",
        "Acquiring resources is very simple. Each turn, a dice roll determines which terrain hexes (indicated by the number above them) produce resources. If, for example, a \"5\" is rolled, the two terrain hexes containing the \"5\" produce resources.",
        "You only collect resources if you own a settlement or a city bordering a terrain hex producing a resource.",
        "Since settlements and cities usually border on 2-3 terrain types, they can \"harvest\" up to 3 different resources based on the dice roll. Since the map layout is random in every game, you must always assess the situation carefully.",
        "Since you rarely have settlements everywhere as the game starts or progresses, you may have to do without certain resources. This is tough, for building requires specific resource combinations. For this reason, you can trade with other players. Make them an offer! A successful trade might yield a big build!",
        "You can only build a new settlement on an unoccupied intersection if you have a road leading to that intersection and the nearest settlement is at least two intersections away.",
        "Carefully consider where you build settlements! There is a higher chance that numbers closer to the number 7 (which has the highest chance to be rolled) will be rolled. Use this to your advantage!"
    };

    private int currentPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        backButton.interactable = false;
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText()
    {
        tutorialText.text = tutorialPages[currentPage];
        currentPageText.text = (currentPage + 1) + "/" + PAGE_COUNT;
    }

    public void Back()
    {

        currentPage--;

        if (!forwardButton.interactable)
        {
            forwardButton.interactable = true;
        }

        if (currentPage == 0)
        {
            backButton.interactable = false;
        }

        UpdateText();
    }

    public void Forward()
    {

        currentPage++;

        if (!backButton.interactable)
        {
            backButton.interactable = true;
        }

        if (currentPage == PAGE_COUNT - 1)
        {
            forwardButton.interactable = false;
        }

        UpdateText();
    }
}
