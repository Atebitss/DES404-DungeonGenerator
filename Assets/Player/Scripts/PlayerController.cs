using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private Rigidbody playerRigid; //player rigidbody used for physics interactions
    [SerializeField] private Camera playerCamera; //player camera used for looking and object interactions
    [SerializeField] private TMP_Text interactionPromptText; //object interaction display
    [SerializeField] private Animator a; //player animator used for running animations
    [SerializeField] private GameObject hitSplashPrefab; //hit splash prefab
    [SerializeField] private GameObject leftHand, rightHand;

    private AbstractSceneManager ASM; //scene manager
    private AudioManager AM; //audio manager
    private DbugDisplayManager DDM; //debug manager
    private AdaptiveDifficultyManager ADM; //adaptive difficulty manager
    private AdaptiveDifficultyDbugManager ADDM; //adaptive difficulty display manager
    public AdaptiveDifficultyDbugManager GetADDM() { return ADDM; }
    private SkillVisualizationManager SVM; //skill visualization manager
    public SkillVisualizationManager GetSVM() { return SVM; }
    private BossHealthDisplayManager BHDM; //boss health display manager
    private SpellDbugManager SDM; //spell debug display


    [Header("-Minimap")]
    [SerializeField] private GameObject MV;
    private MinimapManager MM; //minimap manager
    private Camera mainCamera;

    [Header("-HUD")]
    [SerializeField] private GameObject CV;
    private ConsumableVisualManager CVM; //consumable visual manager
    public ConsumableVisualManager GetCVM() { return CVM; }

    [Header("-Debug Displays")]
    [SerializeField] private GameObject DbugDisplay;
    [SerializeField] private GameObject ADDbugDisplay;
    [SerializeField] private GameObject SpellDbugDisplay;
    [SerializeField] private GameObject SVDisplay;

    [SerializeField] private bool active = false; //allows player input
    public void SetActive(bool newActive) 
    {
        //Debug.Log("PlayerController.SetActive: " + newActive);
        active = newActive;

        if (!active)
        {
            playerRigid.linearVelocity = Vector3.zero;
            playerRigid.angularVelocity = Vector3.zero;
        }
        else
        {
            UpdatePlayerHealthBar();
        }
    }



    private void Awake()
    {
        //set refrences
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = ASM.GetAudioManager();
        ADM = ASM.GetComponent<AdaptiveDifficultyManager>();
        DDM = this.gameObject.transform.parent.GetChild(1).GetComponent<DbugDisplayManager>();
        ADDM = this.gameObject.transform.parent.GetChild(1).GetComponent<AdaptiveDifficultyDbugManager>();
        SVM = this.gameObject.transform.parent.GetChild(1).GetComponent<SkillVisualizationManager>();
        SDM = this.gameObject.transform.parent.GetChild(1).GetComponent<SpellDbugManager>();
        MM = this.gameObject.transform.parent.GetChild(1).GetComponent<MinimapManager>();
        CVM = this.gameObject.transform.parent.GetChild(1).GetComponent<ConsumableVisualManager>();

        //update camera reference
        if (mainCamera == null) { mainCamera = Camera.main; }

        //update weapon collider references
        if (ADM != null) { PWCM.SetADM(ADM); }
        if (AM != null) { PWCM.SetAM(AM); }
        if (PPS != null) { PWCM.SetHitParticle(PPS); }
        PWCM.SetPC(this);

        //display debug infos
        if (ASM.GetDevMode())
        {
            DbugDisplay.SetActive(true);
            ADDbugDisplay.SetActive(true);
            //SVDisplay.SetActive(true);
            SpellDbugDisplay.SetActive(true);
        }
        else
        {
            DbugDisplay.SetActive(false);
            ADDbugDisplay.SetActive(false);
            //SVDisplay.SetActive(false);
            SpellDbugDisplay.SetActive(false);
        }


        //update health displays
        maxPlayerHealthBarWidth = playerHealthBarRect.sizeDelta.x;
        UpdatePlayerHealthBar();
        BHDM = this.gameObject.transform.parent.GetChild(1).GetComponent<BossHealthDisplayManager>();
        BHDM.DisableBossHealthDisplay();

        //wake minimap
        if (GameObject.Find("MinimapCamera") == null) 
        {
            //if there is no minimap camera, disable the minimap
            MM.enabled = false;
            MV.SetActive(false);
        }
        else { MM.Wake(ASM.GetMG().GetBoundsX(), ASM.GetMG().GetBoundsZ()); }
    }

    private void OnDestroy()
    {
        if (active)
        {
            active = false;
            ADM.End();
            ASM.RestartScene();
        }
    }


    private void FixedUpdate()
    {
        if (active)
        {
            if (ASM.GetDevMode()) 
            {
                UpdateDDM();
                UpdateSDM();
            }

            UpdatePlayerMovement();
            UpdatePlayerLooking();
            UpdatePlayerStates();
            UpdateInteractionPrompt();
        }
    }


    private void UpdateDDM()
    {
        if (active)
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
    }
    private void UpdateSDM()
    {
        if (active)
        {
            //spell
            SDM.spellShape = shapeName;
            SDM.spellEffect = effectName;
            SDM.spellElement = elementName;
            SDM.spellCooldown = spellCooldownTimer;
            SDM.spellCooldownMax = spellCooldownMax;
            SDM.radius = curSpell.GetRadius();
            SDM.speed = curSpell.GetSpeed();
            SDM.damage = curSpell.GetDamage();
            SDM.spellPower = curSpell.GetSpellPower();
            SDM.valid = curSpell.GetSpellValid();
            SDM.casted = curSpell.GetCasted();
            SDM.persistent = curSpell.GetSpellPersist();
            SDM.targetPoints = curSpell.GetTargetPoints().Length;
            SDM.triggerPoints = curSpell.GetTriggerPoints().Length;
            SDM.targets = curSpell.GetSpellTargets().Length;
            SDM.aimingTargets = curSpell.GetAimingTargets().Length;
            SDM.ignoredTargets = curSpell.GetIgnoredTargets().Length;
            SDM.startPos = curSpell.GetStartPos();
            SDM.endPos = curSpell.GetEndPos();
            SDM.direction = curSpell.GetDir();
            SDM.distance = curSpell.GetJourneyLength();
        }
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]

    //moving
    [SerializeField] private float movementSpeed = 5f; //players movement velocity
    public float GetMovementSpeed() { return movementSpeed; }
    public void AlterMovementSpeed(float alter) { movementSpeed += alter; } //increase/decrease movement speed
    public void SetMovementSpeed(float newSpeed) { movementSpeed = newSpeed; } //set movement speed

    private float tempMovementSpeed = 1f; //players temp movement velocity
    public float GetTempMovementSpeed() { return tempMovementSpeed; }
    public void AlterTempMovementSpeed(float alter) { tempMovementSpeed += alter; } //increase/decrease temp movement speed
    public void SetTempMovementSpeed(float newSpeed) { tempMovementSpeed = newSpeed; } //set temp movement speed
    public void ResetTempMovementAfter(float resetTimer)
    {
        //Debug.Log("starting reset temp speed, timer: " + resetTimer);
        StartCoroutine(ResetTempMovement(resetTimer)); //reset temp movement speed after x seconds
    }
    public IEnumerator ResetTempMovement(float resetTimer) 
    {
        yield return new WaitForSeconds(resetTimer);
        ResetTempMovement();
    } 
    public void ResetTempMovement() 
    {
        //Debug.Log("resetting temp movement speed"); 
        tempMovementSpeed = 1f; //reset temp movement speed to 1
    }

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
        if (active)
        {
            //WASD / Left Thumbstick
            Vector2 input = ctx.ReadValue<Vector2>(); //get input from input system
            movement = new Vector3(input.x, 0, input.y); //translate input to impact
        }
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //Space / Button South
            if (ctx.performed && grounded && !jumping) //if player is on the ground and not currently jumping
            {
                Debug.Log("jump");


            }
        }
    }
   
    public void OnDodge(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //(Space / Button South) + (WASD/ Left Thumbstick)
            if (ctx.performed && dodgeCooldownTimer <= 0 && !dodging) //if dodge cooldown is not active and the player is not currently dodging
            {
                //Debug.Log("dodge");
                StartCoroutine(Dodge());
            }
        }
    }
    public IEnumerator Dodge()
    {
        if (active)
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
    }
    
    private void UpdatePlayerMovement()
    {
        if (active)
        {
            if (!dodging) //if player is moving as normal
            {
                //add velocity force in direction moving
                playerVelocity = (((playerRigid.transform.right * movement.x) * (movementSpeed + tempMovementSpeed)) + ((playerRigid.transform.forward * movement.z) * (movementSpeed + tempMovementSpeed)));
                playerRigid.AddForce(playerVelocity - playerRigid.linearVelocity, ForceMode.VelocityChange);
            }
            else if (dodging) //if player is dodging
            {
                playerRigid.AddForce(dodgeVelocity - playerRigid.linearVelocity, ForceMode.VelocityChange); //set immediate velocity to calced velocity

                if ((Time.time - dodgeStartTime) >= dodgeDuration) //if the player has been dodging for 1 second
                {
                    playerRigid.linearVelocity = Vector3.zero; //reset velocity
                    //for (int i = 0; i < dodgeLayerIDs.Length; i++) { Physics.IgnoreLayerCollision(gameObject.layer, dodgeLayerIDs[i], false); } //allow dodging through enemies
                    //Debug.Log("Dodge end");
                    dodging = false; //set tracker to false
                }
            }

            if (dodgeCooldownTimer > 0) { dodgeCooldownTimer -= Time.deltaTime; } //count down cooldown
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        if (active)
        {
            if (dodging)
            {
                if (col.gameObject.tag == "EnemyWeapon")
                {
                    if (ADM != null) { ADM.DodgeSuccess(); } //update adaptive difficulty
                }
            }
        }
    }


    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //Mouse / Right Thumbstick
            //Debug.Log(ctx.ReadValue<Vector2>());
            lookment = ctx.ReadValue<Vector2>();
        }
    }
    private void UpdatePlayerLooking()
    {
        if (active)
        {
            xRot -= (lookment.y * lookSensitivity); //increase rotation by sensitivity
            xRot = Mathf.Clamp(xRot, -70f, 70f); //lock rotation between 70 up & down

            targetCameraRot = Quaternion.Euler(xRot, 0f, 0f); //new camera rotation
            playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, targetCameraRot, (Time.deltaTime / 0.1f));


            targetPlayerRot *= Quaternion.Euler(0f, (lookment.x * lookSensitivity), 0f); //new player turn rotation
            //Debug.Log("targetPlayerRot: " + targetPlayerRot);
            playerRigid.transform.rotation = Quaternion.Lerp(playerRigid.transform.localRotation, targetPlayerRot, (Time.deltaTime / 0.1f));
        }
    }
    public void SetPlayerLookAt(Vector3 newLookPos)
    {
        //Debug.Log("look at: " + newLookPos);
        //make player look at a position
        Vector3 lookPos = newLookPos - playerRigid.transform.position; //calculate direction to look
        Quaternion targetRot = Quaternion.LookRotation(lookPos); //calculate new rotation

        Quaternion lookTargetCameraRot = Quaternion.Euler(targetRot.x, targetRot.y, targetRot.z); //new camera rotation
        playerCamera.transform.localRotation = lookTargetCameraRot;
        targetCameraRot = lookTargetCameraRot;

        playerRigid.transform.rotation = targetRot;
        targetPlayerRot = targetRot;
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~attacking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Melee")]
    private bool attacking = false; //tracker false to allow for first attack, updated in UpdatePlayerStates when attack cooldown timer is 0
    [SerializeField] private float attackCooldownTimer = 0f, attackStartTime = 0f; //used to reset attack timer in UpdatePlayerStates

    [SerializeField] private float lightAttackCooldownMax = 0.5f, heavyAttackCooldownMax = 1f;
    private float tempAttackCooldownModifier = 1f; //players attack cooldown modifier
    public float GetTempAttackCooldownModifier() { return tempAttackCooldownModifier; }
    public void AlterTempAttackCooldownModifier(float alter) { tempAttackCooldownModifier += alter; } //increase/decrease temp movement speed
    public void SetTempAttackCooldownModifier(float newModifer) { tempAttackCooldownModifier = newModifer; } //set temp movement speed
    public void ResetTempAttackCooldownModifierAfter(float resetTimer)
    {
        //Debug.Log("starting reset temp attack cooldown, timer: " + resetTimer);
        StartCoroutine(ResetTempAttackCooldownModifier(resetTimer)); //reset temp movement speed after x seconds
    }
    public IEnumerator ResetTempAttackCooldownModifier(float resetTimer)
    {
        yield return new WaitForSeconds(resetTimer);
        ResetTempAttackCooldownModifier();
    }
    public void ResetTempAttackCooldownModifier() 
    {
        //Debug.Log("resetting temp attack cooldown modifier");
        tempAttackCooldownModifier = 1f; //reset temp movement speed to 1
    } 

    private float lightAttackAnimLength = 0.5f, heavyAttackAnimLength = 1f;
    private int attackComboDamage = 0; //additional damage
    public int GetAttackComboDamage() { return attackComboDamage; } //get additional damage
    private int lightAttackComboCounter = 0; //combo tracker set to one to skip first rotation
    private float lightAttackComboTimer = 0f, lightAttackComboStartTime = 0f; //used to reset combo timer
    [SerializeField] private float lightAttackComboTimerMax = 2.5f;
    [SerializeField] private GameObject weaponParent; //weapon parent
    [SerializeField] private GameObject weaponAttackCollider; //weapon collider
    [SerializeField] private PlayerWeaponColliderManager PWCM; //weapon collider script

    private int attackDamage = 5; //dafault damage
    public int GetAttackDamage() { return attackDamage; }
    private int attackDamageModifier = 0; //players attack damage modifier
    public int GetAttackDamageModifier() { return attackDamageModifier; }
    public void AlterAttackDamageModifier(int alter) { attackDamageModifier += alter; } //increase/decrease damage modifier
    public void SetAttackDamageModifier(int newModifer) { attackDamageModifier = newModifer; } //set damage modifier
    public void ResetAttackDamageModifierAfter(float resetTimer)
    {
        StartCoroutine(ResetAttackDamageModifier(resetTimer)); //reset damage modifier after x seconds
    }
    public IEnumerator ResetAttackDamageModifier(float resetTimer)
    {
        yield return new WaitForSeconds(resetTimer);
        ResetAttackDamageModifier();
    }
    public void ResetAttackDamageModifier() 
    {
        //Debug.Log("resetting attack damage modifier");
        attackDamageModifier = 0; //reset damage modifier to 0
    }

    [SerializeField] private float attackSpeed = 1f; //default speed
    private bool comboing = false;
    [SerializeField] private ParticleSystem PPS;

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //lightAttackAnimLength = FindAnimationLength("swordLightAttack30degrees");
            //Debug.Log("lightAttackAnimLength: " + lightAttackAnimLength);

            if (ctx.performed && attackCooldownTimer <= 0 && !attacking && !interacting)
            {
                //Debug.Log("light attack");
                //Debug.Log("combo: " + lightAttackComboCounter);

                if (lightAttackComboCounter == 0)
                {
                    attackComboDamage = 0; //reset any temp combo damage
                }


                //track light attack
                attackCooldownTimer = (lightAttackCooldownMax / tempAttackCooldownModifier);
                if(attackCooldownTimer == Mathf.Infinity && tempAttackCooldownModifier != 0) { attackCooldownTimer = 0.1f; } //stops timer setting to infinite if not intended
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
                PWCM.EnableAttackCheck(((lightAttackAnimLength) - 0.1f), "light");

                if (lightAttackComboCounter == 3)
                {
                    Invoke("ResetLightAttackAnimInt", (lightAttackAnimLength - 0.1f));
                    ADM.ComboPerformed();
                }
            }
        }
    }
    private void ResetLightAttackAnimInt()
    {
        if (active)
        {
            lightAttackComboCounter = 0; //reset counters
            lightAttackComboTimer = 0;
            a.SetInteger("lightSwingCombo", lightAttackComboCounter);
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //heavyAttackAnimLength = FindAnimationLength("swordHeavyAttackCleave");
            //Debug.Log("heavyAttackAnimLength: " + heavyAttackAnimLength);

            if (ctx.performed && attackCooldownTimer <= 0 && !attacking && !interacting)
            {
                //Debug.Log("heavy attack");
                attackCooldownTimer = (heavyAttackCooldownMax / tempAttackCooldownModifier);

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
                PWCM.EnableAttackCheck((FindAnimationLength("swordHeavyAttackCleave") - 0.1f), "heavy");

                //Debug.Log("invoke reset heavy attack, " + (heavyAttackAnimLength - 0.1f));
                Invoke("ResetHeavyAttackAnimBool", FindAnimationLength("swordHeavyAttackCleave"));
            }
        }
    }
    private void ResetHeavyAttackAnimBool() 
    {
        /*Debug.Log("reset heavy attack");*/ 
        a.SetBool("attackingHeavy", false); 
    }


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





    //~~~~~magic~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Magic")]
    //components
    [SerializeField] private string effectName;
    [SerializeField] private string elementName;
    [SerializeField] private string shapeName;

    //cooldown
    [SerializeField] private float spellCooldownTimer = 0f;
    [SerializeField] private float spellCooldownMax = 10f;
    private float spellStartTime = 0f;
    public void SetSpellCooldownTimer(float newCooldown) { spellCooldownTimer = newCooldown; }
    [SerializeField] private bool castable = true;
    private bool spellReady = false;

    //spell
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private SpellScript curSpell;
    public SpellScript GetCurSpell() { return curSpell; }
    [SerializeField] private LayerMask aimLayerMask = 0;
    public LayerMask GetAimLayerMask() { return aimLayerMask; }

    //random spell assigned on awake
    public void AssignSpell()
    {
        //Debug.Log("PlayerController, AssignSpell");

        if (castable && curSpell == null)
        {
            //Debug.Log("init spell");
            //instantiate new spell game object & reference it's script while updating it with spell components
            GameObject spellInstance = Instantiate(spellPrefab, this.transform.position, Quaternion.identity);
            spellInstance.transform.SetParent(leftHand.transform);
            spellInstance.transform.localPosition = Vector3.zero;
            curSpell = spellInstance.transform.GetChild(0).GetComponent<SpellScript>().StartSpellScript(ASM);
            //Debug.Log(curSpell);
        }


        //1f for base difficulty, 0.5f for easy, 1.5f for hard
        float spellStrength = 1f; //used by adaptive difficulty as a spell skill modifier
        if (curSpell != null)
        {
            //determine random spell shape, effect, and element
            switch (Random.Range(0, 1))
            {
                case 0:
                    shapeName = "Ball";
                    spellStrength *= 1f;
                    break;
                case 1:
                    shapeName = "Line";
                    spellStrength *= 0.5f;
                    break;
            }

            switch (Random.Range(0, 3))
            {
                case 0:
                    effectName = "Arc";
                    spellStrength *= 1f;
                    break;
                case 1:
                    effectName = "Chain";
                    spellStrength *= 0.5f;
                    break;
                case 2:
                    effectName = "Explode";
                    spellStrength *= 0.5f;
                    break;
            }

            switch (Random.Range(0, 4))
            {
                case 0:
                    elementName = "Electric";
                    spellStrength *= 1f;
                    break;
                case 1:
                    elementName = "Fire";
                    spellStrength *= 0.75f;
                    break;
                case 2:
                    elementName = "Force";
                    spellStrength *= 0.75f;
                    break;
                case 3:
                    elementName = "Water";
                    spellStrength *= 1.5f;
                    break;
            }

            //testing
            shapeName = "Ball";
            effectName = "Charge";
            elementName = "Fire";

            curSpell.UpdateSpellScriptShape(shapeName);
            curSpell.UpdateSpellScriptEffect(effectName);
            curSpell.UpdateSpellScriptElement(elementName);

            ADM.SetSpellStrength(spellStrength); //update adaptive difficulty
            spellCooldownMax = curSpell.GetSpellCooldownMax(); //update spell cooldown max
            spellReady = true;
        }
    }

    private bool castHeld = false; //used to track if the player is holding down the cast
    public bool GetCastHeld() { return castHeld; }

    public void OnCast(InputAction.CallbackContext context)
    {
        if (effectName.Contains("Automatic") || effectName.Contains("Charge"))
        {
            if (context.started) { castHeld = true; }
            if (context.canceled) 
            {
                castHeld = false;

                if(effectName.Contains("Charge"))
                {
                    //Debug.Log("PlayerController, CastSpell");
                    if (castable && spellReady) //if spell is castable
                    {
                        //Debug.Log("PlayerController, spell casted");
                        curSpell.CastSpell();
                        spellCooldownTimer = spellCooldownMax;
                        ADM.SpellRan(); //update adaptive difficulty
                    }
                }
            }
        }

        if (context.performed && !effectName.Contains("Automatic") && !effectName.Contains("Charge"))
        {
            //Debug.Log("PlayerController, CastSpell");
            if (castable && spellReady) //if spell is castable
            {
                //Debug.Log("PlayerController, spell casted");
                curSpell.CastSpell();
                spellCooldownTimer = spellCooldownMax;
                ADM.SpellRan(); //update adaptive difficulty
            }
            else
            {
                //Debug.Log("PlayerController, spell not casted");
            }
        }
    }
    //~~~~~magic~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



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
    [SerializeField] Animator vignetteOverlayAnimator;

    //health points
    public void SetCurrentHealthPoints(int newHealth) { if (!invincible) { /*Debug.Log("setting health: " + newHealth);*/ healthPointsCurrent = newHealth; HealthCheck(); } }
    public void AlterCurrentHealthPoints(int alter) { /*Debug.Log("altering health: " + alter);*/ healthPointsCurrent += alter; HealthCheck(); }
    public void DamageTarget(int alter) 
    {
        if (!invincible) 
        {
            //Debug.Log("damaging health: " + alter);
            //Debug.Log("resistance: " + resistanceModifier);
            alter += resistanceModifier; //apply resistance modifier
            //Debug.Log("damaging health after resistance: " + alter);
            if (alter > 0)
            {
                healthPointsCurrent -= alter;
                VignetteHit();
                HealthCheck();
                ADM.DamageTaken();
                ADM.AddDamageTaken(alter);
            }
        }
    }
    public int GetCurrentHealthPoints() { return healthPointsCurrent; }
    private void HealthCheck()
    {
        if (active)
        {
            if (healthPointsCurrent <= 0)
            {
                /*Debug.Log("health check: " + healthPointsCurrent);*/
                UpdatePlayerHealthBar();
                ASM.DestroyPlayer();
            }
        }
    }
    private void VignetteHit()
    {
        vignetteOverlayAnimator.SetBool("hit", true);
        Invoke("ResetVignetteHit", 0.5f);
    }
    private void ResetVignetteHit()
    {
        vignetteOverlayAnimator.SetBool("hit", false);
    }

    public void AlterMaxHealthPoints(int alter) { healthPointsMax += alter; }
    public int GetMaxHealthPoints() { return healthPointsMax; }

    //resistance
    private int resistanceModifier = 0; //players resistance modifier
    public int GetResistanceModifier() { return resistanceModifier; }
    public void AlterResistanceModifier(int alter) { resistanceModifier += alter; } //increase/decrease resistance modifier
    public void SetResistanceModifier(int newModifer) { resistanceModifier = newModifer; } //set resistance modifier
    public IEnumerator ResetResistanceModifierAfter(float resetTimer)
    {
        yield return new WaitForSeconds(resetTimer);
        ResetResistanceModifier();
    }
    public void ResetResistanceModifier() { resistanceModifier = 0; } //reset resistance modifier to 0

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
        if (active)
        {
            //invincibility
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



            //melee attack
            if (attackCooldownTimer > 0 && !attacking) //on attack
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



            //melee combo
            if (lightAttackComboTimer > 0 && !comboing) { comboing = true; } //when combo starts
            if (lightAttackComboTimer > 0) { lightAttackComboTimer -= Time.deltaTime; } //count down timer
            if (lightAttackComboTimer <= 0 && comboing)  //when timer runs out
            {
                lightAttackComboTimer = 0;
                lightAttackComboCounter = 0; //set tracker to start
                comboing = false; //set tracker false
                a.SetInteger("lightSwingCombo", lightAttackComboCounter);
            }



            //spell attack
            if(spellCooldownTimer > 0 && castable) //on cast
            {
                castable = false; //set tracker
                spellReady = false;
                spellStartTime = Time.time; //track when attack started
            }

            if(spellCooldownTimer > 0) { spellCooldownTimer -= Time.deltaTime; } //count down timer
            if(spellCooldownTimer <= 0 && !castable) //when timer runs out, set tracker false
            {
                spellCooldownTimer = 0;
                spellStartTime = 0;
                castable = true;
                curSpell = null;
                AssignSpell(); //assign new spell
            }

            //automatic spell attack
            if (castHeld && castable && curSpell.GetSpellCastable() && effectName.Contains("Automatic")) //if spell is castable
            {
                //Debug.Log("PlayerController, spell casted");
                curSpell.CastSpell();
                spellCooldownTimer = spellCooldownMax;
                ADM.SpellRan(); //update adaptive difficulty
            }
        }
    }
    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Interaction")]
    [SerializeField] private LayerMask interactionMask;
    private float interactDistance = 2.5f, interactCooldown = .25f;
    private bool interacting = false;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (active)
        {
            //E / Right Bumper
            if (ctx.performed && !interacting)
            {
                //Debug.Log("interact");

                RaycastHit hit;
                if (Physics.Raycast(playerCamera.transform.position, (playerCamera.transform.forward * interactDistance), out hit, interactDistance, interactionMask))
                {
                    //Debug.Log("interact hit " + hit.collider.name);
                    //Debug.Log("tag " + hit.collider.tag);
                    interacting = true;

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
                        case "Consumable":
                            //Debug.Log("consumable, " + hit.collider.GetComponent<AbstractConsumable>());
                            hit.collider.GetComponent<AbstractConsumable>().Interact();
                            break;
                    }
                }

                StartCoroutine(ResetInteraction());
            }
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
    [SerializeField] private GameObject HUDParent;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private RectTransform playerHealthBarRect;
    [SerializeField] private Image playerHealthBarImage;
    private float maxPlayerHealthBarWidth;

    public void ToggleHUD(bool newState)
    {
        HUDParent.SetActive(newState);
    }

    private void UpdatePlayerHealthBar()
    {
        //update health bar
        float hpPercentage = (float)healthPointsCurrent / (float)healthPointsMax;
        //Debug.Log("hpPercentage: " + hpPercentage);
        playerHealthBarRect.sizeDelta = new Vector2(maxPlayerHealthBarWidth * hpPercentage, playerHealthBarRect.sizeDelta.y);
        playerHealthText.text = healthPointsCurrent + " / " + healthPointsMax;

        if (invincible)
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

        //Debug.Log("vignetteOverlayAnimator: " + vignetteOverlayAnimator);
        vignetteOverlayAnimator.SetFloat("healthPercentage", hpPercentage);
        //Debug.Log("healthPercentage: " + vignetteOverlayAnimator.GetFloat("healthPercentage"));
    }

    private void UpdateInteractionPrompt()
    {
        if (active)
        {
            //update interaction prompt
            RaycastHit hit;
            Debug.DrawRay(playerCamera.transform.position, (playerCamera.transform.forward * interactDistance), Color.red, 1f);
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactDistance, interactionMask))
            {
                string promptText = hit.collider.tag;
                switch (hit.collider.tag)
                {
                    case "Door":
                        promptText = "door";
                        break;
                    case "Portal":
                        promptText = "exit portal";
                        break;
                    case "Consumable":
                        //remove 'prefab' from name string
                        string consumableName = hit.collider.name;
                        consumableName = consumableName.Replace("Prefab", "");
                        promptText = consumableName;
                        break;
                    default:
                        promptText = "???";
                        break;
                }

                interactionPromptText.text = "'RB' to interact with " + promptText;
            }
            else { interactionPromptText.text = ""; }
        }
    }
    //~~~~~HUD~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void OnToggleDevMode(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            //update scene manager with dev status
            if(ASM.GetDevMode())
            {
                ASM.SetDevMode(false);
            }
            else { ASM.SetDevMode(true); }

            //display debug infos
            if (ASM.GetDevMode())
            {
                DbugDisplay.SetActive(true);
                ADDbugDisplay.SetActive(true);
                //SVDisplay.SetActive(true);
                SpellDbugDisplay.SetActive(true);
            }
            else
            {
                DbugDisplay.SetActive(false);
                ADDbugDisplay.SetActive(false);
                //SVDisplay.SetActive(false);
                SpellDbugDisplay.SetActive(false);
            }
        }
    }


    public void OnResetDungeon(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //reset dungeon
            Debug.Log("reset dungeon");
            ASM.RestartScene();
        }
    }
    //~~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
