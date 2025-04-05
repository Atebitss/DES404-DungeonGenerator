using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerLarge : AbstractEnemy
{
    private void Awake()
    {
        //update basic stats
        health = 5;
        attackDamage = 3;
        type = "basicLarge";
    }
}