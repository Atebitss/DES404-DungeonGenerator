using UnityEngine;

public class EffectPierce : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = -1; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;

        if (!SS.GetSpellPersist()) //set once
        {
            Debug.Log("Pierce effect applied");
            SS.SetSpellPersist(true);
        }
    }
    public override void ApplyEffect()
    {
    }
}
