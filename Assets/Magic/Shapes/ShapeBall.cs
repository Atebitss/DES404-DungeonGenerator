using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBall : AbstractShape
{
    // 1. Core initialization
    // 2. Aiming phase functions
    // 3. Unity lifecycle
    // 4. Execution phase

    public override void StartShapeScript(SpellScript SS)
    {
        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBall");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);

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

        firstPointConfirmed = true;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        spellAim[0].transform.position = GetAimedWorldPos();
        //Debug.Log("spellaimtransformposition: " + spellAim[0].transform.position);
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
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
    }


    private void FixedUpdate()
    {
        //if spell is being aimed, update first line renderer point with player position
        if (firstPointConfirmed && !lastPointConfirmed)
        {
            aimingLine.SetPosition(0, this.transform.position);
            pathPoints[0] = this.transform.position;
            maxLength = SS.GetMaxSpellLength();
            if (!castable && aimPos != Vector3.zero) { castable = true; }
            //Debug.Log("aiming line set to: " + this.transform.position);
        }
    }


    public override void ApplyShape()
    {
        if (!delayed)
        {
            Debug.Log("Ball shape applied");

            if (effectScript == null) { effectScript = SS.GetEffectScript(); }
            if (elementScript == null) { elementScript = SS.GetElementScript(); }
            //Debug.Log("effectScript: " + effectScript);
            //Debug.Log("elementScript: " + elementScript);

            speed = SS.GetSpeed();
            speed *= speedModifier;
            speed *= effectScript.speedModifier;
            speed *= elementScript.speedModifier;

            pathPoints[0] = this.transform.position;
            Debug.Log("start pos: " + pathPoints[0]);
            if (effectScript.targets[0] != null)
            {
                Debug.Log("es target: " + effectScript.targets[0].transform.parent.name);
                pathPoints[(pathPoints.Length - 1)] = effectScript.targets[0].transform.position;
            }

            StartCoroutine(MoveBall());
        }
    }
    private IEnumerator MoveBall()
    {
        Debug.Log("ShapeBall, MoveBall");
        for (int step = 0; step < pathPoints.Length - 1; step++)
        {
            Vector3 curStartPos = pathPoints[step];
            Vector3 curEndPos = pathPoints[step + 1];
            dir = (curEndPos - curStartPos).normalized;
            journeyLength = Vector3.Distance(curStartPos, curEndPos);
            startTime = Time.time;

            while (Vector3.Distance(this.transform.position, curEndPos) > 0.01f && Vector3.Distance(curStartPos, this.transform.position) < maxLength)
            {
                if (effectScript.targets[0] != null)
                {
                    pathPoints[(pathPoints.Length - 1)] = effectScript.targets[0].transform.position;
                }

                //update spell with component impact
                if (effectScript.componentWeight == 2) { effectScript.ApplyEffect(); } //if effect weight is 2, apply effect during flight

                curEndPos = pathPoints[step + 1];
                dir = (curEndPos - curStartPos).normalized;
                journeyLength = Vector3.Distance(curStartPos, curEndPos);


                //Debug.Log(Vector3.Distance(this.transform.position, endPos) > 0.01f);
                float travelInterpolate = (Time.time - startTime) * speed / journeyLength;
                Vector3 nextPosition = Vector3.Lerp(curStartPos, curEndPos, travelInterpolate);


                Debug.DrawRay(transform.position, dir, Color.red, 10f);
                //check if the spell intersects with an object on the Enemy layer
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Vector3.Distance(transform.position, nextPosition) + 0.1f, LayerMask.GetMask("Enemy")) && !SS.CheckIgnoredTargets(hit.collider.gameObject) && !HasAlreadyHitTarget(hit.collider.gameObject))
                {
                    //Debug.Log("Spell hit: " + hit.collider.gameObject.transform.parent.name);
                    SS.EndSpell();
                    spellEnded = true;
                    if (!SS.GetEffectName().Contains("Pierce")) { break; } //stop if not piercing
                }

                if (this.transform.parent.gameObject != null) { this.transform.parent.transform.position = nextPosition; }

                yield return null;
            }
        }
        Debug.Log("MoveBall finished");

        //stop persistance and end spell after reaching destination
        if (SS.GetEffectName().Contains("Pierce")) 
        {
            SS.SetSpellPersist(false);
            SS.EndSpell();
        } 

        if (!spellEnded) { SS.EndSpell(); }
    }

    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBall, FindShapeTargets");
        Collider[] cols = Physics.OverlapSphere(this.transform.position, SS.GetRadius());

        //add all unhit & unignored enemies to targets array
        for (int i = 0; i < cols.Length; i++) //for each collision
        {
            //if collider belongs to a unhit & uinignored enemy
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

        Debug.Log("Shape Ball, found " + targets.Length + " targets");
        return targets;
    }
}