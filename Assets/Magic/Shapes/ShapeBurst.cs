using UnityEngine;

public class ShapeBurst : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Burst shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBurst");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Burst shape aim spell");
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Burst shape update aim path");
    }


    private void FixedUpdate()
    {
    }


    public override void ApplyShape()
    {
        Debug.Log("Burst shape applied");
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBurst, FindShapeTargets");



        return null;
    }
}