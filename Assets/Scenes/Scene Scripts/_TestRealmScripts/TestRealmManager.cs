using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    void Awake()
    {
        SpawnPlayer(Vector3.zero);
        //Instantiate(enemyPrefab, new Vector3(0, 0, 5f), Quaternion.identity);
        int maxEnemies = 0;// = Random.Range(10, 10);
        for (int i = 0; i < maxEnemies; i++)
        {
            Instantiate(smallEnemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(5, 15)), Quaternion.identity);
        }
    }
}
