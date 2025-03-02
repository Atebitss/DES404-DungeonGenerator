using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AbstractEnemy
{
    private void Awake()
    {
        SM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = SM.GetAudioManager();
        PC = SM.GetPlayerController();

        health = 1;
        attackDamage = 1;
    }
    private void Start()
    {
        EWCM.SetAM(AM);
        EWCM.SetWeaponDamage(attackDamage);
    }
}