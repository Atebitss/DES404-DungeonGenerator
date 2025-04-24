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

    private GameObject[] consumableVisuals = new GameObject[0];

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

        //increase the size of the array and add visual
        GameObject[] newCVs = new GameObject[consumableVisuals.Length + 1]; //increased array size
        for (int i = 0; i < consumableVisuals.Length; i++) { newCVs[i] = consumableVisuals[i]; } //copy old array to new array
        newCVs[newCVs.Length - 1] = curCV; //add the visual to the end of the array
        consumableVisuals = newCVs; //set the new array to the old one

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
        GameObject[] newTrackedCVs = new GameObject[consumableVisuals.Length - 1]; //decreased array size
        int trackedCVIndex = 0; //index of the tracked visual
        for (int i = 0; i < consumableVisuals.Length; i++)
        {
            if (consumableVisuals[i] == trackedCV) { continue; } //skip the visual to be destroyed
            newTrackedCVs[trackedCVIndex] = consumableVisuals[i]; //copy old array to new array
            trackedCVIndex++; //increase index
        }
        consumableVisuals = newTrackedCVs; //set the new array to the old one
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
            consumableVisuals[i].transform.localPosition = new Vector3((spawnOffset * i), 0, 0);
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

        consumableVisuals = new GameObject[0]; //reset array
    }
}
