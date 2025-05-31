using UnityEngine;
public abstract class AbstractShape : MonoBehaviour
{
    //will be overridden by concrete shape classes
    //spell vars
    public float damageModifier, speedModifier, radiusModifier, cooldownModifier;

    //spell shape
    public Mesh shapeMesh;
    public MeshFilter spellMeshFilter, aimMeshFilter;

    //spell info
    public bool castable = false;
    public SpellScript SS;
    public abstract void StartShapeScript(SpellScript SS);
    public abstract void ApplyShape();

    //spell aiming
    public Camera mainCamera;
    public Vector3 aimPos;
    public Vector3 arcAxis;
    public Vector3[] pathPoints = new Vector3[2];
    public GameObject[] spellAim = new GameObject[1];
    public bool firstPointConfirmed = false;
    public bool lastPointConfirmed = false;
    public LineRenderer aimingLine;
    public abstract void AimSpell();
    public abstract void UpdateAimPath(Vector3[] pathPoints);
    public void EndAim() 
    {
        /*Debug.Log("shape end aim");*/
        lastPointConfirmed = true;
        SS.SetStartPos(pathPoints[0]);
        SS.SetEndPos(pathPoints[pathPoints.Length - 1]);
        for (int i = 0; i < spellAim.Length; i++) { Destroy(spellAim[i]); }
    }

    //spell triggers
    private Vector3[] trigPoints = new Vector3[0];
    public void SetTriggerPoints(Vector3[] points) { trigPoints = points; }
    public void SetTriggerPoint(int point, Vector3 pos) { trigPoints[point] = pos; }
    public Vector3[] GetTriggerPoints() { return trigPoints; }
}