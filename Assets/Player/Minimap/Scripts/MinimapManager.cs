using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private Camera minimapCamera;
    public void Wake(int boundsX, int boundsZ)
    {
        //find the minimap camera in the scene
        minimapCamera = GameObject.Find("MinimapCamera").GetComponent<Camera>();
        minimapCamera.transform.position = new Vector3((boundsX / 2), (((boundsX / 2) + (boundsZ / 2)) * 0.8f), (boundsZ / 2));
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
