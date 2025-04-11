using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementFire : AbstractElement
{
    void Awake() { damageModifier = 1.5f; speedModifier = 1f; }
    public override void ApplyElement(SpellScript SS)
    {
        //Debug.Log("Fire element applied");
        this.SS = SS;
        SS.SetDamageType("fire");

        //set spell colour
        Material elementMaterial = Resources.Load<Material>("SpellMaterials/ElementFireMaterial");
        SS.SetSpellColour(elementMaterial);
    }

    public override void SetupCondition() { }
    public override void ApplyCondition()
    {
        //Debug.Log("Fire element condition");

        GameObject[] targets = SS.GetSpellTargets();
        //for (int i = 0; i < targets.Length; i++) { Debug.Log("Element target" + i + ": " + targets[i]); }

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                //Debug.Log("Element target" + i + ": " + targets[i]);

                //apply burning condition
                ConditionBurning burningCondition = targets[i].GetComponent<ConditionBurning>();
                if (burningCondition == null)   //if target isnt already burning, apply
                {
                    burningCondition = targets[i].AddComponent<ConditionBurning>();
                    burningCondition.ApplyCondition();
                }                               //if target is burning, increase timer
                else if (burningCondition != null) { burningCondition.AlterConditionTime(3); }
            }
        }
    }
}
