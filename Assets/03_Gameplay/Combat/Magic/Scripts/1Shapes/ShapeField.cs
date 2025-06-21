using UnityEngine;

public class ShapeField : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Field shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeField");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;

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
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Field shape aim spell");
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Field shape update aim path");
    }


    private void FixedUpdate()
    {
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        Debug.Log("Field shape applied");
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeField, FindShapeTargets");



        return null;
    }
}