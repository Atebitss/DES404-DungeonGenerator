using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionColliderNear : MonoBehaviour
{
    private bool playerNear = false;
    public bool IsPlayerNear() { return playerNear; }

    private GameObject[] enemiesNear = new GameObject[100];
    public GameObject[] GetEnemiesNear() { return enemiesNear; }
    private int enemyCount = 0;
    private bool enemyNear = false;
    public bool IsEnemyNear() { return enemyNear; }

    private GameObject[] othersNear = new GameObject[500];
    public GameObject[] GetOthersNear() { return othersNear; }
    private int otherCount = 0;
    private bool otherNear = false;
    public bool IsOtherNear() { return otherNear; }


    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerNear = true; }
        if (col.gameObject.tag == "Enemy")
        {
            enemyNear = true;

            //add enemy to array
            for (int i = 0; i < enemiesNear.Length; i++)
            {
                if (enemiesNear[i] == null) //find first empty slot
                {
                    enemiesNear[i] = col.gameObject; //add enemy to array
                    break; //exit loop
                }
            }
            enemyCount++; //increase enemy count
        }
        if (col.gameObject.tag == "Repel" || col.gameObject.tag == "Compel")
        {
            //Debug.Log("Other detected: " + col.gameObject.name);
            otherNear = true;

            //add other to array
            for(int i = 0; i < othersNear.Length; i++)
            {
                if (othersNear[i] == null) //find first empty slot
                {
                    othersNear[i] = col.gameObject; //add other to array
                    break; //exit loop
                }
            }
            otherCount++; //increase other count
        }
    }

    private void OnTriggerExit(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerNear = false; }
        if (col.gameObject.tag == "Enemy") 
        {
            for (int i = 0; i < enemiesNear.Length; i++)
            {
                if (enemiesNear[i] == col.gameObject)
                {
                    enemiesNear[i] = null; //remove enemy from array
                    break; //exit loop
                }
            }
            enemyCount--; //decrease enemy count

            if (enemyCount <= 0) { enemyNear = false; }
        }
        if (col.gameObject.tag == "Repel" || col.gameObject.tag == "Compel")
        {
            if (othersNear.Length > 0)
            {
                //remove other from array
                for (int i = 0; i < othersNear.Length; i++)
                {
                    if (othersNear[i] == col.gameObject) //find other in array
                    {
                        othersNear[i] = null; //remove other from array
                        break; //exit loop
                    }
                }
                otherCount--; //decrease other count

                if (otherCount <= 0) { otherNear = false; }
            }
        }
    }
}
