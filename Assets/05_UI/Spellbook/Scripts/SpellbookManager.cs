using System.Collections;
using UnityEngine;


public class SpellbookManager : MonoBehaviour
{    
    //state
    private bool spellbookActive = false, processing = false;
    public bool GetSpellbookActive() { return spellbookActive; }
    private int curPageNum = 0; // 0=shapes, 1=effects, 2=elements
    private bool pageContentsDisplayed = false;
    private float pageAdvanceTime = 0f;
    private string selectedShape = "";
    private string selectedEffect = "";
    private string selectedElement = "";
    private WaitForSeconds WFS_pageAdvance = new WaitForSeconds(0.1f);

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

    //pages
    [SerializeField] private GameObject spellbookContentsPagePrefab, spellbookComponentsPagePrefab, spellbookTypePagePrefab;
    [SerializeField] private Canvas leftPageCanvas, rightPageCanvas;
    private SpellbookContentsPageController curSCnPC;
    private SpellbookComponentsPageController curSCmPC;
    private SpellbookTypePageController curSTPC;
    private GameObject curMainPage, curTypePage;

    //misc
    private PlayerController PC;


    public void Wake(PlayerController PC)
    {
        if (this.PC == null)
        {
            this.PC = PC;

            //initialize spellbook
            spellbookActive = false;
            curPageNum = 0;
            pageAdvanceTime = 0.5f;

            //initialise component inputs
            GenerateAllPossibleInputs();
        }
    }
    private void GenerateAllPossibleInputs()
    {
        //Debug.Log("Generating all possible inputs");

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
            //if (availableShapes.Length > 0) { //Debug.Log("Shape " + availableShapes[i] + " Input: " + shapeInputs[i, 0]); }
        }
        

        //generate effect inputs (16 possible 2-button combos)
        int comboIndex = 0;
        for (int button1 = 0; button1 < 4 && comboIndex < availableEffects.Length; button1++)
        {
            for (int button2 = 0; button2 < 4 && comboIndex < availableEffects.Length; button2++)
            {
                effectInputs[comboIndex, 0] = button1;
                effectInputs[comboIndex, 1] = button2;
                comboIndex++;
            }
        }

