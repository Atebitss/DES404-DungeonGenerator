using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class AbstractSceneManager : MonoBehaviour
{
    //debug info
    [SerializeField] private bool devMode = false;
    public bool GetDevMode() { return devMode; }



    //prefabs
    [SerializeField] public GameObject amPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject doorPrefab;



    //Generation Managers
    private MapGeneration MG;
    public MapGeneration GetMG() { if (MG != null) { return MG; } return null; }
    private DungeonGeneration DG;
    public DungeonGeneration GetDG() { if (DG != null) { return DG; } return null; }
    private PathGeneration PG;
    public PathGeneration GetPG() { if (PG != null) { return PG; } return null; }
    private AdaptiveDifficultyManager ADM;
    public AdaptiveDifficultyManager GetADM() { if (ADM != null) { return ADM; } return null; }
    private AdaptiveDifficultyDisplayManager ADDM;
    public void SetADDM(AdaptiveDifficultyDisplayManager newADDM) { ADDM = newADDM; }
    public AdaptiveDifficultyDisplayManager GetADDM() { if (ADDM != null) { return ADDM; } return null; }



    //audio manager
    public AudioManager AM;
    public void SetAudioManager(AudioManager newAM) { AM = newAM; }
    public AudioManager GetAudioManager() { return AM; }



    //player controller
    public GameObject player;
    public void SetPlayerObject(GameObject newPlayer) { Debug.Log("newPlayer: " + newPlayer); player = newPlayer; }
    public GameObject GetPlayerObject() { if (player != null) { return player.transform.GetChild(0).gameObject; } return null; }

    public PlayerController PC;
    public void SetPlayerController (PlayerController newPC) {  PC = newPC; SetPlayerObject(newPC.gameObject); }
    public PlayerController GetPlayerController() { return PC; }
    public void SpawnPlayer(Vector3 pos)
    {
        if (player == null)
        {

            //Debug.Log("Spawning player at: " + pos);
            if (MG != null) { if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Spawning Player"); } }
            player = Instantiate(playerPrefab, pos, Quaternion.identity);
            //Debug.Log(player.name);
            PC = player.transform.GetChild(0).gameObject.GetComponent<PlayerController>();
            SetADDM(PC.GetADDM());

            ADM.Wake(this);
        }
        else
        {
            Debug.Log("Player already exists");
        }
    }
    public void DestroyPlayer()
    {
        if (player != null)
        {
            Debug.Log("Destroying player");
            if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Destroying Player"); }
            if (ADM != null) { ADM.End(); }
            Destroy(player); 
        }
    }
    public Vector3 GetPlayerPosition() { if(PC != null) { return PC.transform.position; } else { return Vector3.zero; } }



    //enemy controller
    private GameObject[] enemyObjects = new GameObject[0];
    public GameObject[] GetEnemyObjects() { return enemyObjects; }


    public void SpawnEnemy(GameObject enemy, Vector3 position)
    {
        if (MG != null) { if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemy.name); } }
        int existingCount = enemyObjects.Length; //current number of enemies tracked
        int newCount = (existingCount + 1); //new enemies to add + cur

        GameObject[] newEnemyObjects = new GameObject[newCount]; //create a new array with the updated size
        for (int i = 0; i < existingCount; i++) { newEnemyObjects[i] = enemyObjects[i]; } //copy old data to new array

        //spawn new enemies and add to new array
        int index = existingCount;
        newEnemyObjects[index] = Instantiate(enemy, position, Quaternion.identity);

        if (!newEnemyObjects[index].name.Contains("boss")) { newEnemyObjects[index].name = "Enemy" + index; }
        else { newEnemyObjects[index].name = "Boss" + enemy.transform.GetChild(0).GetComponent<AbstractEnemy>().type; }

        enemyObjects = newEnemyObjects; //replace old array with new array
    }
    public void SpawnEnemies(GameObject[] enemies, Vector3[] positions)
    {
        if (MG != null) { if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemies"); } }
        int existingCount = enemyObjects.Length; //current number of enemies tracked
        int newCount = existingCount + enemies.Length; //new enemies to add + cur

        GameObject[] newEnemyObjects = new GameObject[newCount]; //create a new array with the updated size
        for (int i = 0; i < existingCount; i++) { newEnemyObjects[i] = enemyObjects[i]; } //copy old data to new array

        //spawn new enemies and add to new array
        for (int i = 0; i < enemies.Length; i++)
        {
            if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemies[i].name); }
            int index = existingCount + i;
            newEnemyObjects[index] = Instantiate(enemies[i], positions[i], Quaternion.identity);

            if (!newEnemyObjects[index].name.Contains("boss")) { newEnemyObjects[index].name = "Enemy" + index; }
            else { newEnemyObjects[index].name = "Boss" + newEnemyObjects[index].GetComponent<AbstractEnemy>().type; }
        }

        enemyObjects = newEnemyObjects; //replace old array with new array
    }

    public void DestroyEnemyObjects()
    {
        if (MG != null) { if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Destroying Enemies"); } }
        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i] != null) { Destroy(enemyObjects[i]); }
        }
        
        enemyObjects = new GameObject[0];
    }
    public void DestroyEnemy(GameObject enemy)
    {
        if (MG != null) { if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Destroying Enemy " + enemy.name); } }
        //Debug.Log("removing enemy from array: " + enemy);
        // Find index of enemy to remove
        int removeIndex = -1;
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            //Debug.Log("enemyObjects" + i + " / " + enemyObjects.Length + ": " + enemyObjects[i]);
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

        //Debug.Log("enemy objects: " + (enemyObjects.Length));
    }


    
    //when scene starts
    void Awake()
    {
        AM = Instantiate(amPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<AudioManager>();
        MG = this.gameObject.GetComponent<MapGeneration>();
        DG = this.gameObject.GetComponent<DungeonGeneration>();
        PG = this.gameObject.GetComponent<PathGeneration>();
        ADM = this.gameObject.GetComponent<AdaptiveDifficultyManager>();
    }

    public void EndFloor()
    {
        if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Floor Complete"); }
        //Debug.Log("Floor complete");
        //would contain scene change to post level
        //and update dungeon stats
        player.transform.GetChild(0).GetComponent<PlayerController>().SetActive(false); //disable player input
        NewFloor(); //generate new floor
    }
    private void NewFloor()
    {
        if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Scene Manager: Starting New Floor"); }
        //Debug.Log("Starting new floor");
        if (MG != null) { MG.ResetMap(); }
        if (DG != null) { DG.ResetDungeon(); }
        if (PG != null) { PG.ResetHallways(); }
        if (MG != null) { MG.RegenerateDungeon(); }
    }
    virtual public void RestartScene(){}
}
