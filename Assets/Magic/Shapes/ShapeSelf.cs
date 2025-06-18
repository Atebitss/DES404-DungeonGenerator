using UnityEngine;

public class ShapeSelf : AbstractShape
{
    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Self shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeSelf");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Self shape aim spell");
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Self shape update aim path");
    }


    private void FixedUpdate()
    {
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        Debug.Log("Self shape applied");
    }
}