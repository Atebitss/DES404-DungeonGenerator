using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public abstract class AbstractEnemy : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private bool isActive = true;
    public void SetIsActive(bool newActive) { isActive = newActive; }

    [SerializeField] private Rigidbody enemyRigid; //enemy rigidbody used for physics interactions
    [SerializeField] private Animator a; //player animator used for running animations

    private AbstractSceneManager ASM;
    public AbstractSceneManager GetASM() { return ASM; }
    private AdaptiveDifficultyManager ADM;
    private AudioManager AM;
    private PlayerController PC;
    public PlayerController GetPC() { return PC; }
    private BossHealthDisplayManager BHDM;
    public void SetBHDM(BossHealthDisplayManager newBHDM) { BHDM = newBHDM; }
    public BossHealthDisplayManager GetBHDM() { return BHDM; }



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
    public bool GetDual() { return dual; }
    public void SetDual(bool newDual)
    {
        dual = newDual;
        if (dual)
        {

            a.SetBool("dual", dual);
        }
    }
    public void Wake(AbstractSceneManager ASM)
    {
        this.ASM = ASM;

        //swap main weapon hand
        if (Random.Range(0, 2) == 1 && !dual) //if random number = 1, swap weapon colliders and scripts positions in array
        {
            GameObject tempWeapon = weaponAttackColliders[0]; //get first weapon collider
            weaponAttackColliders[0] = weaponAttackColliders[1]; //set first position to second weapon
            weaponAttackColliders[1] = tempWeapon; //set second position to first weapon

            EnemyWeaponColliderManager tempEWCM = EWCMs[0]; //get first EWCM
            EWCMs[0] = EWCMs[1]; //set first position to second EWCM
            EWCMs[1] = tempEWCM; //set second position to first EWCM
        }

        //Debug.Log("dual: " + dual);
        a.SetBool("dual", dual);

        //if not dual, disable offhand weapon
        if (!dual)
        {
            //Debug.Log(this.gameObject.transform.parent.gameObject.name + " dual: " + dual);
            weaponAttackColliders[1].SetActive(false);
            EWCMs[1].gameObject.SetActive(false);
        }

        if (boss)
        {
            SetBHDM(ASM.GetPlayerParent().transform.GetChild(1).GetComponent<BossHealthDisplayManager>());
            UpdateBossStates();
            BHDM.Wake(this);
        }

        health = maxHealth;
    }
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
    virtual public void UpdateBossStates() { }
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
    public int maxHealth;
    public int GetMaxHealth() { return maxHealth; }
    public void SetMaxHealth(int newMaxHealth) { maxHealth = newMaxHealth; }

    [SerializeField] private GameObject hitSplashPrefab; //hit splash prefab
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int newHealth) { health = newHealth; HealthCheck(); }
    public void AlterHealth(int alter) 
    {
        /*Debug.Log("altering health: " + alter);*/
        health += alter; 
        HealthCheck(); 
    }
    public void DamageTarget(int alter, string dmgType)
    {
        /*Debug.Log("damaging enemy: " + alter);*/
        GameObject curHitSplash = Instantiate(hitSplashPrefab, transform.position, Quaternion.Euler(0f, 0f, 0f));

        //wake and play hit splash animation
        curHitSplash.GetComponent<HitSplashController>().Wake(PC, alter, dmgType);

        health -= alter;
        HealthCheck();
    }
    private void HealthCheck()
    {
        /*Debug.Log("health check: " + health);*/
        if (boss) { BHDM.UpdateCurrentBossHealth(health); }

        if (health <= 0)
        {
            ASM.DestroyEnemy(this.transform.parent.gameObject);
            Destroy(this.transform.parent.gameObject);
        }
    }
    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Attacking")]
    //attack
    [SerializeField] public bool attacking = false; //tracker false to allow for first attack
    [SerializeField] public float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer
    [SerializeField] public float attackCooldownMax = 2.5f;
    [SerializeField] public int attackDamage = 1; //dafault damage
    private int attackState = 0; //used to track attack state, 0 = idle, 1 = preparing, 2 = swinging, 3 = reset
    private Coroutine curAttackCoroutine; //used to track current attack coroutine
    public int GetAttackDamage() { return attackDamage; }
    public void SetAttackDamage(int newDamage) { attackDamage = newDamage; }

    [SerializeField] public float attackSpeed = 1f; //default speed
    public float GetAttackSpeed() { return attackSpeed; }
    public void SetAttackSpeed(float newSpeed) { attackSpeed = newSpeed; }

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
            curAttackCoroutine = StartCoroutine(AttackPrepare());
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
        attackState = 1; //set attack state to preparing
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        yield return new WaitForSeconds((GetCurAnimLength() + (0.5f / attackSpeed))); //wait aniamtion length + overhead time

        curAttackCoroutine = StartCoroutine(AttackSwing()); //start swinging attack coroutine
    }
    private IEnumerator AttackSwing()
    {
        //swing attack
        //Debug.Log("swing attack");
        a.SetInteger("attackStage", 2);
        attackState = 2; //set attack state to swinging
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        for (int i = 0; i < weaponAttackColliders.Length; i++)
        {
            //~~~ CHANGE LATER SO ONLY APPROPRIATE COLLIDER IS ENABLED ~~~//
            if (EWCMs[i].gameObject.activeSelf) { EWCMs[i].EnableAttackCheck((GetCurAnimLength())); }
        }
        AM.Play("Sword_Swing1");
        yield return new WaitForSeconds(GetCurAnimLength()); //exact animation length

        curAttackCoroutine = StartCoroutine(AttackRest()); //start rest coroutine
    }
    private IEnumerator AttackRest()
    {
        //reset attack
        //Debug.Log("reset attack");
        a.SetInteger("attackStage", 3);
        attackState = 3; //set attack state to reset
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        yield return new WaitForSeconds(GetCurAnimLength());

        curAttackCoroutine = StartCoroutine(AttackReset()); //start reset coroutine
    }
    private IEnumerator AttackReset()
    {
        //idle
        yield return new WaitForSeconds(0.1f); //wait for animation to start
        //Debug.Log("return to idle");
        a.SetBool("attacking", false);
        a.SetInteger("attackStage", 0);
        attackState = 0; //set attack state to idle

        yield return new WaitForSeconds(0.1f); //wait for animation to start
        attacking = false; //reset tracker

        Retreat();
    }


    public void InterruptAttack()
    {
        //Debug.Log("interrupt attack called on " + this.gameObject.name);

        if (attacking)
        {
            //Debug.Log("interrupt attack");

            StopCoroutine(curAttackCoroutine); //stop current attack coroutine
            ResetAttackState();
            Retreat();
        }
    }

    public void ParryAttack()
    {
        if (attackState == 1) //if preparing swing
        {
            StopCoroutine(AttackPrepare());
            ResetAttackState();
            Retreat();
        }
    }

    private void ResetAttackState()
    {
        //Debug.Log("reset attack state");
        a.SetBool("attacking", false);
        a.SetInteger("attackStage", 0);
        attackState = 0; //set attack state to idle
        attacking = false; //reset tracker
        attackCooldownTimer = 1f; //reset cooldown timer
    }
    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]
    //moving
    [SerializeField] private float movementSpeed = 2.5f; //players movement velocity
    public float GetMovementSpeed() { return movementSpeed; }
    public void SetMovementSpeed(float newSpeed) { movementSpeed = newSpeed; }

    //boid movement
    [SerializeField] private float seperationDistance = 50f; //seperation distance
    public void SetSeperationDistance(float newDistance) { seperationDistance = newDistance; }
    [SerializeField] private float seperationWeight = 100f; //seperation weight
    public void SetSeperationWeight(float newWeight) { seperationWeight = newWeight; }

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
        if (!attacking && !dodging && isActive)
        {
            //check if player is within close range
            if (EDCNear.IsPlayerNear())
            {
                //if the player is near the enemy
                //stop movement and update tracker
                if (!playerNear) { playerNear = true; }

                //begin attack movement
                //calc distance between enemy and player
                float distToPlayer = Vector3.Distance(transform.position, PC.transform.position);
                //Debug.Log(attackDistance + " / " + distToPlayer);

                //if distance greater than melee distance, move into melee range
                if (distToPlayer > attackDistance)
                {
                    Vector3 directionToPlayer = (PC.transform.position - transform.position).normalized; 
                    Vector3 seperationForce = CalculateSeperationForce();
                    Vector3 otherForce = CalculateOtherSeperationForces();
                    Vector3 combinedForce = (seperationForce + otherForce);
                    Vector3 newMovement = (directionToPlayer + combinedForce).normalized * movementSpeed;

                    enemyRigid.linearVelocity = new Vector3(newMovement.x, enemyRigid.linearVelocity.y, newMovement.z);
                }
                else if (distToPlayer <= attackDistance)
                {
                    Vector3 seperationForce = CalculateSeperationForce();
                    Vector3 otherForce = CalculateOtherSeperationForces();
                    Vector3 combinedForce = (seperationForce + otherForce);
                    Vector3 newMovement = new Vector3((combinedForce.x * movementSpeed), enemyRigid.linearVelocity.y, (seperationForce.z * movementSpeed));
                    enemyRigid.linearVelocity = newMovement;
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
                Vector3 otherForce = CalculateOtherSeperationForces();
                Vector3 combinedForce = (seperationForce + otherForce);
                Vector3 newMovement = (transform.forward + combinedForce).normalized * movementSpeed;

                enemyRigid.linearVelocity = new Vector3(newMovement.x, enemyRigid.linearVelocity.y, newMovement.z);
            }
        }
    }
    private Vector3 CalculateSeperationForce()
    {
        Vector3 seperationForce = Vector3.zero;

        GameObject[] enemiesNear = EDCNear.GetEnemiesNear();
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
        //if (enemyCount > 0) { seperationForce /= enemyCount; }
        return seperationForce;
    }
    private Vector3 CalculateOtherSeperationForces()
    {
        //Debug.Log("calculating other forces");
        //calculate other forces from repel and compel objects
        GameObject[] othersNear = EDCNear.GetOthersNear(); //get other objects
        Vector3 otherForce = Vector3.zero; //init force
        int otherCount = 0; //init counter

        //for each other object
        for (int i = 0; i < othersNear.Length; i++)
        {
            if (othersNear[i] != null)
            {
                //Debug.Log("other object: " + othersNear[i].name);
                //get distance from other object
                float distanceToOther = Vector3.Distance(transform.position, othersNear[i].transform.position);
                //Debug.Log("distance to other: " + distanceToOther);

                //if the other object
                if (othersNear.Length != 0)
                {
                    SeperationForce otherScript = othersNear[i].GetComponent<SeperationForce>();
                    if (distanceToOther < otherScript.GetSeperationDistance())
                    {
                        Vector3 directionFromOther = (transform.position - othersNear[i].transform.position).normalized;
                        otherForce += (directionFromOther * otherScript.GetSeperationForce());
                        Debug.Log(directionFromOther * otherScript.GetSeperationForce());
                        otherCount++;
                    }
                }
            }
        }

        //if (otherCount > 0) { otherForce /= otherCount; }
        return otherForce;
    }

    private void Retreat()
    {
        dodging = true;
        Vector3 dodgeVelocity = ((-enemyRigid.transform.forward * dodgeForce).normalized * (dodgeForce / dodgeTime));
        enemyRigid.AddForce(dodgeVelocity - enemyRigid.linearVelocity, ForceMode.VelocityChange); //set immediate velocity to calced velocity
        Invoke("StopDodge", dodgeTime);
    }
    private void StopDodge() { dodging = false; }


    private void UpdateEnemyLooking()
    {
        if (isActive)
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
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~visual feedback~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //VFX
    [SerializeField] private ParticleSystem EPS;
    public void MoveEPS(Vector3 hitPos)
    {
        EPS.gameObject.transform.position = hitPos;
        EPS.Play();
        Invoke("StopPS", 0.5f);
    }
    private void StopPS() { EPS.Stop(); }

    //materials
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Material baseMaterial;
    public void ResetMaterial() { enemyRenderer.material = baseMaterial; }
    public void SetMaterial(Material newMaterial) { enemyRenderer.material = newMaterial; }
    //~~~~~visual feedback~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, seperationDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}