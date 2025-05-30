using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HitSplashController : MonoBehaviour
{
    [SerializeField] private Material m_default, m_fire, m_water, m_electric, m_force;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image bg;
    private PlayerController PC;
    public void Wake(PlayerController newPC, int newText, string dmgType)
    {
        this.PC = newPC;
        text.text = newText.ToString();

        //set color
        switch(dmgType)
        {
            case "fire":
                bg.material = m_fire;
                break;
            case "water":
                bg.material = m_water;
                break;
            case "electric":
                bg.material = m_electric;
                break;
            case "force":
                bg.material = m_force;
                break;
            default:
                bg.material = m_default;
                break;
        }

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
        if (PC != null)
        {
            //face towards player
            Vector3 playerLookPosition = new Vector3(PC.transform.position.x, (PC.transform.position.y - 0.5f), PC.transform.position.z);
            Quaternion targetRot = Quaternion.LookRotation(this.gameObject.transform.position - playerLookPosition);
            this.gameObject.transform.rotation = targetRot;
        }
    }
}
