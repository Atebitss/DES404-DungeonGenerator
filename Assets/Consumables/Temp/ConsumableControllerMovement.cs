using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerMovement : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerMovement, applying effect to player");
        PC.SetTempMovementSpeed(PC.GetMovementSpeed()); //doubles the player's movement speed
        PC.ResetTempMovementAfter(10f); //resets the player's movement speed after 10 seconds
    }
}
