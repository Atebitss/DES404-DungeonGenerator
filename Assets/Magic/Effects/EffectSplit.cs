using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSplit : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 0.5f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1.5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Split effect applied");

        //create 2-5 spells
        //modify spell damage, size and radius by the number of spells created
        //send spells in random directions

        SS.AlterRadius(0.5f);
    }
}