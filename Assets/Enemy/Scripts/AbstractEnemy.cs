using UnityEngine;
using System.Collections.Generic;
using Unity.Android.Gradle;
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
    private void FixedUpdate()
    {
        UpdateEnemyStates();
        UpdateEnemyLooking();
        UpdateEnemyMovement();
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
    [HideInInspector] public bool attacking = false; //tracker false to allow for first attack
    public float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer
    [SerializeField] public float attackCooldownMax = 2f;
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject weaponAttackCollider; //weapon collider
    [SerializeField] private EnemyWeaponColliderManager EWCM; //weapon collider script
    [SerializeField] public int attackDamage = 5; //dafault damage
    [SerializeField] public float attackSpeed = 1f; //default speed

    private void Attack()
    {
        if (attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("enemy attack");

            attackCooldownTimer = attackCooldownMax;

            AM.Play("Sword_Swing1");

            a.SetBool("attacking", true);
            Debug.Log(GetCurAnimLength());
            EWCM.EnableAttackCheck(GetCurAnimLength());

            Invoke("ResetAttackAnimBool", GetCurAnimLength());
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
        //if enemy is moving as normal
        //add velocity force in forward direction
        enemyRigid.velocity = new Vector3((enemyRigid.transform.forward.x * movementSpeed), enemyRigid.velocity.y, (enemyRigid.transform.forward.z * movementSpeed));
    }

    private void UpdateEnemyLooking()
    {
        if (!attacking)
        {
            Vector3 weaponOffset = (enemyRigid.transform.right * -0.5f) + (enemyRigid.transform.up * 0.1f) + (enemyRigid.transform.forward * -0.5f);
            weaponParent.transform.position = ((enemyRigid.transform.position + weaponOffset) + enemyRigid.transform.forward);
            weaponParent.transform.rotation = enemyRigid.transform.rotation;
        }


        targetEnemyRot = Quaternion.LookRotation(PC.transform.position - enemyRigid.position);
        enemyRigid.transform.rotation = Quaternion.Lerp(enemyRigid.transform.rotation, targetEnemyRot, Time.deltaTime * lookSensitivity);
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
