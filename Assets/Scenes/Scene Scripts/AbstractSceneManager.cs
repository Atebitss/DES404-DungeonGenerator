using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AbstractSceneManager : MonoBehaviour
{
    //debug info
    [SerializeField] private bool devMode = false;
    public bool isDevMode() { return devMode; }


    //prefabs
    [SerializeField] public GameObject amPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public GameObject doorPrefab;


    //audio manager
    public AudioManager AM;
    public void SetAudioManager(AudioManager newAM) { AM = newAM; }
    public AudioManager GetAudioManager() { return AM; }


    //player controller
    public GameObject player;
    public void SetPlayerObject(GameObject newPlayer) { player = newPlayer; }
    public GameObject GetPlayerObject() { return player; }

    public PlayerController PC;
    public void SetPlayerController (PlayerController newPC) {  PC = newPC; SetPlayerObject(newPC.gameObject); }
    public PlayerController GetPlayerController () { return PC; }

    public void SpawnPlayer(Vector3 pos)
    {
        player = Instantiate(playerPrefab, pos, Quaternion.identity);
        player = player.transform.GetChild(0).gameObject;
        PC = player.GetComponent<PlayerController>();
    }
    
    public Vector3 GetPlayerPosition() { if(PC != null) { return PC.transform.position; } else { return Vector3.zero; } }


    //combat manager
    /*public CombatManager CM;
    public void SetCombatManager(CombatManager newCM) { CM = newCM; }
    public CombatManager GetCombatManager() { return CM; }*/


    
    //when scene starts
    void Start()
    {
        AM = Instantiate(amPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<AudioManager>();
    }
}
