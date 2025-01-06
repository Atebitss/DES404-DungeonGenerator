using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

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
        Debug.DrawRay(transform.position, new Vector3(transform.position.x * interactDistance, transform.position.y, transform.position.z), Color.red);
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
    private float movementSpeed = 5f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 movement = Vector3.zero;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        movement = new Vector3(input.x, 0, input.y);
    }

    private void UpdatePlayerMovement()
    {
        velocity = (movement * movementSpeed);
        playerRigid.AddForce(velocity - playerRigid.velocity, ForceMode.VelocityChange);
        if (DDC != null) { DDC.playerVelocity = velocity; }
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~




    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private LayerMask interactionMask;
    private float interactDistance = 100f;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        //E
        Debug.Log("interact");
        RaycastHit hit;
        /*if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, interactDistance, interactionMask))
        { 
            Debug.DrawRay(transform.position, hit.transform.position, Color.red);
        }*/
    }
    //~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
