using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectArc : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    { 
        componentWeight = 0; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
        elementScript = SS.GetElementScript();   //get the element script for this spell
        shapeScript = SS.GetShapeScript();   //get the shape script for this spell
    }

    public override void ApplyEffect()
    {
        //Debug.Log("Arc effect applied");
        if (SS.GetShapeName().Contains("Ball") || SS.GetShapeName().Contains("Beam"))
        {
            CalculateArcMovement();
        }
        else if(SS.GetShapeName().Contains("Field"))
        {
            CalculateArcPlacement();
        }
    }

    private void CalculateArcMovement()
    {
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

    private void CalculateArcPlacement()
    {
        //calculate arc points with end point as center
        //Debug.Log("Arc placement effect applied");
        if (shapeScript != null && shapeScript.spellAim != null && shapeScript.firstPointConfirmed)
        {
            int numOfPoints = 5; //maximum number of points for arc
            float segmentSpacing = SS.GetRadius(); //spacing between arc points
            Vector3[] arcPathPoints = new Vector3[numOfPoints]; //position points for arc
            Vector3 centerPoint = Vector3.zero;
            if (SS.GetShapeName().Contains("Field")) { centerPoint = shapeScript.pathPoints[0]; }
            else { centerPoint = shapeScript.pathPoints[(shapeScript.pathPoints.Length - 1)]; }

            Vector3 aimDirection = shapeScript.GetDir(); //get direction of the spell
            Vector3 arcDirection = new Vector3(-aimDirection.z, 0, aimDirection.x); // //calculate arc direction by rotating aim direction 90 degrees
            Debug.Log("NumOfPoints: " + numOfPoints + ", Center point: " + centerPoint + ", Aim direction: " + aimDirection + ", Arc direction: " + arcDirection + ", Spacing: " + segmentSpacing);

            //calculate each arc point based on the center point
            for (int i = 0; i < numOfPoints; i++)
            {
                float offset = (i - (numOfPoints - 1) / 2f); //calculate wedge position
                Debug.Log("Offset for point " + i + ": " + offset);

                if (offset == 0)
                {
                    Debug.Log("Arc point " + i + ": " + centerPoint);
                    arcPathPoints[i] = centerPoint; //if offset is 0, use center point
                }
                else
                {
                    //left/right side
                    Vector3 sideOffset = arcDirection * (offset * 2) * segmentSpacing; //calculate sideways position
                    Vector3 backOffset = -aimDirection * (Mathf.Abs(offset) * segmentSpacing); //calculate backwards position
                    Vector3 tempPos = centerPoint + sideOffset + backOffset; //calculate final position
                    tempPos.y = centerPoint.y; //keep y level with center point
                    Debug.Log("Arc point " + i + ": " + tempPos);
                    arcPathPoints[i] = tempPos; //update path points with new position
                }
            }

            Debug.Log("Arc path points: " + string.Join(", ", arcPathPoints));
            if (!SS.GetCasted()) { shapeScript.UpdateAimPath(arcPathPoints); } //update the shape script with the new arcing points
        }
    }
}