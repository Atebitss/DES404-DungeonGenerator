using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerStrength : AbstractConsumable
{
    private float consumableTimer = 10f; //length of consumable
    private void Awake()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        consumableType = "Strength";
        consumableTime = consumableTimer;
    }
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerStrength, applying effect to player");
        PC.SetAttackDamageModifier(1); //increases the player's damage modifier
        PC.ResetAttackDamageModifierAfter(consumableTimer); //resets the player's damage modifier after 10 seconds
    }
}
