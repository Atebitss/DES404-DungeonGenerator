using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementNull : AbstractElement
{
    void Awake() { damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f; }
    public override void ApplyElement(SpellScript SS)
    {
        //Debug.Log("Null element applied");
        this.SS = SS;
        SS.SetDamageType("null");
    }

    public override void SetupCondition() { }
    public override void ApplyCondition() { }
}
