using UnityEngine;
using System.Collections;

public class ShapeField : AbstractShape
{
    private BoxCollider shapeCollider;
    private GameObject[] fieldSegments = new GameObject[1];

    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Field shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 2f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeField");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
        shapeCollider = this.gameObject.AddComponent<BoxCollider>();
        shapeCollider.isTrigger = true; //set collider as trigger
        pathPoints = new Vector3[1];

        //if current aim game object is empty
        if (spellAim[0] == null)
        {
            //set new game object and update mesh then remove the objects collider
            spellAim[0] = Instantiate(Resources.Load<GameObject>("SpellAiming/AimSpellPrefab"), transform);
            spellAim[0].transform.localScale = new Vector3(0.25f, 1f, 0.25f);

            aimingLine = spellAim[0].GetComponent<LineRenderer>();
            aimingLine.positionCount = (pathPoints.Length + 1);
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
        if (active)
        {
            //Debug.Log("Field shape aim spell");
            //set start pos as player pos
            //set end pos as aimed world pos
            //update vars & line renderer

            startPos = this.transform.parent.transform.position;
            aimPos = GetAimedWorldPos();
            this.transform.position = aimPos;
            spellAim[0].transform.position = aimPos;
            dir = (aimPos - startPos).normalized;
            dir = new Vector3(dir.x, 0, dir.z); //flatten the direction vector to only use x and z axis
            this.transform.rotation = Quaternion.LookRotation(dir); //set rotation to face forwar
            endPos = aimPos;

            aimingLine.SetPosition(0, startPos);
            aimingLine.SetPosition(1, endPos);

            pathPoints[0] = endPos;

            SS.SetStartPos(endPos);
            SS.SetEndPos(endPos);
        }
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        //Debug.Log("Field shape update aim path");
        segmentCount = addPoints.Length; //set segment count to the number of points in the path
        pathPoints = addPoints; //update path points to the new points
        segmentsCreated = false; //reset segments created flag to false so segments will be recreated next update
    }


