using UnityEngine;
using System.Collections.Generic;
public abstract class AbstractEnemy : MonoBehaviour
{
    /*
     * ~~~~~~TO DO~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
     * add player detector for attack trigger & movement trigger
     * add attack trigger to start attacking when within x distance of player
     * add movement trigger to stop moving when within x distance of player
     */
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Rigidbody enemyRigid; //enemy rigidbody used for physics interactions
    [SerializeField] private Animator a; //player animator used for running animations

    [HideInInspector] public AbstractSceneManager SM;
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
    private void Start()
    {
    }
    private void FixedUpdate()
    {
        UpdateEnemyStates();

        if (isActive)
        {
            UpdateEnemyLooking();
            UpdateEnemyMovement();
        }
    }
    public void UpdateEnemyStates()
    {
        if (health <= 0) { Destroy(this.gameObject); }


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
    public void AlterHealth(int change) { health += change; HealthCheck(); }
    private void HealthCheck() { if (health <= 0) { Destroy(this.gameObject); } }
    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Attacking")]
    //attack
    [HideInInspector] public bool attacking = false; //tracker false to allow for first attack
    public float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer
    [SerializeField] public float attackCooldownMax = 5f;
    [SerializeField] public int attackDamage = 5; //dafault damage
    [SerializeField] public float attackSpeed = 1f; //default speed
    [SerializeField] public float attackDistance = 1.5f; //default attack distance

    //weapon
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject weaponAttackCollider; //weapon collider
    [SerializeField] public EnemyWeaponColliderManager EWCM; //weapon collider script

    //detection
    private bool playerDetected = false;
    [SerializeField] private EnemyPlayerCollider EPC;

    private void Attack()
    {
        if (attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("enemy attack");

            attackCooldownTimer = attackCooldownMax;

            AM.Play("Sword_Swing1");

            a.SetBool("attacking", true);
            //Debug.Log(GetCurAnimLength());
            EWCM.EnableAttackCheck((GetCurAnimLength() * 2));

            Invoke("ResetAttackAnimBool", (GetCurAnimLength() * 2));
        }
    }
    private void ResetAttackAnimBool() { a.SetBool("attacking", false); }

    public void DamagePlayer() { PC.AlterCurrentHealthPoints(-attackDamage); }
    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]
    //moving
    [SerializeField] private float movementSpeed = 2.5f; //players movement velocity
    public void SetMovementSpeed(int newSpeed) { movementSpeed = newSpeed; }
    private Quaternion targetEnemyRot = Quaternion.identity; //used to lerp enemy rotation
    private Vector3 movement = Vector3.zero; //enemy movement directions
    private Vector3 enemyVelocity = Vector3.zero; //used to calc movement

    //looking
    [SerializeField] private float lookSensitivity = 2.5f; //enemy looking velocity

    private void UpdateEnemyMovement()
    {
        //check if player is within close range
        if(EPC.IsPlayerDetected())
        {
            //if the player is found
            //stop movement and update tracker
            if(!playerDetected) { playerDetected = true; }

            //begin attack movement
            if (!attacking) //if not attacking
            {
                //calc distance between enemy and player
                float distToPlayer = Vector3.Distance(transform.position, PC.transform.position);
                //Debug.Log(attackDistance + " / " + distToPlayer);

                //if distance greater than melee distance, move into melee range
                if (distToPlayer > attackDistance) 
                {
                    enemyRigid.velocity = new Vector3((enemyRigid.transform.forward.x * (movementSpeed * 2)), enemyRigid.velocity.y, (enemyRigid.transform.forward.z * (movementSpeed * 2)));
                }
                else 
                {
                    enemyRigid.velocity = Vector3.zero;
                    Attack();
                    Invoke("Retreat", 2f);
                }
            }
        }
        else
        {
            //if enemy is moving as normal
            //update tracker
            if (playerDetected) { playerDetected = false; }
            //add velocity force in forward direction
            enemyRigid.velocity = new Vector3((enemyRigid.transform.forward.x * movementSpeed), enemyRigid.velocity.y, (enemyRigid.transform.forward.z * movementSpeed));
        }
    }
    private void Retreat()
    {
        enemyRigid.velocity = new Vector3((-enemyRigid.transform.forward.x * (movementSpeed * 5)), enemyRigid.velocity.y, (-enemyRigid.transform.forward.z * (movementSpeed * 5)));
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
