using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Misc")]
    [SerializeField] private Rigidbody playerRigid; //player rigidbody used for physics interactions
    [SerializeField] private Camera playerCamera; //player camera used for looking and object interactions
    [SerializeField] private TMP_Text interactionPromptText; //object interaction display

    private DbugDisplayController DDC; //debug display
    public void SetDDC(DbugDisplayController DDC) { this.DDC = DDC; }

    private void FixedUpdate()
    {
        if (DDC != null) { DDC.playerPosition = playerRigid.position; } //update debug display

        UpdatePlayerMovement();
        UpdatePlayerLooking();
        UpdateInteractionPrompt();
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Movement")]
    //moving
    [SerializeField] private float movementSpeed = 2.5f; //players movement velocity
    private Quaternion targetPlayerRot = Quaternion.identity; //used to lerp player rotation
    private Vector3 movement = Vector3.zero; //players movement directions
    private Vector3 playerVelocity = Vector3.zero; //used to calc movement

    //dodging
    [SerializeField] private LayerMask dodgeMask; //enemy layer
    [SerializeField] private float dodgeForce = 10f; //--------|
    [SerializeField] private float dodgeDuration = 0.25f; //---|- each used to calc dodge
    [SerializeField] private float dodgeDistance = 5f; //------|
    private Vector3 dodgeVelocity = Vector3.zero; //used to calc directional velocity
    private bool dodging = false; //tracker
    private float dodgeCD = 0f; //current dodge cooldown time
    [SerializeField] private float dodgeCDMax = 0.5f; //time between dodges
    private float dodgeStartTime = 0f; //track real time dodge began

    //jumping
    [SerializeField] private float jumpForce = 10f;
    private bool jumping = false;
    private bool grounded = false;

    //looking
    [SerializeField] private float lookSensitivity = 2.5f; //players looking velocity
    private Quaternion targetCameraRot = Quaternion.identity; //used to lerp camera rotation
    private Vector2 lookment = Vector2.zero;
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
        if (ctx.performed && dodgeCD <= 0 && !dodging) //if dodge cooldown is not active and the player is not currently dodging
        {
            Debug.Log("dodge");

            dodgeStartTime = Time.time; //remember time when dodge started
            dodgeCD = dodgeCDMax; //set cooldown timer
            dodging = true; //set tracker to true
            Physics.IgnoreLayerCollision(gameObject.layer, dodgeMask, true); //allow dodging through enemies

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
        if (!dodging) //if player is moving as normal
        {
            //add velocity force in direction moving
            playerVelocity = (((playerRigid.transform.right * movement.x) * movementSpeed) + ((playerRigid.transform.forward * movement.z) * movementSpeed));
            playerRigid.AddForce(playerVelocity - playerRigid.velocity, ForceMode.VelocityChange);
        }
        else if (dodging) //if player is dodging
        {
            playerRigid.velocity = dodgeVelocity; //set immediate velocity to calced velocity

            if ((Time.time - dodgeStartTime) >= dodgeDuration) //if the player has been dodging for 1 second
            {
                playerRigid.velocity = Vector3.zero; //reset velocity
                Physics.IgnoreLayerCollision(gameObject.layer, dodgeMask, false); //disallow dodging through enemies
                dodging = false; //set tracker to false
            }
        }

        if (dodgeCD > 0) { dodgeCD -= Time.deltaTime; } //count down cooldown 


        if (DDC != null) { DDC.playerVelocity = playerVelocity; }
    }


    public void OnLook(InputAction.CallbackContext ctx)
    {
        //Mouse / Right Thumbstick
        //Debug.Log(ctx.ReadValue<Vector2>());
        lookment = ctx.ReadValue<Vector2>();
    }
    private void UpdatePlayerLooking()
    {
        xRot -= (lookment.y * lookSensitivity);
        //Debug.Log(xRot);
        xRot = Mathf.Clamp(xRot, -70f, 70f);

        targetCameraRot = Quaternion.Euler(xRot, 0f, 0f);
        playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, targetCameraRot, (Time.deltaTime / 0.1f));

        targetPlayerRot *= Quaternion.Euler(0f, (lookment.x * lookSensitivity), 0f);
        playerRigid.transform.rotation = Quaternion.Lerp(playerRigid.transform.localRotation, targetPlayerRot, (Time.deltaTime / 0.1f));
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Interaction")]
    [SerializeField] private LayerMask interactionMask;
    private float interactDistance = 2.5f, interactCooldown = .25f;
    private bool interacting = false;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        //E / Button West
        if (!interacting)
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



    private void UpdateInteractionPrompt()
    {
        RaycastHit hit;
        Debug.DrawRay(playerCamera.transform.position, (playerCamera.transform.forward * interactDistance), Color.red, 1f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactDistance, interactionMask))
        {
            interactionPromptText.text = "'E' to interact with " + hit.collider.tag;
        }
        else { interactionPromptText.text = ""; }
    }
    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("-Stat")]
    [SerializeField] private int CurrentHealthPoints = 10;
    [SerializeField] private int MaxHealthPoints = 10;
    [SerializeField] private int CurrentStaminaPoints = 10;
    [SerializeField] private int MaxStaminaPoints = 10;
    [SerializeField] private int CurrentMagicPoints = 10, MaxMagicPoints = 10;
    [SerializeField] private int StrengthPoints = 1, DexterityPoints = 1, IntelligencePoints = 1, WisdomPoints = 1;

    //health points
    public void AlterCurrentHealthPoints(int alter) { CurrentHealthPoints += alter; }
    public int GetCurrentHealthPoints() { return CurrentHealthPoints; }

    public void AlterMaxHealthPoints(int alter) { MaxHealthPoints += alter; }
    public int GetMaxHealthPoints() { return MaxHealthPoints; }

    //stamina points
    public void AlterCurrentStaminaPoints(int alter) { CurrentStaminaPoints += alter; }
    public int GetCurrentStaminaPoints() { return CurrentStaminaPoints; }

    public void AlterMaxStaminaPoints(int alter) { MaxStaminaPoints += alter; }
    public int GetMaxStaminaPoints() { return MaxStaminaPoints; }

    //magic points
    public void AlterCurrentMagicPoints(int alter) { CurrentMagicPoints += alter; }
    public int GetCurrentMagicPoints() { return CurrentMagicPoints; }

    public void AlterMaxMagicPoints(int alter) { MaxMagicPoints += alter; }
    public int GetMaxMagicPoints() { return MaxMagicPoints; }

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
    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
