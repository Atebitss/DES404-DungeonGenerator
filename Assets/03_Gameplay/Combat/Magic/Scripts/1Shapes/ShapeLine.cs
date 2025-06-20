using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeLine : AbstractShape
{
    private int numOfTriggerPoints = 5;
    private Vector3 startPos, endPos;

    public override void StartShapeScript(SpellScript SS) 
    {
        //Debug.Log("ShapeLine.StartShapeScript");
        damageModifier = 0.5f; speedModifier = 1f; radiusModifier = 3f; cooldownModifier = 1.5f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBall");
        spellMeshFilter = gameObject.GetComponent<MeshFilter>();
        spellMeshFilter.mesh = shapeMesh;
        pathPoints = new Vector3[1];
        SetTriggerPoints(new Vector3[numOfTriggerPoints]);
        mainCamera = Camera.main;
        arcAxis = new Vector3(1, 0, 0);
        this.SS = SS;

        spellAim = new GameObject[2];

        //if current aim game object is empty
        if (spellAim[0] == null)
        {
            //set new game object and update mesh then remove the objects collider
            spellAim[0] = Instantiate(Resources.Load<GameObject>("AimSpellPrefab"), transform);
            spellAim[0].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            aimMeshFilter = spellAim[0].GetComponent<MeshFilter>();
            aimMeshFilter.mesh = shapeMesh;

            aimingLine = spellAim[0].GetComponent<LineRenderer>();
            aimingLine.positionCount = pathPoints.Length;
            aimingLine.SetPosition(0, transform.position);
        }
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        if (spellAim[1] == null)
        {
            startPos = GetAimedWorldPos();
            startPos += new Vector3(0, 1, 0);
            spellAim[0].transform.position = startPos;
            pathPoints[0] = startPos;
            aimingLine.SetPosition(0, startPos);
            //Debug.Log("spellaimtransformposition: " + spellAim[0].transform.position);
        }
        else if (spellAim[1] != null)
        {
            endPos = GetAimedWorldPos();
            endPos += new Vector3(0, 1, 0);
            spellAim[1].transform.position = endPos;
            pathPoints[pathPoints.Length - 1] = endPos;

            //calculate triggering path
            Vector3 startPoint = startPos;   //begin point
            Vector3 endPoint = endPos;   //end point
            float maxDist = Vector3.Distance(startPoint, endPoint);

            if (aimingLine.positionCount < 3)
            {
                aimingLine.SetPosition((pathPoints.Length - 1), endPos);
                //Debug.Log("spellaimtransformposition: " + spellAim[1].transform.position);

                for (int point = 0; point < numOfTriggerPoints; point++)
                {
                    float progress = point / (float)(numOfTriggerPoints - 1); //complete % from 0 to 1
                    SetTriggerPoint(point, Vector3.Lerp(startPoint, endPoint, progress));
                }
            }
            else
            {
                aimingLine.SetPosition((pathPoints.Length - 2), endPos);
                //Debug.Log("spellaimtransformposition: " + spellAim[1].transform.position);

                for (int point = 0; point < numOfTriggerPoints; point++)
                {
                    float progress = point / (float)(numOfTriggerPoints - 1); //complete % from 0 to 1 // Complete % from 0 to 1
                    Vector3 curvePoint = Vector3.Lerp(startPoint, endPoint, progress);
                    float arcOffset = Mathf.Sin(progress * Mathf.PI) * maxDist * 0.1f;
                    curvePoint += arcAxis * arcOffset;
                    SetTriggerPoint(point, curvePoint);
                }
            }
        }
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Vector3 pathPointStart = pathPoints[0];
        Vector3 pathPointEnd = pathPoints[pathPoints.Length - 1];
        pathPoints = new Vector3[addPoints.Length];
        aimingLine.positionCount = pathPoints.Length - 1;

        for (int i = 0; i < addPoints.Length - 1; i++)
        {
            //Debug.Log("addPoints[" + i + "]: " + addPoints[i]);
            pathPoints[i] = addPoints[i];

            aimingLine.SetPosition(i, pathPoints[i]);
        }

        pathPoints[0] = pathPointStart;
        pathPoints[pathPoints.Length - 1] = pathPointEnd;
    }


    private void FixedUpdate()
    {
    }


    public override void ApplyShape()
    {
        //Debug.Log("Line shape applied");
        if (spellAim[1] == null)
        {
            //Debug.Log("confirmed pos 1");
            //if second aim game object is empty
            //set new game object as current aim
            spellAim[1] = Instantiate(Resources.Load<GameObject>("AimSpellPrefab"), transform);
            spellAim[1].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Destroy(spellAim[1].GetComponent<LineRenderer>());

            aimMeshFilter = spellAim[1].GetComponent<MeshFilter>();
            aimMeshFilter.mesh = shapeMesh;

            Vector3[] tempPathPoints = pathPoints;
            pathPoints = new Vector3[tempPathPoints.Length + 1];
            for (int point = 0; point < tempPathPoints.Length; point++)
            {
                pathPoints[point] = tempPathPoints[point];
            }

            aimingLine.positionCount = pathPoints.Length;
            aimingLine.SetPosition(pathPoints.Length - 1, transform.position);

            //Debug.Log("castable now true");
            firstPointConfirmed = true;
            castable = true;
        }
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeLine, FindShapeTargets");



        return null;
    }
}