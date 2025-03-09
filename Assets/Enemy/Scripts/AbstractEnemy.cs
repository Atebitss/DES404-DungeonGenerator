using UnityEngine;
using System.Collections.Generic;
public abstract class AbstractEnemy : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Rigidbody enemyRigid; //enemy rigidbody used for physics interactions
    [SerializeField] private Animator a; //player animator used for running animations

    [HideInInspector] public AbstractSceneManager ASM;
    [HideInInspector] public AudioManager AM;
    [HideInInspector] public PlayerController PC;


    private float GetCurAnimLength()
    {
        //find legnth of current animation
        foreach (AnimationClip clip in a.runtimeAnimatorController.animationClips) { if (a.GetCurrentAnimatorStateInfo(0).IsName(clip.name)) { return clip.length; } }
        return -1;
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~state updates~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public string type = "";
    public bool boss = false;
    public bool dual = false;
    private void Start()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = ASM.GetAudioManager();
        PC = ASM.GetPlayerController();
        for (int i = 0; i < weaponAttackColliders.Length; i++)
        {
            EWCMs[i].SetAM(AM);
            EWCMs[i].SetWeaponDamage(attackDamage);
        }
        //Debug.Log("AE.setBool, dual: " + dual);
        if (boss) { a.SetBool("dual", dual); }
    }
    private void FixedUpdate()
    {
        UpdateEnemyStates();

        if (boss) { UpdateBossStates(); }

        if (isActive && PC != null)
        {
            UpdateEnemyLooking();
            UpdateEnemyMovement();
        }
    }
    virtual public void UpdateBossStates() {}
    private void UpdateEnemyStates()
    {
        HealthCheck();


        if (attackCooldownTimer > 0 && !attacking)
        {
            attacking = true; //set tracker
            attackStartTime = Time.time; //track when attack started
        }

        if (attackCooldownTimer > 0) { attackCooldownTimer -= Time.deltaTime; } //count down timer
        if (attackCooldownTimer <= 0) //when timer runs out, set tracker false
        {
            attackCooldownTimer = 0;
            attackStartTime = 0;
            attacking = false;
        }
    }
    //~~~~~state updates~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int newHealth) { health = newHealth; HealthCheck(); }
    public void AlterHealth(int change) { /*Debug.Log("altering health: " + change);*/ health += change; HealthCheck(); }
    private void HealthCheck() { /*Debug.Log("health check: " + health);*/ if (health <= 0) { ASM.DestroyEnemy(this.transform.parent.gameObject); Destroy(this.transform.parent.gameObject); } }
    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Attacking")]
    //attack
    [HideInInspector] public bool attacking = false; //tracker false to allow for first attack
    public float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer
    [SerializeField] public float attackCooldownMax = 2.5f;
    [SerializeField] public int attackDamage = 1; //dafault damage
    [SerializeField] public float attackSpeed = 1f; //default speed
    [SerializeField] public float attackDistance = 1.5f; //default attack distance

    //weapon
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject[] weaponAttackColliders; //weapon colliders
    [SerializeField] public EnemyWeaponColliderManager[] EWCMs; //weapon collider scripts

    //detection
    private bool playerNear = false;
    [SerializeField] private EnemyDetectionColliderNear EDCNear;
    [SerializeField] private EnemyDetectionColliderFar EDCFar;

    private void Attack()
    {
        if (attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("enemy attack");

            attackCooldownTimer = attackCooldownMax;


            AM.Play("Sword_Swing1");

            a.SetBool("attacking", true);
            //Debug.Log(GetCurAnimLength());
            //this will probably need changed later as not all colliders should be activated at once
            for (int i = 0; i < weaponAttackColliders.Length; i++)
            {
                EWCMs[i].EnableAttackCheck((GetCurAnimLength() * 2));
            }

            Invoke("ResetAttackAnimBool", (GetCurAnimLength() * 2));
        }
    }
    private void ResetAttackAnimBool() { a.SetBool("attacking", false); }

    public void DamagePlayer() { PC.AlterCurrentHealthPoints(-attackDamage); }


    [SerializeField] private ParticleSystem EPS;
    public void MoveEPS(Vector3 hitPos) 
    {
        EPS.gameObject.transform.position = hitPos;
        EPS.Play();
        Invoke("StopPS", 0.5f);
    }
    private void StopPS() { EPS.Stop(); }
    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]
    //moving
    [SerializeField] private float movementSpeed = 2.5f; //players movement velocity
    public void SetMovementSpeed(int newSpeed) { movementSpeed = newSpeed; }
    private Vector3 movement = Vector3.zero; //enemy movement directions
    private Vector3 enemyVelocity = Vector3.zero; //used to calc movement

    //boid movement
    [SerializeField] private float seperationDistance = 2.5f; //seperation distance
    public void SetSeperationDistance(int newDistance) { seperationDistance = newDistance; }
    [SerializeField] private float seperationWeight = 2.5f; //seperation weight
    public void SetSeperationWeight(int newWeight) { seperationWeight = newWeight; }

    //retreating
    [SerializeField] private float retreatSpeed = 5f; //retreat speed
    public void SetRetreatSpeed(int newSpeed) { retreatSpeed = newSpeed; }
    [SerializeField] private float retreatTime = 2f; //retreat time
    public void SetRetreatTime(int newTime) { retreatTime = newTime; }

    //looking
    [SerializeField] private float lookSensitivity = 2.5f; //enemy looking velocity
    public void SetLookSensitivity(int newSensitivity) { lookSensitivity = newSensitivity; }
    private Quaternion targetEnemyRot = Quaternion.identity; //used to lerp enemy rotation


    private void UpdateEnemyMovement()
    {
        if(!attacking && isActive)
        {
            //check if player is within close range
            if(EDCNear.IsPlayerNear())
            {
                //if the player is near the enemy
                //stop movement and update tracker
                if(!playerNear) { playerNear = true; }

                //begin attack movement
                if (!attacking) //if not attacking
                {
                    //calc distance between enemy and player
                    float distToPlayer = Vector3.Distance(transform.position, PC.transform.position);
                    //Debug.Log(attackDistance + " / " + distToPlayer);

                    //if distance greater than melee distance, move into melee range
                    if (distToPlayer > attackDistance) 
                    {
                        Vector3 directionToPlayer = (PC.transform.position - transform.position).normalized;
                        Vector3 seperationForce = CalculateSeperationForce();
                        Vector3 newMovement = (directionToPlayer + seperationForce).normalized * movementSpeed;

                        enemyRigid.velocity = new Vector3(newMovement.x, enemyRigid.velocity.y, newMovement.z);
                    }
                    else if(distToPlayer <= attackDistance)
                    {
                        Vector3 seperationForce = CalculateSeperationForce();
                        Vector3 newMovement = new Vector3((seperationForce.x * movementSpeed), enemyRigid.velocity.y, (seperationForce.z * movementSpeed));
                        enemyRigid.velocity = newMovement;
                        Attack();
                        Invoke("Retreat", retreatTime);
                    }
                }
            }
            else
            {
                //if enemy is moving as normal
                //update tracker
                if (playerNear) { playerNear = false; }

                //add velocity force in forward direction
                Vector3 seperationForce = CalculateSeperationForce();
                Vector3 newMovement = (transform.forward + seperationForce).normalized * movementSpeed;

                enemyRigid.velocity = new Vector3(newMovement.x, enemyRigid.velocity.y, newMovement.z);
            }
        }
    }
    private Vector3 CalculateSeperationForce()
    {
        GameObject[] enemiesNear = EDCNear.GetEnemiesNear();
        Vector3 seperationForce = Vector3.zero;
        int enemyCount = 0;

        foreach (GameObject enemy in enemiesNear)
        {
            if (enemy != null && enemy != this.gameObject)
            {
                //calc distance between current enemy and other enemy
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < seperationDistance) //if enemy is within seperation distance
                {
                    //calc direction to enemy, informs the force direction
                    Vector3 directionToEnemy = (transform.position - enemy.transform.position).normalized;

                    //add force direction to seperation force
                    seperationForce += directionToEnemy / (distanceToEnemy * seperationWeight);

                    //increase enemy count
                    enemyCount++;
                }
            }
        }


        //if there are enemies near, normalize the seperation force
        if (enemyCount > 0) { seperationForce /= enemyCount; }

        return seperationForce;
    }
    
    private void Retreat()
    {
        enemyRigid.velocity = new Vector3((-enemyRigid.transform.forward.x * retreatSpeed), enemyRigid.velocity.y, (-enemyRigid.transform.forward.z * retreatSpeed));
    }


    private void UpdateEnemyLooking()
    {
        if (!attacking)
        {
            Vector3 weaponOffset = (enemyRigid.transform.right * -0.5f) + (enemyRigid.transform.up * 0.1f) + (enemyRigid.transform.forward * -0.5f);
            weaponParent.transform.position = ((enemyRigid.transform.position + weaponOffset) + enemyRigid.transform.forward);
            weaponParent.transform.rotation = enemyRigid.transform.rotation;
        }


        Vector3 playerLookPosition = new Vector3(PC.transform.position.x, (PC.transform.position.y - 0.5f), PC.transform.position.z);
        targetEnemyRot = Quaternion.LookRotation(playerLookPosition - enemyRigid.position);
        enemyRigid.transform.rotation = Quaternion.Lerp(enemyRigid.transform.rotation, targetEnemyRot, Time.deltaTime * lookSensitivity);
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}