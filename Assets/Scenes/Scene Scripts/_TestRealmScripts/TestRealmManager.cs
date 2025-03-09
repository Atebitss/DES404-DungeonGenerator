using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    [SerializeField] GameObject smallEnemyPrefab, largeEnemtPrefab, bossBeserkPrefab;
    [SerializeField] GameObject curTestEnemyPrefab;
    void Awake()
    {
        SpawnPlayer(Vector3.zero);
        int maxEnemies = Random.Range(10, 10);
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy(curTestEnemyPrefab, new Vector3(Random.Range(-5, 5), 0, Random.Range(5, 15)));
        }
    }
}
