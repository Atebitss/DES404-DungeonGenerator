using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionShocked : AbstractCondition
{
    void Update()
    {
        if (curDuration >= duration) { EndCondition(); }
        else { curDuration += Time.deltaTime; }
    }


    public override void ApplyCondition()
    {
        //Debug.Log("Shocked condition applied to " + this.gameObject.name);
        //add shock animation

        targetScript = this.GetComponent<AbstractEnemy>();

        //set enemy colour
        Material elementMaterial = Resources.Load<Material>("SpellMaterials/ElementElectricMaterial");
        targetScript.SetMaterial(elementMaterial);

        duration = 2;
        curDuration = 0;

        if (this.gameObject.GetComponent<ConditionSoaked>()) { /*Debug.Log(targetScript.gameObject.name + " already soaked, extra electric damage");*/ targetScript.AlterHealth(-10); }
        else if (!this.gameObject.GetComponent<ConditionSoaked>()) { targetScript.AlterHealth(-2); }
    }


    void OnDestroy()
    {
        targetScript.ResetMaterial();
    }
}