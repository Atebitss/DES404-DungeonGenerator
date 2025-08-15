using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class SpellbookManager : MonoBehaviour
{    
    //state
    private bool spellbookActive = false, processing = false;
    public bool GetSpellbookActive() { return spellbookActive; }
    private int currentPage = 0; // 0=shapes, 1=effects, 2=elements
    private float pageAdvanceTime = 0f;
    private string selectedShape = "";
    private string selectedEffect = "";
    private string selectedElement = "";

    //available components
    private string[] availableShapes = new string[0];
    private string[] availableEffects = new string[0];
    private string[] availableElements = new string[0];

    //component inputs
    private string currentInputCombo = "";
    private float lastInputTime = 0f;
    private int[,] shapeInputs = new int[5, 1];
    private int[,] effectInputs = new int[20, 2];
    private int[,] elementInputs = new int[5, 2];

    //misc
    private PlayerController PC;


    public void Wake(PlayerController PC)
    {
        if (this.PC == null)
        {
            this.PC = PC;

            //initialize spellbook
            spellbookActive = false;
            currentPage = 0;
            pageAdvanceTime = 0.5f;

            //initialise component inputs
            GenerateAllPossibleInputs();
        }
    }
    private void GenerateAllPossibleInputs()
    {
        Debug.Log("Generating all possible inputs");

        //generate shape inputs (4 possible single buttons)
        int[] allSingleButtons = { 0, 1, 2, 3 };

        //Fisher–Yates shuffle
        //https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        for (int i = 0; i < (allSingleButtons.Length - 1); i++)
        {
            int temp = allSingleButtons[i];
            int randomIndex = Random.Range(i, (allSingleButtons.Length - 1));
            allSingleButtons[i] = allSingleButtons[randomIndex];
            allSingleButtons[randomIndex] = temp;
        }

        //assign shuffled buttons to shape inputs
        for (int i = 0; i < shapeInputs.GetLength(0) && i < (allSingleButtons.Length - 1); i++)
        {
            shapeInputs[i, 0] = allSingleButtons[i];
            Debug.Log("Shape " + availableShapes[i] + " Input: " + shapeInputs[i, 0]);
        }


        //generate effect inputs (16 possible 2-button combos)
        int comboIndex = 0;
        for (int button1 = 0; button1 < 4 && comboIndex < effectInputs.GetLength(0); button1++)
        {
            for (int button2 = 0; button2 < 4 && comboIndex < effectInputs.GetLength(0); button2++)
            {
                effectInputs[comboIndex, 0] = button1;
                effectInputs[comboIndex, 1] = button2;
                comboIndex++;
            }
        }


        //shuffle effect combinations
        for (int i = 0; i < (comboIndex - 1); i++)
        {
            int randomIndex = Random.Range(i, comboIndex);
            int temp1 = effectInputs[i, 0];
            int temp2 = effectInputs[i, 1];
            effectInputs[i, 0] = effectInputs[randomIndex, 0];
            effectInputs[i, 1] = effectInputs[randomIndex, 1];
            effectInputs[randomIndex, 0] = temp1;
            effectInputs[randomIndex, 1] = temp2;

            Debug.Log("Effect " + availableEffects[i] + " Input: " + effectInputs[i, 0] + "," + effectInputs[i, 1]);
        }


        //generate element inputs
        comboIndex = 0;
        for (int button1 = 0; button1 < 4 && comboIndex < elementInputs.GetLength(0); button1++)
        {
            for (int button2 = 0; button2 < 4 && comboIndex < elementInputs.GetLength(0); button2++)
            {
                elementInputs[comboIndex, 0] = button1;
                elementInputs[comboIndex, 1] = button2;
                comboIndex++;
            }
        }

        //shuffle element combinations
        for (int i = 0; i < (comboIndex - 1); i++)
        {
            int randomIndex = Random.Range(i, comboIndex);
            int temp1 = elementInputs[i, 0];
            int temp2 = elementInputs[i, 1];
            elementInputs[i, 0] = elementInputs[randomIndex, 0];
            elementInputs[i, 1] = elementInputs[randomIndex, 1];
            elementInputs[randomIndex, 0] = temp1;
            elementInputs[randomIndex, 1] = temp2;

            Debug.Log("Element " + availableElements[i] + " Input: " + elementInputs[i, 0] + "," + elementInputs[i, 1]);
        }
    }


    public void Held()
    {
        //Debug.Log("Spellbook Held");
        if (!spellbookActive && !processing)
        {
            Debug.Log("Spellbook not active");
            //if the spellbook is not active, open it
            StartCoroutine(OpenSpellbook());
        }
    }
    public void Release()
    {
        //Debug.Log("Spellbook Released");
        if (spellbookActive)
        {
            Debug.Log("Spellbook active");
            
            //if the spellbook is active, close it
            StartCoroutine(CloseSpellbook());
        }
    }
    private void ResetSpellCreation()
    {
        currentInputCombo = "";
        selectedShape = "";
        selectedEffect = "";
        selectedElement = "";
        currentPage = 0;
    }

    private IEnumerator OpenSpellbook()
    {
        Debug.Log("Opening Spellbook");
        //run any initialization logic here
        ResetSpellCreation();
        processing = true;

        //play opening animation

        yield return new WaitForSeconds(pageAdvanceTime); // simulate animation time
        spellbookActive = true;
        processing = false;
        Debug.Log("Spellbook Opened");
    }
    private IEnumerator AdvancePage()
    {
        Debug.Log("Advancing Page");
        //run any page transition logic here
        processing = true;

        //create spell if passing last page
        if (currentPage == 2)
        {
            CompleteSpell();
            yield break; // exit coroutine
        }

        //play page transition animation

        yield return new WaitForSeconds(pageAdvanceTime); // simulate animation time
        currentPage++;
        processing = false;
        Debug.Log("Page Advanced to: " + currentPage);
    }
    private IEnumerator CloseSpellbook()
    {
        Debug.Log("Closing Spellbook");
        //run any cleanup logic here
        ResetSpellCreation();
        processing = true;
        spellbookActive = false;

        //play closing animation

        yield return new WaitForSeconds(pageAdvanceTime); // simulate animation time
        processing = false;
        Debug.Log("Spellbook Closed");
    }

    public void SpellbookInput(int inputID)
    {
        Debug.Log("Spellbook Input: " + inputID);

        if (!processing)
        {
            processing = true;

            //add input to current combo
            currentInputCombo += inputID;
            lastInputTime = Time.time;
            Debug.Log("Current combo: " + currentInputCombo);

            // Check if combo matches any available component
            if (CheckForComboMatch(inputID))
            {
                // Found match, advance to next page
                currentInputCombo = "";
                StartCoroutine(AdvancePage());
            }
            else if (currentInputCombo.Length >= 3)
            {
                // Too many inputs without match, reset combo
                Debug.Log("Combo too long, resetting: " + currentInputCombo);
                currentInputCombo = "";
            }
        }
    }
    private bool CheckForComboMatch(int newButtonID)
    {
        // Parse current combo into button array
        int[] buttons = new int[currentInputCombo.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = int.Parse(currentInputCombo[i].ToString());
        }

        switch (currentPage)
        {
            case 0: //shapes page
                if (buttons.Length == 1)
                {
                    for (int i = 0; i < availableShapes.Length; i++)
                    {
                        if (shapeInputs[i, 0] == buttons[0])
                        {
                            selectedShape = availableShapes[i];
                            Debug.Log("Selected Shape: " + selectedShape);
                            return true;
                        }
                    }
                }
                break;

            case 1: //effects page
                if (buttons.Length == 2)
                {
                    for (int i = 0; i < availableEffects.Length; i++)
                    {
                        if (effectInputs[i, 0] == buttons[0] && effectInputs[i, 1] == buttons[1])
                        {
                            selectedEffect = availableEffects[i];
                            Debug.Log("Selected Effect: " + selectedEffect);
                            return true;
                        }
                    }
                }
                break;

            case 2: //elements page
                if (buttons.Length == 2)
                {
                    for (int i = 0; i < availableElements.Length; i++)
                    {
                        if (elementInputs[i, 0] == buttons[0] && elementInputs[i, 1] == buttons[1])
                        {
                            selectedElement = availableElements[i];
                            Debug.Log("Selected Element: " + selectedElement);
                            return true;
                        }
                    }
                }
                break;
        }

        processing = false;
        return false;
    }

    private void CompleteSpell()
    {
        Debug.Log("Spell Completed");

        PC.AssignSpell(selectedShape, selectedEffect, selectedElement);
    }



    public void AddAllAvailableComponents()
    {
        Debug.Log("Adding all available components");
        availableShapes = new string[] { "Ball", "Beam", "Field" };
        availableEffects = new string[] { "Arc", "Automatic", "Block", "Chain", "Charge", "Delay", "Explode", "Grow", "Homing", "Link", "Multicast", "Pierce", "Repel", "Split", "Teleport" };
        availableElements = new string[] { "Electric", "Fire", "Force", "Water" };
    }
    public void AddAvailableShape(string shapeName)
    {
        string[] newShapeArray = new string[availableShapes.Length + 1];
        for (int i = 0; i < availableShapes.Length; i++)
        {
            newShapeArray[i] = availableShapes[i];
        }
        newShapeArray[newShapeArray.Length - 1] = shapeName;
        availableShapes = newShapeArray;
    }

    public void AddAvailableEffect(string effectName)
    {
        string[] newEffectArray = new string[availableEffects.Length + 1];
        for (int i = 0; i < availableEffects.Length; i++)
        {
            newEffectArray[i] = availableEffects[i];
        }
        newEffectArray[newEffectArray.Length - 1] = effectName;
        availableEffects = newEffectArray;
    }

    public void AddAvailableElement(string elementName)
    {
        string[] newElementArray = new string[availableElements.Length + 1];
        for (int i = 0; i < availableElements.Length; i++)
        {
            newElementArray[i] = availableElements[i];
        }
        newElementArray[newElementArray.Length - 1] = elementName;
        availableElements = newElementArray;
    }
}