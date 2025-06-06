using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class AbstractSceneManager : MonoBehaviour
{
    //debug info
    [SerializeField] private bool devMode = false;
    public bool GetDevMode() { return devMode; }
    public void SetDevMode(bool newDevMode) { devMode = newDevMode; }

    [SerializeField] private bool dbugMode = false;
    public bool GetDbugMode() { return dbugMode; }

    [SerializeField] private bool visualMode = false;
    public bool GetVisualMode() { return visualMode; }

    [SerializeField] private bool regenMode = false;
    public bool GetRegenMode() { return regenMode; }


    //prefabs
    [SerializeField] public GameObject amPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject doorPrefab;


    //camera references
    [SerializeField] public Camera playerCamera;
    [SerializeField] public Camera loadingCamera;
    [SerializeField] public Camera postLevelCamera;


    //Generation Managers
    private MapGeneration MG;
    public MapGeneration GetMG() { if (MG != null) { return MG; } return null; }

    private DungeonGeneration DG;
    public DungeonGeneration GetDG() { if (DG != null) { return DG; } return null; }

    private PathGeneration PG;
    public PathGeneration GetPG() { if (PG != null) { return PG; } return null; }

    private AdaptiveDifficultyManager ADM;
    public AdaptiveDifficultyManager GetADM() { if (ADM != null) { return ADM; } return null; }

    private AdaptiveDifficultyDbugManager ADDM;
    public AdaptiveDifficultyDbugManager GetADDM() { if (ADDM != null) { return ADDM; } return null; }

    private ConsumableGenerationManager CGM;
    public ConsumableGenerationManager GetCGM() { if (CGM != null) { return CGM; } return null; }

    private PostLevelVisualManager PLVM;
    public PostLevelVisualManager GetPLVM() { if (PLVM != null) { return PLVM; } return null; }




    //audio manager
    public AudioManager AM;
    public void SetAudioManager(AudioManager newAM) { AM = newAM; }
    public AudioManager GetAudioManager() { return AM; }



    //player controller
    public GameObject player;
    public void SetPlayerObject(GameObject newPlayer) { player = newPlayer; }
    public GameObject GetPlayerObject() { if (player != null) { return player.transform.GetChild(0).gameObject; } return null; }
    public GameObject GetPlayerParent() { if (player != null) { return player; } return null; }

    public PlayerController PC;
    public PlayerController GetPlayerController() { return PC; }
    public void SpawnPlayer(Vector3 pos)
    {
        if (player == null)
        {
            //Debug.Log("Spawning player at: " + pos);
            if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Player"); } }
            player = Instantiate(playerPrefab, pos, Quaternion.identity);
            //Debug.Log(player.name);
            PC = player.transform.GetChild(0).gameObject.GetComponent<PlayerController>();
            ADDM = PC.GetADDM();
            PC.AssignSpell();
            playerCamera = player.transform.GetChild(0).transform.GetChild(0).GetComponent<Camera>();

            ADM.Wake(this);
        }
        else
        {
            //Debug.Log("Player already exists");
        }
    }
    public void DestroyPlayer()
    {
        if (player != null)
        {
            //Debug.Log("Destroying player");
            PLVM.SetVisualHeader("Death!");
            PLVM.UpdateVisualText();
            playerCamera.enabled = false;
            loadingCamera.enabled = false;
            postLevelCamera.enabled = true;
            if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Destroying Player"); }
            Destroy(player); 
        }
    }
    public Vector3 GetPlayerPosition() { if(PC != null) { return PC.transform.position; } else { return Vector3.zero; } }



    //enemy controller
    private GameObject[] enemyObjects = new GameObject[0];
    public GameObject[] GetEnemyObjects() { return enemyObjects; }


    public void SpawnEnemy(GameObject enemy, Vector3 position)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemy.name); } }
        int existingCount = enemyObjects.Length; //current number of enemies tracked
        int newCount = (existingCount + 1); //new enemies to add + cur

        GameObject[] newEnemyObjects = new GameObject[newCount]; //create a new array with the updated size
        for (int i = 0; i < existingCount; i++) { newEnemyObjects[i] = enemyObjects[i]; } //copy old data to new array

        //spawn new enemies and add to new array
        int index = existingCount;
        newEnemyObjects[index] = Instantiate(enemy, position, Quaternion.identity);
        GenerateEnemy(newEnemyObjects[index]);

        if (!newEnemyObjects[index].name.Contains("boss")) { newEnemyObjects[index].name = "Enemy" + index; }
        else { newEnemyObjects[index].name = "Boss" + enemy.transform.GetChild(0).GetComponent<AbstractEnemy>().type; }

        enemyObjects = newEnemyObjects; //replace old array with new array
    }
    public void SpawnEnemies(GameObject[] enemies, Vector3[] positions)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemies"); } }
        int existingCount = enemyObjects.Length; //current number of enemies tracked
        int newCount = existingCount + enemies.Length; //new enemies to add + cur

        GameObject[] newEnemyObjects = new GameObject[newCount]; //create a new array with the updated size
        for (int i = 0; i < existingCount; i++) { newEnemyObjects[i] = enemyObjects[i]; } //copy old data to new array

        //spawn new enemies and add to new array
        for (int i = 0; i < enemies.Length; i++)
        {
            if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemies[i].name); }
            int index = existingCount + i;
            newEnemyObjects[index] = Instantiate(enemies[i], positions[i], Quaternion.identity);
            GenerateEnemy(newEnemyObjects[index]);
            

            if (!newEnemyObjects[index].name.Contains("boss")) { newEnemyObjects[index].name = "Enemy" + index; }
            else { newEnemyObjects[index].name = "Boss" + newEnemyObjects[index].GetComponent<AbstractEnemy>().type; }
        }

        enemyObjects = newEnemyObjects; //replace old array with new array
    }
    private void GenerateEnemy(GameObject curEnemy)
    {
        if (MG != null) { MG.UpdateHUDDbugText("Scene Manager: Generating Enemy"); }
        //Debug.Log("Generating enemy: " + curEnemy.name);

        AbstractEnemy curEnemyScript = curEnemy.transform.GetChild(0).GetComponent<AbstractEnemy>();
        //Debug.Log("curEnemyScript: " + curEnemyScript.name);
        float healthModifier = 1.0f;
        float damageModifier = 1.0f;
        float speedModifier = 1.0f;
        float attackSpeedModifier = 1.0f;
        int dualChance = 10;

        //set modifiers accoring to ADDM difficulty
        switch (ADM.GetDifficulty())
        {
            case -1:
                healthModifier = 0.5f; //alter how much health an enemy has
                damageModifier = 0.5f; //alter how much damage an enemy does
                speedModifier = 0.5f; //alter how fast an enemy moves
                attackSpeedModifier = 0.5f; //alter how fast an enemy attacks
                dualChance = -1; //alter how likely an enemy is to be dual
                break;
            case 0:
                healthModifier = 0.75f;
                damageModifier = 0.75f;
                speedModifier = 0.75f;
                attackSpeedModifier = 0.75f;
                dualChance = 10;
                break;
            case 1:
                healthModifier = 1.0f;
                damageModifier = 1.0f;
                speedModifier = 1.0f;
                attackSpeedModifier = 1.0f;
                dualChance = 25;
                break;
            case 2:
                healthModifier = 1.25f;
                damageModifier = 1.25f;
                speedModifier = 1.25f;
                attackSpeedModifier = 1.25f;
                dualChance = 40;
                break;
            case 3:
                healthModifier = 1.5f;
                damageModifier = 1.5f;
                speedModifier = 1.5f;
                attackSpeedModifier = 1.5f;
                dualChance = 55;
                break;
            case 4:
                healthModifier = 1.75f;
                damageModifier = 1.75f;
                speedModifier = 1.75f;
                attackSpeedModifier = 1.75f;
                dualChance = 75;
                break;
            case 5:
                healthModifier = 2f;
                damageModifier = 2f;
                speedModifier = 2f;
                attackSpeedModifier = 2f;
                dualChance = 100;
                break;
        }


        //wake enemy
        curEnemyScript.Wake(this);
        //Debug.Log("Enemy awake: " + curEnemyScript.name);

        //set enemy stats
        //health
        int newHealth = Mathf.RoundToInt(curEnemyScript.GetMaxHealth() * healthModifier);
        curEnemyScript.SetMaxHealth(newHealth);

        //damamge
        int newAttackDamage = Mathf.RoundToInt(curEnemyScript.GetAttackDamage() * damageModifier);
        curEnemyScript.SetAttackDamage(newAttackDamage);

        //movement speed
        curEnemyScript.SetMovementSpeed((curEnemyScript.GetMovementSpeed() * speedModifier));

        //attack speed
        curEnemyScript.SetAttackSpeed((curEnemyScript.GetAttackSpeed() * attackSpeedModifier));

        //dual chance
        if(Random.Range(0, 100) <= dualChance && !curEnemyScript.GetDual())
        {
            curEnemyScript.SetDual(true);
        }
    }

    public void DestroyEnemyObjects()
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Destroying Enemies"); } }

        GameObject[] linkedEnemies = PC.GetLinkedEnemies();
        if(linkedEnemies != null)
        {
            for (int i = 0; i < linkedEnemies.Length; i++)
            {
                PC.RemoveLinkedEnemy(linkedEnemies[i]);
            }
        }

        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i] != null) { Destroy(enemyObjects[i]); }
        }
        
        enemyObjects = new GameObject[0];
    }
    public void DestroyEnemy(GameObject enemy)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Destroying Enemy " + enemy.name); } }
        //Debug.Log("removing enemy from array: " + enemy);
        //find index of enemy to remove
        int removeIndex = -1;
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            //Debug.Log("enemyObjects" + i + " / " + enemyObjects.Length + ": " + enemyObjects[i]);
            if(enemyObjects[i] == enemy)
            {
                CGM.OnEnemyDeath(enemy.transform.GetChild(0).position);
                removeIndex = i;
                break;
            }
        }

        GameObject[] linkedEnemies = PC.GetLinkedEnemies();
        if (linkedEnemies != null)
        {
            for (int i = 0; i < linkedEnemies.Length; i++)
            {
                if (linkedEnemies[i] == enemy) { PC.RemoveLinkedEnemy(linkedEnemies[i]); }
            }
        }

        //if enemy was found, create new smaller array without it
        if (removeIndex != -1)
        {
            GameObject[] newArray = new GameObject[enemyObjects.Length - 1];
            int newArrayIndex = 0;
            
            //copy all elements except the removed enemy
            for(int i = 0; i < enemyObjects.Length; i++)
            {
                if(i != removeIndex)
                {
                    newArray[newArrayIndex] = enemyObjects[i];
                    newArrayIndex++;
                }
            }
            
            //update array
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
        CGM = this.gameObject.GetComponent<ConsumableGenerationManager>();
        PLVM = this.gameObject.GetComponent<PostLevelVisualManager>();
        if (PLVM) { PLVM.Wake(this); }

        dbugMode = GetDbugMode();
        visualMode = GetVisualMode();

        if (postLevelCamera) { postLevelCamera.enabled = false; }
        if (loadingCamera) { loadingCamera.enabled = true; }
    }
    void Start()
    {
        NewFloor();
    }

    public void EndFloor()
    {
        if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Floor Complete"); }
        //Debug.Log("Floor complete");
        //would contain scene change to post level

        //reset consumable visuals
        if (PC.GetCVM() != null) { PC.GetCVM().ResetVisuals(); }

        //and update dungeon stats
        PC.SetActive(false); //disable player input
        PC.ToggleHUD(false); //disable player hud

        //swap main camera to loading camera
        playerCamera.enabled = false;
        loadingCamera.enabled = false;
        postLevelCamera.enabled = true;

        PLVM.SetVisualHeader("Floor Cleared!");
        PLVM.UpdateVisualText();

        NewFloor(); //generate new floor
    }
    private void NewFloor()
    {
        if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Starting New Floor"); }
        //Debug.Log("Starting new floor");
        if (MG != null) { MG.ResetMap(); }
        if (DG != null) { DG.ResetDungeon(); }
        if (PG != null) { PG.ResetHallways(); }
        if (MG != null) { MG.RegenerateDungeon(); }
    }
    virtual public void RestartScene(){}
}
