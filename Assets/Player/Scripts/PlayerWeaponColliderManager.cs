using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponColliderManager : MonoBehaviour
{
    private bool attacking = false;
    private int attackDamage = 1;
    private GameObject[] colObjs = new GameObject[0];
    private AudioManager AM;


    public void SetWeaponDamage(int newDamage) { attackDamage = newDamage; }
    public void SetAM(AudioManager newAM) { AM = newAM; }


    public void EnableAttackCheck(float attackAnimDur)
    {
        //Debug.Log("attack enabled");
        attacking = true;
        Invoke("DisableAttackCheck", attackAnimDur);
    }
    private void DisableAttackCheck()
    {
        //Debug.Log("attack disabled");
        attacking = false;
    }


    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log(col.gameObject.name);
        if (attacking && col.gameObject.tag == "Enemy")
        {
            Debug.Log("enemy found");
            col.gameObject.GetComponent<AbstractEnemy>().AlterHealth(-attackDamage);
            AM.Play("Sword_Hit" + Random.Range(1, 3));
        }
    }
}
