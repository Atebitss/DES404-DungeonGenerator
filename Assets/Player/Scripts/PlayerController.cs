using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerController : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private Rigidbody playerRigid; //player rigidbody used for physics interactions
    [SerializeField] private Camera playerCamera; //player camera used for looking and object interactions
    [SerializeField] private TMP_Text interactionPromptText; //object interaction display
    [SerializeField] private Animator a; //player animator used for running animations
    [SerializeField] private GameObject hitSplashPrefab; //hit splash prefab

    private AbstractSceneManager ASM; //scene manager
    private AudioManager AM; //audio manager
    private DbugDisplayManager DDM; //debug manager
    private AdaptiveDifficultyManager ADM; //adaptive difficulty manager
    private AdaptiveDifficultyDisplayManager ADDM; //adaptive difficulty display manager
    public AdaptiveDifficultyDisplayManager GetADDM() { return ADDM; }
    private SkillVisualizationManager SVM; //skill visualization manager
    public SkillVisualizationManager GetSVM() { return SVM; }
    private BossHealthDisplayManager BHDM; //boss health display manager



    private void Awake()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = ASM.GetAudioManager();
        ADM = ASM.GetComponent<AdaptiveDifficultyManager>();
        DDM = this.gameObject.transform.parent.GetChild(1).GetComponent<DbugDisplayManager>();
        ADDM = this.gameObject.transform.parent.GetChild(1).GetComponent<AdaptiveDifficultyDisplayManager>();
        SVM = this.gameObject.transform.parent.GetChild(1).GetComponent<SkillVisualizationManager>();

        PWCM.SetWeaponDamage(attackDamage);
        if (ADM != null) { PWCM.SetADM(ADM); }
        PWCM.SetAM(AM);
        PWCM.SetHitParticle(PPS);
        PWCM.SetHitSplash(hitSplashPrefab);
        PWCM.SetPC(this);

        maxPlayerHealthBarWidth = playerHealthBarRect.sizeDelta.x;
        UpdatePlayerHealthBar();
        BHDM = this.gameObject.transform.parent.GetChild(1).GetComponent<BossHealthDisplayManager>();
        BHDM.DisableBossHealthDisplay();
    }

    private void OnDestroy()
    {
        ASM.RestartScene();
    }


    private void FixedUpdate()
    {
        if (ASM.GetDevMode()) { UpdateDDM(); }
        UpdatePlayerMovement();
        UpdatePlayerLooking();
        UpdatePlayerStates();
        UpdateInteractionPrompt();
    }


    private void UpdateDDM()
    {
        //world space
        DDM.playerJumping = jumping;
        DDM.playerGrounded = grounded;
        DDM.playerPosition = playerRigid.position;
        DDM.playerVelocity = playerVelocity;
        DDM.playerMovement = movement;

        //stats
        DDM.playerHealthCurrent = healthPointsCurrent;
        DDM.playerHealthMax = healthPointsMax;
        DDM.playerHealthPerSecond = 0f;
        DDM.playerStaminaCurrent = staminaPointsCurrent;
        DDM.playerStaminaMax = staminaPointsMax;
        DDM.playerStaminaPerSecond = 0f;
        DDM.playerMagicCurrent = magicPointsCurrent;
        DDM.playerMagicMax = magicPointsMax;
        DDM.playerMagicPerSecond = 0f;
        DDM.playerAttackDamage = attackDamage;
        DDM.playerAttackSpeed = attackSpeed;

        //dodge
        DDM.playerDodging = dodging;
        DDM.playerDodgeCooldownTimer = dodgeCooldownTimer;
        DDM.playerDodgeStartTime = dodgeStartTime;
        DDM.playerDodgeVelocity = dodgeVelocity;

        //attack
        DDM.playerAttacking = attacking;
        DDM.playerAttackCooldownTimer = attackCooldownTimer;
        DDM.playerAttackStartTime = attackStartTime;

        //attack combo
        DDM.playerComboing = comboing;
        DDM.lightAttackComboCounter = lightAttackComboCounter;
        DDM.playerLightAttackComboTimer = lightAttackComboTimer;
        DDM.playerLightAttackComboStartTime = lightAttackComboStartTime;

        //invincibility
        DDM.playerInvincible = invincible;
        DDM.playerInvincibilityTimer = invincibilityTimer;
        DDM.playerInvincibilityStartTime = invincibilityStartTime;
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]
    //moving
    [SerializeField] private float movementSpeed = 5f; //players movement velocity
    private Quaternion targetPlayerRot = Quaternion.identity; //used to lerp player rotation
    private Vector3 movement = Vector3.zero; //players movement directions
    private Vector3 playerVelocity = Vector3.zero; //used to calc movement

    //dodging
    [SerializeField] private LayerMask dodgeMask; //enemy layer
    private int dodgeLayer = -1;
    [SerializeField] private float dodgeForce = 10f; //--------|
    [SerializeField] private float dodgeDuration = 0.25f; //---|- each used to calc dodge
    [SerializeField] private float dodgeDistance = 5f; //------|
    private Vector3 dodgeVelocity = Vector3.zero; //used to calc directional velocity
    private bool dodging = false; //tracker
    private float dodgeCooldownTimer = 0f; //current dodge cooldown time
    private float dodgeStartTime = 0f; //track real time dodge began
    [SerializeField] private float dodgeCDMax = 0.5f; //time between dodges

    //jumping
    [SerializeField] private float jumpForce = 10f;
    private bool jumping = false;
    private bool grounded = false;

    //looking
    [SerializeField] private float lookSensitivity = 2.5f; //players looking velocity
    private Quaternion targetCameraRot = Quaternion.identity; //used to lerp camera rotation
    private Vector2 lookment = Vector2.zero; //like movement but for looking
    private float xRot = 0f;


    public void OnMove(InputAction.CallbackContext ctx)
    {
        //WASD / Left Thumbstick
        Vector2 input = ctx.ReadValue<Vector2>(); //get input from input system
        movement = new Vector3(input.x, 0, input.y); //translate input to impact
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        //Space / Button South
        if (ctx.performed && grounded && !jumping) //if player is on the ground and not currently jumping
        {
            Debug.Log("jump");


        }
    }
   
    public void OnDodge(InputAction.CallbackContext ctx)
    {
        //(Space / Button South) + (WASD/ Left Thumbstick)
        if (ctx.performed && dodgeCooldownTimer <= 0 && !dodging) //if dodge cooldown is not active and the player is not currently dodging
        {
            //Debug.Log("dodge");
            StartCoroutine(Dodge());
        }
    }
    public IEnumerator Dodge()
    {
        yield return new WaitForFixedUpdate(); // Wait until the next physics frame

        dodgeStartTime = Time.time; //remember time when dodge started
        dodgeCooldownTimer = dodgeCDMax; //set cooldown timer
        dodging = true; //set tracker to true
        //Debug.Log("Dodge start");

        if (ADM != null) { ADM.DodgeRan(); } //update adaptive difficulty

        MakePlayerInvincible(dodgeDuration);

        AM.Play("Player_Dodge" + Random.Range(1, 4));

        if (movement.x != 0 && movement.z != 0) //if the player is moving horizontally
        {
            //calc velocity in direction moving
            dodgeVelocity = ((playerRigid.transform.TransformDirection(movement.normalized) * dodgeForce).normalized * (dodgeDistance / dodgeDuration));

        }
        else //if player is not moving horizontally
        {
            //calc velocity in direction faced
            dodgeVelocity = ((playerRigid.transform.forward * dodgeForce).normalized * (dodgeDistance / dodgeDuration));
        }
    }
    
    private void UpdatePlayerMovement()
    {
        if (!dodging) //if player is moving as normal
        {
            //add velocity force in direction moving
            playerVelocity = (((playerRigid.transform.right * movement.x) * movementSpeed) + ((playerRigid.transform.forward * movement.z) * movementSpeed));
            playerRigid.AddForce(playerVelocity - playerRigid.velocity, ForceMode.VelocityChange);
        }
        else if (dodging) //if player is dodging
        {
            playerRigid.AddForce(dodgeVelocity - playerRigid.velocity, ForceMode.VelocityChange); //set immediate velocity to calced velocity

            if ((Time.time - dodgeStartTime) >= dodgeDuration) //if the player has been dodging for 1 second
            {
                playerRigid.velocity = Vector3.zero; //reset velocity
                //for (int i = 0; i < dodgeLayerIDs.Length; i++) { Physics.IgnoreLayerCollision(gameObject.layer, dodgeLayerIDs[i], false); } //allow dodging through enemies
                //Debug.Log("Dodge end");
                dodging = false; //set tracker to false
            }
        }

        if (dodgeCooldownTimer > 0) { dodgeCooldownTimer -= Time.deltaTime; } //count down cooldown
    }
    private void OnTriggerEnter(Collider col)
    {
        if (dodging)
        {
            if (col.gameObject.tag == "EnemyWeapon")
            {
                if (ADM != null) { ADM.DodgeSuccess(); } //update adaptive difficulty
            }
        }
    }


    public void OnLook(InputAction.CallbackContext ctx)
    {
        //Mouse / Right Thumbstick
        //Debug.Log(ctx.ReadValue<Vector2>());
        lookment = ctx.ReadValue<Vector2>();
    }
    private void UpdatePlayerLooking()
    {
        xRot -= (lookment.y * lookSensitivity); //increase rotation by sensitivity
        xRot = Mathf.Clamp(xRot, -70f, 70f); //lock rotation between 70 up & down

        targetCameraRot = Quaternion.Euler(xRot, 0f, 0f); //new camera rotation
        playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, targetCameraRot, (Time.deltaTime / 0.1f));


        if (!attacking)
        {
            Vector3 weaponOffset = (playerCamera.transform.right * 0.25f) + (playerCamera.transform.up * -0.5f) + (playerCamera.transform.forward * -0.5f);
            weaponParent.transform.position = ((playerRigid.transform.position + weaponOffset) + playerCamera.transform.forward);
            weaponParent.transform.rotation = playerCamera.transform.rotation;
        }


        targetPlayerRot *= Quaternion.Euler(0f, (lookment.x * lookSensitivity), 0f); //new player turn rotation
        playerRigid.transform.rotation = Quaternion.Lerp(playerRigid.transform.localRotation, targetPlayerRot, (Time.deltaTime / 0.1f));
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Attacking")]
    private bool attacking = false; //tracker false to allow for first attack, updated in UpdatePlayerStates when attack cooldown timer is 0
    private float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer in UpdatePlayerStates
    [SerializeField] private float lightAttackCooldownMax = 0.5f, heavyAttackCooldownMax = 1f;
    private float lightAttackAnimLength = 0.5f, heavyAttackAnimLength = 1f;
    private int attackComboDamage = 0; //additional damage
    private int lightAttackComboCounter = 0; //combo tracker set to one to skip first rotation
    private float lightAttackComboTimer = 0f, lightAttackComboStartTime = 0f; //used to reset combo timer
    [SerializeField] private float lightAttackComboTimerMax = 1f;
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject weaponAttackCollider; //weapon collider
    [SerializeField] private PlayerWeaponColliderManager PWCM; //weapon collider script
    [SerializeField] private int attackDamage = 5; //dafault damage
    [SerializeField] private float attackSpeed = 1f; //default speed
    private bool comboing = false;
    [SerializeField] private ParticleSystem PPS;

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        //lightAttackAnimLength = FindAnimationLength("swordLightAttack30degrees");
        //Debug.Log("lightAttackAnimLength: " + lightAttackAnimLength);

        if (ctx.performed && attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("light attack");
            //Debug.Log("combo: " + lightAttackComboCounter);
            
            if (lightAttackComboCounter == 0) 
            {
                attackComboDamage = 0; //reset any temp combo damage
            }


            //track light attack
            attackCooldownTimer = lightAttackCooldownMax;
            lightAttackComboTimer = lightAttackComboTimerMax;
            lightAttackComboStartTime = Time.time; //track when last combo hit started


            if (ADM != null) { ADM.AttackRan(); } //update adaptive difficulty


            //switch depending on combo
            if (lightAttackComboCounter == 0) { AM.Play("Sword_Swing1"); }
            else if (lightAttackComboCounter == 1) { AM.Play("Sword_Swing2"); }
            else if (lightAttackComboCounter == 2)
            {
                AM.Play("Sword_SwingFinal");
                attackComboDamage = attackDamage * 2; //double damage added on to regular damage
            }

            lightAttackComboCounter++; //increase combo

            //update sword swing animation
            a.SetInteger("lightSwingCombo", lightAttackComboCounter);
            PWCM.EnableAttackCheck((lightAttackAnimLength - 0.1f));

            if (lightAttackComboCounter == 3) 
            {
                Invoke("ResetLightAttackAnimInt", (lightAttackAnimLength - 0.1f));
                ADM.ComboPerformed();
            }
        }
    }
    private void ResetLightAttackAnimInt()
    {
        lightAttackComboCounter = 0; //reset counters
        lightAttackComboTimer = 0;
        a.SetInteger("lightSwingCombo", lightAttackComboCounter); 
    }

    public void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        //heavyAttackAnimLength = FindAnimationLength("swordHeavyAttackCleave");
        //Debug.Log("heavyAttackAnimLength: " + heavyAttackAnimLength);

        if (ctx.performed && attackCooldownTimer <= 0 && !attacking)
        {
            //Debug.Log("heavy attack");
            attackCooldownTimer = heavyAttackCooldownMax;

            if (lightAttackComboCounter != 0)  //reset light attack
            {
                //Debug.Log("reset light attack");
                lightAttackComboCounter = 0;
                lightAttackComboTimer = 0;
                a.SetInteger("lightSwingCombo", lightAttackComboCounter);
            }

            if (ADM != null) { ADM.AttackRan(); } //update adaptive difficulty

            //Debug.Log("play heavy attack");
            AM.Play("Sword_SwingCleave");
            a.SetBool("attackingHeavy", true);

            //Debug.Log("enable attack check");
            PWCM.EnableAttackCheck((FindAnimationLength("swordHeavyAttackCleave") - 0.1f));

            //Debug.Log("invoke reset heavy attack, " + (heavyAttackAnimLength - 0.1f));
            Invoke("ResetHeavyAttackAnimBool", FindAnimationLength("swordHeavyAttackCleave"));
        }
    }
    private void ResetHeavyAttackAnimBool() { /*Debug.Log("reset heavy attack");*/ a.SetBool("attackingHeavy", false); }


    private float FindAnimationLength(string clipName)
    {
        //find length of attack animations
        foreach (AnimationClip clip in a.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }
    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Interaction")]
    [SerializeField] private LayerMask interactionMask;
    private float interactDistance = 2.5f, interactCooldown = .25f;
    private bool interacting = false;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        //E / Right Bumper
        if (ctx.performed && !interacting)
        {
            //Debug.Log("interact");

            interacting = true;
            RaycastHit hit;

            if (Physics.Raycast(playerCamera.transform.position, (playerCamera.transform.forward * interactDistance), out hit, interactDistance, interactionMask))
            {
                //Debug.Log("interact hit " + hit.collider.name);
                //Debug.Log("tag " + hit.collider.tag);
                switch (hit.collider.tag)
                {
                    case "Door":
                        //Debug.Log("door, " + hit.collider.GetComponent<AbstractDoorScript>());
                        hit.collider.GetComponent<AbstractDoorScript>().InteractWithDoor();
                        break;
                    case "Portal":
                        //Debug.Log("portal, " + hit.collider.GetComponent<PortalManager>());
                        hit.collider.GetComponent<PortalManager>().InteractWithPortal();
                        break;

                }
            }

            StartCoroutine(ResetInteraction());
        }
    }
    IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactCooldown);
        //Debug.Log("interact reset");
        interacting = false;
    }
    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~HUD~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-HUD")]
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private RectTransform playerHealthBarRect;
    [SerializeField] private Image playerHealthBarImage;
    private float maxPlayerHealthBarWidth;

    private void UpdatePlayerHealthBar()
    {
        //Debug.Log("invincible: " + invincible);
        //update health bar
        float hpPercentage = (float)healthPointsCurrent / (float)healthPointsMax;
        playerHealthBarRect.sizeDelta = new Vector2(maxPlayerHealthBarWidth * hpPercentage, playerHealthBarRect.sizeDelta.y);
        playerHealthText.text = healthPointsCurrent + " / " + healthPointsMax;

        if(invincible)
        {
            playerHealthBarImage.color = Color.magenta;
        }
        else if (hpPercentage > 0.5f && hpPercentage <= 1f && !invincible)
        {
            playerHealthBarImage.color = Color.green; // Healthy
        }
        else if (hpPercentage > 0.2f && hpPercentage <= 0.5f && !invincible)
        {
            playerHealthBarImage.color = Color.yellow; // Warning
        }
        else if (hpPercentage <= 0.2f && !invincible)
        {
            playerHealthBarImage.color = Color.red; // Critical
        }
    }



    private void UpdateInteractionPrompt()
    {
        //update interaction prompt
        RaycastHit hit;
        Debug.DrawRay(playerCamera.transform.position, (playerCamera.transform.forward * interactDistance), Color.red, 1f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactDistance, interactionMask))
        {
            interactionPromptText.text = "'RB' to interact with " + hit.collider.tag;
        }
        else { interactionPromptText.text = ""; }
    }

    //~~~~~HUD~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Stat")]
    [SerializeField] private int healthPointsCurrent = 10;
    [SerializeField] private int healthPointsMax = 10;
    [SerializeField] private int staminaPointsCurrent = 10;
    [SerializeField] private int staminaPointsMax = 10;
    [SerializeField] private int magicPointsCurrent = 10;
    [SerializeField] private int magicPointsMax = 10;
    [SerializeField] private int StrengthPoints = 1, DexterityPoints = 1, IntelligencePoints = 1, WisdomPoints = 1;
    [SerializeField] private bool invincible = false, permaInvincible = false;
    [SerializeField] private float invincibilityTimer = 0f;
    private float invincibilityStartTime = 0f;

    //health points
    public void SetCurrentHealthPoints(int newHealth) { if (!invincible) { /*Debug.Log("setting health: " + newHealth);*/ healthPointsCurrent = newHealth; HealthCheck(); } }
    public void AlterCurrentHealthPoints(int alter) { if(!invincible) { /*Debug.Log("altering health: " + alter);*/ healthPointsCurrent += alter; HealthCheck(); ADM.DamageTaken(); } }
    public int GetCurrentHealthPoints() { return healthPointsCurrent; }
    private void HealthCheck() 
    { 
        if (healthPointsCurrent <= 0) 
        { 
            /*Debug.Log("health check: " + healthPointsCurrent);*/ 
            UpdatePlayerHealthBar(); 
            ASM.DestroyPlayer(); 
        } 
    }

    public void AlterMaxHealthPoints(int alter) { healthPointsMax += alter; }
    public int GetMaxHealthPoints() { return healthPointsMax; }

    //stamina points
    public void AlterCurrentStaminaPoints(int alter) { staminaPointsCurrent += alter; }
    public int GetCurrentStaminaPoints() { return staminaPointsCurrent; }

    public void AlterMaxStaminaPoints(int alter) { staminaPointsMax += alter; }
    public int GetMaxStaminaPoints() { return staminaPointsMax; }

    //magic points
    public void AlterCurrentMagicPoints(int alter) { magicPointsCurrent += alter; }
    public int GetCurrentMagicPoints() { return magicPointsCurrent; }

    public void AlterMaxMagicPoints(int alter) { magicPointsMax += alter; }
    public int GetMaxMagicPoints() { return magicPointsMax; }

    //other stats
    //stength
    public void AlterStrengthPoints(int alter) { StrengthPoints += alter; }
    public int GetStrengthPoints() { return StrengthPoints; }

    //dexterity
    public void AlterDexterityPoints(int alter) { DexterityPoints += alter; }
    public int GetDexterityPoints() { return DexterityPoints; }

    //inteligence
    public void AlterIntelligencePoints(int alter) { IntelligencePoints += alter; }
    public int GetIntelligencePoints() { return IntelligencePoints; }

    //wisdom
    public void AlterWisdomPoints(int alter) { WisdomPoints += alter; }
    public int GetWisdomPoints() { return WisdomPoints; }


    //invincibility
    public void MakePlayerInvincible(float iTime) { invincibilityTimer = iTime; }
    public void UpdatePlayerStates()
    {
        if (invincibilityTimer > 0 && !invincible)
        {
            invincible = true; //set tracker true
            invincibilityStartTime = Time.time; //track when invisibility started
            UpdatePlayerHealthBar();
            
        }
        else if (permaInvincible && !invincible) 
        {
            invincible = true; 
            invincibilityTimer = 1f;
            UpdatePlayerHealthBar();
        } 

        if (invincibilityTimer > 0 && !permaInvincible) { invincibilityTimer -= Time.deltaTime; } //count down timer
        if (invincibilityTimer <= 0 && !permaInvincible) 
        { 
            //when timer runs out, set tracker false
            invincibilityTimer = 0; 
            invincibilityStartTime = 0; 
            invincible = false; 
            UpdatePlayerHealthBar();
        } 



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



        if (lightAttackComboTimer > 0 && !comboing) { comboing = true; }
        if (lightAttackComboTimer > 0) { lightAttackComboTimer -= Time.deltaTime; } //count down timer
        if (lightAttackComboTimer <= 0 && comboing)  //when timer runs out
        {
            lightAttackComboTimer = 0;
            lightAttackComboCounter = 0; //set tracker to start
            comboing = false; //set tracker false
            a.SetInteger("lightSwingCombo", lightAttackComboCounter);
        }
    }
    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
