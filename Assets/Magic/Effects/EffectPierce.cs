using UnityEngine;

public class EffectPierce : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 1; damageModifier = 0.5f; speedModifier = 2f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Pierce effect applied");
    }
}
