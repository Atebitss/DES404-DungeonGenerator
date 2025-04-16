using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerBoss : AbstractEnemy
{   private void Awake()
    {
        //update basic stats
        boss = true;
        SetBHDM(GetASM().GetPlayerObject().transform.GetChild(1).GetComponent<BossHealthDisplayManager>());
    }

    override public void UpdateBossStates() 
    {
        //Debug.Log("UpdateBossStates");
        GetBHDM().EnableBossHealthDisplay();

        int randBossTypeID = Random.Range(0, 0);
        //Debug.Log("randBossTypeID: " + randBossTypeID);
        switch (randBossTypeID)
        {
            case 0:
                type = "bossBeserk";
                dual = true;

                maxHealth = 20;
                attackDamage = 1;
                /*Debug.Log("id: " + randBossTypeID);
                Debug.Log("type: " + type);
                Debug.Log("dual: " + dual);
                Debug.Log("health: " + health);
                Debug.Log("attackDamage: " + attackDamage);*/
                break;
        }
    }

    private void OnDestroy()
    {
        if (GetPC() != null) { GetBHDM().DisableBossHealthDisplay(); }
    }
}