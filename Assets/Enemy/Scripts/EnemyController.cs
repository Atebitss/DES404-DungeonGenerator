using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AbstractEnemy
{
    void Start()
    {
        //Debug.Log("Moving Target awake");

        //set stats
        health = 1;
        damage = 1;
        speed = 5;

        enemyRenderer = this.gameObject.GetComponent<Renderer>();
        baseMaterial = enemyRenderer.material;

        SM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = SM.GetAudioManager();
        PC = SM.GetPlayerController();

        //aim towards player
        //Transform playerPos = PC.gameObject.transform;
        //transform.LookAt(playerPos);
    }
    private void OnTriggerEnter(Collider col)
    {
        //check to find if collided target is player
        /*Collider[] targetCol = Physics.OverlapSphere(this.transform.position, 1f);
        for (int check = 0; check < targetCol.Length; check++)
        {
            Debug.Log(this.gameObject.name + ": " + targetCol[check]);
            if (targetCol[check].CompareTag("Player"))
            {
                SM.GetPlayerScript().DecPlayerHealth();
                Destroy(this.gameObject);
            }
        }*/

        //Debug.Log(this.gameObject.name + ": " + col);
        if (col.gameObject.tag == "Player")
        {
            PC.AlterCurrentHealthPoints(-damage);
            //Destroy(this.gameObject);
        }
    }

    public void StartEnemy(AbstractSceneManager SM)
    {
        //Debug.Log("Moving Target start enemy");

        this.SM = SM;
        moving = true;

        StartMovement();
    }

    public override void StartMovement() { /*Debug.Log("Moving Target start movement: " + moving);*/ if (moving && this.gameObject != null) { StartCoroutine(MoveToTarget()); } }
    IEnumerator MoveToTarget()
    {
        //Debug.Log(this.gameObject.name + ": " + "Moving Target move to target");
        //Debug.Log(this.gameObject.name + ": " + this.transform.position);

        //begining position, distance between start and end, time spell began travelling
        Vector3 startPos = this.transform.position;
        Vector3 endPos = PC.gameObject.transform.position;
        Vector3 dir = (endPos - startPos).normalized;
        //Debug.Log(this.gameObject.name + ": " + startPos);
        //Debug.Log(this.gameObject.name + ": " + endPos);
        //Debug.Log(this.gameObject.name + ": " + dir);

        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;
        //Debug.Log(this.gameObject.name + ": " + journeyLength);
        //Debug.Log(this.gameObject.name + ": " + startTime);

        //while the enemy is not at the target (with a small leeway) and allowed to move, 
        while (Vector3.Distance(this.transform.position, endPos) > 1f && moving)
        {
            //Debug.Log(this.gameObject.name + ": " + (Vector3.Distance(this.transform.position, endPos) > 0.01f));
            float travelInterpolate = (Time.time - startTime) * speed / journeyLength;
            Vector3 nextPosition = Vector3.Lerp(startPos, endPos, travelInterpolate);

            //check if the enemy has reached its position
            Debug.DrawRay(transform.position, dir, Color.red, 1f);
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Vector3.Distance(transform.position, nextPosition) + 0.1f) && hit.collider.gameObject.tag != "Spell" && hit.collider.gameObject.tag != "Enemy") { yield break; }

            transform.position = nextPosition;

            yield return null;
        };
    }

    /*void OnDestroy()
    {
        PC.RemoveEnemyFromArrays(this.gameObject);
    }*/



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the gizmo
        Gizmos.DrawWireSphere(this.transform.position, 1f); // Draw the wire sphere
    }
}