using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    private AbstractSceneManager ASM;
    public void SetASM(AbstractSceneManager newASM) {  ASM = newASM; }



    public void InteractWithPortal()
    {
        ASM.EndFloor();
    }
}
