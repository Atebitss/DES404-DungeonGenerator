using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponColliderManager : MonoBehaviour
{
    private bool attacking = false;
    private int attackDamage = 1;
    private GameObject[] foundEnemies = new GameObject[0];
    private AudioManager AM;
    private AdaptiveDifficultyManager ADM;
    private PlayerController PC;
    private ParticleSystem PS;
    private GameObject hitSplashPrefab;


    public void SetHitParticle(ParticleSystem newPS) { PS = newPS; }
    public void SetHitSplash(GameObject newHS) { hitSplashPrefab = newHS; }
    public void SetWeaponDamage(int newDamage) { attackDamage = newDamage; }
    public void SetAM(AudioManager newAM) { AM = newAM; }
    public void SetADM(AdaptiveDifficultyManager newADM) { ADM = newADM; }
    public void SetPC(PlayerController newPC) { PC = newPC; }


    public void EnableAttackCheck(float attackAnimDur)
    {
        Debug.Log("attack enabled");
        attacking = true;
        Invoke("OverlapCheck", (attackAnimDur / 2));
        Invoke("DisableAttackCheck", attackAnimDur);
    }
    private void DisableAttackCheck()
    {
        //Debug.Log("attack disabled");
        attacking = false;
    }


    private void OverlapCheck()
    {
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
                //Debug.Log("enemy found in overlap");
                //damage enemies found in overlap
                DamageEnemy(hitColliders[hitIndex].gameObject);
            }
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (attacking && col.gameObject.tag == "Enemy")
        {
            //Debug.Log("enemy found in swing");
            DamageEnemy(col.gameObject);
        }
    }
    private void OnTriggerExit(Collider exitCol)
    {
        //remove enemy from array
        for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++)
        {
            if (foundEnemies[enemyIndex] == exitCol.gameObject)
            {
                GameObject[] newFoundEnemies = new GameObject[foundEnemies.Length - 1];
                for (int newIndex = 0; newIndex < enemyIndex; newIndex++) { newFoundEnemies[newIndex] = foundEnemies[newIndex]; }
                for (int newIndex = enemyIndex; newIndex < newFoundEnemies.Length; newIndex++) { newFoundEnemies[newIndex] = foundEnemies[newIndex + 1]; }
                foundEnemies = newFoundEnemies;
            }
        }
    }

    private void DamageEnemy(GameObject enemy)
    {
        //Debug.Log("enemy damaged");
        //if enemy isnt already in array
        for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++)
        {
            if (foundEnemies[enemyIndex] == enemy) { return; }
        }

        //add enemy to array
        GameObject[] newFoundEnemies = new GameObject[foundEnemies.Length + 1];
        for (int enemyIndex = 0; enemyIndex < foundEnemies.Length; enemyIndex++) { newFoundEnemies[enemyIndex] = foundEnemies[enemyIndex]; }
        newFoundEnemies[foundEnemies.Length] = enemy;
        foundEnemies = newFoundEnemies;


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
}
