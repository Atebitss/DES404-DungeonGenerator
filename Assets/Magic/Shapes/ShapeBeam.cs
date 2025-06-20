using UnityEngine;

public class ShapeBeam : AbstractShape
{
    private BoxCollider beamCollider;
    private float beamWidth = 1f;
    private float beamLength = 10f;
    private float detectionInterval = 0.1f;
    private float nextDetectionTime = 0f;

    public override void StartShapeScript(SpellScript SS)
    {
        Debug.Log("Beam shape script started");

        damageModifier = 1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = 1f;
        shapeMesh = Resources.Load<Mesh>("CustomMeshes/shapeBeam");
        mainCamera = Camera.main;
        arcAxis = new Vector3(0, 1, 0);
        this.SS = SS;

        if (spellAim[0] == null)
        {
            spellAim[0] = Instantiate(Resources.Load<GameObject>("AimSpellPrefab"), transform);
            aimingLine = spellAim[0].GetComponent<LineRenderer>();
            aimingLine.positionCount = 2;
        }

        firstPointConfirmed = true;
        castable = true;
    }


    //runs when shape is added to spell
    public override void AimSpell()
    {
        Debug.Log("Beam shape aim spell");
        Vector3 playerPos = SS.GetPlayerController().transform.position;
        Vector3 aimPos = GetAimedWorldPos();

        aimingLine.SetPosition(0, playerPos);
        aimingLine.SetPosition(1, aimPos);

        pathPoints[0] = playerPos;
        pathPoints[1] = aimPos;
    }

    public override void UpdateAimPath(Vector3[] addPoints)
    {
        Debug.Log("Beam shape update aim path");
    }


    private void FixedUpdate()
    {
        if (castable && Time.time >= nextDetectionTime)
        {
            //DetectTargets();
            nextDetectionTime = Time.time + detectionInterval;
        }
    }


    //runs when spell is cast
    public override void ApplyShape()
    {
        Debug.Log("Beam shape applied");
        Vector3 playerPos = SS.GetPlayerController().transform.position;
        Vector3 aimPos = GetAimedWorldPos();
        Vector3 beamDirection = (aimPos - playerPos).normalized;
        float beamLength = Vector3.Distance(playerPos, aimPos);

        // Position beam at player location
        transform.position = playerPos;
        transform.rotation = Quaternion.LookRotation(beamDirection);

        // Setup collider for beam detection
        beamCollider = gameObject.AddComponent<BoxCollider>();
        beamCollider.isTrigger = true;
        beamCollider.size = new Vector3(beamWidth, beamWidth, beamLength);
        beamCollider.center = new Vector3(0, 0, beamLength * 0.5f);

        // Remove aiming visualization
        if (spellAim[0] != null)
        {
            Destroy(spellAim[0]);
        }

        castable = true;
        nextDetectionTime = Time.time;
    }


    public override GameObject[] FindShapeTargets()
    {
        Debug.Log("ShapeBeam, FindShapeTargets");



        return null;
    }
}