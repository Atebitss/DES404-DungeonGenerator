using UnityEngine;

public class EffectCharge : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 0; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;

        damageIncrement = damageModifier * 0.01f; // 10% of current damage modifier
        radiusIncrement = radiusModifier * 0.01f; // 10% of current radius modifier
    }
    public override void ApplyEffect()
    {
        //if player is holding cast
        //increase spell damage and radius
        callCounter++;

        if (SS.GetPlayerController().GetCastHeld() && callCounter >= 5)
        {
            Debug.Log("Charge effect applied");

            if(damageModifier < 2f) //limit max damage increase to 100%
            {
                damageModifier += damageIncrement; //increase damage by 10%
            }

            if (radiusModifier < 2f) //limit max radius increase to 100%
            {
                radiusModifier += radiusIncrement; //increase radius by 10%
            }

            SS.UpdateRadius();
            callCounter = 0; //reset counter after applying effect
        }
    }
}
