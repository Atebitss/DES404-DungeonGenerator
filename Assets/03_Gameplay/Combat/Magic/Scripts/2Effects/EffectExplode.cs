using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectExplode : AbstractEffect
{
    private int explosionsPerSegment = 1;
    public override void StartEffectScript(SpellScript SS) 
    { 
        componentWeight = -1; damageModifier = 0.25f; speedModifier = 0.5f; radiusModifier = 5f; cooldownModifier = 2f;
        this.SS = SS;

        if (SS.GetShapeName().Contains("Beam")) { radiusModifier = 2f; }
        else if (SS.GetShapeName().Contains("Field")) { componentWeight = 3; damageModifier = 5f; radiusModifier = 1f; }
    }
    public override void ApplyEffect()
    {
        //Debug.Log("Explosion effect applied");

        //if spell uses shape beam 
        if (SS.GetShapeName().Contains("Beam"))
        {
            targets = new GameObject[0];
            float radius = SS.GetRadius(); //get radius for explosion radius
            pathPoints = shapeScript.GetPathPoints(); //start to end points of the spell path

            //for each path position
            for (int pathPos = 0; pathPos < (pathPoints.Length - 1); pathPos++)
            {
                Vector3 segStart = pathPoints[pathPos];
                Vector3 segEnd = pathPoints[pathPos + 1];
                float segLength = (Vector3.Distance(segStart, segEnd) * (1.5f * radius));

                //for each explosion in the segment
                for (int explosionCount = 0; explosionCount < explosionsPerSegment; explosionCount++)
                {
                    //run an overlap sphere the radius of the explosion at (1/explosionsPerSegment) of the segment length
                    Vector3 segDir = (segEnd - segStart).normalized;
                    float explosionDistance = ((segLength / explosionsPerSegment) * (explosionCount + 1));
                    Vector3 explosionPos = segStart + (segDir * explosionDistance);

                    Collider[] cols = Physics.OverlapSphere(explosionPos, radius);

                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (cols[i].gameObject.tag == "Enemy" && !SS.CheckIgnoredTargets(cols[i].gameObject) && !HasAlreadyHitTarget(cols[i].gameObject))
                        {
                            //add target to targets array
                            //Debug.Log("Explosion hit enemy: " + cols[i].gameObject.name);
                            GameObject[] tempTargets = new GameObject[targets.Length + 1];
                            for (int j = 0; j < targets.Length; j++)
                            {
                                tempTargets[j] = targets[j];
                            }
                            tempTargets[tempTargets.Length - 1] = cols[i].gameObject;
                            targets = tempTargets;
                        }
                    }
                }
            }
        }
        else if (SS.GetShapeName().Contains("Field") && shapeScript.targets.Length > 0) { SS.SetSpellPersist(false); }
    }
    private bool HasAlreadyHitTarget(GameObject enemy)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == enemy)
            {
                //Debug.Log("already hit enemy: " + enemy.name);
                return true;
            }
        }

        return false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (SS.GetShapeName().Contains("Ball")) { Gizmos.DrawWireSphere(this.transform.position, SS.GetRadius()); }
        else if (SS.GetShapeName().Contains("Beam"))
        {
            Vector3[] pathPoints = shapeScript.GetPathPoints();
            float radius = SS.GetRadius(); //get radius for explosion radius
            for (int pathPos = 0; pathPos < (pathPoints.Length - 1); pathPos++)
            {
                Vector3 segStart = pathPoints[pathPos];
                Vector3 segEnd = pathPoints[pathPos + 1];
                Vector3 segDir = (segEnd - segStart).normalized;
                float segLength = (Vector3.Distance(segStart, segEnd) * (1.5f * radius)); //~~~SPAGHETTI~~~//
                //Debug.Log("Segment " + pathPos + ": Start: " + segStart + ", End: " + segEnd + ", Length: " + segLength);

                //for each explosion in the segment
                for (int explosionCount = 0; explosionCount < explosionsPerSegment; explosionCount++)
                {
                    float explosionDistance = ((segLength / explosionsPerSegment) * (explosionCount + 1));
                    Vector3 explosionPos = segStart + (segDir * explosionDistance);

                    Gizmos.DrawWireSphere(explosionPos, radius);
                }
            }
        }
    }
}
