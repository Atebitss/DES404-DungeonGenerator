using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRealmManager : AbstractSceneManager
{
    override public void RestartScene()
    {
        MapGeneration MG = this.gameObject.GetComponent<MapGeneration>();
        if (MG.IsDbugEnabled()) { MG.UpdateHUDDbugText("Dungeon Scene Manager: Restarting Scene"); }
        GameObject SM = this.gameObject;
        DestroyEnemyObjects();
        DestroyPlayer();
        MG.ResetMap();
        SM.GetComponent<DungeonGeneration>().ResetDungeon();
        SM.GetComponent<PathGeneration>().ResetHallways();
        MG.RegenerateDungeon();
    }
}