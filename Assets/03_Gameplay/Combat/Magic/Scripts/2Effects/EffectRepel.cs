using UnityEngine;

public class EffectRepel : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 1; damageModifier = 1f; speedModifier = .5f; radiusModifier = 1f; cooldownModifier = 1f; //set component weights for spell script to use
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        SS.gameObject.tag = "Repel"; //set spell object tag to Repel
        //add new sepertaion force script to spell object
        SeperationForce sf = SS.gameObject.AddComponent<SeperationForce>();

        switch (SS.GetShapeName())
        {
            case "Ball":
                sf.SetSeperationDistance(50f); //set sepertaion distance
                sf.SetSeperationForce(100f); //set seperation force
                break;
            case "Beam":
                sf.SetSeperationDistance(100f);
                sf.SetSeperationForce(1f);
                break;
            case "Field":
                sf.SetSeperationDistance(100f);
                sf.SetSeperationForce(1f);
                break;
            default:
                sf.SetSeperationDistance(1f);
                sf.SetSeperationForce(1f);
                break;
        }
    }
}
