using UnityEngine;

public class EffectBlock : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = 0.1f; speedModifier = 2f; radiusModifier = 5f; cooldownModifier = .25f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        Debug.Log("Block effect applied");

        GameObject[] targets = SS.GetSpellTargets();
        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GetComponent<AbstractEnemy>().InterruptAttack();
            }
        }
    }
}
