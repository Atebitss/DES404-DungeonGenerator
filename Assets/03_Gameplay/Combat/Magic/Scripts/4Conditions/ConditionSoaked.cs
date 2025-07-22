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
        targetStatusDisplay = this.GetComponent<StatusVisualManager>();

        //set enemy colour
        Material elementMaterial = Resources.Load<Material>("Materials/Spells/ElementWaterMaterial");
        targetScript.SetMaterial(elementMaterial);
        targetStatusDisplay.ApplyVisual("Soaked", duration);

        duration = 60;
        curDuration = 0;
    }


    void OnDestroy()
    {
        targetScript.ResetMaterial();
    }
}