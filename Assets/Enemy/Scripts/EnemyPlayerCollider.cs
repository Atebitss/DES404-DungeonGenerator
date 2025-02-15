using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerCollider : MonoBehaviour
{
    private bool playerDetected = false;
    public bool IsPlayerDetected() { return playerDetected; }


    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerDetected = true; }
    }

    private void OnTriggerExit(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerDetected = false; }
    }
}
