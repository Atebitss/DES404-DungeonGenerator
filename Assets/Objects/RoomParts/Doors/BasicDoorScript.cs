using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDoorScript : MonoBehaviour
{
    [SerializeField] private Renderer r;
    [SerializeField] private Material open, closed, locked;
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isLocked = false;

    private void Start()
    {
        if (isLocked) { r.material = locked; }
    }


    public void LockDoor() { isLocked = true; r.material = locked; }
    public void UnlockDoor() { isLocked = false; r.material = closed; }
    public bool IsLocked() { return isLocked; }



    public void InteractWithDoor()
    {
        //if is locked & player has key, unlock door & play unlock sound
        //if is locked & player doesnt have key, play locked sound
        //if is not locked & is open, close door & play close sound
        //if is not locked & is not open, open door & play open sound
    }
}