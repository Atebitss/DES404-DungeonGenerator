using UnityEngine;

public class EffectAutomatic : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = -1; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 0.1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Automatic effect applied");
    }
}
