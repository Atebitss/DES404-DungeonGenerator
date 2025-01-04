using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //~~~~~~player~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private Rigidbody playerRigid;
    [SerializeField] private Camera playerCamera;

    void FixedUpdate()
    {
        playerRigid.velocity = (movement * movementSpeed);
    }
    //~~~~~player~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



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
    private Vector3 movement;


    public void OnW(InputAction.CallbackContext ctx)
    {
        //W
        Debug.Log("W");
        if (ctx.performed){movement += new Vector3(0, 0, 1);}
        if(ctx.canceled){movement += new Vector3(0, 0, -1);}
    }
    public void OnS(InputAction.CallbackContext ctx)
    {
        //S
        if (ctx.performed){movement += new Vector3(0, 0, -1);}
        if (ctx.canceled){movement += new Vector3(0, 0, 1);}
    }
    public void OnA(InputAction.CallbackContext ctx)
    {
        //A
        if (ctx.performed){movement += new Vector3(-1, 0, 0);}
        if (ctx.canceled){movement += new Vector3(1, 0, 0);}
    }
    public void OnD(InputAction.CallbackContext ctx)
    {
        //D
        if (ctx.performed){movement += new Vector3(1, 0, 0);}
        if (ctx.canceled){movement += new Vector3(-1, 0, 0);}
    }
    //~~~~~movement~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