    private void FixedUpdate()
    {
        if (!segmentsCreated) { CreateFieldSegments(); }
        if (!casting) { segmentsCreated = false; }

        if (!castable && casting) //while the beam is being cast, before ending
        {
            //update spell with component impact
            if (effectScript.componentWeight == 2) { effectScript.ApplyEffect(); } //if effect weight is 2, apply effect during cast

            if (SS.GetEffectName().Contains("Homing"))
            {
                //if effect homing, update end position with found target
                if (effectScript.targets[0] != null)
                {
                    pathPoints[pathPoints.Length - 1] = Vector3.Lerp(pathPoints[pathPoints.Length - 1],
                    effectScript.targets[0].transform.position,
                    (0.5f * Time.deltaTime));
                    segmentsCreated = false; //reset segments created flag so segments will be recreated next update
                }
                else
                {
                    //if no target found, end spell
                    SS.SetSpellPersist(false);
                    SS.EndSpell();
                }
            }
        }
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        //place the field segments at the aimed position
        if (castable && !casting && !delayed)
        {
            Debug.Log("Field shape applied");
            //Debug.Log("maxRunTime: " + maxRunTime + ", checkInterval: " + checkInterval);

            CalculateRunTimes();

            shapeCollider.size = new Vector3((SS.GetRadius() * 25), 0.1f, (SS.GetRadius() * 25));

            //disallow more casts
            casting = true;
            castable = false;

            if (!SS.GetEffectName().Contains("Delay") && !SS.GetEffectName().Contains("Arc") && !SS.GetEffectName().Contains("Null")) { AimSpell(); } //ensure spell is aimed before casting

            if (SS.GetEffectName().Contains("Chain"))
            {
                effectScript.maxTargets = (int)(maxRunTime / checkInterval);
                effectScript.ApplyEffect();
                if (effectScript.targets[0] != null)
                {
                    pathPoints[pathPoints.Length - 1] = effectScript.targets[0].transform.position;
                    this.transform.position = pathPoints[pathPoints.Length - 1]; //set position to last target position
                    segmentsCreated = false;
                }
            }
            else if(!SS.GetEffectName().Contains("Arc") && !SS.GetEffectName().Contains("Null"))
            {
                Debug.Log("Setting field position to last target position: " + this.transform.position);
                this.transform.parent.transform.position = this.transform.position; //set position to last target position
                this.transform.localPosition = Vector3.zero;
            }

            CreateFieldSegments();

            //start overlap check coroutine & end timer
            StartCoroutine(EndField());
            StartCoroutine(OverlapCheck());
        }
    }
    private void CalculateRunTimes()
    {
        //Debug.Log("Setting maximum time and check interval for field shape");
        //determine maximum run time
        //calculate check interval based on run time

        float minPercentage = 0.1f, maxPercentage = 0.5f; // 10% minimum, 50% maximum of spell cooldown
        float referenceTime = 2f; //average expected time for a field shape to be active
        float curvePower = 1f; //lower means lower interval, higher means higher interval

        if (SS.GetEffectName().Contains("Delay")) { maxRunTime = SS.GetSpellCooldownMax(); } //if spell has a delay, set max run time to the full cooldown
        else if (SS.GetEffectName().Contains("Multicast")) { maxRunTime = (SS.GetSpellCooldownMax() * 2); }
        else { maxRunTime = (SS.GetSpellCooldownMax() / 2f); } // //if spell does not have a delay, set max run time to half of the cooldown

        if (SS.GetEffectName().Contains("Pierce")) { curvePower = 2f; }

        float normalizedValue = Mathf.Pow((referenceTime / maxRunTime), curvePower); //calculate duration compared to reference time
        float percentage = Mathf.Lerp(minPercentage, maxPercentage, normalizedValue); //calculate percentage based on normalized value
        percentage = Mathf.Clamp(percentage, minPercentage, maxPercentage); //limit percentage to min and max values
        checkInterval = maxRunTime * percentage; //set check interval to calculated percentage of max run time

        Debug.Log("maxRunTime: " + maxRunTime + ", checkInterval: " + checkInterval);
    }
    private IEnumerator OverlapCheck()
    {
        while (!castable && casting)
        {
            //Debug.Log("Checking for overlapping targets");
            yield return new WaitForSeconds(checkInterval); //wait for x seconds

            SS.EndSpell();
            targetCheckCount++;

            //if effect is chain, update the last point in the path to the next target and reset segments
            if (SS.GetEffectName().Contains("Chain"))
            {
                if (effectScript.curTargetNum >= effectScript.maxTargets) { SS.SetSpellPersist(false); }
                if (effectScript.targets[0] != null)
                {
                    pathPoints[pathPoints.Length - 1] = effectScript.targets[0].transform.position;
                    this.transform.position = pathPoints[pathPoints.Length - 1]; //set position to last target position
                    segmentsCreated = false;
                }
                else { SS.SetSpellPersist(false); }
            }
        }
    }
    private IEnumerator EndField()
    {
        yield return new WaitForSeconds(maxRunTime); //wait for y second
        Debug.Log("Ending field shape");

        for (int i = 0; i < fieldSegments.Length; i++)
        {
            if (fieldSegments[i] != null)
            {
                Destroy(fieldSegments[i].gameObject);
            }
        }

        casting = false;
        SS.SetSpellPersist(false);
        StopCoroutine(OverlapCheck());
        SS.EndSpell();
    }

