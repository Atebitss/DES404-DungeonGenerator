using UnityEngine;
public abstract class AbstractEffect : MonoBehaviour
{
    //THESE SHOULD ALL BE PRIVATE WITH GET/SET FUNCTIONS maybe next update
    //will be overridden by concrete effect classes
    //spell vars
    public int componentWeight;
    public float damageModifier, speedModifier, radiusModifier;

    //spell info
    public SpellScript SS;
    public Vector3[] pathPoints = new Vector3[2];
    public GameObject[] targets;
    public abstract void StartEffectScript(SpellScript SS);
    public abstract void ApplyEffect();
}
