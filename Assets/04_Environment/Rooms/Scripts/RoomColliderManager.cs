using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomColliderManager : MonoBehaviour
{
    private RoomGeneration RG;

    public void SetColliderVariables(RoomGeneration RG)
    {
        this.RG = RG;
    }
    private void OnTriggerEnter(Collider col)
    {
        //Debug.Log("room trigger enter: " + col.gameObject.tag);
        if(col.gameObject.tag == "Player" && !RG.GetRoomEntered()) //if player enters room and room is not entered
        {
            RG.SetPlayerInRoom(true); //set player in room
            StartCoroutine(RG.RoomEntered()); //start combat
        }
    }
    private void OnTriggerExit(Collider col)
    {
        //Debug.Log("room trigger exit: " + col.gameObject.tag);
        if(col.gameObject.tag == "Player") //if player exits room
        {
            RG.SetPlayerInRoom(false); //set player not in room
            //will block combat from starting
        }
    }
}