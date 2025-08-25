using UnityEngine;

public class SpellbookContentsPageController : MonoBehaviour
{
    [SerializeField] private SpellbookComponentController[] SCCs = new SpellbookComponentController[4];
    private string[] availableComponents;
    private int[,] componentInputs;

    public void Wake(string[] availableComponents, int[,] componentInputs)
    {
        Debug.Log("SpellbookContentsPageController: Wake called with " + availableComponents.Length + " available components.");

        //update local data
        this.availableComponents = availableComponents;
        this.componentInputs = componentInputs;

        DisplayContentsPage();
    }
    private void DisplayContentsPage()
    {
        Debug.Log("Displaying contents page with " + availableComponents.Length + " components.");

        //instantiate and display components in spellbook page contents grid

    }
}
