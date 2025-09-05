using UnityEngine;
using UnityEngine.UI;

public class SpellbookContentsPageController : MonoBehaviour
{
    //component references
    [SerializeField] private GameObject componentPrefab;
    [SerializeField] private GameObject[] contentsGrids = new GameObject[4];
    private GameObject[] southComponents = new GameObject[4];
    private SpellbookComponentController[] sSCCs = new SpellbookComponentController[4];
    private GameObject[] eastComponents = new GameObject[4];
    private SpellbookComponentController[] eSCCs = new SpellbookComponentController[4];
    private GameObject[] westComponents = new GameObject[4];
    private SpellbookComponentController[] wSCCs = new SpellbookComponentController[4];
    private GameObject[] northComponents = new GameObject[4];
    private SpellbookComponentController[] nSCCs = new SpellbookComponentController[4];

    //icon references
    [SerializeField] private Image[] componentIconDisplays = new Image[4];

    //data
    private string[] availableComponents;
    private int[,] componentInputs;
    private int pageNum = -1;

    private int[] southComponentIDs = new int[] {-1,-1,-1,-1};
    public int[] GetSouthComponentIDs() { return southComponentIDs; }

    private int[] eastComponentIDs = new int[] { -1, -1, -1, -1 };
    public int[] GetEastComponentIDs() { return eastComponentIDs; }

    private int[] westComponentIDs = new int[] { -1, -1, -1, -1 };
    public int[] GetWestComponentIDs() { return westComponentIDs; }

    private int[] northComponentIDs = new int[] { -1, -1, -1, -1 };
    public int[] GetNorthComponentIDs() { return northComponentIDs; }

    public void Wake(string[] availableComponents, int[,] componentInputs, int pageNum)
    {
        Debug.Log("SpellbookContentsPageController: Wake called with " + availableComponents.Length + " available components on page " + pageNum + ".");

        //update local data
        this.availableComponents = availableComponents;
        this.componentInputs = componentInputs;
        this.pageNum = pageNum;

        DisplayContentsPage();
    }
    private void DisplayContentsPage()
    {
        Debug.Log("Displaying contents page with " + availableComponents.Length + " contents.");

        int southCount = 0, eastCount = 0, westCount = 0, northCount = 0;

        //display input icons
        componentIconDisplays[0].sprite = Resources.Load<Sprite>("InputIcons/InputAIcon");
        componentIconDisplays[1].sprite = Resources.Load<Sprite>("InputIcons/InputBIcon");
        componentIconDisplays[2].sprite = Resources.Load<Sprite>("InputIcons/InputXIcon");
        componentIconDisplays[3].sprite = Resources.Load<Sprite>("InputIcons/InputYIcon");

        //display components in their respective grids
        for (int i = 0; i < availableComponents.Length; i++)
        {
            Debug.Log("Displaying component " + i + ", " + availableComponents[i] + " with input " + componentInputs[i, 0]);
            switch (componentInputs[i, 0])
            {
                case 0: //south (A)
                    //add component to south grid
                    Debug.Log("Placing component " + i + " in A grid.");

                    if (southComponents[southCount] == null)
                    {
                        southComponents[southCount] = Instantiate(componentPrefab, contentsGrids[0].transform);
                        sSCCs[southCount] = southComponents[southCount].GetComponent<SpellbookComponentController>();
                        sSCCs[southCount].Wake(availableComponents[i], "", pageNum);
                        southComponentIDs[southCount] = i;
                        southCount++;
                    }
                    break;
                case 1: //east (B)
                    //add component to east grid
                    Debug.Log("Placing component " + i + ", " + availableComponents[i] + " in B grid.");
                    if (eastComponents[eastCount] == null)
                    {
                        eastComponents[eastCount] = Instantiate(componentPrefab, contentsGrids[1].transform);
                        eSCCs[eastCount] = eastComponents[eastCount].GetComponent<SpellbookComponentController>();
                        eSCCs[eastCount].Wake(availableComponents[i], "", pageNum);
                        eastComponentIDs[eastCount] = i;
                        eastCount++;
                    }
                    break;
                case 2: //west (X)
                    //add component to west grid
                    Debug.Log("Placing component " + i + ", " + availableComponents[i] + " in X grid.");
                    if (westComponents[westCount] == null)
                    {
                        westComponents[westCount] = Instantiate(componentPrefab, contentsGrids[2].transform);
                        wSCCs[westCount] = westComponents[westCount].GetComponent<SpellbookComponentController>();
                        wSCCs[westCount].Wake(availableComponents[i], "", pageNum);
                        westComponentIDs[westCount] = i;
                        westCount++;
                    }
                    break;
                case 3: //north (Y)
                    //add component to north grid
                    Debug.Log("Placing component " + i + ", " + availableComponents[i] + " in Y grid.");
                    if (northComponents[northCount] == null)
                    {
                        northComponents[northCount] = Instantiate(componentPrefab, contentsGrids[3].transform);
                        nSCCs[northCount] = northComponents[northCount].GetComponent<SpellbookComponentController>();
                        nSCCs[northCount].Wake(availableComponents[i], "", pageNum);
                        northComponentIDs[northCount] = i;
                        northCount++;
                    }
                    break;
                default:
                    Debug.Log("Error: unknown input " + componentInputs[i, 0] + " for component " + i);
                    break;
            }
        }
    }
}
