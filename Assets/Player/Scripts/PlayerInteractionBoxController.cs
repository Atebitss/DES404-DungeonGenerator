using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerInteractionBoxController : MonoBehaviour
{
    private GameObject colObj;
    public GameObject GetColObj() { return colObj; }

    private void OnTriggerEnter(Collider col)
    {
        colObj = col.gameObject;
        //Debug.Log(colObj);
    }
}
