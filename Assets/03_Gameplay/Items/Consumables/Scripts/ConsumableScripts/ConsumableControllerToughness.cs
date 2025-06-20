using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerToughness : AbstractConsumable
{
    private float consumableTimer = 10f; //length of consumable
    private void Awake() 
    {
        ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        consumableType = "Toughness"; 
        consumableTime = consumableTimer;
    }
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerToughness, applying effect to player");
        PC.SetResistanceModifier(1); //doubles the player's resistance modifier
        PC.ResetResistanceModifierAfter(consumableTimer); //resets the player's resistance modifier after 10 seconds
    }
}
