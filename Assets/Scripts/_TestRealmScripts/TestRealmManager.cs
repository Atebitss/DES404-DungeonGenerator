using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealmManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject doorPrefab;

    void Start()
    {
        Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
