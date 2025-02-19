using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionColliderNear : MonoBehaviour
{
    private bool playerNear = false;
    public bool IsPlayerNear() { return playerNear; }

    private GameObject[] enemiesNear = new GameObject[0];
    public GameObject[] GetEnemiesNear() { return enemiesNear; }
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
            GameObject[] tempEnemiesNear = new GameObject[enemiesNear.Length + 1]; //increase array size
            for (int i = 0; i < enemiesNear.Length; i++) { tempEnemiesNear[i] = enemiesNear[i]; } //copy old array
            enemiesNear = tempEnemiesNear; //assign new array
            enemiesNear[enemyCount] = col.gameObject; //add new enemy
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
            GameObject[] tempEnemiesNear = new GameObject[enemiesNear.Length - 1]; //decrease array size
            int tempIndex = 0;
            for (int i = 0; i < enemiesNear.Length; i++) //copy old array except the enemy that left
            {
                if (enemiesNear[i] != col.gameObject) 
                {
                    tempEnemiesNear[tempIndex] = enemiesNear[i];
                    tempIndex++; 
                }
            } 

            enemiesNear = tempEnemiesNear; //assign new array
            enemyCount--; //decrease enemy count
        }
    }
}
