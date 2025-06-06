using UnityEngine;
public abstract class AbstractEffect : MonoBehaviour
{
    //THESE SHOULD ALL BE PRIVATE WITH GET/SET FUNCTIONS maybe next update
    //will be overridden by concrete effect classes
    //spell vars
    public int callCounter = 0; //to ensure ApplyEffect is run when appropriate
    public int componentWeight; //-1 = no effect, 0 = aiming, 1 = on cast, 2 = flight, 3 = impact
    public float damageModifier, speedModifier, radiusModifier, cooldownModifier;
    public float damageIncrement, radiusIncrement;

    //spell info
    public SpellScript SS;
    public Vector3[] pathPoints = new Vector3[2];
    public GameObject[] targets;
    public abstract void StartEffectScript(SpellScript SS);
    public abstract void ApplyEffect();
}
