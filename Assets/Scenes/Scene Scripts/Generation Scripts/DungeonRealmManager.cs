using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRealmManager : AbstractSceneManager
{
    private GameObject[] enemyObjects = new GameObject[0];
    public GameObject[] GetEnemyObjects() { return enemyObjects; }

    override public void SpawnEnemies(Vector3[] positions)
    {
        Debug.Log("positions: " + positions.Length);
        //spawn enemies
        for(int i = 0; i < positions.Length; i++)
        {
            GameObject[] tempEnemyArray = new GameObject[enemyObjects.Length + 1];
            for(int enemyIndex = 0; enemyIndex < enemyObjects.Length; enemyIndex++)
            {
                tempEnemyArray[enemyIndex] = enemyObjects[enemyIndex];
            }
            enemyObjects = tempEnemyArray;
            enemyObjects[enemyObjects.Length - 1] = Instantiate(enemyPrefab, positions[i], Quaternion.identity);
        }
    }



    public void RemoveEnemyFromArray(GameObject enemy)
    {
        // Find index of enemy to remove
        int removeIndex = -1;
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            if(enemyObjects[i] == enemy)
            {
                removeIndex = i;
                break;
            }
        }

        // If enemy was found, create new smaller array without it
        if(removeIndex != -1)
        {
            GameObject[] newArray = new GameObject[enemyObjects.Length - 1];
            
            // Copy elements before the removed enemy
            for(int i = 0; i < removeIndex; i++)
            {
                newArray[i] = enemyObjects[i];
            }
            
            // Copy elements after the removed enemy
            for(int i = removeIndex + 1; i < enemyObjects.Length; i++) 
            {
                newArray[i - 1] = enemyObjects[i];
            }
            
            enemyObjects = newArray;
        }
    }
}