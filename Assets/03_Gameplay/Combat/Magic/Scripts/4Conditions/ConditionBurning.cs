using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionBurning : AbstractCondition
{
    void Update()
    {
        if (curDuration >= triggerTime) { targetScript.DamageTarget(1, "fire"); triggerTime++; }

        if (curDuration >= duration) { EndCondition(); }
        else { curDuration += Time.deltaTime; }
    }


    public override void ApplyCondition()
    {
        //Debug.Log("Burning condition applied to " + this.gameObject.name);
        //add burning animation

        duration = 5;
        curDuration = 0;

        targetScript = this.GetComponent<AbstractEnemy>();
        targetStatusDisplay = this.GetComponent<StatusVisualManager>();

        //set enemy colour & icon display
        Material elementMaterial = Resources.Load<Material>("Materials/Spells/ElementFireMaterial");
        targetScript.SetMaterial(elementMaterial);
        targetStatusDisplay.ApplyVisual("Burning", duration);
    }


    void OnDestroy()
    {
        //Debug.Log("Burning condition ended on " + this.gameObject.name);
        targetScript.ResetMaterial();
    }
}
