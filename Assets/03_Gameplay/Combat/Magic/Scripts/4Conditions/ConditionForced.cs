using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionForced : AbstractCondition
{
    private float spellStartTime = float.PositiveInfinity;
    private Vector3 dir;
    public void SetDir(Vector3 dir) { this.dir = dir; }//Debug.Log(dir); }

    public override void ApplyCondition()
    {
        //Debug.Log("Force condition applied to " + this.gameObject.name);

        targetScript = this.GetComponent<AbstractEnemy>();
        targetScript.SetIsActive(false);

        //set enemy colour
        Material elementMaterial = Resources.Load<Material>("SpellMaterials/ElementForceMaterial");
        targetScript.SetMaterial(elementMaterial);
        spellStartTime = Time.time;
        StartCoroutine(MoveToTarget());
    }
    IEnumerator MoveToTarget()
    {
        //Debug.Log("Condition Forced move to target");
        //move target away from impact
        Vector3 startPos = this.transform.position;
        //Debug.Log(startPos);
        Vector3 targetPos = (this.transform.position + (dir * 2.5f));
        //Debug.Log(targetPos);

        Collider collider = this.gameObject.GetComponent<Collider>();
        Vector3[] cornerOffsets = new Vector3[]
        {
                new Vector3(-collider.bounds.size.x / 2, collider.bounds.size.y / 2, collider.bounds.size.z / 2),  //top left
                new Vector3(collider.bounds.size.x / 2, collider.bounds.size.y / 2, -collider.bounds.size.z / 2),   //top right
                new Vector3(-collider.bounds.size.x / 2, -collider.bounds.size.y / 2, collider.bounds.size.z / 2), //bottom left
                new Vector3(collider.bounds.size.x / 2, -collider.bounds.size.y / 2, -collider.bounds.size.z / 2)   //bottom right
        };

        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;
        //Debug.Log("CF, startTime: " + startTime);

        //while the target is not at end point within 0.1f
        float remainingDistance = Vector3.Distance(this.transform.position, targetPos);
        while (remainingDistance >= 0.1f)
        {
            //Debug.Log("CF, remainingDistance: " + remainingDistance + "   time: " + Time.time + "/" + (startTime + 1f));

            //Debug.Log(this.transform.position);
            float travelInterpolate = (Time.time - startTime) * 5 / journeyLength;
            Vector3 nextPosition = Vector3.Lerp(startPos, targetPos, travelInterpolate);

            //check if the target intersects with an object on the Solid layer
            for (int i = 0; i < cornerOffsets.Length; i++)
            {
                Debug.DrawRay((collider.bounds.center + cornerOffsets[i]), dir * 0.1f, Color.blue, 10f);
                if (Physics.Raycast((collider.bounds.center + cornerOffsets[i]), dir, out RaycastHit hit, 0.1f))
                {
                    //Debug.Log(this.gameObject.name + " solid collision: " + hit.collider.gameObject.name);
                    targetScript.DamageTarget(1, "force");
                    yield break;
                }
            }

            transform.position = nextPosition;

            yield return null;
        }

        EndCondition();
    }

    private void FixedUpdate()
    {
        if (Time.time >= (spellStartTime + 1f))
        {
            StopCoroutine(MoveToTarget());
            EndCondition(); 
        }
    }

    void OnDestroy()
    {
        targetScript.SetIsActive(true);
        targetScript.ResetMaterial();
    }
}