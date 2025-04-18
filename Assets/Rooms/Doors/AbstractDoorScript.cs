using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDoorScript : MonoBehaviour
{
    //scripts
    AbstractSceneManager ASM;
    void Start() { ASM = GameObject.FindWithTag("SceneManager").GetComponent<AbstractSceneManager>(); }


    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int newHealth) { health = newHealth; HealthCheck(); }
    public void AlterHealth(int change)
    {
        health += change;
        HealthCheck();
    }
    private void HealthCheck()
    {
        if (health <= 0)
        {
            //Debug.Log(this.gameObject.name + " has broken!");
            Destroy(this.gameObject);
        }
        else
        {
            //Debug.Log(this.gameObject.name + " HP: " + health);
        }
    }
    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~state~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private bool isBusy = false;
    [SerializeField] private bool isOpen = false;
    public bool GetIsOpen() { return isOpen; }
    [SerializeField] private bool isLocked = false;
    public bool GetIsLocked() { return isLocked; }
    [SerializeField] private Animator a;


    //interaction
    public void InteractWithDoor()
    {
        if(!isBusy && !isLocked)
        {
            isBusy = true;
            //find door direction based on name
            string edge = "";
            if (gameObject.name.Contains("North")) edge = "North";
            else if (gameObject.name.Contains("South")) edge = "South"; 
            else if (gameObject.name.Contains("East")) edge = "East";
            else if (gameObject.name.Contains("West")) edge = "West";

            int direction = 0;
            switch(edge)
            {
                case "North":
                    if (ASM.GetPlayerPosition().z > transform.position.z) direction = 1; //if player is north of door, open door clockwise
                    else direction = -1; //else if player is south of door, open door anticlockwise
                    break;
                case "South":
                    if (ASM.GetPlayerPosition().z > transform.position.z) direction = -1; //if player is south of door, open door anticlockwise
                    else direction = 1; //else if player is north of door, open door clockwise
                    break;
                case "East":
                    if (ASM.GetPlayerPosition().x > transform.position.x) direction = 1; //if player is east of door, open door anticlockwise
                    else direction = -1; //else if player is west of door, open door clockwise
                    break;
                case "West":
                    if (ASM.GetPlayerPosition().x > transform.position.x) direction = -1; //if player is west of door, open door anticlockwise
                    else direction = 1; //else if player is east of door, open door clockwise
                    break;
            }

            //if is isLocked & player has key, unlock door & play unlock sound
            //if is isLocked & player doesnt have key, play isLocked sound
            //if is not isLocked & is open, close door & play close sound
            //if is not isLocked & is not open, open door & play open sound
            /*if(isLocked /*&& playerController.CheckForItemID(keyID)/)
            {
                //isLocked & has key
                isLocked = false;
                //play(doorUnlockSound)
                //playerController.DestroyItemByID(keyID)
                UpdateDoorMaterial();
            }
            else if(isLocked /*&& !playerController.CheckForItemID(keyID)/)
            {
                //isLocked & does not have key
                //play(doorisLockedSound)
            }
            else*/ if (!isLocked)
            {
                a.SetInteger("openDirection", direction);
                if(isOpen) { CloseDoor(); }
                else if(!isOpen) { OpenDoor(); }
                ResetDoorStates(direction); //reset animation state
            }
        }
    }
    public void OpenDoor()
    {
        //Debug.Log("Opening " + gameObject.name);
        a.SetBool("isOpen", true);
        isOpen = true;
        //play(doorOpeningSound)
        UpdateDoorMaterial();
    }
    public void CloseDoor()
    {
        //Debug.Log("Closing " + gameObject.name);
        a.SetBool("isOpen", false); //run close animation
        isOpen = false; //update state
        //play(doorClosingSound) //play closing audio
        UpdateDoorMaterial(); //update visual
    }
    public void LockDoor() 
    {
        //Debug.Log("Locking " + gameObject.name);
        a.SetBool("isLocked", false);
        if (isOpen) { a.SetInteger("openDirection", 2); CloseDoor(); }
        isLocked = true; 
        UpdateDoorMaterial();
    }
    public void UnlockDoor() { isLocked = false; UpdateDoorMaterial(); }
    //~~~~~state~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~visual~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private Renderer r;
    [SerializeField] private Material open, closed, locked;

    public void UpdateDoorMaterial()
    {
        if (isLocked) { r.material = locked; }
        else if (!isLocked && isOpen) { r.material = open; }
        else if (!isLocked && !isOpen) { r.material = closed; }
    }
    //~~~~~visual~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~reset~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void ResetDoorStates(int direction)
    {
        AnimationClip[] clips = a.runtimeAnimatorController.animationClips;
        string clipName = "";
        float clipLength = 0f;

        if (!isOpen && direction == -1) { clipName = "DoorCloseAnticlockwise"; }
        else if (!isOpen && direction == 1) { clipName = "DoorCloseClockwise"; }
        else if (isOpen && direction == -1) { clipName = "DoorOpenAnticlockwise"; }
        else if (isOpen && direction == 1) { clipName = "DoorOpenClockwise"; }

        foreach (AnimationClip clip in clips) { if (clip.name == clipName) { clipLength = clip.length; } }

        Invoke("ResetAnimationBooleans", clipLength);
    }
    private void ResetAnimationBooleans()
    {
        a.SetInteger("openDirection", 0);
        isBusy = false;
    }
}
