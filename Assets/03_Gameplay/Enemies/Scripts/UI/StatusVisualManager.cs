using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusVisualManager : MonoBehaviour
{
    [Header("-Status Visuals")]
    [SerializeField] private GameObject statusVisualsParent;
    [SerializeField] private GameObject svPrefab;
    [SerializeField] private Sprite[] statusImages;

    private GameObject[] statusVisuals = new GameObject[0];
    private float spawnOffset = 0f; //offset for spawning visuals

    private float[] statusTimers = new float[0];

    public void ApplyVisual(string statusType, float statusTime)
    {
        Debug.Log("StatusVisualManager, applying status visual: " + statusType + " for " + statusTime + " seconds");

                                 //~~~~~   ADD CHECK TO AVOID DUPLICATE VISUALS   ~~~~~~//

        //determine which status image to use
        Sprite statusImage = null;
        switch (statusType)
        {
            case "Burning":
                statusImage = statusImages[0];
                break;
            case "Forced":
                statusImage = statusImages[1];
                break;
            case "Shocked":
                statusImage = statusImages[2];
                break;
            case "Soaked":
                statusImage = statusImages[3];
                break;
            case "Linked":
                statusImage = statusImages[4];
                break;
            //dazed, blinded, frozen
        }
        Debug.Log("statusImage: " + statusImage);

        //instantiate new visual and set parent
        GameObject curSV = Instantiate(svPrefab, transform);
        curSV.transform.SetParent(statusVisualsParent.transform); //set parent to status visuals parent
        Debug.Log("StatusVisualManager, instantiated status visual: " + curSV.name);

        //increase the size of the array and add visual
        GameObject[] newSVs = new GameObject[statusVisuals.Length + 1]; //increased array size
        for (int i = 0; i < statusVisuals.Length; i++) { newSVs[i] = statusVisuals[i]; } //copy old array to new array
        newSVs[newSVs.Length - 1] = curSV; //add the visual to the end of the array
        statusVisuals = newSVs; //set the new array to the old one

        //increase the size of timer array and add new timer
        float[] newStatusTimers = new float[statusTimers.Length + 1];
        for (int i = 0; i < statusTimers.Length; i++)
        {
            newStatusTimers[i] = statusTimers[i];
        }
        newStatusTimers[newStatusTimers.Length - 1] = (Time.time + statusTime); //add new timer
        statusTimers = newStatusTimers;

        //set the image of the visual
        curSV.transform.GetChild(2).GetComponent<Image>().sprite = statusImage;

        //set the position of the visual based on the number of visuals
                                                                            //~~~~~ RECALCULATE SPAWN OFFSET ~~~~~//
        if (spawnOffset == 0) { spawnOffset = (curSV.GetComponent<RectTransform>().rect.width + 10f); }
        curSV.transform.localPosition = new Vector3((spawnOffset * (statusVisuals.Length - 1)), 0, 0);

        //destroy visual after status time
        StartCoroutine(DestroyVisual(curSV, statusTime));
    }
    private IEnumerator DestroyVisual(GameObject trackedCV, float timer)
    {
        Debug.Log("StatusVisualManager, destroying status visual: " + trackedCV.name + " in " + timer + " seconds");
        yield return new WaitForSeconds(timer); //wait for timer
        Debug.Log("StatusVisualManager, destroying status visual: " + trackedCV.name);

        //remove visual from array
        GameObject[] newTrackedCVs = new GameObject[statusVisuals.Length - 1]; //decreased array size
        float[] newStatusTimers = new float[statusTimers.Length - 1]; //decreased timer array size
        int trackedCVIndex = 0; //index of the tracked visual
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            if (statusVisuals[i] == trackedCV) { continue; } //skip the visual to be destroyed
            newTrackedCVs[trackedCVIndex] = statusVisuals[i]; //copy old array to new array
            newStatusTimers[trackedCVIndex] = statusTimers[i]; //copy corresponding timer
            trackedCVIndex++; //increase index
        }
        statusVisuals = newTrackedCVs; //set the new array to the old one
        statusTimers = newStatusTimers; //set the new timer array to the old one
        Debug.Log("StatusVisualManager, removed status visual: " + trackedCV.name + " from array");

        OrginizeVisuals();
        Destroy(trackedCV.gameObject); //destroy the visual
        Debug.Log("StatusVisualManager, destroyed status visual: " + trackedCV.name);
    }
    private void OrginizeVisuals()
    {
        Debug.Log("StatusVisualManager, organizing status visuals");
        //update position of the remaining visuals
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            statusVisuals[i].transform.localPosition = new Vector3((spawnOffset * i), 0, 0);
        }
    }


    private void FixedUpdate()
    {
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            if (Time.time <= statusTimers[i])
            {
                statusVisuals[i].transform.GetChild(3).GetComponent<TMP_Text>().text = "" + (statusTimers[i] - Time.time).ToString("#");
            }
        }
    }



    public void ResetVisuals()
    {
        Debug.Log("StatusVisualManager, resetting status visuals");
        //destroy all visuals
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            Destroy(statusVisuals[i].gameObject);
        }

        statusVisuals = new GameObject[0]; //reset array
    }
}
