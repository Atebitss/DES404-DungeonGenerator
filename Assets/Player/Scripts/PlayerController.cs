using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    
    private DbugDisplayController DDC;
    public void SetDDC(DbugDisplayController DDC) { this.DDC = DDC; }

    private void FixedUpdate()
    {
        if (DDC != null) { DDC.playerPosition = playerRigid.position; }
        UpdatePlayerMovement();
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
        Vector2 input = ctx.ReadValue<Vector2>();
        movement = new Vector3(input.x, 0, input.y);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.ReadValue<Vector2>());
        float lookX = ctx.ReadValue<Vector2>().x;
        float lookY = ctx.ReadValue<Vector2>().y;

        xRot -= (lookY * lookSensitivity);
        //Debug.Log(xRot);
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        playerRigid.transform.Rotate(Vector3.up * (lookX * lookSensitivity));
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("jump");
    }



    private void UpdatePlayerMovement()
    {
        velocity = ((playerRigid.transform.right * movement.x) + (playerRigid.transform.forward * movement.z) * movementSpeed);
        playerRigid.AddForce(velocity - playerRigid.velocity, ForceMode.VelocityChange);
        if (DDC != null) { DDC.playerVelocity = velocity; }
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~





    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private LayerMask interactionMask;
    private float interactDistance = 10f, interactCooldown = .5f;
    private bool interacting = false;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        //E
        if (!interacting)
        {
            Debug.Log("interact");

            interacting = true;
            RaycastHit hit;

            Debug.DrawRay(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + interactDistance)), Color.red, 100f);
            if (Physics.Raycast(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + interactDistance)), out hit, interactDistance, interactionMask))
            {
                Debug.Log("interact hit " + hit.collider.name);
            }

            StartCoroutine(ResetInteraction());
        }
    }
    IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactCooldown);
        Debug.Log("interact reset");
        interacting = false;
    }
    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
