using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectArc : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    { 
        componentWeight = 1; damageModifier = 2f; speedModifier = 0.75f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }

    public override void ApplyEffect()
    {
        //Debug.Log("Arc effect applied");
        if (SS.GetShapeScript() != null && SS.GetShapeScript().spellAim != null && SS.GetShapeScript().firstPointConfirmed)
        {
            //calculate arced path
            int numOfPoints = 10;
            Vector3[] arcPathPoints = new Vector3[numOfPoints+1];

            //get path length based from start point to end point
            //divide total length by x providing a number of points for the curve to follow
            //while less than half way through point total, increase each point by x on the X axis
            //while more than half way through point total, lower each point by x on the X axis
            //update the line renderer with the new points

            Vector3 startPoint = SS.GetShapeScript().pathPoints[0];   //begin point of arc
            Vector3 endPoint = SS.GetShapeScript().pathPoints[(SS.GetShapeScript().pathPoints.Length - 1)];   //end point of arc
            float maxDist = Vector3.Distance(startPoint, endPoint);

            for (int point = 0; point < numOfPoints; point++)
            {
                float progress = point / (float)(numOfPoints - 1); //complete % from 0 to 1
                Vector3 curvePoint = Vector3.Lerp(startPoint, endPoint, progress);
                float arcOffset = Mathf.Sin(progress * Mathf.PI) * maxDist * 0.1f;
                curvePoint += SS.GetShapeScript().arcAxis * arcOffset;
                arcPathPoints[point] = curvePoint;
                //Debug.Log("progress: " + progress + "\tcurvePoint: " + curvePoint);
            }

            //Debug.Log(SS.GetCasted());
            if (!SS.GetCasted()) { SS.GetShapeScript().UpdateAimPath(arcPathPoints); }
        }
    }
}