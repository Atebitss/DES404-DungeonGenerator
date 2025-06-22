using UnityEngine;
using System.Collections;
public abstract class AbstractShape : MonoBehaviour
{
    // 1. Core initialization
    // 2. Aiming phase functions
    // 3. Unity lifecycle
    // 4. Execution phase

    //will be overridden by concrete shape classes
    //spell vars
    public float damageModifier, speedModifier, radiusModifier, cooldownModifier;
    public float speed = 1f, maxLength = 10f;

    //spell shape
    public Mesh shapeMesh;
    public MeshFilter spellMeshFilter, aimMeshFilter;

    //spell info
    public bool castable = false, delayed = false, spellEnded = false;
    public bool GetSpellCastable() { return castable; }
    public bool GetSpellDelayed() { return delayed; }
    public SpellScript SS;
    public AbstractEffect effectScript;
    public AbstractElement elementScript;
    public LayerMask enemyLayerMask;


    //spell aiming
    public Vector3 aimPos, startPos, endPos;
    public Camera mainCamera;
    public LineRenderer aimingLine;
    public GameObject[] spellAim = new GameObject[1];

    public bool firstPointConfirmed = false;
    public bool lastPointConfirmed = false;

    public Vector3 arcAxis;
    public Vector3[] pathPoints = new Vector3[2];


    //spell data
    public Vector3 dir;
    public Vector3 GetDir() { return dir; }
    public float journeyLength;
    public float GetJourneyLength() { return journeyLength; }
    public float startTime;
    public float GetStartTime() { return startTime; }


    //targetting
    public abstract GameObject[] FindShapeTargets();

    public GameObject[] targets = new GameObject[0]; //to keep track of targets that have already been hit
    public bool HasAlreadyHitTarget(GameObject enemy)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == enemy)
            {
                Debug.Log("already hit enemy: " + enemy.name);
                return true;
            }
        }

        return false;
    }


    public abstract void StartShapeScript(SpellScript SS);
    public abstract void AimSpell();
    public abstract void UpdateAimPath(Vector3[] pathPoints);
    public Vector3 GetAimedWorldPos()
    {
        if (mainCamera != null)
        {
            //raycasts from center of view to world position, if hit then update, then return
            LayerMask aimingMask = 0;
            if (SS.GetPlayerController() != null)
            {
                aimingMask = SS.GetPlayerController().GetAimLayerMask();

                //if the spell has a pierce effect, then ignore enemies
                if (SS.GetEffectName().Contains("Pierce") || SS.GetShapeName().Contains("Beam")) { aimingMask = aimingMask & ~LayerMask.GetMask("Enemy"); }
            }

            Ray cameraToWorld = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(cameraToWorld, out RaycastHit hit, SS.GetMaxLookLength(), aimingMask))
            {
                //Debug.Log("hit: " + hit.collider.gameObject);
                aimPos = hit.point;
                pathPoints[pathPoints.Length - 1] = aimPos;
            }

            Debug.Log("aiming line: " + aimingLine + ", pathPoints.Length: " + pathPoints.Length + ", aimingLine.Length: " + aimingLine.positionCount);
            if (aimingLine.positionCount < 3) { aimingLine.SetPosition(pathPoints.Length - 1, aimPos); }
            //Debug.Log("aimpos: " + aimPos);
            return aimPos;
        }

        return Vector3.zero;
    }
    public void EndAim() 
    {
        /*Debug.Log("shape end aim");*/
        lastPointConfirmed = true;
        SS.SetStartPos(pathPoints[0]);
        SS.SetEndPos(pathPoints[pathPoints.Length - 1]);
        for (int i = 0; i < spellAim.Length; i++) { Destroy(spellAim[i]); }
    }
    public abstract void ApplyShape();


    public IEnumerator DelayCast(float delayTime)
    {
        //Debug.Log("Spell Script delay cast");
        delayed = true; //set delayed to true so the spell does not cast immediately
        yield return new WaitForSeconds(delayTime); //wait for the specified time
        delayed = false; //set delayed to false so the spell can be cast

        ApplyShape();
    }


    //spell triggers
    private Vector3[] trigPoints = new Vector3[0];
    public void SetTriggerPoints(Vector3[] points) { trigPoints = points; }
    public void SetTriggerPoint(int point, Vector3 pos) { trigPoints[point] = pos; }
    public Vector3[] GetTriggerPoints() { return trigPoints; }
}