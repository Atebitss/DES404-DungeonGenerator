using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectChain : AbstractEffect
{
    private int maxTargets, curTargetNum = 0;           //maximum number of chaining, current number of chains
    private GameObject[] unsortedTargets, previousTargets;      //all targets found within range, the previously hit targets
    public Vector3[] checkPoss;

    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f; //set component weights for spell script to use
        this.SS = SS;
        SS.SetSpellPersist(true);   //ensures the spell wont be destroyed upon impact

        maxTargets = SS.GetSpellPower();   //set max targets to players level
        targets = new GameObject[maxTargets];
        previousTargets = new GameObject[maxTargets];

        checkPoss = new Vector3[maxTargets];
    }
    public override void ApplyEffect()
    {
        //Debug.Log("");
        Debug.Log("Effect Chain apply effect");
        //find targets
        //sort targets by distance
        //set path points between spell and new target

        if (SS.GetShapeScript().GetTriggerPoints().Length == 0) { curTargetNum++; }   //increase the current number of chains
        Debug.Log("Chain effect applied, " + curTargetNum + "/" + maxTargets);

        //if max chains reached, update spell script so its no longer persistent
        if (curTargetNum == maxTargets) 
        { 
            SS.SetSpellPersist(false);
        }
        //otherwise if max chains not reached,
        //check if there are trigger points & if the spell isnt yet casted
        else if (curTargetNum < maxTargets && SS.GetShapeScript().GetTriggerPoints().Length > 0 && !SS.GetCasted())
        {
            MultiPointSort();
        }
        //check if the spell has been cast
        else if (curTargetNum < maxTargets && SS.GetShapeScript().GetTriggerPoints().Length == 0 && SS.GetCasted())
        {
            SinglePointSort();
        }
    }


    //change trigger points, path points
    private void MultiPointSort()
    {
        //Debug.Log("");
        //Debug.Log("EffectChain MultiPointSort");

        //get trigger & path points from spell script
        Vector3 startPoint = SS.GetStartPos();
        Vector3 endPoint = SS.GetEndPos();

        //debug
        //Debug.Log("startPoi: " + startPoint);
        //Debug.Log("endPoi: " + endPoint);

        //get distance from point a & b then divide distance by max targets, initialise new path for spell to use
        float totalDist = Vector3.Distance(startPoint, endPoint);
        float checkDist = totalDist / maxTargets;
        //Vector3[] newPathPoints = new Vector3[maxTargets + 2];
        //newPathPoints[0] = startPoint;
        //newPathPoints[newPathPoints.Length - 1] = endPoint;

        //find nearest target to each point
        for (int point = 0; point < maxTargets; point++)
        {
            //Debug.Log("");

            //reset target array
            unsortedTargets = new GameObject[1];

            //calc point to check
            Vector3 checkPos = Vector3.Lerp(startPoint, endPoint, ((float)(point + 1) / (maxTargets + 1)));
            checkPoss[point] = checkPos;
            //Debug.Log("CheckPos" + (point + 1) + ": " + checkPos);

            //find targets in sphere overlap at points along path
            unsortedTargets = FindTargetsAt(checkPos); //increases the size of unsorted array


            //if there was at least one target found
            if (unsortedTargets[0] != null)
            {
                //for each point, find distance between check pos and point
                float[] dists = new float[unsortedTargets.Length - 1];
                for (int i = 0; i < unsortedTargets.Length - 1; i++)
                {
                    dists[i] = Vector3.Distance(checkPos, unsortedTargets[i].transform.position);
                    //Debug.Log(targets[i].name + " found at " + targets[i].gameObject.transform.position + ", " + dists[i] + " away");
                }

                //sort distance from highest to lowest
                for (int j = 0; j < dists.Length - 1; j++)
                {
                    for (int i = 0; i < dists.Length - 1; i++)
                    {
                        if (dists[i] > dists[i + 1])
                        {
                            float tempDist = dists[i + 1];
                            dists[i + 1] = dists[i];
                            dists[i] = tempDist;

                            GameObject tempTarget = unsortedTargets[i + 1];
                            unsortedTargets[i + 1] = unsortedTargets[i];
                            unsortedTargets[i] = tempTarget;
                        }
                    }
                }
                //Debug.Log("Closest point: " + unsortedTargets[0] + " - " + dists[0]);

                //add closest unsorted target to lowest empty target array pos
                if (unsortedTargets[0] != null) 
                { 
                    for (int pos = 0; pos < targets.Length; pos++) 
                    {
                        if (targets[pos] == null) 
                        {
                            //Debug.Log("target pos" + pos + " empty, adding " + unsortedTargets[0]); 
                            targets[pos] = unsortedTargets[0];
                            break;
                        }
                    }
                }

                //set new path points array to point positions (first & last must stay the same)
                //newPathPoints[point + 1] = targets[0].transform.position;
                //Debug.Log("new path points" + point + ": " + targets[0].transform.position);

                //fill lowest open position with current target to be ignored later
                for (int i = 0; i < previousTargets.Length; i++)
                {
                    if (previousTargets[i] == null && unsortedTargets[0] != null)
                    {
                        //Debug.Log("adding prev target " + targets[0].name);
                        previousTargets[i] = unsortedTargets[0];
                        break;
                    }
                }

                //for(int i = 0; i < newPathPoints.Length; i++) { Debug.Log("new path points" + i + ": " + newPathPoints[i]); }
            }
            //else { newPathPoints[point + 1] = checkPos; }//Debug.Log("targets null"); }
            curTargetNum++;
        }

        //pathPoints = newPathPoints; 
        //for(int i = 0; i < targets.Length; i++) { Debug.Log("Effect Chain target " + i + ": " + targets[i]); }

        if (curTargetNum == maxTargets && SS.GetShapeScript().GetTriggerPoints().Length > 0)
        {
            //for (int i = 0; i < previousTargets.Length; i++) { Debug.Log("Effect Chain prev target" + i + ": " + previousTargets[i]); }
            SS.SetIgnoredTargets(previousTargets);
        }
    }
    private GameObject[] FindTargetsAt(Vector3 checkPos)
    {
        //Debug.Log("");
        //Debug.Log("Finding targets at " + checkPos);
        //find all nearby targets
        int numOfTargets = 0;
        GameObject[] newTargets = new GameObject[1];
        Collider[] collisions = Physics.OverlapSphere(checkPos, 25f);
        for (int check = 0; check < collisions.Length; check++)
        {
            if (collisions[check].CompareTag("Enemy")) //ensure targets are enemies and not current target
            {
                if (!CheckPrevTargets(collisions[check].gameObject)) //a seperate statement to not be run on every object hit, only tagged enemies
                {
                    newTargets[numOfTargets] = collisions[check].gameObject;
                    //Debug.Log(collisions[check].name + " found at " + collisions[check].gameObject.transform.position);
                    numOfTargets++;

                    if (numOfTargets >= newTargets.Length)
                    {
                        GameObject[] tempTargets = new GameObject[numOfTargets + 1];

                        for (int i = 0; i < newTargets.Length; i++) { tempTargets[i] = newTargets[i]; }

                        newTargets = tempTargets;
                    }
                }
            }
        }

        //if (newTargets[0] != null) { for (int i = 0; i < newTargets.Length - 1; i++) { Debug.Log("new target: " + newTargets[i]); } }
        //else { Debug.Log("new targets null"); }
        //Debug.Log("");
        return newTargets;
    }



    private void SinglePointSort()
    {
        //Debug.Log("EffectChain SinglePointSort");
        //Debug.Log("CheckPos: " + this.transform.position);
        //reset targets for new check
        targets = new GameObject[1];
        unsortedTargets = new GameObject[maxTargets];

        //find possible targets, ignoring current target
        unsortedTargets = FindTargets();
        SS.SetIgnoredTargets(previousTargets);

        //for each target, find distance between spell and target
        float[] dists = new float[unsortedTargets.Length];
        for (int i = 0; i < unsortedTargets.Length - 1; i++)
        {
            dists[i] = Vector3.Distance(this.transform.position, unsortedTargets[i].transform.position);
            Debug.Log(unsortedTargets[i].gameObject.transform.parent.name + " " + dists[i] + " away from impact");
        }

        //sort distance from highest to lowest
        for (int j = 0; j < unsortedTargets.Length - 2; j++) //for each target
        {
            for (int i = 0; i < unsortedTargets.Length - 2; i++) //for each other target
            {
                if (dists[i] > dists[i + 1]) //if first distance is greaten than second distance
                {
                    Debug.Log("Swapping " + unsortedTargets[i].gameObject.transform.parent.name + " with " + unsortedTargets[i + 1].gameObject.transform.parent.name);
                    //swap distances
                    float tempDist = dists[i + 1];
                    dists[i + 1] = dists[i];
                    dists[i] = tempDist;

                    GameObject tempTarget = unsortedTargets[i + 1];
                    unsortedTargets[i + 1] = unsortedTargets[i];
                    unsortedTargets[i] = tempTarget;
                }
            }
        }

        //update path points (for spell script later)
        if (unsortedTargets[0] != null)
        {
            Debug.Log("Closest target: " + unsortedTargets[0].gameObject.transform.parent.name + " - " + dists[0]);
            targets[0] = unsortedTargets[0];
        }
        //Debug.Log("new startPos: " + pathPoints[0] + "   new endPos: " + pathPoints[1]);

        //fill lowest open position with current target to be ignored later
        for (int i = 0; i < previousTargets.Length; i++)
        {
            if (previousTargets[i] == null && unsortedTargets[0] != null)
            {
                //Debug.Log("adding prev target " + targets[0].name);
                previousTargets[i] = unsortedTargets[0];
                break;
            }

            if (previousTargets[i] == null)
            {
                SS.SpellDestroy();
                break;
            }
        }
    }
    private GameObject[] FindTargets()
    {
        //Debug.Log("Finding targets at " + this.transform.position);

        //tiny check to find current target
        Collider[] targetCol = Physics.OverlapSphere(this.transform.position, 0.25f);
        if (previousTargets[0] == null)
        {
            for (int check = 0; check < targetCol.Length; check++)
            {
                //Debug.Log(targetCol[check]);
                if (targetCol[check].CompareTag("Enemy"))
                {
                    previousTargets[0] = targetCol[check].gameObject;
                }
            }
            //if (previousTargets[0] != null) { Debug.Log("previous target: " + previousTargets[0].name); }
        }

        //find all nearby targets
        int numOfTargets = 0;
        GameObject[] newTargets = new GameObject[1];
        Collider[] collisions = Physics.OverlapSphere(this.transform.position, 25f);
        for (int check = 0; check < collisions.Length; check++)
        {
            if (collisions[check].CompareTag("Enemy") && !CheckPrevTargets(collisions[check].gameObject)) //ensure targets are enemies and not current target
            {
                newTargets[numOfTargets] = collisions[check].gameObject;
                //Debug.Log(collisions[check].name + " found at " + collisions[check].gameObject.transform.position);
                numOfTargets++;

                if (numOfTargets >= newTargets.Length)
                {
                    GameObject[] tempTargets = new GameObject[numOfTargets + 1];

                    for (int i = 0; i < newTargets.Length; i++) { tempTargets[i] = newTargets[i]; }

                    newTargets = tempTargets;
                }
            }
        }

        //if (newTargets[0] != null) { for (int i = 0; i < newTargets.Length; i++) { Debug.Log("new target: " + newTargets[i]); } }
        //else { Debug.Log("new targets null"); }
        return newTargets;
    }


    private bool CheckPrevTargets(GameObject col) 
    { 
        for(int i = 0; i < previousTargets.Length; i++)
        {
            if (previousTargets[i] != null)
            {
                if (previousTargets[i] == col) { /*Debug.Log("prev target found: " + previousTargets[i]);*/ return true; }
            }
        }

        return false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Set the color of the gizmo
        Gizmos.DrawWireSphere(this.transform.position, 10f); // Draw the wire sphere

        Gizmos.color = Color.yellow;
        if (checkPoss != null) { for (int i = 0; i < checkPoss.Length; i++) { Gizmos.DrawSphere(checkPoss[i], .1f); } }
    }
}