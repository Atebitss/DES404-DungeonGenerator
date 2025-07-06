using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectArc : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    { 
        componentWeight = 0; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = .1f;
        this.SS = SS;
        elementScript = SS.GetElementScript();   //get the element script for this spell
        shapeScript = SS.GetShapeScript();   //get the shape script for this spell
    }

    public override void ApplyEffect()
    {
        //Debug.Log("Arc effect applied");
        if (shapeScript != null && shapeScript.spellAim != null && shapeScript.firstPointConfirmed)
        {
            //calculate arced path
            int numOfPoints = 10;
            Vector3[] arcPathPoints = new Vector3[numOfPoints];

            //get path length based from start point to end point
            //divide total length by x providing a number of points for the curve to follow
            //while less than half way through point total, increase each point by x on the X axis
            //while more than half way through point total, lower each point by x on the X axis
            //update the line renderer with the new points
            Vector3 startPoint = shapeScript.pathPoints[0];   //begin point of arc
            Vector3 endPoint;

            //if shape is beam, apply range limitation
            if (SS.GetShapeName().Contains("Beam"))
            {
                //calculate limited endpoint for beam shapes
                Vector3 aimPos = shapeScript.GetAimedWorldPos();
                Vector3 dir = (aimPos - startPoint).normalized;

                //use beam's effective range (length * 3f * radius scaling)
                float beamRange = (6f * SS.GetRadius()); // 2f * 3f * radius like in ShapeBeam

                endPoint = startPoint + (dir * beamRange);
            }
            else
            {
                //use normal endpoint for non-beam shapes
                endPoint = shapeScript.pathPoints[(shapeScript.pathPoints.Length - 1)];
            }

            float maxDist = Vector3.Distance(startPoint, endPoint);

            for (int point = 0; point < numOfPoints; point++)
            {
                float progress = point / (float)(numOfPoints - 1); //complete % from 0 to 1
                Vector3 curvePoint = Vector3.Lerp(startPoint, endPoint, progress);
                float arcOffset = Mathf.Sin(progress * Mathf.PI) * maxDist * 0.1f;
                curvePoint += shapeScript.arcAxis * arcOffset;
                arcPathPoints[point] = curvePoint;
                //Debug.Log("progress: " + progress + "\tcurvePoint: " + curvePoint);
            }

            //Debug.Log(SS.GetCasted());
            if (!SS.GetCasted()) { shapeScript.UpdateAimPath(arcPathPoints); }
        }
    }
}