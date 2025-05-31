using UnityEngine;

public class EffectTeleport : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 0f; speedModifier = 10f; radiusModifier = 1f; cooldownModifier = 5f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Teleport effect applied");

        //move hit targets to random position
        //or if shape self, move player to looked at position


    }
}
