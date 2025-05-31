using UnityEngine;

public class EffectBlock : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = -1; damageModifier = 0.25f; speedModifier = 2f; radiusModifier = 1f; cooldownModifier = 0.5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Block effect applied");
    }
}
