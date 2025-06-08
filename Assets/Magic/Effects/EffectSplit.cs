using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSplit : AbstractEffect
{
    private int splitMin = 2, splitMax = 2;
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
            Vector3 currentPos = this.transform.position; // current position of the spell
            int splitCount = Random.Range(splitMin, splitMax); //randomly choose how many spells to create

            //create x split projectiles
            for (int i = 0; i < splitCount; i++)
            {
                Debug.Log("split " + i);
                //create new spell instance
                GameObject splitSpell = Instantiate(Resources.Load<GameObject>("SpellParent"), currentPos, Quaternion.identity);
                SpellScript splitSS = splitSpell.transform.GetChild(0).GetComponent<SpellScript>().StartSpellScript(SS.GetASM());

                splitSS.SetIgnoredTargets(SS.GetIgnoredTargets()); //copy ignored targets
                splitSS.SetSpellPower(Mathf.RoundToInt(SS.GetSpellPower() / 2)); //reduce spell power

                //copy original spell components
                splitSS.UpdateSpellScriptShape(SS.GetShapeName());
                splitSS.UpdateSpellScriptEffect("Null");
                splitSS.UpdateSpellScriptElement(SS.GetElementName());

                //set random direction
                Vector3 baseDirection = SS.GetDir(); //get origional direction
                float randomAngleY = Random.Range(-45f, 45f);//random spread within 45 degrees
                Vector3 randomDirection = (Quaternion.Euler(0, randomAngleY, 0) * baseDirection);

                Vector3 startPos = currentPos;
                Vector3 endPos = startPos + (randomDirection * 10f); //10 unit range

                Vector3[] newTargetPoints = new Vector3[2];
                newTargetPoints[0] = startPos;
                newTargetPoints[1] = endPos;

                AbstractShape splitShapeScript = splitSS.GetShapeScript();
                Debug.Log(endPos + " " + startPos + " " + splitShapeScript);
                if (splitShapeScript != null)
                {
                    splitShapeScript.pathPoints = newTargetPoints;
                    splitShapeScript.lastPointConfirmed = true;
                    splitShapeScript.castable = true;
                }

                splitSS.SetStartPos(startPos);
                splitSS.SetEndPos(endPos);

                //setup spell
                splitSS.CastSpell(); //cast the split spell
            }
        }
    }
}