using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionSoaked : AbstractCondition
{
    void Update()
    {
        if (curDuration >= duration) { EndCondition(); }
        else { curDuration += Time.deltaTime; }
    }


    public override void ApplyCondition()
    {
        //Debug.Log("Soaked condition applied to " + this.gameObject.name);
        //add dripping animation

        targetScript = this.GetComponent<AbstractEnemy>();

        //set enemy colour
        Material elementMaterial = Resources.Load<Material>("SpellMaterials/ElementWaterMaterial");
        targetScript.SetMaterial(elementMaterial);

        duration = 60;
        curDuration = 0;
    }


    void OnDestroy()
    {
        targetScript.ResetMaterial();
    }
}