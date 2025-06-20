using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerBoss : AbstractEnemy
{   private void Awake()
    {
        //update basic stats
        boss = true;
    }

    override public void UpdateBossStates() 
    {
        //Debug.Log("UpdateBossStates");

        int randBossTypeID = Random.Range(0, 0);
        //Debug.Log("randBossTypeID: " + randBossTypeID);
        switch (randBossTypeID)
        {
            case 0:
                type = "bossBeserk";
                dual = true;

                maxHealth = 50;
                attackDamage = 1;
                /*Debug.Log("id: " + randBossTypeID);
                Debug.Log("type: " + type);
                Debug.Log("dual: " + dual);
                Debug.Log("health: " + health);
                Debug.Log("attackDamage: " + attackDamage);*/
                break;
        }

        GetBHDM().EnableBossHealthDisplay();
    }

    private void OnDestroy()
    {
        if (GetPC() != null) { GetBHDM().DisableBossHealthDisplay(); }
    }
}