using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRealmManager : AbstractSceneManager
{
    override public void RestartScene()
    {
        MapGeneration MG = this.gameObject.GetComponent<MapGeneration>();
        GameObject SM = this.gameObject;
        if (GetDbugMode()) { MG.UpdateHUDDbugText("Dungeon Scene Manager: Restarting Scene"); }

        if (GetEnemyObjects().Length > 0) { DestroyEnemyObjects(); }
        MG.ResetMap();
        SM.GetComponent<DungeonGeneration>().ResetDungeon();
        SM.GetComponent<PathGeneration>().ResetHallways();
        MG.RegenerateDungeon();
    }


    private float startTime = 0f, endTime = 0f, floorClearTime = 0f;
    public float GetFloorClearTime() { return floorClearTime; }
    public void StartFloorCounter() { startTime = Time.time; }
    public void StopFloorCounter()
    {
        endTime = Time.time;
        floorClearTime = endTime - startTime;

        GetADM().FloorCleared(floorClearTime);
    }
}