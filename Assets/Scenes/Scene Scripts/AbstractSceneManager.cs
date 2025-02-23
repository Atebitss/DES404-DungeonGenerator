using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AbstractSceneManager : MonoBehaviour
{
    //audio manager
    public AudioManager AM;
    public void SetAudioManager(AudioManager newAM) { AM = newAM; }
    public AudioManager GetAudioManager() { return AM; }


    //player controller
    public PlayerController PC;
    public void SetPlayerController (PlayerController newPC) {  PC = newPC; }
    public PlayerController GetPlayerController () { return PC; }
    public Vector3 GetPlayerPosition() { if(PC != null) { return PC.transform.position; } else { return Vector3.zero; } }


    //combat manager
    /*public CombatManager CM;
    public void SetCombatManager(CombatManager newCM) { CM = newCM; }
    public CombatManager GetCombatManager() { return CM; }*/
}
