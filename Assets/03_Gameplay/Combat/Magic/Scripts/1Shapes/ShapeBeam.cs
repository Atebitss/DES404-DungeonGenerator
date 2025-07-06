using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBeam : AbstractShape
{
    private GameObject[] beamSegments = new GameObject[0];
    private int segmentCount = 1;
    private float width = 1f, length = 2f;
    private float maxRunTime = 10f, checkInterval = 1f;
    private bool casting = false, segmentsCreated = false;
    private GameObject[] homingTargets = new GameObject[0];

    public override void StartShapeScript(SpellScript SS)
    {
        //Debug.Log("Beam shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBeam");

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

        this.GetComponent<Renderer>().enabled = false; //hide the parent object

        SS.SetSpellPersist(true);
        firstPointConfirmed = true;
        castable = true;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        //Debug.Log("Beam shape aim spell");
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
        //Debug.Log("Beam shape update aim path");

        Vector3 pathPointStart = pathPoints[0];
        Vector3 pathPointEnd = addPoints[addPoints.Length - 1];
        pathPoints = new Vector3[addPoints.Length];
        aimingLine.positionCount = pathPoints.Length - 1;

        for (int i = 0; i < addPoints.Length - 1; i++)
        {
            Debug.Log("addPoints["+i+"]: " + addPoints[i]);
            pathPoints[i] = addPoints[i];

            aimingLine.SetPosition(i, pathPoints[i]);
        }

        pathPoints[0] = pathPointStart;
        pathPoints[pathPoints.Length - 1] = pathPointEnd;

        segmentsCreated = false; //reset segments created flag to false so segments will be recreated next FixedUpdate
    }


    private void FixedUpdate()
    {
        if (!segmentsCreated) { CreateBeamSegments(); }
        if (!castable && casting) //while the beam is being cast, before ending
        {
            if (SS.GetEffectName().Contains("Homing"))
            {
                //if effect homing, update end position with found target
                if (effectScript.targets[0] != null)
                {
                    pathPoints[pathPoints.Length - 1] = effectScript.targets[0].transform.position;
                    segmentsCreated = false; //reset segments created flag to false so segments will be recreated next update
                }
                else
                {
                    //if no target found, end spell
                    SS.SetSpellPersist(false);
                    SS.EndSpell();
                }
            }
            else
            {
                AimSpell();
            }
        }
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        if (castable && !casting)
        {
            //disallow more casts
            casting = true;
            castable = false;
            AimSpell(); //ensure spell is aimed before casting

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

        casting = false;
        SS.SetSpellPersist(false);
        StopCoroutine(OverlapCheck());
        SS.EndSpell();
    }

    private void CreateBeamSegments()
    {
        Debug.Log("Creating beam segments");

        //destroy old segments if they exist
        for (int i = 0; i < beamSegments.Length; i++)
        {
            if (beamSegments[i] != null)
            {
                Destroy(beamSegments[i].gameObject);
            }
        }

        segmentCount = (pathPoints.Length - 1); //determine number of segments based on path points
        pathPoints[0] = this.transform.position; //ensure first point is the spell position

        if (SS.GetEffectName().Contains("Arc"))
        {
            //update mesh type and remove end segment
            shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBeamSegment");
            segmentCount--;
        }

        Debug.Log("segmentCount: " + segmentCount);
        beamSegments = new GameObject[segmentCount];

        for (int i = 0; i < pathPoints.Length; i++)
        {
            Debug.Log("pathPoints[" + i + "]: " + pathPoints[i]);
        }

        //create individual segments
        for (int i = 0; i < segmentCount; i++)
        {
            Debug.Log("Creating segment " + (i + 1) + " of " + segmentCount);

            //calculate position
            Vector3 segStart = pathPoints[i];
            Vector3 segEnd = pathPoints[i + 1];
            Vector3 segDir = (segEnd - segStart).normalized;
            float segLength = Vector3.Distance(segStart, segEnd);
            if (SS.GetEffectName().Contains("Homing")){ segLength -= (segLength * 0.333f); } //reduce segment length by 33% to avoid overshooting
            else if(SS.GetEffectName().Contains("Arc")) { segLength *= 2f; } //double segment length to fill gaps
            Vector3 segCenter = segStart + (segDir * (segLength / 2f));
            Debug.Log("Segment " + (i + 1) + " start: " + segStart + ", end: " + segEnd + ", length: " + segLength);

            //create segment parent
            beamSegments[i] = new GameObject("BeamSegment" + (i + 1));
            beamSegments[i].transform.parent = this.transform;
            beamSegments[i].transform.position = segStart;
            beamSegments[i].transform.localScale = new Vector3(width, width, segLength);
            beamSegments[i].transform.rotation = Quaternion.LookRotation(segDir);

            //add visual
            MeshFilter meshFilter = beamSegments[i].AddComponent<MeshFilter>();
            meshFilter.mesh = shapeMesh;
            Renderer renderer = beamSegments[i].AddComponent<MeshRenderer>();
            renderer.material = SS.GetSpellMaterial();

            //add collider
            BoxCollider curSegmentCollider = beamSegments[i].AddComponent<BoxCollider>();
            curSegmentCollider.isTrigger = true;
        }

        segmentsCreated = true;
    }



    public override GameObject[] FindShapeTargets()
    {
        //Debug.Log("ShapeBeam, FindShapeTargets");
        targets = new GameObject[0];
        for (int seg = 0; seg < beamSegments.Length; seg++)
        {
            if (beamSegments[seg] != null)
            {
                BoxCollider segmentCollider = beamSegments[seg].GetComponent<BoxCollider>();

                //check for overlapping enemy colliders
                Collider[] cols = Physics.OverlapBox(
                    segmentCollider.bounds.center,
                    segmentCollider.bounds.extents,
                    segmentCollider.transform.rotation,
                    LayerMask.GetMask("Enemy")
                );

                for (int i = 0; i < cols.Length; i++)
                {
                    //Debug.Log(i + ": " + cols[i].gameObject.name);
                    if (cols[i].gameObject.tag == "Enemy" && !SS.CheckIgnoredTargets(cols[i].gameObject) && !HasAlreadyHitTarget(cols[i].gameObject))
                    {
                        //Debug.Log("Found target: " + cols[i].gameObject.name);
                        //increase targets array and add the enemy
                        GameObject[] tempTargets = new GameObject[targets.Length + 1];
                        for (int j = 0; j < targets.Length; j++) { tempTargets[j] = targets[j]; }
                        tempTargets[tempTargets.Length - 1] = cols[i].gameObject;
                        targets = tempTargets;
                    }
                }
            }
        }

        //Debug.Log("Shape Beam, found " + targets.Length + " targets");
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
                Vector3 center = beamSegments[i].GetComponent<BoxCollider>().center;
                Vector3 size = beamSegments[i].GetComponent<BoxCollider>().size;

                Gizmos.DrawWireCube(center, size);
            }
        }

        // Reset matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
}