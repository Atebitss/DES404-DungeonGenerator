using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    [SerializeField] GameObject smallEnemyPrefab, largeEnemtPrefab, bossBeserkPrefab;
    [SerializeField] GameObject curTestEnemyPrefab;
    [SerializeField] GameObject[] validEnemyTypes;
    [SerializeField] int maxEnemies = -1;
    [SerializeField] bool enemiesActive = true;
    //[SerializeField] enum TestRealmEnemyType { SmallEnemy, LargeEnemy, BossBerserk, Random };
    //[SerializeField] enum TestRealmEnemyNumber { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten };
    enum spawnVarient { Random, Line, Diagonal };
    [SerializeField] private spawnVarient spawnType = spawnVarient.Random;

    void Start()
    {
        SpawnPlayer(Vector3.zero);
        PC.SetActive(true);

        if (maxEnemies != -1)
        {
            switch (spawnType)
            {
                case spawnVarient.Random:
                    SpawnVarientRandom();
                    break;
                case spawnVarient.Line:
                    SpawnVarientLine();
                    break;
                case spawnVarient.Diagonal:
                    SpawnVarientDiagonal();
                    break;
            }
        }
    }

    private void SpawnVarientRandom()
    {
        Vector3[] enemyPositions = new Vector3[maxEnemies];
        GameObject[] enemyTypes = new GameObject[maxEnemies];
        for (int i = 0; i < maxEnemies; i++)
        {
            //for each enemy, randomly select a position and a type
            enemyPositions[i] = new Vector3(Random.Range(-15, 15), 0, Random.Range(5, 15)); 
            enemyTypes[i] = validEnemyTypes[0];
        }

        SpawnEnemies(enemyTypes, enemyPositions, enemiesActive);
    }

    private void SpawnVarientLine()
    {
        Vector3[] enemyPositions = new Vector3[maxEnemies];
        GameObject[] enemyTypes = new GameObject[maxEnemies];
        int lineZPosition = Random.Range(5, 15); //find random Z position for the line of enemies
        float spacing = 5f; //distance between enemies
        float totalWidth = (maxEnemies - 1) * spacing; //calculate total x length based on number of enemies
        float lineXStartPosition = -totalWidth / 2f; //calculate starting x position based on number of enemies
        for (int i = 0; i < maxEnemies; i++)
        {
            //for each enemy, place subsequent enemies in a line and assign type
            float lineXPosition = lineXStartPosition + i * spacing;
            enemyPositions[i] = new Vector3(lineXPosition * 2, 0, lineZPosition);
            enemyTypes[i] = validEnemyTypes[0];
        }

        SpawnEnemies(enemyTypes, enemyPositions, enemiesActive);
    }

    private void SpawnVarientDiagonal()
    {
        Vector3[] enemyPositions = new Vector3[maxEnemies];
        GameObject[] enemyTypes = new GameObject[maxEnemies];
        float spacing = 5f; //distance between each enemy along both axes

        //calculate total offset
        float totalOffset = (maxEnemies - 1) * spacing / 2f;
        float startX = -totalOffset;
        float startZ = 5f;

        for (int i = 0; i < maxEnemies; i++)
        {
            float xPos = startX + i * spacing;
            float zPos = startZ + i * spacing;
            enemyPositions[i] = new Vector3(xPos, 0, zPos);
            enemyTypes[i] = validEnemyTypes[0];
        }

        SpawnEnemies(enemyTypes, enemyPositions, enemiesActive);
    }
}
