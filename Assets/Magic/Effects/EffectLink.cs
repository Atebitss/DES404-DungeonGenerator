using UnityEngine;

public class EffectLink : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 0.25f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 0.5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Explosion effect applied");
    }
}
