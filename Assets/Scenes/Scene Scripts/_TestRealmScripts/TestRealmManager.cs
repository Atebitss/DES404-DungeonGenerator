using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : AbstractSceneManager
{
    [SerializeField] private bool devMode = false;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject doorPrefab;

    private GameObject player;

    private DbugDisplayController DDC;

    void Awake()
    {
        if (GameObject.Find("DbugDisplayHUD")) { DDC = GameObject.Find("DbugDisplayHUD").GetComponent<DbugDisplayController>(); }
        if (!devMode) { DDC.SwitchVisible(); }

        player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player = player.transform.GetChild(0).gameObject;
        SetPlayerController(player.GetComponent<PlayerController>());

        if (devMode) { PC.SetDDC(DDC); }


        Instantiate(doorPrefab, new Vector3(0, 0, 5), Quaternion.identity);
    }
}
