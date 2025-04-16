using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBall : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    { 
        damageModifier = 1.5f; speedModifier = 2f; 
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBall"); 
        castable = true;
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        firstPointConfirmed = true;
        this.SS = SS;

        //Debug.Log(transform.position);
        //if current aim game object is empty
        if (spellAim[0] == null)
        {
            //set new game object and update mesh then remove the objects collider
            spellAim[0] = Instantiate(Resources.Load<GameObject>("AimSpellPrefab"), transform);
            spellAim[0].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            aimingLine = spellAim[0].GetComponent<LineRenderer>();
            aimingLine.positionCount = pathPoints.Length;
            aimingLine.SetPosition(0, this.transform.position);

            aimMeshFilter = spellAim[0].GetComponent<MeshFilter>();
            aimMeshFilter.mesh = shapeMesh;

            spellMeshFilter = gameObject.GetComponent<MeshFilter>();
            spellMeshFilter.mesh = shapeMesh;

            pathPoints[0] = this.transform.position;
        }
    }
    private void FixedUpdate()
    {
        //if spell is being aimed, update first line renderer point with player position
        if (firstPointConfirmed && !lastPointConfirmed)
        {
            aimingLine.SetPosition(0, this.transform.position);
            pathPoints[0] = this.transform.position;
            //Debug.Log("aiming line set to: " + this.transform.position);
        }
    }
    public override void ApplyShape()
    {
        //Debug.Log("Ball shape applied");
    }

    //runs when shape is added to spell
    public override void AimSpell()
    {
        spellAim[0].transform.position = GetAimedWorldPos();
        //Debug.Log("spellaimtransformposition: " + spellAim[0].transform.position);
    }
    private Vector3 GetAimedWorldPos()
    {
        if (mainCamera != null)
        {
            //raycasts from center of view to world position, if hit then update, then return
            Ray cameraToWorld = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(cameraToWorld, out RaycastHit hit, SS.GetMaxLength()))
            {
                //Debug.Log("hit: " + hit.collider.gameObject);
                aimPos = hit.point;
                pathPoints[pathPoints.Length - 1] = aimPos;
            }

            if (aimingLine.positionCount < 3) { aimingLine.SetPosition(pathPoints.Length - 1, aimPos); }
            //Debug.Log("aimpos: " + aimPos);
            return aimPos;
        }

        return Vector3.zero;
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Vector3 pathPointStart = pathPoints[0];
        Vector3 pathPointEnd = pathPoints[pathPoints.Length - 1];
        pathPoints = new Vector3[addPoints.Length];
        aimingLine.positionCount = pathPoints.Length - 1;

        for (int i = 0; i < addPoints.Length-1; i++)
        {
            //Debug.Log("addPoints["+i+"]: " + addPoints[i]);
            pathPoints[i] = addPoints[i];

            aimingLine.SetPosition(i, pathPoints[i]);
        }

        pathPoints[0] = pathPointStart;
        pathPoints[pathPoints.Length - 1] = pathPointEnd;
    }
}