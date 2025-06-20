using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementForce : AbstractElement
{
    private Vector3 startPos, endPos, dir;
    void Awake() { damageModifier = 0.25f; speedModifier = 1f; radiusModifier = 2f; cooldownModifier = 1.5f; }
    public override void ApplyElement(SpellScript SS)
    {
        //Debug.Log("Force element applied");
        this.SS = SS;
        SS.SetDamageType("force");

        //set spell colour
        Material elementMaterial = Resources.Load<Material>("Materials/Spells/ElementForceMaterial");
        SS.SetSpellColour(elementMaterial);
    }

    public override void SetupCondition()
    {
        startPos = SS.GetStartPos();
        endPos = SS.GetEndPos();
        dir = (endPos - startPos).normalized; //calculate direction enemy will move
        dir = new Vector3(dir.x, 0, dir.z); //remove y component
        //Debug.Log(dir);
    }
    public override void ApplyCondition()
    {
        //Debug.Log("Force element condition");

        GameObject[] targets = SS.GetSpellTargets();
        for (int i = 0; i < targets.Length; i++) { Debug.Log("Element target" + i + ": " + targets[i]); }

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                //apply forced condition
                ConditionForced forcedCondition = targets[i].GetComponent<ConditionForced>();
                if (forcedCondition == null)   //if target isnt already forced, apply
                {
                    forcedCondition = targets[i].AddComponent<ConditionForced>();
                    forcedCondition.SetDir(dir);
                    forcedCondition.ApplyCondition();
                }
            }
        }
    }
}