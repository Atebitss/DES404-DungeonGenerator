using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AbstractSceneManager : MonoBehaviour
{
    //debug info
    [SerializeField] private bool devMode = false;
    public bool isDevMode() { return devMode; }



    //prefabs
    [SerializeField] public GameObject amPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public GameObject doorPrefab;



    //audio manager
    public AudioManager AM;
    public void SetAudioManager(AudioManager newAM) { AM = newAM; }
    public AudioManager GetAudioManager() { return AM; }



    //player controller
    public GameObject player;
    public void SetPlayerObject(GameObject newPlayer) { Debug.Log("newPlayer: " + newPlayer); player = newPlayer; }
    public GameObject GetPlayerObject() { return player.transform.GetChild(0).gameObject; }

    public PlayerController PC;
    public void SetPlayerController (PlayerController newPC) {  PC = newPC; SetPlayerObject(newPC.gameObject); }
    public PlayerController GetPlayerController() { return PC; }
    public void SpawnPlayer(Vector3 pos)
    {
        player = Instantiate(playerPrefab, pos, Quaternion.identity);
        PC = player.transform.GetChild(0).gameObject.GetComponent<PlayerController>();
    }
    public void DestroyPlayer()
    {
        DestroyEnemyObjects(); 
        if(this.GetComponent<DungeonGeneration>() != null){ this.GetComponent<DungeonGeneration>().ResetDungeon(); }
        Destroy(player);
    }
    public Vector3 GetPlayerPosition() { if(PC != null) { return PC.transform.position; } else { return Vector3.zero; } }



    //enemy controller
    private GameObject[] enemyObjects = new GameObject[0];
    public GameObject[] GetEnemyObjects() { return enemyObjects; }

    public void SpawnEnemies(Vector3[] positions)
    {
        enemyObjects = new GameObject[positions.Length - 1];

        //spawn enemies
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            Debug.Log("spawning enemy" + i + " / " + enemyObjects.Length);
            enemyObjects[i] = Instantiate(enemyPrefab, positions[i], Quaternion.identity);
            Debug.Log("enemyObjects" + i + " / " + enemyObjects.Length + ": " + enemyObjects[i]);
            enemyObjects[i].transform.GetChild(0).GetComponent<AbstractEnemy>().ASM = this;
            enemyObjects[i].name = "Enemy" + i;
        }
    }

    public void DestroyEnemyObjects()
    {
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            Destroy(enemyObjects[i]);
        }
        
        enemyObjects = new GameObject[0];
    }
    public void DestroyEnemy(GameObject enemy)
    {
        Debug.Log("removing enemy from array: " + enemy);
        // Find index of enemy to remove
        int removeIndex = -1;
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            Debug.Log("enemyObjects" + i + " / " + enemyObjects.Length + ": " + enemyObjects[i]);
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
            int newArrayIndex = 0;
            
            // Copy all elements except the removed enemy
            for(int i = 0; i < enemyObjects.Length; i++)
            {
                if(i != removeIndex)
                {
                    newArray[newArrayIndex] = enemyObjects[i];
                    newArrayIndex++;
                }
            }
            
            // Update enemy objects array and destroy removed enemy
            enemyObjects = newArray;
        }

        Debug.Log("enemy objects: " + (enemyObjects.Length));
    }


    
    //when scene starts
    void Start()
    {
        AM = Instantiate(amPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<AudioManager>();
    }
}
