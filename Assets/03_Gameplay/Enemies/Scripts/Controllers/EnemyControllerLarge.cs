using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerLarge : AbstractEnemy
{
    private void Awake()
    {
        //update basic stats
        maxHealth = 25;
        attackDamage = 3;
        type = "basicLarge";
    }
}