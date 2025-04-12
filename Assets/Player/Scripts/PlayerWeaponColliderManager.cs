using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponColliderManager : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private AudioManager AM;
    public void SetAM(AudioManager newAM) { AM = newAM; }

    private AdaptiveDifficultyManager ADM;
    public void SetADM(AdaptiveDifficultyManager newADM) { ADM = newADM; }

    private PlayerController PC;
    public void SetPC(PlayerController newPC) { PC = newPC; }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~attack on/off~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private bool attacking = false;
    private string attackType = "";

    public void EnableAttackCheck(float attackAnimDur, string attackType)
    {
        Debug.Log("attack enabled");
        attacking = true;
        this.attackType = attackType; //set attack type
        StartCoroutine(OverlapCheck()); //start overlap check coroutine
        Invoke("DisableAttackCheck", attackAnimDur);
    }
    private void DisableAttackCheck()
    {
        Debug.Log("attack disabled");
        StopCoroutine(OverlapCheck()); //start overlap check coroutine
        ResetAttack(); //reset attack state
        attacking = false;
    }
    private void ResetAttack()
    {
        //Debug.Log("resetting attack");
        foundEnemies = new GameObject[0]; //reset found enemies array
        ignoredTrackedEnemies = new GameObject[0]; //reset ignored tracked enemies array

        closestEnemyPos = Vector3.zero; //reset closest enemy position
        closestEnemyRot = Quaternion.identity; //reset closest enemy rotation
        checkSize = Vector3.zero; //reset check size

        attackType = ""; //reset attack type
    }
    //~~~~~attack on/off~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~enemy tracking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private GameObject[] foundEnemies = new GameObject[0]; //track found enemies, used to avoid repeatedly damaging the same enemy

    private IEnumerator OverlapCheck()
    {
        yield return new WaitForSeconds(0.1f); //wait for 0.1 seconds

        //check for already overlapping enemy colliders
        BoxCollider weaponAttackCollider = GetComponent<BoxCollider>();
        //Debug.Log("weaponAttackCollider: " + weaponAttackCollider);
        Collider[] hitColliders = Physics.OverlapBox
        (
            weaponAttackCollider.transform.position,
            weaponAttackCollider.transform.localScale / 2,
            weaponAttackCollider.transform.rotation,
            LayerMask.GetMask("Enemy")
        );

        for (int hitIndex = 0; hitIndex < hitColliders.Length; hitIndex++)
        {
            //Debug.Log(hitIndex + ": " + hitColliders[hitIndex].gameObject.name);
            if (hitColliders[hitIndex].gameObject.tag == "Enemy")
            {
                Debug.Log("enemy found in overlap: " + hitColliders[hitIndex].gameObject.transform.parent.name);
                //process enemies found in overlap
                ProcessEnemy(hitColliders[hitIndex].gameObject);
            }
        }

        StartCoroutine(OverlapCheck()); //start overlap check coroutine
    }
    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (attacking && col.gameObject.tag == "Enemy")
        {
            Debug.Log("enemy found in swing: " + col.gameObject.transform.parent.name);
            ProcessEnemy(col.gameObject); //check if enemy is the closest enemy
        }
    }
    private void OnTriggerExit(Collider exitCol)
    {
        //remove enemy from array
        for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++)
        {
            if (foundEnemies[enemyIndex] == exitCol.gameObject)
            {
                //remove enemy from tracked enemies array
                GameObject[] newFoundEnemies = new GameObject[foundEnemies.Length - 1];
                for (int newIndex = 0; newIndex < enemyIndex; newIndex++) { newFoundEnemies[newIndex] = foundEnemies[newIndex]; }
                for (int newIndex = enemyIndex; newIndex < newFoundEnemies.Length; newIndex++) { newFoundEnemies[newIndex] = foundEnemies[newIndex + 1]; }
                foundEnemies = newFoundEnemies;
            }
        }

        for(int enemyIndex = 0; enemyIndex < ignoredTrackedEnemies.Length; enemyIndex++)
        {
            if (ignoredTrackedEnemies[enemyIndex] == exitCol.gameObject)
            {
                //remove enemy from ignored tracked enemies array
                GameObject[] newIgnoredTrackedEnemies = new GameObject[ignoredTrackedEnemies.Length - 1];
                for (int newIndex = 0; newIndex < enemyIndex; newIndex++) { newIgnoredTrackedEnemies[newIndex] = ignoredTrackedEnemies[newIndex]; }
                for (int newIndex = enemyIndex; newIndex < newIgnoredTrackedEnemies.Length; newIndex++) { newIgnoredTrackedEnemies[newIndex] = ignoredTrackedEnemies[newIndex + 1]; }
                ignoredTrackedEnemies = newIgnoredTrackedEnemies;
            }
        }
    }
    //~~~~~enemy tracking~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~enemy processing~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private GameObject[] ignoredTrackedEnemies = new GameObject[0]; //track enemies that are ignored by the collider
    //temp
    private Vector3 closestEnemyPos = Vector3.zero;
    private Quaternion closestEnemyRot = Quaternion.identity;
    private Vector3 checkSize = Vector3.zero;

    private void ProcessEnemy(GameObject curEnemy)
    {
        Debug.Log("processing enemy: " + curEnemy.transform.parent.name);

        //add enemy to array
        GameObject[] newFoundEnemies = new GameObject[foundEnemies.Length + 1];
        for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++) { newFoundEnemies[enemyIndex] = foundEnemies[enemyIndex]; }
        newFoundEnemies[foundEnemies.Length] = curEnemy.gameObject;
        foundEnemies = newFoundEnemies;

        if(attackType == "light")
        { 
            //find closest enemy
            float closestDist = Mathf.Infinity; //set closest distance to infinity
            int closestEnemyID = -1; //set closest enemy to null

            //for each enemy in array
            for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++)
            {
                //check if enemy is the closest enemy to player
                float distCheck;
                if (foundEnemies[enemyIndex] != null)
                {
                    distCheck = Vector3.Distance(foundEnemies[enemyIndex].transform.position, this.transform.position);

                    if (distCheck < closestDist)
                    {
                        closestDist = distCheck; //set closest distance to distance to enemy
                        closestEnemyID = enemyIndex; //set closest enemy to enemy
                    }
                }
            }

            Debug.Log("closest enemy: " + foundEnemies[closestEnemyID].transform.parent.name);
            //if closest enemy is not null
            if (closestEnemyID != -1)
            {
                //Debug.Log("closest enemy: " + closestEnemy.name);
                //check for enemies behind it
                closestEnemyPos = foundEnemies[closestEnemyID].transform.position; //enemy pos 
                closestEnemyRot = foundEnemies[closestEnemyID].transform.rotation; //enemy rotation
                                                                                   //space to check behind enemy ie. half enemy scale x and y, twice player weapon scale z
                checkSize = new Vector3((foundEnemies[closestEnemyID].transform.localScale.x / 2), (foundEnemies[closestEnemyID].transform.localScale.y / 2), 5f); //size of check box

                //enemies found behind closest enemy
                Collider[] hitColliders = Physics.OverlapBox
                (
                    closestEnemyPos,
                    checkSize,
                    closestEnemyRot,
                    LayerMask.GetMask("Enemy")
                );


                //for each enemy found behind closest enemy
                for (int hitIndex = 0; hitIndex < hitColliders.Length; hitIndex++)
                {
                    //if enemy is not the closest enemy
                    if (hitColliders[hitIndex].gameObject != foundEnemies[closestEnemyID] && hitColliders[hitIndex].gameObject.tag == "Enemy")
                    {
                        Debug.Log("enemy found behind closest enemy: " + hitColliders[hitIndex].transform.parent.gameObject.name);
                        //if the enemy is already in the ignore array, skip
                        for (int enemyIndex = 0; enemyIndex < ignoredTrackedEnemies.Length; enemyIndex++)
                        {
                            if (hitColliders[hitIndex].gameObject == ignoredTrackedEnemies[enemyIndex]) { Debug.Log("enemy already ignored"); return; }
                        }

                        Debug.Log("enemy added to ignored tracked enemies");
                        //add enemy to ignored tracked enemies
                        GameObject[] newIgnoredTrackedEnemies = new GameObject[ignoredTrackedEnemies.Length + 1];
                        for (int enemyIndex = 0; enemyIndex < ignoredTrackedEnemies.Length; enemyIndex++) { newIgnoredTrackedEnemies[enemyIndex] = ignoredTrackedEnemies[enemyIndex]; }
                        newIgnoredTrackedEnemies[ignoredTrackedEnemies.Length] = hitColliders[hitIndex].gameObject;
                        ignoredTrackedEnemies = newIgnoredTrackedEnemies;
                    }
                }
            }
        }


        DamageEnemy(curEnemy); //damage enemy
    }
    private void OnDrawGizmos()
    {
        //draw gizmos for hitColliders Physics.OverlapBox
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(closestEnemyPos, checkSize); //draw box for enemy check
    }
    //~~~~~enemy processing~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~attack damage~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //player weapon particle system
    private ParticleSystem PS;
    public void SetHitParticle(ParticleSystem newPS) { PS = newPS; }

    //damage number visual feedback
    private GameObject hitSplashPrefab;
    public void SetHitSplash(GameObject newHS) { hitSplashPrefab = newHS; }

    //player damage
    private int attackDamage = 1;
    public void SetWeaponDamage(int newDamage) { attackDamage = newDamage; }


    //damage found enemies
    private void DamageEnemy(GameObject enemy)
    {
        Debug.Log("attempting to damage " + enemy.transform.parent.name);
        attackDamage = (PC.GetAttackDamage() + PC.GetAttackDamageModifier()); //get attack damage from player controller

        //if enemy is ignored array, skip
        for (int enemyIndex = 0; enemyIndex < ignoredTrackedEnemies.Length; enemyIndex++)
        {
            if (ignoredTrackedEnemies[enemyIndex] == enemy) { Debug.Log("enemy is ignored"); return; }
        }

        Debug.Log("adding enemy to ignore list");
        //add enemy to ignored tracked enemies so they cant be hit again
        GameObject[] newIgnoredTrackedEnemies = new GameObject[ignoredTrackedEnemies.Length + 1];
        for (int enemyIndex = 0; enemyIndex < ignoredTrackedEnemies.Length; enemyIndex++) { newIgnoredTrackedEnemies[enemyIndex] = ignoredTrackedEnemies[enemyIndex]; }
        newIgnoredTrackedEnemies[ignoredTrackedEnemies.Length] = enemy;
        ignoredTrackedEnemies = newIgnoredTrackedEnemies;


        Debug.Log("damaging enemy");
        //damage enemy
        Vector3 collisionPoint = enemy.GetComponent<Collider>().ClosestPoint(transform.position); //find collision point
        enemy.GetComponent<AbstractEnemy>().MoveEPS(collisionPoint); //move hit particle system to collision point
        enemy.GetComponent<AbstractEnemy>().AlterHealth(-attackDamage); //deal damage to enemy


        //create hit splash halfway to collision point
        Vector3 midPoint = ((((collisionPoint + this.transform.position) / 2) + (this.transform.position - collisionPoint).normalized));
        GameObject curHitSplash = Instantiate(hitSplashPrefab, collisionPoint, Quaternion.Euler(0f, 0f, 0f));

        //wake and play hit splash animation
        curHitSplash.GetComponent<HitSplashController>().Wake(PC, attackDamage);


        //play hit sound and particle system
        AM.Play("Sword_Hit" + Random.Range(1, 3));
        PS.Play();


        ADM.AttackSuccess(); //increment attack success count in adaptive difficulty manager
        Invoke("StopPS", 0.5f); //stop particle system after 0.5 seconds
    }
    private void StopPS() { PS.Stop(); }
    //~~~~~attack damage~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
