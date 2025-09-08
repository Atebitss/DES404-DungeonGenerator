using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionColliderFar : MonoBehaviour
{
    private bool playerNear = false;
    public bool IsPlayerNear() { return playerNear; }

    private GameObject[] enemiesNear = new GameObject[100];
    private int enemyCount = 0;
    private bool enemyNear = false;
    public bool IsEnemyNear() { return enemyNear; }


    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerNear = true; }
        if (col.gameObject.tag == "Enemy") 
        {
            enemyNear = true; 

            //add enemy to array
            for(int i = 0; i < enemiesNear.Length; i++)
            {
                if (enemiesNear[i] == null) //find first empty slot
                {
                    enemiesNear[i] = col.gameObject; //add enemy to array
                    break; //exit loop
                }
            }
            enemyCount++; //increase enemy count
        }
    }

    private void OnTriggerExit(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerNear = false; }
        if (col.gameObject.tag == "Enemy") 
        {
            enemyNear = false;
            //remove enemy from array
            for (int i = 0; i < enemiesNear.Length; i++)
            {
                if (enemiesNear[i] == col.gameObject) //find enemy in array
                {
                    enemiesNear[i] = null; //remove enemy from array
                    break; //exit loop
                }
            }
            enemyCount--; //decrease enemy count
        }
    }
}
