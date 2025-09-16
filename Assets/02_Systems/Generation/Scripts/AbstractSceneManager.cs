using UnityEditor;
using UnityEngine;
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

    [SerializeField] private bool playerSpellOverwrite = false;
    public bool GetPlayerSpellOverwrite() { return playerSpellOverwrite; }


    //prefabs
    [SerializeField] public GameObject amPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject doorPrefab;


    //camera references
    [SerializeField] public Camera playerCamera;
    [SerializeField] public Camera loadingCamera;
    [SerializeField] public Camera postLevelCamera;

    //enemy overwrite
    [SerializeField] [Range(0.1f,100f)] private float healthModifierOverwrite = 1.0f;
    [SerializeField] [Range(0.1f, 100f)] private float damageModifierOverwrite = 1.0f;
    [SerializeField] [Range(0.1f, 100f)] private float speedModifierOverwrite = 1.0f;
    [SerializeField] [Range(0.1f, 100f)] private float attackSpeedModifierOverwrite = 1.0f;
    [SerializeField] [Range(0, 100)] private int dualChanceOverwrite = 10;

    //player stats overwrite
    //health
    [SerializeField][Range(0.1f, 100f)] public float playerHealthOverwrite = 1.0f;

    //speed
    [SerializeField][Range(0.1f, 100f)] public float playerSpeedOverwrite = 1.0f;

    //dodge
    [SerializeField][Range(0.1f, 100f)] public float playerDodgeForceOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerDodgeDurationOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerDodgeDistanceOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerDodgeCooldownOverwrite = 1.0f;

    //melee
    [SerializeField][Range(0.1f, 100f)] public float playerMeleeDamageOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerMeleeAttackSpeedOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerLightMeleeAttackCooldownOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerLightMeleeAttackComboTimerMaxOverwrite = 1.0f;
    [SerializeField][Range(0.1f, 100f)] public float playerHeavyMeleeAttackCooldownOverwrite = 1.0f;

    //magic
    enum shapeVarient { Ball, Beam, Field };
    [SerializeField] private shapeVarient shapeType = shapeVarient.Ball;
    public string GetShapeType() { return shapeType.ToString(); }

    enum effectVarient { Arc, Automatic, Block, Chain, Charge, Delay, Explode, Grow, Homing, Link, Multicast, Null, Pierce, Repel, Split, Teleport };
    [SerializeField] private effectVarient effectType = effectVarient.Arc;
    public string GetEffectType() { return effectType.ToString(); }

    enum elementVarient { Electric, Fire, Force, Null, Water };
    [SerializeField] private elementVarient elementType = elementVarient.Electric;
    public string GetElementType() { return elementType.ToString(); }

    [SerializeField][Range(0.1f, 100f)] public float playerMagicAttackCooldownOverwrite = 1.0f;

    //misc
    [SerializeField][Range(0.1f, 100f)] public float playerLookSensitivityOverwrite = 1.0f;



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
            //PC.AssignSpell();
            playerCamera = player.transform.GetChild(0).transform.GetChild(0).GetComponent<Camera>();

            ADM.Wake(this);
            StaticOcclusionCulling.Compute();
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
    private GameObject[] enemyObjects = new GameObject[100];
    public GameObject[] GetEnemyObjects() { return enemyObjects; }
    public int GetNumOfEnemies()
    {
        int count = 0;
        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i] != null) { count++; }
        }
        return count;
    }


    public void SpawnEnemy(GameObject enemy, Vector3 position, bool active)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemy.name); } }
        int index = -1;

        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i] == null)
            {
                enemyObjects[i] = Instantiate(enemy, position, Quaternion.identity);
                index = i;
                break;
            }
        }

        GenerateEnemy(enemyObjects[index], active);

        if (!enemyObjects[index].name.Contains("boss")) { enemyObjects[index].name = "Enemy" + index; }
        else { enemyObjects[index].name = "Boss" + enemy.transform.GetChild(0).GetComponent<AbstractEnemy>().type; }
    }
    public void SpawnEnemies(GameObject[] enemies, Vector3[] positions, bool active)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemies"); } }

        //spawn new enemies and add to new array
        Debug.Log(enemies.Length + " enemies to spawn");
        for (int newEnemyIndex = 0; newEnemyIndex < (enemies.Length - 1); newEnemyIndex++)
        {
            if (enemies[newEnemyIndex] != null)
            {
                if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Spawning Enemy " + enemies[newEnemyIndex].name); }
                for (int enemyIndex = 0; enemyIndex < (enemyObjects.Length - 1); enemyIndex++)
                {
                    if (enemyObjects[enemyIndex] == null)
                    {
                        Debug.Log("Scene Manager: Spawning Enemy " + enemies[newEnemyIndex] + " at array position " + enemyIndex);
                        enemyObjects[enemyIndex] = Instantiate(enemies[newEnemyIndex], positions[newEnemyIndex], Quaternion.identity);
                        GenerateEnemy(enemyObjects[enemyIndex], active);
                        if (enemyObjects[enemyIndex].name.Contains("boss")) { enemyObjects[enemyIndex].name = "Boss" + enemyObjects[enemyIndex].transform.GetChild(0).GetComponent<AbstractEnemy>().type; }
                        else
                        {
                            enemyObjects[enemyIndex].name = "Enemy" + enemyIndex;
                            enemyObjects[enemyIndex].transform.GetChild(0).name = "EnemyCharacter" + enemyIndex;
                        }
                        break;
                    }
                }
            }
        }
    }
    private void GenerateEnemy(GameObject curEnemy, bool active)
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


        //testing
        if (devMode)
        {
            healthModifier = healthModifierOverwrite;
            damageModifier = damageModifierOverwrite;
            speedModifier = speedModifierOverwrite;
            attackSpeedModifier = attackSpeedModifierOverwrite;
            dualChance = dualChanceOverwrite;
        }


        //wake enemy
        curEnemyScript.Wake(this);
        //Debug.Log("Enemy awake: " + curEnemyScript.name);

        //set enemy stats
        //health
        //Debug.Log(healthModifier);
        int newHealth = Mathf.RoundToInt(curEnemyScript.GetMaxHealth() * healthModifier);
        curEnemyScript.SetMaxHealth(newHealth);
        curEnemyScript.FullHeal();

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

        //active state
        curEnemyScript.SetIsActive(active);

        //rotate to face player
        if (PC != null)
        {
            Vector3 direction = PC.gameObject.transform.position - curEnemy.transform.position;
            direction.y = 0; //keep rotation on the horizontal plane
            curEnemy.transform.rotation = Quaternion.LookRotation(direction);
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
        
        enemyObjects = new GameObject[100];
    }
    public void DestroyEnemy(GameObject enemy)
    {
        if (MG != null) { if (dbugMode) { MG.UpdateHUDDbugText("Scene Manager: Destroying Enemy " + enemy.name); } }
        //Debug.Log("removing enemy from array: " + enemy);
        //find index of enemy to remove
        for(int i = 0; i < enemyObjects.Length; i++)
        {
            //Debug.Log("enemyObjects" + i + " / " + enemyObjects.Length + ": " + enemyObjects[i]);
            if(enemyObjects[i] == enemy)
            {
                CGM.OnEnemyDeath(enemy.transform.GetChild(0).position);
                enemyObjects[i] = null;
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
