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

    private GameObject[] othersNear = new GameObject[0];
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
            GameObject[] tempEnemiesNear = new GameObject[enemiesNear.Length + 1]; //increase array size
            for (int i = 0; i < enemiesNear.Length; i++) { tempEnemiesNear[i] = enemiesNear[i]; } //copy old array
            enemiesNear = tempEnemiesNear; //assign new array
            enemiesNear[enemyCount] = col.gameObject; //add new enemy
            enemyCount++; //increase enemy count
        }
        if (col.gameObject.tag == "Repel" || col.gameObject.tag == "Compel")
        {
            Debug.Log("Other detected: " + col.gameObject.name);
            otherNear = true;

            //add other to array
            GameObject[] tempOthersNear = new GameObject[othersNear.Length + 1]; //increase array size
            for (int i = 0; i < othersNear.Length; i++) { tempOthersNear[i] = othersNear[i]; } //copy old array
            othersNear = tempOthersNear; //assign new array
            othersNear[otherCount] = col.gameObject; //add new other
            otherCount++; //increase other count
        }
    }

    private void OnTriggerExit(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { playerNear = false; }
        if (col.gameObject.tag == "Enemy") 
        {
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

            if (enemyCount <= 0) { enemyNear = false; }
        }
        if (col.gameObject.tag == "Repel" || col.gameObject.tag == "Compel")
        {
            //remove other from array
            GameObject[] tempOthersNear = new GameObject[othersNear.Length - 1]; //decrease array size
            int tempIndex = 0;
            for (int i = 0; i < othersNear.Length; i++) //copy old array except the other that left
            {
                if (othersNear[i] != col.gameObject)
                {
                    tempOthersNear[tempIndex] = othersNear[i];
                    tempIndex++;
                }
            }
            othersNear = tempOthersNear; //assign new array
            otherCount--; //decrease other count

            if (otherCount <= 0) { otherNear = false; }
        }
    }
}
