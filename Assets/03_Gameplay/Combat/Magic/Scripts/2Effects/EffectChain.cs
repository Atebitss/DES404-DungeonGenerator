using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectChain : AbstractEffect
{
    private GameObject[] unsortedTargets, previousTargets, groupTargets;      //all targets found within range, the previously hit targets
    public Vector3[] checkPoss; //create array to hold targets for path points
    private Vector3 searchPos;
    private bool triggered = false;

    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f; //set component weights for spell script to use
        this.SS = SS;
        SS.SetSpellPersist(true);   //ensures the spell wont be destroyed upon impact

        maxTargets = SS.GetSpellPower();   //set max targets to players level
        targets = new GameObject[1];
        previousTargets = new GameObject[maxTargets];

        checkPoss = new Vector3[maxTargets];

        //if shape is beam, find targets on cast
        if (SS.GetShapeName().Contains("Beam")) { componentWeight = 1; }
    }
    public override void ApplyEffect()
    {
        //Debug.Log("Effect Chain apply effect");
        //find targets
        //sort targets by distance
        //set path points between spell and new target

        if (!SS.GetShapeName().Contains("Beam")) { curTargetNum++; }   //increase the current number of chains for incremental shapes

        //if max chains reached, update spell script so its no longer persistent
        if (SS.GetShapeName().Contains("Ball") && curTargetNum == maxTargets) 
        { 
            SS.SetSpellPersist(false);
        }
        //otherwise if max chains not reached,
        //check if the spell has been cast
        else if (curTargetNum <= maxTargets)
        {
            //Debug.Log(curTargetNum + " < " + maxTargets + ", checking for targets");
            if (SS.GetShapeName().Contains("Beam"))
            {
                //find all targets
                groupTargets = new GameObject[maxTargets]; //create array to hold targets for path points

                //for each possible target
                for (int i = 0; curTargetNum < maxTargets; i++)
                {
                    //Debug.Log("Chain effect applied, " + curTargetNum + "/" + maxTargets);
                    SinglePointSort(); //find one target
                    //Debug.Log("Target found: " + targets[0].gameObject.transform.parent.name + " at " + targets[0].transform.position);
                    groupTargets[i] = targets[0]; //add target position to array
                    previousTargets[i] = targets[0]; //add target to previous targets array
                    //Debug.Log("Target confirmed: " + groupTargets[i]);
                    curTargetNum++;
                }

                targets = groupTargets;
                triggered = true;
            }
            else if(SS.GetCasted())
            {
                //find one target
                SinglePointSort();
            }
        }
    }

    private void SinglePointSort()
    {
        //Debug.Log("EffectChain SinglePointSort");
        //Debug.Log("CheckPos: " + this.transform.position);
        //reset targets for new check
        targets = new GameObject[1];
        unsortedTargets = new GameObject[10];

        //find possible targets, ignoring current target
        unsortedTargets = FindTargets();


        //for each target, find distance between spell and target
        float[] dists = new float[unsortedTargets.Length];
        for (int i = 0; i < unsortedTargets.Length - 1; i++)
        {
            dists[i] = Vector3.Distance(this.transform.position, unsortedTargets[i].transform.position);
            //Debug.Log(unsortedTargets[i].gameObject.transform.parent.name + " " + dists[i] + " away from impact");
        }

        //sort distance from highest to lowest
        for (int j = 0; j < unsortedTargets.Length - 2; j++) //for each target
        {
            for (int i = 0; i < unsortedTargets.Length - 2; i++) //for each other target
            {
                if (dists[i] > dists[i + 1]) //if first distance is greaten than second distance
                {
                    //Debug.Log("Swapping " + unsortedTargets[i].gameObject.transform.parent.name + " with " + unsortedTargets[i + 1].gameObject.transform.parent.name);
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

        //update primary target with closest target
        if (unsortedTargets[0] != null)
        {
            //Debug.Log("Closest target: " + unsortedTargets[0].gameObject.transform.parent.name + " - " + dists[0]);
            targets[0] = unsortedTargets[0];
        }
        //Debug.Log("new startPos: " + pathPoints[0] + "   new endPos: " + pathPoints[1]);
    }
    private GameObject[] FindTargets()
    {
        if (SS.GetShapeName().Contains("Beam"))
        {
            //Debug.Log("z, " + groupTargets[(curTargetNum - 1)] + ", " + (curTargetNum - 1));
            if (curTargetNum > 1)
            {
                //Debug.Log("Finding targets at " + groupTargets[(curTargetNum - 2)].transform.position);
                searchPos = groupTargets[(curTargetNum - 2)].transform.position;
            }
            else
            {
                //Debug.Log("Finding targets at " + SS.GetEndPos());
                searchPos = SS.GetEndPos();
            }
        }
        else
        {
            //Debug.Log("Finding targets at " + this.transform.position);
            searchPos = this.transform.position; //default search position is the spell position
        }

        if (!SS.GetShapeName().Contains("Beam"))
        {
            //tiny check to find current target
            Collider[] targetCol = Physics.OverlapSphere(searchPos, 0.25f);
            for (int obj = 0; obj < targetCol.Length; obj++) //for each found object
            {
                //Debug.Log("targetCol: " + targetCol[obj]);

                //if the previous target is null, is an enemy, and is not already in the previous targets
                if (targetCol[obj].CompareTag("Enemy") && !CheckPrevTargets(targetCol[obj].gameObject))
                {
                    for (int check = 0; check < previousTargets.Length; check++) //for each position
                    {
                        if (previousTargets[check] == null)
                        {
                            //add found target to first empty previous targets pos
                            //Debug.Log("current target found: " + targetCol[obj].transform.parent.name);
                            previousTargets[check] = targetCol[obj].gameObject;
                            //SS.SetIgnoredTargets(previousTargets);
                            check = previousTargets.Length; //break out of loop, no need to check further
                            break;
                        }
                    }
                }
            }
        }

        //find all nearby targets
        int numOfTargets = 0;
        GameObject[] newTargets = new GameObject[1];
        Collider[] collisions = Physics.OverlapSphere(this.transform.position, 25f);
        for (int check = 0; check < collisions.Length; check++)
        {
            if (collisions[check].CompareTag("Enemy") && !CheckPrevTargets(collisions[check].gameObject)) //ensure targets are enemies and not current target
            {
                //add found target to end of new targets array
                newTargets[numOfTargets] = collisions[check].gameObject;
                //Debug.Log(collisions[check].name + " found at " + collisions[check].gameObject.transform.position);
                numOfTargets++;

                //increase size of new targets array
                if (numOfTargets >= newTargets.Length)
                {
                    GameObject[] tempTargets = new GameObject[numOfTargets + 1];
                    for (int i = 0; i < newTargets.Length; i++) { tempTargets[i] = newTargets[i]; }
                    newTargets = tempTargets;
                }
            }
        }

        //if (newTargets[0] != null) { for (int i = 0; i < newTargets.Length; i++) { //Debug.Log("new target: " + newTargets[i]); } }
        //else { //Debug.Log("new targets null"); }
        return newTargets;
    }


    private bool CheckPrevTargets(GameObject col) 
    { 
        for(int i = 0; i < previousTargets.Length; i++)
        {
            if (previousTargets[i] != null)
            {
                if (previousTargets[i] == col) { /*//Debug.Log("prev target found: " + previousTargets[i]);*/ return true; }
            }
        }

        return false;
    }


    private void FixedUpdate()
    {
        if (triggered)
        {
            bool foundDeadEnemy = false;

            //check if any enemies are dead
            for (int enemyID = 0; enemyID < groupTargets.Length; enemyID++)
            {
                if (groupTargets[enemyID] == null)
                {
                    foundDeadEnemy = true;
                    break;
                }
            }

            //if dead enemies found, rebuild the array
            if (foundDeadEnemy)
            {
                //count living enemies
                int livingCount = 0;
                for (int i = 0; i < groupTargets.Length; i++)
                {
                    if (groupTargets[i] != null)
                    {
                        livingCount++;
                    }
                }

                //create new array with only living enemies
                GameObject[] newTargets = new GameObject[livingCount];
                int newIndex = 0;
                for (int i = 0; i < groupTargets.Length; i++)
                {
                    if (groupTargets[i] != null)
                    {
                        newTargets[newIndex] = groupTargets[i];
                        newIndex++;
                    }
                }

                //update vars
                groupTargets = newTargets;
                targets = groupTargets;
                maxTargets = livingCount;
                curTargetNum = livingCount;
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Set the color of the gizmo
        Gizmos.DrawWireSphere(this.transform.position, 10f); // Draw the wire sphere

        Gizmos.color = Color.green;
        if (checkPoss[0] != Vector3.zero) { for (int i = 0; i < checkPoss.Length; i++) { Gizmos.DrawSphere(checkPoss[i], .1f); } }
        else { Gizmos.DrawSphere(searchPos, .25f); }
    }
}