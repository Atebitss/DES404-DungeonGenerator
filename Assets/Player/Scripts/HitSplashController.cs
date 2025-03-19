using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HitSplashController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private PlayerController PC;
    public void Wake(PlayerController newPC, int newText)
    {
        this.PC = newPC;
        text.text = newText.ToString();

        //get animator
        Animator a = this.gameObject.GetComponent<Animator>();

        //play animation
        a.Play("HitSplashFadeUp");

        //get animation length
        AnimationClip[] clips = a.runtimeAnimatorController.animationClips;
        float animLength = 0;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "HitSplashFadeUp")
            {
                animLength = clip.length;
            }
        }

        //destroy after animation
        Destroy(gameObject, animLength);
    }


    //looking
    [SerializeField] private float lookSensitivity = 10f; //enemy looking velocity
    public void SetLookSensitivity(int newSensitivity) { lookSensitivity = newSensitivity; }
    private void FixedUpdate()
    {
        //face towards player
        Vector3 playerLookPosition = new Vector3(PC.transform.position.x, (PC.transform.position.y - 0.5f), PC.transform.position.z);
        Quaternion targetRot = Quaternion.LookRotation(this.gameObject.transform.position - playerLookPosition);
        this.gameObject.transform.rotation = targetRot;
    }
}
