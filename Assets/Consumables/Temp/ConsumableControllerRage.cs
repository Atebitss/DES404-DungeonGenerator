using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerRage : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        Debug.Log("ConsumableControllerRage, applying effect to player");
        PC.SetTempAttackCooldownModifier(2f); //doubles the player's attack cooldown modifier
        PC.ResetTempAttackCooldownModifierAfter(10f); //resets the player's attack cooldown modifier after 10 seconds
    }
}
