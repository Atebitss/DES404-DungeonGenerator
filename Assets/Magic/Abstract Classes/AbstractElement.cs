using UnityEngine;
public abstract class AbstractElement : MonoBehaviour
{
    //will be overridden by concrete element classes
    //spell vars
    public float damageModifier, speedModifier, radiusModifier, cooldownModifier;

    //spell info
    public SpellScript SS;
    public abstract void ApplyElement(SpellScript SS);
    public abstract void SetupCondition();
    public abstract void ApplyCondition();
}