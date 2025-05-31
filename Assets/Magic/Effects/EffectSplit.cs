using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSplit : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 2; damageModifier = 2f; speedModifier = 0.75f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Split effect applied");
        SS.AlterRadius(0.5f);
    }
}