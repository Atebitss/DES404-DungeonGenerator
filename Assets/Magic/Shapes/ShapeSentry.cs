using UnityEngine;

public class ShapeSentry : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Sentry shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeSentry");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Sentry shape aim spell");
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Sentry shape update aim path");
    }


    private void FixedUpdate()
    {
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        Debug.Log("Sentry shape applied");
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBeam, FindShapeTargets");



        return null;
    }
}