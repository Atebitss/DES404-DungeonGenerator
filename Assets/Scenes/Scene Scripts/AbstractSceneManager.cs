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
}
