using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerRage : AbstractConsumable
{
    private float consumableTimer = 10f; //length of consumable
    private void Awake()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        consumableType = "Rage";
        consumableTime = consumableTimer;
    }
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerRage, applying effect to player");
        PC.SetTempAttackCooldownModifier(2f); //doubles the player's attack cooldown modifier
        PC.ResetTempAttackCooldownModifierAfter(consumableTimer); //resets the player's attack cooldown modifier after 10 seconds
    }
}
