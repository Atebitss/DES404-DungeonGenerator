using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    private AbstractSceneManager ASM;
    public void SetASM(AbstractSceneManager newASM) {  ASM = newASM; }
    private DungeonRealmManager DRM;
    public void SetDRM(DungeonRealmManager newDRM) { DRM = newDRM; }



    public void InteractWithPortal()
    {
        DRM.StopFloorCounter();
        ASM.EndFloor();
    }
}
