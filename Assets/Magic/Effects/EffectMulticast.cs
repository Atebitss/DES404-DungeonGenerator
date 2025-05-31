using UnityEngine;

public class EffectMulticast : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 1; damageModifier = 0.5f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 2f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Explosion effect applied");
    }
}
