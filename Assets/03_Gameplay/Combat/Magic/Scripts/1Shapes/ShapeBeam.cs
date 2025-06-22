using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBeam : AbstractShape
{
    private BoxCollider[] beamSegments = new BoxCollider[0]; 
    private MeshRenderer[] beamMeshes = new MeshRenderer[0];
    private int segmentCount = 1;
    private float width = 1f, length = 2f;
    private float maxRunTime = 1f, checkInterval = 0.5f;
    private bool casting = false;

    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Beam shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBeamSegment");
        SS.SetSpellPersist(true);

        //if current aim game object is empty
        if (spellAim[0] == null)
        {
            //set new game object and update mesh then remove the objects collider
            spellAim[0] = Instantiate(Resources.Load<GameObject>("SpellAiming/AimSpellPrefab"), transform);
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

        firstPointConfirmed = true;
        castable = true;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Beam shape aim spell");
        Vector3 startPos = this.transform.position;
        Vector3 aimPos = GetAimedWorldPos();
        Vector3 dir = (aimPos - startPos).normalized;
        Vector3 endPos = (startPos + (dir * length));

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(dir);

        aimingLine.SetPosition(0, startPos);
        aimingLine.SetPosition(1, endPos);

        pathPoints[0] = startPos;
        pathPoints[1] = endPos;

        SS.SetStartPos(pathPoints[0]);
        SS.SetEndPos(pathPoints[1]);
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Beam shape update aim path");

        Vector3 pathPointStart = pathPoints[0];
        Vector3 pathPointEnd = pathPoints[pathPoints.Length - 1];
        pathPoints = new Vector3[addPoints.Length];
        aimingLine.positionCount = pathPoints.Length - 1;

        for (int i = 0; i < addPoints.Length - 1; i++)
        {
            //Debug.Log("addPoints["+i+"]: " + addPoints[i]);
            pathPoints[i] = addPoints[i];

            aimingLine.SetPosition(i, pathPoints[i]);
        }

        pathPoints[0] = pathPointStart;
        pathPoints[pathPoints.Length - 1] = pathPointEnd;

        CreateBeamSegments();
    }


    private void FixedUpdate()
    {
        if (!castable && casting) { AimSpell(); }
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        if (castable && !casting)
        {
            Debug.Log("Beam shape applied");
            Vector3 startPos = this.transform.position;
            Vector3 aimPos = GetAimedWorldPos();
            Vector3 dir = (aimPos - startPos).normalized;

            length *= SS.GetRadius();
            width *= SS.GetRadius();
            Debug.Log("beam length: " + length + ", beam width: " + width);


            //set beam at player location
            transform.position = startPos;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.localScale = new Vector3(width, width, length);

            CreateBeamSegments();

            //disallow more casts
            casting = true;
            castable = false;

            //start overlap check coroutine & end timer
            StartCoroutine(EndBeam());
            StartCoroutine(OverlapCheck());
        }
    }
    private IEnumerator OverlapCheck()
    {
        while (!castable && casting)
        {
            Debug.Log("Checking for overlapping targets");
            SS.EndSpell();
            yield return new WaitForSeconds(checkInterval); //wait for 0.25 seconds
        }
    }
    private IEnumerator EndBeam()
    {
        yield return new WaitForSeconds(maxRunTime); //wait for 1 second
        Debug.Log("Ending beam shape");

        for (int i = 0; i < beamSegments.Length; i++)
        {
            if (beamSegments[i] != null)
            {
                Destroy(beamSegments[i].gameObject);
            }
        }

        beamSegments = new BoxCollider[0];
        beamMeshes = new MeshRenderer[0];
        casting = false;
        SS.SetSpellPersist(false);
        StopCoroutine(OverlapCheck());
        SS.EndSpell();
    }

    private void CreateBeamSegments()
    {
        //clean up existing segments
        for (int i = 0; i < beamSegments.Length; i++)
        {
            if (beamSegments[i] != null)
            {
                Destroy(beamSegments[i].gameObject);
            }
        }

        //determine number of segments based on path points
        segmentCount = (pathPoints.Length - 1);
        beamSegments = new BoxCollider[segmentCount];
        beamMeshes = new MeshRenderer[segmentCount];

        //create individual segments
        for (int i = 0; i < (segmentCount - 1); i++)
        {
            Vector3 segStart = pathPoints[i];
            Vector3 segEnd = pathPoints[i + 1];
            Vector3 segCenter = (segStart + segEnd) * 0.5f;
            Vector3 segDir = (segEnd - segStart).normalized;
            float segLength = Vector3.Distance(segStart, segEnd);

            //create segment
            GameObject segmentObj = new GameObject("BeamSegment" + i);
            segmentObj.transform.parent = this.transform;
            segmentObj.transform.position = segCenter;
            segmentObj.transform.localScale = new Vector3(width, width, (segLength*2.5f));
            segmentObj.transform.rotation = Quaternion.LookRotation(segDir);

            beamSegments[i] = segmentObj.AddComponent<BoxCollider>();
            beamSegments[i].isTrigger = true;
            beamSegments[i].size = new Vector3((width*2), (width*2), (segLength*3));


            MeshFilter meshFilter = segmentObj.AddComponent<MeshFilter>();
            meshFilter.mesh = shapeMesh;

            beamMeshes[i] = segmentObj.AddComponent<MeshRenderer>();
            beamMeshes[i].material = SS.GetSpellMaterial();
        }
    }



    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBeam, FindShapeTargets");
        targets = new GameObject[0];
        for (int seg = 0; seg < beamSegments.Length; seg++)
        {
            if (beamSegments[seg] != null)
            {
                //check for overlapping enemy colliders
                Collider[] cols = Physics.OverlapBox(
                    beamSegments[seg].bounds.center,
                    beamSegments[seg].bounds.extents,
                    beamSegments[seg].transform.rotation,
                    LayerMask.GetMask("Enemy")
                );

                for (int i = 0; i < cols.Length; i++)
                {
                    Debug.Log(i + ": " + cols[i].gameObject.name);
                    if (cols[i].gameObject.tag == "Enemy" && !SS.CheckIgnoredTargets(cols[i].gameObject) && !HasAlreadyHitTarget(cols[i].gameObject))
                    {
                        Debug.Log("Found target: " + cols[i].gameObject.name);
                        //increase targets array and add the enemy
                        GameObject[] tempTargets = new GameObject[targets.Length + 1];
                        for (int j = 0; j < targets.Length; j++) { tempTargets[j] = targets[j]; }
                        tempTargets[tempTargets.Length - 1] = cols[i].gameObject;
                        targets = tempTargets;
                    }
                }
            }
        }

        Debug.Log("Shape Beam, found " + targets.Length + " targets");
        return targets;
    }



    void OnDrawGizmos()
    {
        if (beamSegments == null || beamSegments.Length == 0) return;

        Gizmos.color = Color.red;

        // Draw each beam segment
        for (int i = 0; i < beamSegments.Length; i++)
        {
            if (beamSegments[i] != null)
            {
                // Set matrix to segment's transform for proper rotation
                Gizmos.matrix = beamSegments[i].transform.localToWorldMatrix;

                // Draw wireframe cube at segment's local center
                Vector3 center = beamSegments[i].center;
                Vector3 size = beamSegments[i].size;

                Gizmos.DrawWireCube(center, size);
            }
        }

        // Reset matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
}