using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableVisualManager : MonoBehaviour
{
    [Header("-Consumable Visuals")]
    [SerializeField] private GameObject consumableVisualsParent;
    [SerializeField] private GameObject cvPrefab;
    [SerializeField] private Sprite[] consumableImages;

    private GameObject[] consumableVisuals = new GameObject[100];

    private float spawnOffset = 0f; //offset for spawning visuals

    public void ApplyHUDVisual(string consumableType, float consumableTime)
    {
        Debug.Log("ConsumableVisualManager, applying consumable visual: " + consumableType + " for " + consumableTime + " seconds");

        //if consumable type healing, skip
        if(consumableType == "Healing") { return; }
        
        //determine which consumable image to use
        Sprite consumableImage = null;
        switch (consumableType)
        {
            case "Invincibility":
                consumableImage = consumableImages[0];
                break;
            case "Movement":
                consumableImage = consumableImages[1];
                break;
            case "Rage":
                consumableImage = consumableImages[2];
                break;
            case "Strength":
                consumableImage = consumableImages[3];
                break;
            case "Toughness":
                consumableImage = consumableImages[4];
                break;
        }

        //instantiate new visual and set parent
        GameObject curCV = Instantiate(cvPrefab, transform);
        curCV.transform.SetParent(consumableVisualsParent.transform); //set parent to consumable visuals parent
        Debug.Log("ConsumableVisualManager, instantiated consumable visual: " + curCV.name);

        for (int i = 0; i < consumableVisuals.Length; i++)
        {
            if (consumableVisuals[i] == null)
            {
                consumableVisuals[i] = curCV; //add new visual to array
                break;
            }
        }

        //set the image of the visual
        curCV.transform.GetChild(0).GetComponent<Image>().sprite = consumableImage;

        //set the position of the visual based on the number of visuals
        if (spawnOffset == 0) { spawnOffset = (curCV.GetComponent<RectTransform>().rect.width + 10f); }
        curCV.transform.localPosition = new Vector3((spawnOffset * (consumableVisuals.Length - 1)), 0, 0);

        //destroy visual after consumable time
        StartCoroutine(DestroyVisual(curCV, consumableTime));
    }
    private IEnumerator DestroyVisual(GameObject trackedCV, float timer)
    {
        Debug.Log("ConsumableVisualManager, destroying consumable visual: " + trackedCV.name + " in " + timer + " seconds");
        yield return new WaitForSeconds(timer); //wait for timer
        Debug.Log("ConsumableVisualManager, destroying consumable visual: " + trackedCV.name);

        //remove visual from array
        for (int i = 0; i < consumableVisuals.Length; i++)
        {
            if (consumableVisuals[i] == trackedCV)
            {
                consumableVisuals[i] = null;
                break;
            }
        }
        Debug.Log("ConsumableVisualManager, removed consumable visual: " + trackedCV.name + " from array");

        OrginizeVisuals();
        Destroy(trackedCV.gameObject); //destroy the visual
        Debug.Log("ConsumableVisualManager, destroyed consumable visual: " + trackedCV.name);
    }
    private void OrginizeVisuals()
    {
        Debug.Log("ConsumableVisualManager, organizing consumable visuals");
        //update position of the remaining visuals
        for (int i = 0; i < consumableVisuals.Length; i++)
        {
            if (consumableVisuals[i] != null)
            {
                consumableVisuals[i].transform.localPosition = new Vector3((spawnOffset * i), 0, 0);
            }
        }
    }


    public void ResetVisuals()
    {
        Debug.Log("ConsumableVisualManager, resetting consumable visuals");
        //destroy all visuals
        for (int i = 0; i < consumableVisuals.Length; i++)
        {
            Destroy(consumableVisuals[i].gameObject);
        }

        consumableVisuals = new GameObject[100]; //reset array
    }
}
