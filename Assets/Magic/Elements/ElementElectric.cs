using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementElectric : AbstractElement
{
    void Awake() { damageModifier = 1.25f; speedModifier = 1f; }
    public override void ApplyElement(SpellScript SS)
    {
        //Debug.Log("Electric element applied");
        this.SS = SS;
        SS.SetDamageType("electric");

        //set spell colour
        Material elementMaterial = Resources.Load<Material>("SpellMaterials/ElementElectricMaterial");
        SS.SetSpellColour(elementMaterial);
    }

    public override void SetupCondition() { }
    public override void ApplyCondition()
    {
        //Debug.Log("Electric element condition");

        GameObject[] targets = SS.GetSpellTargets();
        //for (int i = 0; i < targets.Length; i++) { Debug.Log("Element target" + i + ": " + targets[i]); }

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                //apply electric condition
                ConditionShocked electricCondition = targets[i].GetComponent<ConditionShocked>();
                if (electricCondition == null)   //if target isnt already shocked, apply
                {
                    electricCondition = targets[i].AddComponent<ConditionShocked>();
                    electricCondition.ApplyCondition();
                }                               //if target is shocked, increase timer
                else if (electricCondition != null) { electricCondition.AlterConditionTime(1); }
            }
        }
    }
}