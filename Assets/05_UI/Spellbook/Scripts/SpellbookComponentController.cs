using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellbookComponentController : MonoBehaviour
{
    [SerializeField] private Image componentIconDisplay;
    [SerializeField] private TMP_Text componentInputDisplay;
    private Sprite componentIcon;

    public void Wake(string componentType, string inputType, int numPage)
    {
        Debug.Log("SpellbookComponentController: Wake called with componentType " + componentType + " and inputType " + inputType + " on page " + numPage);

        //display appropriate input based on input type
        componentInputDisplay.text = inputType;

        //display appropriate icon based on component type
        switch (numPage)
        {
            case 0:                              //---probably wrong path---
                componentIcon = Resources.Load<Sprite>("ComponentIcons/Shape" + componentType + "Icon");
                break;
            case 1:
                componentIcon = Resources.Load<Sprite>("ComponentIcons/Effect" + componentType + "Icon");
                break;
            case 2:
                componentIcon = Resources.Load<Sprite>("ComponentIcons/Element" + componentType + "Icon");
                break;
            default:
                Debug.Log("SpellbookComponentController: Invalid page number " + numPage + " provided.");
                break;
        }

        //load appropriate icon based on component type 
        if (componentType != null) { componentIconDisplay.sprite = componentIcon; }
    }
}
