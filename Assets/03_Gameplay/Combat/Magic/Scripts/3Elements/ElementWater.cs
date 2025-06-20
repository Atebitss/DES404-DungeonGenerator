using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementWater : AbstractElement
{
    void Awake() { damageModifier = 0.25f; speedModifier = 1f; radiusModifier = 3f; cooldownModifier = 0.25f; }
    public override void ApplyElement(SpellScript SS)
    {
        //Debug.Log("Water element applied");
        this.SS = SS;
        SS.SetDamageType("water");

        //set spell colour
        Material elementMaterial = Resources.Load<Material>("Materials/Spells/ElementWaterMaterial");
        SS.SetSpellColour(elementMaterial);
    }

    public override void SetupCondition() { }
    public override void ApplyCondition()
    {
        //Debug.Log("Water element condition");

        GameObject[] targets = SS.GetSpellTargets();
        for (int i = 0; i < targets.Length; i++) { Debug.Log("Element target" + i + ": " + targets[i]); }

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                //apply burning condition
                ConditionSoaked soakedCondition = targets[i].GetComponent<ConditionSoaked>();
                if (soakedCondition == null)   //if target isnt already soaked, apply
                {
                    soakedCondition = targets[i].AddComponent<ConditionSoaked>();
                    soakedCondition.ApplyCondition();
                }                               //if target is soaked, increase timer
                else if (soakedCondition != null) { soakedCondition.AlterConditionTime(15); }
            }
        }
    }
}
