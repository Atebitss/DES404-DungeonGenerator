using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerInvincibility : AbstractConsumable
{
    private float consumableTimer = 5f; //length of consumable
    private void Awake()
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        consumableType = "Invincibility";
        consumableTime = consumableTimer;
    }
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerInvincibility, applying effect to player");
        PC.MakePlayerInvincible(consumableTimer); //make the player invincible for 5 seconds
    }
}
