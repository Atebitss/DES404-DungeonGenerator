using UnityEngine;

public class EffectCharge : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 0; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        //if player is holding cast
        //increase spell damage and radius

        if(SS.GetPlayerController().GetCastHeld())
        {
            Debug.Log("Charge effect applied");

            if(damageModifier < 2f) //limit max damage increase to 100%
            {
                damageModifier += 0.01f; //increase damage by 10%
            }

            if (radiusModifier < 2f) //limit max radius increase to 100%
            {
                radiusModifier += 0.01f; //increase radius by 10%
            }

            SS.transform.localScale = Vector3.one * radiusModifier;
        }
    }
}