        //shuffle effect combinations
        for (int i = 0; i < (comboIndex - 1); i++)
        {
            //Debug.Log("Shuffling effect combo " + i);

            int randomIndex = Random.Range(i, comboIndex);
            int temp1 = effectInputs[i, 0];
            int temp2 = effectInputs[i, 1];
            effectInputs[i, 0] = effectInputs[randomIndex, 0];
            effectInputs[i, 1] = effectInputs[randomIndex, 1];
            effectInputs[randomIndex, 0] = temp1;
            effectInputs[randomIndex, 1] = temp2;

            //if (availableEffects.Length > 0) { //Debug.Log("Effect " + availableEffects[i] + " Input: " + effectInputs[i, 0] + "," + effectInputs[i, 1]); }
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

            //if (availableElements.Length > 0) { //Debug.Log("Element " + availableElements[i] + " Input: " + elementInputs[i, 0] + "," + elementInputs[i, 1]); }
        }
    }


    public void Held()
    {
        //Debug.Log("Spellbook Held");
        if (!spellbookActive && !processing)
        {
            //Debug.Log("Spellbook not active");
            //if the spellbook is not active, open it
            StartCoroutine(OpenSpellbook());
        }
    }
    public void Release()
    {
        //Debug.Log("Spellbook Released");
        if (spellbookActive)
        {
            //Debug.Log("Spellbook active");
            
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
        curPageNum = -1;
    }

    private IEnumerator OpenSpellbook()
    {
        //Debug.Log("Opening Spellbook");
        //run any initialization logic here
        ResetSpellCreation();
        processing = true;
        curPageNum = 0;

        //run book open animation
        DeterminePageDisplay();

        yield return WFS_pageAdvance; // simulate animation time
        spellbookActive = true;
        processing = false;
        //Debug.Log("Spellbook Opened");
    }
    private IEnumerator AdvancePage()
    {
        //Debug.Log("Advancing Page");

        //run any page transition logic here
        processing = true;

        if (pageContentsDisplayed) { pageContentsDisplayed = false; } //reset contents page flag if currently on contents page

        //create spell if passing last page
        if (curPageNum == 2)
        {
            CompleteSpell();
            yield break; // exit coroutine
        }

        //play page transition animation

        yield return WFS_pageAdvance; // simulate animation time
        curPageNum++;
        DeterminePageDisplay();
        processing = false;
        //Debug.Log("Page Advanced to: " + curPageNum);
    }
    private void DeterminePageDisplay()
    {
        //destroy existing page
        if(curMainPage != null) { Destroy(curMainPage); Destroy(curTypePage); }

        //determine what to display on the current page
        switch (curPageNum)
        {
            case 0: //shapes page
                //update left page to show type icon
                GenerateTypePage(0);

                //update right page to show components/contents
                if (availableShapes.Length == 0)
                {
                    //Debug.Log("No available shapes");
                }
                else if(availableShapes.Length > 4)
                {
                    //Debug.Log("More than 4 shapes, displaying Shapes Contents Page");
                    pageContentsDisplayed = true;
                    GenerateContentsPage(0);
                }
                else if(availableShapes.Length <= 4)
                {
                    //Debug.Log("Displaying Shapes Page");
                    pageContentsDisplayed = false;
                    GenerateComponentsPage(0);
                }
            break;
            case 1: //effects page
                //update left page to show type icon
                GenerateTypePage(1);

                //update right page to show components/contents
                if (availableEffects.Length == 0)
                {
                    //Debug.Log("No available effects");
                }
                else if (availableEffects.Length > 4)
                {
                    //Debug.Log("More than 4 effects, displaying effects contents page");
                    pageContentsDisplayed = true;
                    GenerateContentsPage(1);
                }
                else if (availableEffects.Length <= 4)
                {
                    //Debug.Log("Displaying effects page");
                    pageContentsDisplayed = false;
                    GenerateComponentsPage(1);
                }
            break;
            case 2: //elements page
                //update left page to show type icon
                GenerateTypePage(2);

                //update right page to show components/contents
                if (availableEffects.Length == 0)
                {
                    //Debug.Log("No available elements");
                }
                else if (availableElements.Length > 4)
                {
                    //Debug.Log("More than 4 elements, displaying elements contents page");
                    pageContentsDisplayed = true;
                    GenerateContentsPage(2);
                }
                else if (availableElements.Length <= 4)
                {
                    //Debug.Log("Displaying elements page");
                    pageContentsDisplayed = false;
                    GenerateComponentsPage(2);
                }
            break;
        }
    }
    private void GenerateTypePage(int pageNum)
    {
        //instantiate type page prefab
        curTypePage = Instantiate(spellbookTypePagePrefab, leftPageCanvas.transform);
        curSTPC = curTypePage.GetComponent<SpellbookTypePageController>();
        curSTPC.Wake(pageNum);
    }
    private void GenerateContentsPage(int pageNum)
    {
        //instantiate contents page prefab
        curMainPage = Instantiate(spellbookContentsPagePrefab, rightPageCanvas.transform); //---add left page later---
        curSCnPC = curMainPage.GetComponent<SpellbookContentsPageController>();

        switch (pageNum)
        {
            case 0: //shapes contents page
                //Debug.Log("Generating shapes contents page");
                curSCnPC.Wake(availableShapes, shapeInputs, 0);
            break;
            case 1: //effects contents page
                //Debug.Log("Generating effects contents page");
                curSCnPC.Wake(availableEffects, effectInputs, 1);
            break;
            case 2: //elements contents page
                //Debug.Log("Generating elements contents page");
                curSCnPC.Wake(availableElements, elementInputs, 2);
            break;
            default:
                //Debug.Log("Could not generate contents page, unknown pageNum: " + pageNum);
            break;
        }
    }
    private void GenerateComponentsPage(int pageNum)
    {
        //instantiate components page prefab
        curMainPage = Instantiate(spellbookComponentsPagePrefab, rightPageCanvas.transform); //---add left page later---
        curSCmPC = curMainPage.GetComponent<SpellbookComponentsPageController>();

        switch (pageNum)
        {
            case 0: //shapes components page
                //Debug.Log("Generating shapes components page");
                curSCmPC.Wake(availableShapes, shapeInputs, 0);
            break;
            case 1: //effects components page
                //Debug.Log("Generating effects components page");
                curSCmPC.Wake(availableEffects, effectInputs, 1);
            break;
            case 2: //elements components page
                //Debug.Log("Generating elements components page");
                curSCmPC.Wake(availableElements, elementInputs, 2);
            break;
            default:
                //Debug.Log("Could not generate component page, unknown pageNum: " + pageNum);
            break;
        }
    }
    private void GenerateContentsComponentsPage(int inputID)
    {
        //Debug.Log("Generating post-contents components page");

        int[] validIDs = new int[0];

        //get valid components and inputs from contents page
        switch (inputID)
        {
            case 0:
                //Debug.Log("Input A pressed, getting south components");
                validIDs = curSCnPC.GetSouthComponentIDs();
                break;
            case 1:
                //Debug.Log("Input B pressed, getting east components");
                validIDs = curSCnPC.GetEastComponentIDs();
                break;
            case 2:
                //Debug.Log("Input X pressed, getting west components");
                validIDs = curSCnPC.GetWestComponentIDs();
                break;
            case 3:
                //Debug.Log("Input Y pressed, getting north components");
                validIDs = curSCnPC.GetNorthComponentIDs();
                break;
            default:
                //Debug.Log("Unknown inputID: " + inputID);
                return;
        }


        string[] validComponents = new string[4];
        int[,] validInputs = new int[4, 1];

        switch(curPageNum)
        {
            case 0:
                for (int i = 0; i < validIDs.Length; i++)
                {
                    if (validIDs[i] != -1)
                    {
                        validComponents[i] = availableShapes[validIDs[i]];
                        validInputs[i, 0] = shapeInputs[validIDs[i], 1];
                    }
                }
                break;
            case 1:
                for (int i = 0; i < validIDs.Length; i++)
                {
                    if (validIDs[i] != -1)
                    {
                        validComponents[i] = availableEffects[validIDs[i]];
                        validInputs[i, 0] = effectInputs[validIDs[i], 1];
                    }
                }
                break;
            case 2:
                for (int i = 0; i < validIDs.Length; i++)
                {
                    if (validIDs[i] != -1)
                    {
                        validComponents[i] = availableElements[validIDs[i]];
                        validInputs[i, 0] = elementInputs[validIDs[i], 1];
                    }
                }
                break;
            default:
                //Debug.Log("Unknown curPageNum: " + curPageNum);
                break;
        }


        //destroy existing page
        if (curMainPage != null) { Destroy(curMainPage); }

        //instantiate components page prefab
        curMainPage = Instantiate(spellbookComponentsPagePrefab, rightPageCanvas.transform); //---add left page later---
        curSCmPC = curMainPage.GetComponent<SpellbookComponentsPageController>();
        curSCmPC.Wake(validComponents, validInputs, curPageNum);
    }

    private IEnumerator CloseSpellbook()
    {
        //Debug.Log("Closing Spellbook");
        //run any cleanup logic here
        ResetSpellCreation();
        processing = true;
        spellbookActive = false;

        //play closing animation
        DeterminePageDisplay();

        yield return WFS_pageAdvance; // simulate animation time
        processing = false;
        //Debug.Log("Spellbook Closed");
    }

    public void SpellbookInput(int inputID)
    {
        //Debug.Log("Spellbook Input: " + inputID);

        if (!processing)
        {
            //Debug.Log("Processing input: " + inputID);
            processing = true;

            //add input to current combo
            currentInputCombo += inputID;
            lastInputTime = Time.time;
            //Debug.Log("Current combo: " + currentInputCombo);

            // Check if combo matches any available component
            if (CheckForComboMatch(inputID))
            {
                //Debug.Log("Combo matched a component: " + currentInputCombo);
                // Found match, advance to next page
                currentInputCombo = "";
                StartCoroutine(AdvancePage());
            }
            else if (currentInputCombo.Length >= 3)
            {
                // Too many inputs without match, reset combo
                //Debug.Log("Combo too long, resetting: " + currentInputCombo);
                currentInputCombo = "";
            }
            else if(pageContentsDisplayed)
            {
                GenerateContentsComponentsPage(inputID);
            }
        }
    }
    private bool CheckForComboMatch(int newButtonID)
    {
        // Parse current combo into button array
        int[] buttons = new int[currentInputCombo.Length];
        for (int i = 0; i < buttons.Length; i++) { buttons[i] = int.Parse(currentInputCombo[i].ToString()); }
        //Debug.Log("Checking combo match for buttons: " + string.Join(",", buttons));

        switch (curPageNum)
        {
            case 0: //shapes page
                //Debug.Log("Checking shape inputs, buttons length: " + buttons.Length);
                if (buttons.Length == 1)
                {
                    for (int i = 0; i < availableShapes.Length; i++)
                    {
                        //Debug.Log("Comparing to shape input: " + shapeInputs[i, 0]);
                        if (shapeInputs[i, 0] == buttons[0])
                        {
                            selectedShape = availableShapes[i];
                            //Debug.Log("Selected Shape: " + selectedShape);
                            return true;
                        }
                    }
                }
                //else { Debug.Log("Shape input length not 1, cannot match"); }
                break;

            case 1: //effects page
                //Debug.Log("Checking effect inputs, buttons length: " + buttons.Length);
                if (buttons.Length == 2)
                {
                    for (int i = 0; i < availableEffects.Length; i++)
                    {
                        //Debug.Log("Comparing to effect input: " + effectInputs[i, 0] + "," + effectInputs[i, 1]);
                        if (effectInputs[i, 0] == buttons[0] && effectInputs[i, 1] == buttons[1])
                        {
                            selectedEffect = availableEffects[i];
                            //Debug.Log("Selected Effect: " + selectedEffect);
                            return true;
                        }
                    }
                }
                //else { Debug.Log("Effect input length not 2, cannot match"); }
                break;

            case 2: //elements page
                //Debug.Log("Checking element inputs, buttons length: " + buttons.Length);
                if (buttons.Length == 2)
                {
                    for (int i = 0; i < availableElements.Length; i++)
                    {
                        //Debug.Log("Comparing to element input: " + elementInputs[i, 0] + "," + elementInputs[i, 1]);
                        if (elementInputs[i, 0] == buttons[0] && elementInputs[i, 1] == buttons[1])
                        {
                            selectedElement = availableElements[i];
                            //Debug.Log("Selected Element: " + selectedElement);
                            return true;
                        }
                    }
                }
                //else { Debug.Log("Element input length not 2, cannot match"); }
                break;
            default:
                //Debug.Log("Unknown curPageNum: " + curPageNum);
            break;
        }

        processing = false;
        //Debug.Log("No match found for combo: " + currentInputCombo);
        return false;
    }

    private void CompleteSpell()
    {
        //Debug.Log("Spell Completed");

        PC.AssignSpell(selectedShape, selectedEffect, selectedElement);
    }



    public void AddAllAvailableComponents()
    {
        //Debug.Log("Adding all available components");
        availableShapes = new string[] { "Ball", "Beam", "Field" };
        //availableEffects = new string[] { "Arc", "Automatic", "Block", "Chain" };
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