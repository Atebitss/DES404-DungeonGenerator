using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractConsumable : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public AbstractSceneManager ASM;
    public string consumableType = ""; //type of consumable
    public float consumableTime = -1f; //length of consumable
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~start~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private float destroyTime = 100f; //time before consumable is destroyed
    private void Start()
    {
        StartCoroutine(StartConsumable());
    }
    private IEnumerator StartConsumable()
    {
        //start consumable
        //Debug.Log("AbstractConsumable, starting consumable: " + this.gameObject.name);
        yield return new WaitForSeconds(destroyTime);
        StartCoroutine("DestroyConsumable");
    }
    //~~~~~~start~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private bool isUsed = false;
    public void Interact()
    {
        //apply consumable effect to player
        if (!isUsed)
        {
            //Debug.Log("AbstractConsumable, using consumable: " + this.gameObject.name);
            PlayerController PC = ASM.GetPlayerController();
            if (PC != null)
            {
                PC.GetCVM().ApplyHUDVisual(consumableType, consumableTime); //update HUD visual
                ApplyEffect(PC); //apply effect to player
                isUsed = true;
                StartCoroutine(DestroyConsumable());
            }
        }
    }
    virtual public void ApplyEffect(PlayerController PC)
    {
        //apply effect to player
    }
    //~~~~~~interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~end~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private IEnumerator DestroyConsumable()
    {
        //Debug.Log("AbstractConsumable, destroying consumable: " + this.gameObject.name);
        if (!isUsed)
        {
            //run flicker animation
            //animation.play("flicker");

            //wait for animation to finish
            yield return new WaitForSeconds(1f);

            //destroy consumable object
            Destroy(this.gameObject);
        }
        else
        {
            //destroy consumable object
            yield return null;
            Destroy(this.gameObject);
        }
    }
    //~~~~~~end~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
