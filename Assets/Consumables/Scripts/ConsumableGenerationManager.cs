using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsumableGenerationManager : MonoBehaviour
{
    //~~~~~~parent~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private GameObject parentObject; //parent object for consumables
    private void Awake() 
    {
        //generate parent object on awake
        parentObject = new GameObject();
        parentObject.name = "ConsumableParent";
    }
    //~~~~~~parent~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~chance~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private int spawnChance = 50;
    public void OnRoomEntered(int roomDifficulty)
    {
        Debug.Log("Consumable Generation, OnRoomEntered");
        switch (roomDifficulty)
        {
            case -1:
                spawnChance = 50;
                break;
            case 0:
                spawnChance = 50;
                break;
            case 1:
                spawnChance = 50;
                break;
            case 2:
                spawnChance = 50;
                break;
            case 3:
                spawnChance = 50;
                break;
            default:
                spawnChance = 50;
                break;
        }
    }
    //~~~~~~chance~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private GameObject[] consumableTempPrefabs;
    [SerializeField] private GameObject[] consumableBoostPrefabs;
    private GameObject[] curConsumables = new GameObject[0];
    public void OnEnemyDeath(Vector3 pos)
    {
        //Debug.Log("Consumable Generation, OnEnemyDeath");
        int randomChance = Random.Range(0, 100); //roll generation chance
        if (randomChance < spawnChance) //if consumable should spawn
        {
            //determine which consumable to spawn
            int randomIndex = Random.Range(0, consumableTempPrefabs.Length);
            Vector3 spawnPos = new Vector3(pos.x, 0.5f, pos.z); //spawn at enemy position
            GameObject consumable = Instantiate(consumableTempPrefabs[randomIndex], pos, Quaternion.identity);
            consumable.transform.SetParent(parentObject.transform);
            consumable.name = consumable.name.Replace("Prefab", "");
            //Debug.Log("spawning consumable: " + consumable.name);

            //increase curConsumables array size
            GameObject[] tempConsumables = new GameObject[curConsumables.Length + 1];
            for (int i = 0; i < curConsumables.Length; i++) { tempConsumables[i] = curConsumables[i]; }
            tempConsumables[tempConsumables.Length - 1] = consumable; //add new consumable to the end of the array
            curConsumables = tempConsumables;
        }
    }


    public void OnRoomClear(Vector3 pos)
    {
        Debug.Log("Consumable Generation, OnRoomClear");
        int randomChance = Random.Range(0, 100); //roll generation chance
        if (randomChance < spawnChance) //if consumable should spawn
        {
            //determine which boost consumable to spawn
            int randomIndex = Random.Range(0, consumableBoostPrefabs.Length);
            GameObject consumable = Instantiate(consumableBoostPrefabs[randomIndex], pos, Quaternion.identity);
            consumable.transform.SetParent(parentObject.transform);

            //increase curConsumables array size
            GameObject[] tempConsumables = new GameObject[curConsumables.Length + 1];
            for (int i = 0; i < curConsumables.Length; i++) { tempConsumables[i] = curConsumables[i]; }
            tempConsumables[tempConsumables.Length - 1] = consumable; //add new consumable to the end of the array
            curConsumables = tempConsumables;
        }
    }
    //~~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
