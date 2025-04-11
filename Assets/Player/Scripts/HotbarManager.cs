using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    private AbstractSceneManager ASM;

    private GameObject[] gridElements;
    [SerializeField] private GameObject gridElementPrefab;

    [SerializeField] private GameObject shapeHotbar;
    [SerializeField] private GameObject effectHotbar;
    [SerializeField] private GameObject elementHotbar;


    //everything hotbar
    /*private string[] availableShapes = new string[] { "Ball", "Cone", "Line", "Ring" };
    private string[] availableEffects = new string[] { "Arc", "Chain", "Explode", "Split" };
    private string[] availableElements = new string[] { "Electric", "Fire", "Force", "Water" };*/

    //testing hotbar
    private string[] availableShapes = new string[] { "Ball", "Line" };
    private string[] availableEffects = new string[] { "Arc", "Chain", "Explode" };
    private string[] availableElements = new string[] { "Electric", "Fire", "Force", "Water" };

    //adaptive hotbar
    /*private string[] availableShapes = new string[0];
    public void AddAvaiableShape(string shapeName)
    {
        Debug.Log("Adding shape: " + shapeName);
        string[] newAvailableShapes = new string[availableShapes.Length + 1];
        for (int i = 0; i < availableShapes.Length; i++)
        {
            newAvailableShapes[i] = availableShapes[i];
        }
        newAvailableShapes[availableShapes.Length] = shapeName;
        availableShapes = newAvailableShapes;

        UpdateHotbar();
    }
    private string[] availableEffects = new string[0];
    public void AddAvaiableEffect(string effectName)
    {
        Debug.Log("Adding effect: " + effectName);
        string[] newAvailableEffects = new string[availableEffects.Length + 1];
        for (int i = 0; i < availableEffects.Length; i++)
        {
            newAvailableEffects[i] = availableEffects[i];
        }
        newAvailableEffects[availableEffects.Length] = effectName;
        availableEffects = newAvailableEffects;

        UpdateHotbar();
    }
    private string[] availableElements = new string[0];
    public void AddAvaiableElement(string elementName)
    {
        Debug.Log("Adding element: " + elementName);
        string[] newAvailableElements = new string[availableElements.Length + 1];
        for (int i = 0; i < availableElements.Length; i++)
        {
            newAvailableElements[i] = availableElements[i];
        }
        newAvailableElements[availableElements.Length] = elementName;
        availableElements = newAvailableElements;

        UpdateHotbar();
    }*/



    public void Wake(AbstractSceneManager newASM)
    {
        ASM = newASM;
        UpdateHotbar();
    }

    private void UpdateHotbar()
    {
        //increase grid elements array size to fit all elements
        if(gridElements.Length != (availableShapes.Length + availableEffects.Length + availableElements.Length))
        {
            Debug.Log("Grid elements array size changed from " + gridElements.Length + " to " + (availableShapes.Length + availableEffects.Length + availableElements.Length));
            GameObject[] newGridElements = new GameObject[(availableShapes.Length + availableEffects.Length + availableElements.Length)];
            for (int i = 0; i < gridElements.Length; i++)
            {
                newGridElements[i] = gridElements[i];
            }
            gridElements = newGridElements;
        }

        PopulateShapesHotbar();
        PopulateEffectsHotbar();
        PopulateElementsHotbar();
    }

    private void PopulateShapesHotbar()
    {
        //for each shape in the available shapes array
        for (int shapeIndex = 0; shapeIndex < availableShapes.Length; shapeIndex++)
        {
            //for each grid element in the grid elements array
            for (int pos = 0; pos < gridElements.Length; pos++)
            {
                //if the shape is already in the hotbar
                if (gridElements[pos].name == availableShapes[shapeIndex] + "GridElement")
                {
                    //break loop and skip current shape
                    Debug.Log("Shape already in hotbar: " + availableShapes[shapeIndex]);
                    return;
                }
            }

            //otherwise, create a new grid element
            Debug.Log("Adding shape to hotbar: " + availableShapes[shapeIndex]);
            GameObject gridElement = Instantiate(gridElementPrefab, shapeHotbar.transform);
            gridElement.GetComponent<ComponentUpdate>().Wake(ASM);
            gridElement.name = availableShapes[shapeIndex] + "GridElement";
            gridElement.transform.Find("ButtonUIImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("ComponentIcons/Shape" + availableShapes[shapeIndex] + "Icon");
            gridElements[(gridElements.Length - 1)] = gridElement;
        }
    }

    private void PopulateEffectsHotbar()
    {
        //for each effect in the available effect array
        for (int effectIndex = 0; effectIndex < availableEffects.Length; effectIndex++)
        {
            //for each grid element in the grid elements array
            for (int pos = 0; pos < gridElements.Length; pos++)
            {
                //if the effect is already in the hotbar
                if (gridElements[pos].name == availableEffects[effectIndex] + "GridElement")
                {
                    //break loop and skip current effect
                    Debug.Log("Effect already in hotbar: " + availableEffects[effectIndex]);
                    return;
                }
            }

            //otherwise, create a new grid element
            GameObject gridElement = Instantiate(gridElementPrefab, effectHotbar.transform);
            gridElement.GetComponent<ComponentUpdate>().Wake(ASM);
            gridElement.name = availableEffects[effectIndex] + "GridElement";
            gridElement.transform.Find("ButtonUIImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("ComponentIcons/Effect" + availableEffects[effectIndex] + "Icon");
            gridElements[(gridElements.Length - 1)] = gridElement;
        }
    }

    private void PopulateElementsHotbar()
    {
        //for each element in the available element array
        for (int elementIndex = 0; elementIndex < availableElements.Length; elementIndex++)
        {
            //for each grid element in the grid elements array
            for (int pos = 0; pos < gridElements.Length; pos++)
            {
                //if the element is already in the hotbar
                if (gridElements[pos].name == availableElements[elementIndex] + "GridElement")
                {
                    //break loop and skip current element
                    Debug.Log("Element already in hotbar: " + availableElements[elementIndex]);
                    return;
                }
            }

            //otherwise, create a new grid element
            GameObject gridElement = Instantiate(gridElementPrefab, elementHotbar.transform);
            gridElement.GetComponent<ComponentUpdate>().Wake(ASM);
            gridElement.name = availableElements[elementIndex] + "GridElement";
            gridElement.transform.Find("ButtonUIImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("ComponentIcons/Element" + availableElements[elementIndex] + "Icon");
            gridElements[(gridElements.Length - 1)] = gridElement;
        }
    }



    //scrap this bc removing hotbar
    /*public void ResetButtonHighlight()
    {
        //Debug.Log("ResetButtonHighlight");
        for (int pos = 0; pos < gridElements.Length; pos++) { if (gridElements[pos].GetComponent<ComponentUpdate>().highlighted == true) { gridElements[pos].GetComponent<ComponentUpdate>().HighlightReset(); } }
    }*/
}