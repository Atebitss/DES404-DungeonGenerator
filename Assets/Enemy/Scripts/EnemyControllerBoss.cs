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
        SetBHDM(GetPC().gameObject.transform.parent.GetChild(1).GetComponent<BossHealthDisplayManager>());
        GetBHDM().EnableBossHealthDisplay();

        int randBossTypeID = Random.Range(0, 0);
        //Debug.Log("randBossTypeID: " + randBossTypeID);
        switch (randBossTypeID)
        {
            case 0:
                type = "bossBeserk";
                dual = true;

                health = 20;
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
        GetBHDM().DisableBossHealthDisplay();
    }
}