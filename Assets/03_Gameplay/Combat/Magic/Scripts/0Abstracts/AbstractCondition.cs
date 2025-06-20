using UnityEngine;
public abstract class AbstractCondition : MonoBehaviour
{
    //THESE SHOULD ALL BE PRIVATE WITH GET/SET FUNCTIONS maybe next update
    //will be overridden by concrete effect classes
    //spell vars
    public int duration, triggerTime = 1;
    public float curDuration;

    //spell info
    public AbstractEnemy targetScript;
    public abstract void ApplyCondition();
    public void AlterConditionTime(int change) { duration += change; }
    public void EndCondition() { Destroy(this); }
}
