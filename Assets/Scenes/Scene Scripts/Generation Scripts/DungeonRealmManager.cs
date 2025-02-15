using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRealmManager : AbstractSceneManager
{
    [SerializeField] private bool devMode = false;
    [SerializeField] private GameObject amPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject doorPrefab;

    private GameObject player;
    private GameObject[] enemies;


    void Start()
    {
        AM = Instantiate(amPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<AudioManager>();

        //player = Instantiate(playerPrefab, center of entry room, Quaternion.identity);
        //player = player.transform.GetChild(0).gameObject;
    }
}
