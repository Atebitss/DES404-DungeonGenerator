using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerSmall : AbstractEnemy
{
    private void Awake()
    {
        //update basic stats
        health = 3;
        attackDamage = 1;
        type = "basicSmall";
    }
}