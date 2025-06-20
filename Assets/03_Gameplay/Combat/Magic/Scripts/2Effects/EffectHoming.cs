using UnityEngine;

public class EffectHoming : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 1; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        //Debug.Log("Homing effect applied");

        if (targets[0] == null)
        {
            //on initial call, find closest target to aim position
            Debug.Log("Homing effect initial call");
            targets[0] = FindClosestEnemyToPos(SS.GetEndPos());
        }
    }

    private GameObject FindClosestEnemyToPos(Vector3 pos)
    {
        Debug.Log("Homing effect finding enemy");
        GameObject closestEnemy = null;
        float closestDistance = float.MaxValue;

        Collider[] collisions = Physics.OverlapSphere(pos, 25f);
        for (int i = 0; i < collisions.Length; i++)
        {
            if (collisions[i].tag == "Enemy")
            {
                Debug.Log("Homing effect found: " + collisions[i].gameObject.name);
                float distance = Vector3.Distance(pos, collisions[i].transform.position);
                Debug.Log("Homing effect target distance: " + distance);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = collisions[i].gameObject;
                    Debug.Log("Homing effect target updated at: " + closestEnemy.transform.position);
                }
            }
        }

        if (closestEnemy != null) { Debug.Log("Homing effect target: " + closestEnemy.name); }
        return closestEnemy;
    }
}
