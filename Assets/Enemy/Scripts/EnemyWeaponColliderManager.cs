using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponColliderManager : MonoBehaviour
{
    private bool attacking = false;
    private int attackDamage = 1;
    private AudioManager AM;
    private AdaptiveDifficultyManager ADM;


    public void SetWeaponDamage(int newDamage) { attackDamage = newDamage; }
    public void SetAM(AudioManager newAM) { AM = newAM; }
    public void SetADM(AdaptiveDifficultyManager newADM) { ADM = newADM; }


    public void EnableAttackCheck(float attackAnimDur)
    {
        //Debug.Log("attack enabled on " + this.gameObject.name);
        attacking = true;
        StartCoroutine(OverlapCheck()); //start overlap check coroutine
        Invoke("DisableAttackCheck", attackAnimDur);
    }
    private void DisableAttackCheck()
    {
        //Debug.Log("attack disabled on " + this.gameObject.name);
        StopCoroutine(OverlapCheck()); //start overlap check coroutine
        attacking = false;
        hasHitPlayer = false;
    }


    private bool hasHitPlayer = false;
    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name + ", " + col.gameObject.tag + ", attacking: " + attacking + ", hit player: " + hasHitPlayer);
        if (attacking && col.gameObject.tag == "Player" && !hasHitPlayer)
        {
            //Debug.Log("player found");
            col.gameObject.GetComponent<PlayerController>().DamagePlayer(-attackDamage);
            AM.Play("Sword_Hit" + Random.Range(1, 3));
            hasHitPlayer = true;
        }
    }
    private IEnumerator OverlapCheck()
    {
        if (attacking && !hasHitPlayer)
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
                LayerMask.GetMask("Player")
            );

            for (int hitIndex = 0; hitIndex < hitColliders.Length; hitIndex++)
            {
                //Debug.Log(hitIndex + ": " + hitColliders[hitIndex].gameObject.name);
                if (hitColliders[hitIndex].gameObject.tag == "Player")
                {
                    //Debug.Log("player found in overlap: " + hitColliders[hitIndex].gameObject.transform.parent.name);
                    hitColliders[hitIndex].gameObject.GetComponent<PlayerController>().DamagePlayer(-attackDamage);
                    AM.Play("Sword_Hit" + Random.Range(1, 3));
                    hasHitPlayer = true;
                }
            }

            StartCoroutine(OverlapCheck()); //start overlap check coroutine
        }
        else
        {
            //Debug.Log("overlap check stopped");
            yield break; //stop coroutine
        }
    }
}
