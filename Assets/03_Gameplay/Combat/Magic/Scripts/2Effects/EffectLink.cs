using UnityEngine;

public class EffectLink : AbstractEffect
{
    public override void StartEffectScript(SpellScript SS)
    {
        componentWeight = 3; damageModifier = .1f; speedModifier = 1f; radiusModifier = 1f; cooldownModifier = .25f;
        this.SS = SS;
    }
    public override void ApplyEffect()
    {
        //add enemy to players linked enemy array
        Debug.Log("Link effect applied");

        PlayerController PC = SS.GetPlayerController();
        GameObject[] targets = shapeScript.targets;
        for(int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<StatusVisualManager>().ApplyVisual("Linked", 30f); //apply linked status visual
            PC.AddLinkedEnemy(targets[i]);
        }
    }
}
