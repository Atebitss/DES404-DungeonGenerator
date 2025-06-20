using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerMovement : AbstractConsumable
{
    private float consumableTimer = 10f; //length of consumable
    private void Awake()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        consumableType = "Movement";
        consumableTime = consumableTimer;
    }
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerMovement, applying effect to player");
        PC.SetTempMovementSpeed(PC.GetMovementSpeed()); //doubles the player's movement speed
        PC.ResetTempMovementAfter(consumableTimer); //resets the player's movement speed after 10 seconds
    }
}
