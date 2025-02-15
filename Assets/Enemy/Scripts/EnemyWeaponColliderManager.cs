using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponColliderManager : MonoBehaviour
{
    private bool attacking = false;
    private int attackDamage = 1;
    private AudioManager AM;


    public void SetWeaponDamage(int newDamage) { attackDamage = newDamage; }
    public void SetAM(AudioManager newAM) { AM = newAM; }


    public void EnableAttackCheck(float attackAnimDur)
    {
        //Debug.Log("attack enabled on " + this.gameObject.name);
        attacking = true;
        Invoke("DisableAttackCheck", attackAnimDur);
    }
    private void DisableAttackCheck()
    {
        //Debug.Log("attack disabled on " + this.gameObject.name);
        attacking = false;
    }


    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (attacking && col.gameObject.tag == "Player")
        {
            //Debug.Log("player found");
            col.gameObject.GetComponent<PlayerController>().AlterCurrentHealthPoints(-attackDamage);
            AM.Play("Sword_Hit" + Random.Range(1, 3));
        }
    }
}
