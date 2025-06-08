using UnityEngine;

public class EffectTeleport : AbstractEffect
{
    private float teleportRange = 15f;
    private int maxAttempts = 3;

    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        //move hit targets to random position
        //or if shape self, move player to looked at position
        Debug.Log("Teleport effect applied");
        GameObject[] targets = SS.GetSpellTargets();
        Debug.Log("targets length: " + targets.Length);

        for (int i = 0; i < targets.Length; i++)
        {
            Debug.Log("Teleporting target: " + targets[i].name);
            Vector3 teleportPosition = FindValidTeleportLocation(targets[i].transform.position);
            if (teleportPosition != Vector3.zero)
            {
                targets[i].transform.position = teleportPosition; //set target position to new position
            }
        }
    }
    private Vector3 FindValidTeleportLocation(Vector3 curPos)
    {
        //find random teleport direction
        //check if position is not colliding with anything
        //if colliding, update position with collided position minus offset
        Debug.Log("Finding valid teleport location");
        Vector3 newPos = Vector3.zero;

        //for each attempt
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            //find random direction
            Vector2 randDir = Random.insideUnitCircle;
            Vector3 dir = new Vector3(randDir.x, 0, randDir.y).normalized;
            Vector3 landingPos = Vector3.zero;
            Debug.Log("dir: " + dir);
            Debug.Log("possible land pos: " + (curPos + (dir * teleportRange)));

            //check if direction collides with terrain
            Debug.DrawRay(curPos, (dir * teleportRange), Color.red, teleportRange);
            if (Physics.Raycast(curPos, dir, out RaycastHit teleHit, teleportRange, LayerMask.GetMask("Terrain")))
            {
                landingPos = (teleHit.point - dir); //if collides, set landing position to hit point - offset
                Debug.Log("new land pos: " + landingPos);
            }
            else
            {
                landingPos = (curPos + (dir * teleportRange)); //calculate new position
                Debug.Log("max land pos: " + landingPos);
            }

            //check if position is over terrain
            Debug.DrawRay(landingPos, (Vector3.down * 5f), Color.red, 5f);
            if (Physics.Raycast(landingPos, Vector3.down, out RaycastHit standHit, 5f, LayerMask.GetMask("Terrain")))
            {
                Debug.Log("Landing position is valid: " + landingPos);
                newPos = landingPos;
                break;
            }
            else { Debug.Log("Landing position is not valid, trying again"); }

            if (attempt == (maxAttempts - 1))
            {
                Debug.LogWarning("Failed to find valid teleport location");
                newPos = Vector3.zero; //if no valid position found, return current position
            }
        }

        Debug.Log("returning new position: " + newPos);
        return newPos;
    }
}
