using UnityEngine;

public class ShapeRing : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Ring shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeRing");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Ring shape aim spell");
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Ring shape update aim path");
    }


    private void FixedUpdate()
    {
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        Debug.Log("Ring shape applied");
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeRing, FindShapeTargets");



        return null;
    }
}