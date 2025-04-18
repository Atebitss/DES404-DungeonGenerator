using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerInvincibility : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerInvincibility, applying effect to player");
        PC.MakePlayerInvincible(5f); //make the player invincible for 5 seconds
    }
}
