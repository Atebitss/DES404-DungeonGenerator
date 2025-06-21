using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBeam : AbstractShape
{
    private BoxCollider beamCollider;
    private float width = 1f, length = 2f;
    private float maxRunTime = 1f, checkInterval = 0.5f;
    private bool casting = false;

    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Beam shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBeam");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
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
        //Debug.Log("Beam shape aim spell");
        Vector3 startPos = this.transform.position;
        Vector3 aimPos = GetAimedWorldPos();
        Vector3 dir = (aimPos - startPos).normalized;

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(dir);

        aimingLine.SetPosition(0, startPos);
        aimingLine.SetPosition(1, aimPos);

        pathPoints[0] = startPos;
        pathPoints[1] = aimPos;

        SS.SetStartPos(pathPoints[0]);
        SS.SetEndPos(pathPoints[pathPoints.Length - 1]);
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Beam shape update aim path");
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

            //setup collider for beam detection
            beamCollider = gameObject.AddComponent<BoxCollider>();
            beamCollider.isTrigger = true;
            beamCollider.size = new Vector3(width, width, 6f);
            beamCollider.center = new Vector3(0f, 0f, 3f);

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
        casting = false;
        SS.SetSpellPersist(false);
        StopCoroutine(OverlapCheck());
        SS.EndSpell();
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBeam, FindShapeTargets");
        targets = new GameObject[0];

        //check for overlapping enemy colliders
        Collider[] cols = Physics.OverlapBox
        (
            beamCollider.transform.position,
            beamCollider.bounds.extents,
            beamCollider.transform.rotation,
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

        Debug.Log("Shape Beam, found " + targets.Length + " targets");
        return targets;
    }



    void OnDrawGizmos()
    {
        if (beamCollider == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 center = beamCollider.center;
        Vector3 size = beamCollider.size;

        Gizmos.DrawWireCube(center, size);
    }
}