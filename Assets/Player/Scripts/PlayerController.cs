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
    [SerializeField] private Rigidbody playerRigid;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text interactionPromptText;

    private DbugDisplayController DDC;
    public void SetDDC(DbugDisplayController DDC) { this.DDC = DDC; }

    private void FixedUpdate()
    {
        if (DDC != null) { DDC.playerPosition = playerRigid.position; }
        UpdatePlayerMovement();
        UpdateInteractionPrompt();
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private int CurrentHealthPoints = 10, MaxHealthPoints = 10, CurrentStaminaPoints = 10, MaxStaminaPoints = 10, CurrentMagicPoints = 10, MaxMagicPoints = 10;
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





    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private float movementSpeed = 5f;
    private float lookSensitivity = 0.1f; //for some reason [SerializeField] sets sensitivity to 1 on start
    private Vector3 velocity = Vector3.zero;
    private Vector3 movement = Vector3.zero;
    private float xRot = 0f;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        //W/A/S/D / Right Thumbstick
        Vector2 input = ctx.ReadValue<Vector2>();
        movement = new Vector3(input.x, 0, input.y);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        //Mouse / Left Thumbstick
        //Debug.Log(ctx.ReadValue<Vector2>());
        float lookX = ctx.ReadValue<Vector2>().x;
        float lookY = ctx.ReadValue<Vector2>().y;

        xRot -= (lookY * lookSensitivity);
        //Debug.Log(xRot);
        xRot = Mathf.Clamp(xRot, -70f, 70f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        playerRigid.transform.Rotate(Vector3.up * (lookX * lookSensitivity));
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        //Space / Button South
        Debug.Log("jump");
    }



    private void UpdatePlayerMovement()
    {
        velocity = (((playerRigid.transform.right * movement.x) * movementSpeed) + ((playerRigid.transform.forward * movement.z) * movementSpeed));
        playerRigid.AddForce(velocity - playerRigid.velocity, ForceMode.VelocityChange);
        if (DDC != null) { DDC.playerVelocity = velocity; }
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
}
