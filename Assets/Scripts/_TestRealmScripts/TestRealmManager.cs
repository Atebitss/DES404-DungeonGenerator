using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : MonoBehaviour
{
    [SerializeField] private bool devMode = false;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject doorPrefab;

    private GameObject player;
    private PlayerController PC;

    private DbugDisplayController DDC;

    void Awake()
    {
        DDC = GameObject.Find("DbugDisplayHUD").GetComponent<DbugDisplayController>();
        if (!devMode) { DDC.SwitchVisible(); }

        player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player = player.transform.GetChild(0).gameObject;
        PC = player.GetComponent<PlayerController>();

        if (devMode) { PC.SetDDC(DDC); }
    }
}
