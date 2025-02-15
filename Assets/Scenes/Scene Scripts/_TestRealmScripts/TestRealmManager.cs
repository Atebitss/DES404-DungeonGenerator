using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    [SerializeField] private bool devMode = false;
    [SerializeField] private GameObject amPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject doorPrefab;

    private GameObject player;

    private DbugDisplayController DDC;

    void Awake()
    {
        if (GameObject.Find("DbugDisplayHUD")) { DDC = GameObject.Find("DbugDisplayHUD").GetComponent<DbugDisplayController>(); }
        if (!devMode) { DDC.SwitchVisible(); }

        AM = Instantiate(amPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<AudioManager>();

        player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player = player.transform.GetChild(0).gameObject;
        SetPlayerController(player.GetComponent<PlayerController>());

        if (devMode) { PC.SetDDC(DDC); }


        Instantiate(enemyPrefab, new Vector3(0, 0, 5f), Quaternion.identity);


        int maxEnemies = 0;// = Random.Range(10, 10);
        for (int i = 0; i < maxEnemies; i++)
        {
            Instantiate(enemyPrefab, new Vector3(Random.Range(-10, 10), 0, Random.Range(5, 15)), Quaternion.identity);
        }
    }
}
