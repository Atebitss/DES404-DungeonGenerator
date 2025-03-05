using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRealmManager : AbstractSceneManager
{
    override public void RestartScene()
    {
        GameObject SM = this.gameObject;
        DestroyEnemyObjects();
        DestroyPlayer();
        SM.GetComponent<MapGeneration>().ResetMap();
        SM.GetComponent<DungeonGeneration>().ResetDungeon();
        SM.GetComponent<PathGeneration>().ResetHallways();
        SM.GetComponent<MapGeneration>().RegenerateDungeon();
    }
}