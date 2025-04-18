using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableControllerHealing : AbstractConsumable
{
    public override void ApplyEffect(PlayerController PC)
    {
        //Debug.Log("ConsumableControllerHealing, applying effect to player");
        int maxPlayerHealth = PC.GetMaxHealthPoints(); //get max player health
        int currentPlayerHealth = PC.GetCurrentHealthPoints(); //get current player health

        float healAmount = (maxPlayerHealth * 0.25f); //calculate x% of max player health

        //if heal amount is not a whole number
        if (healAmount % 1 != 0)
        {
            //round heal amount up to next whole number
            //ie. 1.43 becomes 2
            healAmount = Mathf.Ceil(healAmount);
        }

        //if heal amount would exceed max health, set player to max health
        if ((currentPlayerHealth + healAmount) > maxPlayerHealth)
        {
            PC.SetCurrentHealthPoints(maxPlayerHealth);
        }
        //otherwise, heal player
        else
        {
            PC.AlterCurrentHealthPoints((int)healAmount);
        }
    }
}
