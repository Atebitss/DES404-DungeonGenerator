using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDoorScript : MonoBehaviour
{
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
            Debug.Log(this.gameObject.name + " has broken!");
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log(this.gameObject.name + " HP: " + health);
        }
    }
    //~~~~~health~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~state~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isLocked = false;

    public void LockDoor() { isLocked = true; UpdateDoorMaterial(); }
    public void UnlockDoor() { isLocked = false; UpdateDoorMaterial(); }
    public bool IsLocked() { return isLocked; }


        //interaction
    public void InteractWithDoor()
    {
        //if is locked & player has key, unlock door & play unlock sound
        //if is locked & player doesnt have key, play locked sound
        //if is not locked & is open, close door & play close sound
        //if is not locked & is not open, open door & play open sound
        if(isLocked /*&& playerController.CheckForItemID(keyID)*/)
        {
            //locked & has key
            isLocked = false;
            //play(doorUnlockSound)
            //playerController.DestroyItemByID(keyID)
            UpdateDoorMaterial();
        }
        else if(isLocked /*&& !playerController.CheckForItemID(keyID)*/)
        {
            //locked & does not have key
            //play(doorLockedSound)
        }
        else if (!isLocked && isOpen)
        {
            //closing door
            transform.Rotate(0, 90, 0);
            //play(doorClosingSound)
            UpdateDoorMaterial();
        }
        else if (!isLocked && !isOpen)
        {
            //opening door
            transform.Rotate(0, 90, 0);
            //play(doorClosingSound)
            UpdateDoorMaterial();
        }

    }
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
}
