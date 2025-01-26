using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightAttackColliderManager : MonoBehaviour
{
    private GameObject[] colObjs = new GameObject[0];
    public GameObject[] GetAttackColliderObjects() {  return colObjs; }
    private void OnTriggerEnter(Collider col)
    {
        GameObject[] tempColObjs = new GameObject[colObjs.Length + 1]; //increase tracker array by 1 for new object
        for(int i = 0; i < colObjs.Length; i++){ tempColObjs[i] = colObjs[i]; } //fill temp array with old array
        tempColObjs[colObjs.Length] = col.gameObject; //update new position with collided object
        colObjs = tempColObjs; //update main array with new array
        Debug.Log("Collided with " + col.gameObject.name);
    }

    private void OnTriggerExit(Collider col)
    {
        RemoveObjectFromArray(col.gameObject);
    }


    public void RemoveObjectFromArray(GameObject obj)
    {
        int objIndex = -1;
        for (int i = 0; i < colObjs.Length; i++) { if (colObjs[i] == obj.gameObject) { objIndex = i; break; } } //find index of exiting object
        if (objIndex != -1)
        {
            GameObject[] tempColObjs = new GameObject[colObjs.Length - 1]; //decrease tracker array by 1 for exiting object

            int newIndex = 0; //track new array size
            for (int i = 0; i < colObjs.Length; i++) //for each object
            {
                if (i != objIndex) //if the current object is not the exiting object
                {
                    tempColObjs[newIndex] = colObjs[i]; //add object to new array
                    newIndex++; //increase size of new array
                }
            }

            colObjs = tempColObjs; //update main array with new array
            //Debug.Log("Exited " + col.gameObject.name);
        }
    }
}
