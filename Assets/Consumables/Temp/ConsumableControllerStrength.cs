using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerStrength : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        Debug.Log("ConsumableControllerStrength, applying effect to player");
        PC.SetAttackDamageModifier(1); //increases the player's damage modifier
        PC.ResetAttackDamageModifierAfter(10f); //resets the player's damage modifier after 10 seconds
    }
}
