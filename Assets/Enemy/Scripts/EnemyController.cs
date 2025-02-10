using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : AbstractEnemy
{
    private void Start()
    {
        SM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>();
        AM = SM.GetAudioManager();
        PC = SM.GetPlayerController();
    }
}