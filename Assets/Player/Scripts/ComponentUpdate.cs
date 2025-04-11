using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentUpdate : MonoBehaviour
{
    private AbstractSceneManager ASM;
    private string type, title;
    [SerializeField] private Image highlight;
    public bool highlighted = false;

    public void Wake(AbstractSceneManager newASM)
    {
        ASM = newASM;

        type = gameObject.transform.parent.name;
        type = type.Replace("sGridUI", "");
        //Debug.Log(type);

        title = gameObject.name;
        title = title.Replace("GridElement", "");
        //Debug.Log(title);

        highlight.enabled = false;
        highlighted = false;
    }

    //scrap this bc removing hotbar
    /*public void ElementClicked()
    {
        //Debug.Log("Element Clicked: " + this.gameObject.name);
        highlight.enabled = true;
        highlighted = true;
        ASM.GetPlayerScript().AddComponent(type, title);
    }

    public void HighlightReset()
    {
        highlight.enabled = false;
        highlighted = false;
    }*/
}