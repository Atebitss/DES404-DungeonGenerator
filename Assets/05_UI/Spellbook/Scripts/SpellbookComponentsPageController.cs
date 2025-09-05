using UnityEngine;

public class SpellbookComponentsPageController : MonoBehaviour
{
    //component references
    [SerializeField] private GameObject componentPrefab;
    [SerializeField] private GameObject componentGrid;
    private GameObject[] components = new GameObject[4];
    private SpellbookComponentController[] SCCs = new SpellbookComponentController[4];

    //data
    private string[] availableComponents;
    private int[,] componentInputs;
    private int pageNum = -1;

    public void Wake(string[] availableComponents, int[,] componentInputs, int pageNum)
    {
        Debug.Log("SpellbookContentsPageController: Wake called with " + availableComponents.Length + " available components.");

        //update local data
        this.availableComponents = availableComponents;
        this.componentInputs = componentInputs;
        this.pageNum = pageNum;

        DisplayComponentsPage();
    }
    private void DisplayComponentsPage()
    {
        Debug.Log("Displaying Components Page with " + availableComponents.Length + " components.");

        //instantiate and display components in spellbook page grid
        if(availableComponents.Length <= SCCs.Length)
        {
            Debug.Log("Filling component slots.");

            for (int i = 0; i < availableComponents.Length; i++)
            {
                string inputTranslation = "";
                for (int j = 0; j < componentInputs.GetLength(1); j++)
                {
                    switch(componentInputs[i, j])
                    {
                        case 0: //south (A)
                            inputTranslation += "A";
                            break;
                        case 1: //east (B)
                            inputTranslation += "B";
                            break;
                        case 2: //west (X)
                            inputTranslation += "X";
                            break;
                        case 3: //north (Y)
                            inputTranslation += "Y";
                            break;
                        default:
                            inputTranslation += "?";
                            break;
                    }
                    if (j < (componentInputs.GetLength(1) - 1)) { inputTranslation += ", "; }
                }

                if (components[i] == null && availableComponents[i] != null)
                {
                    components[i] = Instantiate(componentPrefab, componentGrid.transform);
                    SCCs[i] = components[i].GetComponent<SpellbookComponentController>();
                    SCCs[i].Wake(availableComponents[i], inputTranslation, pageNum);
                }
            }
        }
    }
}
