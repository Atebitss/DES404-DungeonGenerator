using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Android.Gradle;
public abstract class AbstractEnemy : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Rigidbody enemyRigid; //enemy rigidbody used for physics interactions
    [SerializeField] private Animator a; //player animator used for running animations

    private AbstractSceneManager ASM;
    private AdaptiveDifficultyManager ADM;
    private AudioManager AM;
    private PlayerController PC;


    private float GetCurAnimLength()
    {
        //find legnth of current animation
        foreach (AnimationClip clip in a.runtimeAnimatorController.animationClips) 
        {
            if (a.GetCurrentAnimatorStateInfo(0).IsName(clip.name)) 
            {
                //Debug.Log("clip name: " + clip.name + ", clip length: " + clip.length);
                return clip.length; 
            }
        }
        return -1;
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~state updates~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public string type = "";
    public bool boss = false;
    public bool dual = false;
    private void Start()
    {
        //Debug.Log("enemy start: " + this.gameObject.name);
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = ASM.GetAudioManager();
        ADM = ASM.GetComponent<AdaptiveDifficultyManager>();
        PC = ASM.GetPlayerController();
        for (int i = 0; i < weaponAttackColliders.Length; i++)
        {
            EWCMs[i].SetAM(AM);
            EWCMs[i].SetWeaponDamage(attackDamage);
            if (ADM != null) { EWCMs[i].SetADM(ADM); }
        }

        if (boss) { UpdateBossStates(); }
        //Debug.Log("dual: " + dual);
        a.SetBool("dual", dual); 
    }
    private void FixedUpdate()
    {
        UpdateEnemyStates();

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

        if (attackCooldownTimer > 0) { attackCooldownTimer -= Time.deltaTime; } //count down timer
        if (attackCooldownTimer <= 0) //when timer runs out, set tracker false
        {
            attackCooldownTimer = 0;
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
    [SerializeField] public bool attacking = false; //tracker false to allow for first attack
    [SerializeField] public float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer
    [SerializeField] public float attackCooldownMax = 2.5f;
    [SerializeField] public int attackDamage = 1; //dafault damage
    [SerializeField] public float attackSpeed = 1f; //default speed
    [SerializeField] public float attackDistance = 1.5f; //default attack distance
    [SerializeField] private int attackStage = 0; //used to track attack stage

    //weapon
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject[] weaponAttackColliders; //weapon colliders
    [SerializeField] public EnemyWeaponColliderManager[] EWCMs; //weapon collider scripts

    //detection
    private bool playerNear = false;
    [SerializeField] private EnemyDetectionColliderNear EDCNear;
    [SerializeField] private EnemyDetectionColliderFar EDCFar;

    private void BeginAttack()
    {
        if (attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("enemy attack");
            attackCooldownTimer = attackCooldownMax;
            attacking = true; //set tracker
            attackStartTime = Time.time; //track when attack started
            StartCoroutine(AttackPrepare());
        }
    }
    private IEnumerator AttackPrepare()
    {
        //Debug.Log("attackCooldownTimer: " + attackCooldownTimer);
        //Debug.Log("attacking: " + attacking);

        //prepare attack
        //Debug.Log("attack prepare");
        a.SetBool("attacking", true);
        a.SetInteger("attackStage", 1);
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        yield return new WaitForSeconds((GetCurAnimLength() + (0.5f / attackSpeed))); //wait aniamtion length + overhead time


        //swing attack
        //Debug.Log("swing attack");
        a.SetInteger("attackStage", 2);
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        for (int i = 0; i < weaponAttackColliders.Length; i++)
        {
            //~~~ CHANGE LATER SO ONLY APPROPRIATE COLLIDER IS ENABLED ~~~//
            EWCMs[i].EnableAttackCheck((GetCurAnimLength()));
        }
        AM.Play("Sword_Swing1");
        yield return new WaitForSeconds(GetCurAnimLength()); //exact animation length


        //reset attack
        //Debug.Log("reset attack");
        a.SetInteger("attackStage", 3);
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        yield return new WaitForSeconds(GetCurAnimLength());


        //idle
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        //Debug.Log("return to idle");
        a.SetBool("attacking", false);
        a.SetInteger("attackStage", 0);

        yield return new WaitForSeconds(0.1f); //wait for animation to start
        attacking = false; //reset tracker

        Retreat();
    }
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
    [SerializeField] private bool dodging = false; //dodge tracker
    [SerializeField] private float dodgeForce = 5f; //dodge force
    public void SetDodgeForce(int newForce) { dodgeForce = newForce; }
    [SerializeField] private float dodgeTime = 1f; //dodge time
    public void SetDodgeTime(int newTime) { dodgeTime = newTime; }

    //looking
    [SerializeField] private float lookSensitivity = 2.5f; //enemy looking velocity
    public void SetLookSensitivity(int newSensitivity) { lookSensitivity = newSensitivity; }
    private Quaternion targetEnemyRot = Quaternion.identity; //used to lerp enemy rotation


    private void UpdateEnemyMovement()
    {
        if(!attacking && !dodging && isActive)
        {
            //check if player is within close range
            if(EDCNear.IsPlayerNear())
            {
                //if the player is near the enemy
                //stop movement and update tracker
                if(!playerNear) { playerNear = true; }

                //begin attack movement
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
                    BeginAttack();
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
        dodging = true;
        Vector3 dodgeVelocity = ((-enemyRigid.transform.forward * dodgeForce).normalized * (dodgeForce / dodgeTime));
        enemyRigid.AddForce(dodgeVelocity - enemyRigid.velocity, ForceMode.VelocityChange); //set immediate velocity to calced velocity
        Invoke("StopDodge", dodgeTime);
    }
    private void StopDodge() { dodging = false; }


    private void UpdateEnemyLooking()
    {
        if (!attacking)
        {
            Vector3 weaponOffset = (enemyRigid.transform.right * -0.5f) + (enemyRigid.transform.up) + (enemyRigid.transform.forward * -0.5f);
            weaponParent.transform.position = ((enemyRigid.transform.position + weaponOffset) + enemyRigid.transform.forward);
            weaponParent.transform.rotation = enemyRigid.transform.rotation;
        }


        Vector3 playerLookPosition = new Vector3(PC.transform.position.x, (PC.transform.position.y - 0.5f), PC.transform.position.z);
        targetEnemyRot = Quaternion.LookRotation(playerLookPosition - enemyRigid.position);
        enemyRigid.transform.rotation = Quaternion.Lerp(enemyRigid.transform.rotation, targetEnemyRot, Time.deltaTime * lookSensitivity);
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~visual feedback~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private ParticleSystem EPS;
    public void MoveEPS(Vector3 hitPos)
    {
        EPS.gameObject.transform.position = hitPos;
        EPS.Play();
        Invoke("StopPS", 0.5f);
    }
    private void StopPS() { EPS.Stop(); }
    //~~~~~visual feedback~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}