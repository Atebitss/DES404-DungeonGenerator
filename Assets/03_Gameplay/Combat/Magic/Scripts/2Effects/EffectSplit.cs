using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSplit : AbstractEffect
{
    private int splitMin = 2, splitMax = 2, usedSign = 0;
    private bool triggered = false;

    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        if (!triggered)
        {
            Debug.Log("Split effect applied");

            //create 2-5 spells
            //modify spell damage, size and radius by the number of spells created
            //send spells in random directions
            triggered = true; //ensure this effect only runs once per spell
            Vector3 currentPos = Vector3.zero; //current position of the spell
            if (SS.GetShapeName().Contains("Ball") || SS.GetShapeName().Contains("Field")) { currentPos = this.transform.parent.position; }
            else if (SS.GetShapeName().Contains("Beam") && shapeScript.targets[0] != null) { currentPos = shapeScript.targets[0].transform.position; }
            else { currentPos = SS.GetStartPos(); } //default to start position
            int splitCount = Random.Range(splitMin, splitMax); //randomly choose how many split spells to create

            //create x split projectiles
            for (int i = 0; i < splitCount; i++)
            {
                Debug.Log("split " + i);
                //create new spell instance
                GameObject splitSpell = Instantiate(Resources.Load<GameObject>("SpellParent"), currentPos, Quaternion.identity);
                SpellScript splitSS = splitSpell.transform.GetChild(0).GetComponent<SpellScript>().StartSpellScript(SS.GetASM());

                splitSS.SetSpellPower(Mathf.RoundToInt(SS.GetSpellPower() / 2)); //reduce spell power

                //copy original spell components
                splitSS.UpdateSpellScriptShape(SS.GetShapeName());
                splitSS.UpdateSpellScriptEffect("Null");
                splitSS.UpdateSpellScriptElement(SS.GetElementName());

                splitSS.GetShapeScript().active = false; //disable the shape script to prevent it from moving

                //set random direction
                Vector3 baseDirection = shapeScript.GetDir(); //get origional direction
                //Debug.Log("Base direction: " + baseDirection);

                int sign = 0;
                while(sign == 0) { sign = Random.Range(-1, 2); } //ensure sign is not zero

                if (sign == usedSign) { sign = (sign * -1); }
                
                float randomAngleY = 0f;
                if (sign == 1)
                {
                    randomAngleY = Random.Range(10f, 45f); //random spread within 35 degrees
                    usedSign = 1;
                }
                else if (sign == -1)
                {
                    randomAngleY = Random.Range(-10f, -45f); //random spread within -35 degrees
                    usedSign = -1;
                }


                Debug.Log("Random angle Y: " + randomAngleY);
                Vector3 randomDirection = (Quaternion.Euler(0, randomAngleY, 0) * baseDirection);

                Vector3 startPos = currentPos;
                Vector3 endPos = Vector3.zero;
                if (SS.GetShapeName().Contains("Ball") || SS.GetShapeName().Contains("Field")) { endPos = (startPos + (randomDirection * 10f)); } //10 unit range
                else if (SS.GetShapeName().Contains("Beam")) 
                {
                    endPos = (startPos + (randomDirection * shapeScript.realSegLength));
                    splitSpell.transform.SetParent(shapeScript.targets[0].transform);
                    SS.SetSpellPersist(false);
                }

                Vector3[] newTargetPoints = new Vector3[2];
                newTargetPoints[0] = startPos;
                newTargetPoints[1] = endPos;

                AbstractShape splitShapeScript = splitSS.GetShapeScript();
                Debug.Log(startPos + " " + endPos);
                if (splitShapeScript != null)
                {
                    splitShapeScript.pathPoints = newTargetPoints;
                    splitShapeScript.dir = randomDirection;
                    splitShapeScript.lastPointConfirmed = true;
                    splitShapeScript.castable = true;
                }

                splitSS.SetIgnoredTargets(shapeScript.targets); //add hit targets to ignore list
                for (int j = 0; j < SS.GetHitTargets().Length; j++)
                {
                    Debug.Log("Split spell ignored target: " + SS.GetHitTargets()[j].name);
                }

                splitSS.UpdateComponentRefs(); //update component references

                //setup spell
                splitSS.CastSpell(); //cast the split spell
            }
        }
    }
}