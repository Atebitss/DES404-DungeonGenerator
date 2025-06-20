using UnityEngine;

public class EffectNull : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = -1; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
    }
}
