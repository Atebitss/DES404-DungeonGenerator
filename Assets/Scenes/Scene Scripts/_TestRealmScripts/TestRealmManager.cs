using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    [SerializeField] GameObject smallEnemyPrefab, largeEnemtPrefab, bossBeserkPrefab;
    [SerializeField] GameObject curTestEnemyPrefab;
    [SerializeField] GameObject[] validEnemyTypes;
    void Start()
    {
        SpawnPlayer(Vector3.zero);
        PC.SetActive(true);

        //difficulty change based on skillScore
        //Debug.Log("final skillScore: " + skillScore);
        int maxEnemies = Random.Range(10, 10);
        Vector3[] enemyPositions = new Vector3[maxEnemies];
        GameObject[] enemyTypes = new GameObject[maxEnemies];
        for (int i = 0; i < maxEnemies; i++)
        {
            enemyPositions[i] = new Vector3(Random.Range(-5, 5), 0, Random.Range(5, 15));

            //alter chance to spawn large enemy appropriate to difficulty
            int largeEnemyChance = 25;
            switch (GetADM().GetDifficulty())
            {
                case -1:
                    largeEnemyChance = -1;
                    break;
                case 0:
                    largeEnemyChance = 25;
                    break;
                case 1:
                    largeEnemyChance = 50;
                    break;
                case 2:
                    largeEnemyChance = 75;
                    break;
                case 3:
                    largeEnemyChance = 101;
                    break;
                case 4:
                    largeEnemyChance = 101;
                    break;
                case 5:
                    largeEnemyChance = 101;
                    break;
            }

            if (Random.Range(0, 100) <= largeEnemyChance) { enemyTypes[i] = validEnemyTypes[1]; }
            else { enemyTypes[i] = validEnemyTypes[0]; }
        }

        SpawnEnemies(enemyTypes, enemyPositions);
    }
}
