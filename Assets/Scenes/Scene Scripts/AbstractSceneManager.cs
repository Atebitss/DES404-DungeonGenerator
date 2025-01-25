using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AbstractSceneManager : MonoBehaviour
{
    public PlayerController PC;
    public void SetPlayerController (PlayerController newPC) {  PC = newPC; }
    public PlayerController GetPlayerController () { return PC; }
}