    private void CreateFieldSegments()
    {
        //Debug.Log("Creating field segments");

        //destroy old segments if they exist
        for (int i = 0; i < fieldSegments.Length; i++)
        {
            if (fieldSegments[i] != null)
            {
                Destroy(fieldSegments[i].gameObject);
            }
        }

        //Debug.Log("segmentCount: " + segmentCount);
        fieldSegments = new GameObject[segmentCount];

        //create new segments
        for (int seg = 0; seg < segmentCount; seg++)
        {
            //Debug.Log("Creating segment " + (seg + 1) + " of " + segmentCount + ": " + pathPoints[seg]);

            //create segment parent
            fieldSegments[seg] = new GameObject("FieldSegment" + (seg + 1)); //create new game object for segment 
            fieldSegments[seg].transform.parent = this.transform; //set segment parent
            if (SS.GetEffectName().Contains("Null")) { fieldSegments[seg].transform.position = pathPoints[(pathPoints.Length - 1)]; } //set position to end pos
            else { fieldSegments[seg].transform.position = pathPoints[seg]; } //set position to relative pos
            fieldSegments[seg].transform.rotation = Quaternion.LookRotation(dir);
            fieldSegments[seg].transform.localScale = new Vector3((SS.GetRadius() * 5), 0.1f, (SS.GetRadius() * 5)); //set scale to radius of spell
            //Debug.Log("Segment " + (seg + 1) + " position: " + fieldSegments[seg].transform.position);

            //check if position is over terrain
            //Debug.DrawRay(fieldSegments[seg].transform.position, (Vector3.down * 5f), Color.red, 5f);
            if (Physics.Raycast(fieldSegments[seg].transform.position, Vector3.down, out RaycastHit standHit, 5f, LayerMask.GetMask("Terrain")))
            {
                //Debug.Log("Landing position is valid: " + standHit.point);
                fieldSegments[seg].transform.position = standHit.point;
            }
            //else { Debug.Log("Landing position is not valid, trying again"); }

            //add visual
            MeshFilter meshFilter = fieldSegments[seg].AddComponent<MeshFilter>();
            meshFilter.mesh = shapeMesh;
            Renderer renderer = fieldSegments[seg].AddComponent<MeshRenderer>();
            renderer.material = SS.GetSpellMaterial();

            //add collider
            BoxCollider curSegmentCollider = fieldSegments[seg].AddComponent<BoxCollider>(); //create new box collider for segment
            curSegmentCollider.size = new Vector3((SS.GetRadius() * 5), 100f, (SS.GetRadius() * 5)); //set size to radius of spell
            curSegmentCollider.isTrigger = true; //set collider as trigger to avoid physics interactions
        }

        segmentsCreated = true; //set segments created to true
    }

    public override GameObject[] FindShapeTargets()
    {
        //Debug.Log("ShapeField, FindShapeTargets");
        //Debug.Log("Field segments length: " + fieldSegments.Length);

        targets = new GameObject[0];

        for (int seg = 0; seg < fieldSegments.Length; seg++)
        {
            //Debug.Log("Checking segment " + (seg + 1) + " of " + beamSegments.Length + ": " + beamSegments[seg]);
            if (fieldSegments[seg] != null)
            {
                BoxCollider segmentCollider = fieldSegments[seg].GetComponent<BoxCollider>();
                Vector3 worldCenter = segmentCollider.transform.TransformPoint(segmentCollider.center);
                Vector3 worldHalfExtents = Vector3.Scale(segmentCollider.size * 0.5f, segmentCollider.transform.lossyScale);
                Quaternion worldRotation = segmentCollider.transform.rotation;
                //Debug.Log("Segment collider: " + segmentCollider);
                //Debug.Log("Segment collider bounds: " + segmentCollider.bounds);
                //Debug.Log("Segment collider rotation: " + segmentCollider.transform.rotation);


                //check for overlapping enemy colliders
                Collider[] cols = Physics.OverlapBox(
                    worldCenter,
                    worldHalfExtents,
                    worldRotation,
                    LayerMask.GetMask("Enemy")
                );

                //Debug.Log("Found " + cols.Length + " colliders in segment " + (seg + 1));
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

        //Debug.Log("Shape Field, found " + targets.Length + " targets");
        return targets;
    }
}