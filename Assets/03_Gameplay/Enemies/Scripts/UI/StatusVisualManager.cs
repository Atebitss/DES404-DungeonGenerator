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

    private GameObject[] statusVisuals = new GameObject[10];
    private float spawnOffset = 0f; //offset for spawning visuals

    private float[] statusTimers = new float[10];

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
        for(int i = 0; i < statusVisuals.Length; i++)
        {
            if (statusVisuals[i] == null)
            {
                statusVisuals[i] = curSV;
                Debug.Log("StatusVisualManager, added status visual: " + curSV.name + " to array at index: " + i);
                break;
            }
        }

        //increase the size of timer array and add new timer
        for(int i = 0; i < statusTimers.Length; i++)
        {
            if (statusTimers[i] == 0f)
            {
                statusTimers[i] = Time.time + statusTime; //set timer to current time + status time
                Debug.Log("StatusVisualManager, added status timer: " + statusTimers[i] + " to array at index: " + i);
                break;
            }
        }

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
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            if (statusVisuals[i] == trackedCV)
            {
                statusVisuals[i] = null; //remove visual from array
                statusTimers[i] = 0f; //remove corresponding timer
                break;
            }
        }

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
            if (statusVisuals[i] != null)
            {
                statusVisuals[i].transform.localPosition = new Vector3((spawnOffset * i), 0, 0);
            }
        }
    }


    private void FixedUpdate()
    {
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            if (statusVisuals[i] != null)
            {
                if (Time.time <= statusTimers[i])
                {
                    statusVisuals[i].transform.GetChild(3).GetComponent<TMP_Text>().text = "" + (statusTimers[i] - Time.time).ToString("#");
                }
            }
        }
    }



    public void ResetVisuals()
    {
        Debug.Log("StatusVisualManager, resetting status visuals");
        //destroy all visuals
        for (int i = 0; i < statusVisuals.Length; i++)
        {
            if (statusVisuals[i] != null)
            {
                Destroy(statusVisuals[i].gameObject);
            }
        }
    }
}
