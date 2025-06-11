using UnityEngine;
public class SeperationForce : MonoBehaviour
{
    [SerializeField] private float seperationDistance = 1f; //default distance to keep from other objects
    [SerializeField] private float seperationForce = 1f; //default force to apply for separation

    public void SetSeperationDistance(float newSepDist) { seperationDistance = newSepDist; }
    public float GetSeperationDistance(){ return seperationDistance; }

    public void SetSeperationForce(float newSepForce) { seperationForce = newSepForce; }
    public float GetSeperationForce() { return seperationForce; }
}
