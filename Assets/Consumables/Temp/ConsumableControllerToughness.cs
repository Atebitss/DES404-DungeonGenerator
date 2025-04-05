using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerToughness : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        Debug.Log("ConsumableControllerToughness, applying effect to player");
        PC.SetResistanceModifier(1); //doubles the player's resistance modifier
        PC.ResetResistanceModifierAfter(10f); //resets the player's resistance modifier after 10 seconds
    }
}
