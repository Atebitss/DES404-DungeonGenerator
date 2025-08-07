using UnityEngine;

public class EffectDelay : AbstractEffect
{
    private float delayTime = 0.5f; //delay time

    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 1; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 0.5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        //delays the spell upon cast
        if (callCounter == 0)
        {
            StartCoroutine(SS.GetShapeScript().DelayCast(delayTime));

            if(SS.GetShapeName().Contains("Field")) { shapeScript.maxRunTime = (shapeScript.maxRunTime * 2); }

            callCounter++;
        }
    }
}
