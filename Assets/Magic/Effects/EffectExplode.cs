using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectExplode : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS) 
    { 
        componentWeight = 4; damageModifier = 0.75f; speedModifier = 0.75f; radiusModifier = 5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Explosion effect applied");
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the gizmo
        Gizmos.DrawWireSphere(this.transform.position, SS.GetRadius()); // Draw the wire sphere}
    }
}
