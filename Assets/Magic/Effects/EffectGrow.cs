using UnityEngine;

public class EffectGrow : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 4; damageModifier = 0.25f; speedModifier = 0.5f; radiusModifier = 5f; cooldownModifier = 2f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Explosion effect applied");
    }
}
