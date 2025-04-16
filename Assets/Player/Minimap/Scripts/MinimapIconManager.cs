using UnityEngine;

public class MinimapIconManager : MonoBehaviour
{
    [SerializeField] private Material entryMat, bossMat, treasureMat, specialMat;
    public void Wake(string iconType)
    {
        switch(iconType)
        {
            case "Entry":
                GetComponent<Renderer>().material = entryMat;
                break;
            case "Boss":
                GetComponent<Renderer>().material = bossMat;
                break;
            case "Treasure":
                GetComponent<Renderer>().material = treasureMat;
                break;
            case "Special":
                GetComponent<Renderer>().material = specialMat;
                break;
        }
    }
}
