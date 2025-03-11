using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContollerBoss : AbstractEnemy
{    private void Awake()
    {
        //update basic stats
        health = 20;
        attackDamage = 1;
        type = "bossBeserk";
        boss = true;
        dual = true;
        Debug.Log("ECB, dual: " + dual);
    }

    override public void UpdateBossStates() 
    {
        switch (type)
        {
            case "bossBeserk":

                break;
        }
    }
}
