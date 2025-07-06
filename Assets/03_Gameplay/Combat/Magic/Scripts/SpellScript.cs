using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellScript : MonoBehaviour
{
    // ===============================================================
    // EXTERNAL REFERENCES
    // ===============================================================
    private AbstractSceneManager ASM;
    public AbstractSceneManager GetASM() { return ASM; }

    private GameObject playerObject;
    private PlayerController PC;
    public PlayerController GetPlayerController() { return PC; }


    // ===============================================================
    // SPELL COMPONENT SYSTEM
    // ===============================================================
    private string effectName, elementName, shapeName = "";
    public string GetEffectName() { return effectName; }
    public string GetElementName() { return elementName; }
    public string GetShapeName() { return shapeName; }

    private string effectScriptName, elementScriptName, shapeScriptName = "";

    private AbstractEffect effectScript;
    private AbstractElement elementScript;
    private AbstractShape shapeScript;
    public AbstractEffect GetEffectScript() { return effectScript; }
    public AbstractElement GetElementScript() { return elementScript; }
    public AbstractShape GetShapeScript() { return shapeScript; }


    // ===============================================================
    // SPELL STATE & VALIDATION
    // ===============================================================
    private bool spellValid = false;
    public bool GetSpellValid() { return spellValid; }

    private bool casted = false;
    public bool GetCasted() { return casted; }

    private bool delayed = false;
    public bool GetDelayed() { return delayed; }

    private bool spellActive = false;
    public bool GetSpellActive() { return spellActive; }

    private bool spellPersist = false;
    public bool GetSpellPersist() { return spellPersist; }
    public void SetSpellPersist(bool persist) { spellPersist = persist; }


    // ===============================================================
    // SPELL PHYSICS & MOVEMENT
    // ===============================================================
    private Vector3 startPos;
    public void SetStartPos(Vector3 startPos) { this.startPos = startPos; }
    public Vector3 GetStartPos() { return startPos; }

    private Vector3 endPos;
    public void SetEndPos(Vector3 endPos) { this.endPos = endPos; }
    public Vector3 GetEndPos() { return endPos; }

    private float speed = 10f;
    public float GetSpeed() { return speed; }
    public void SetSpeed(float change) { speed = change; }
    public void AlterSpeed(float change) { speed += change; }

    private float radius = 1f;
    public float GetRadius() { return radius; }

    private float maxLookLength = 1000f;
    public float GetMaxLookLength() { return maxLookLength; }
    public void SetMaxLookLength(float newMax) { maxLookLength = newMax; }

    private float maxSpellLength = 500f;
    public float GetMaxSpellLength() { return maxSpellLength; }
    public void SetMaxSpellLength(float newMax) { maxSpellLength = newMax; }


    // ===============================================================
    // COLLISION & TARGETING SYSTEM
    // ===============================================================
    [SerializeField] private SphereCollider spellSphereCollider;
    [SerializeField] private GameObject lineMarkerPrefab;
    private GameObject[] triggerObjects;
    public GameObject[] GetTriggerObjects() { return triggerObjects; }

    private GameObject[] targets = new GameObject[0];
    public void SetSpellTargets(GameObject[] targets) { this.targets = targets; }
    public GameObject[] GetSpellTargets() { return targets; }

    private GameObject[] ignoredTargets = new GameObject[1];
    public void SetIgnoredTargets(GameObject[] targets) { ignoredTargets = targets; }
    public void SetIgnoredTarget(GameObject target) { ignoredTargets[0] = target; }
    public GameObject[] GetIgnoredTargets() { return ignoredTargets; }
    public void ResetIgnoredTargets() { ignoredTargets = new GameObject[1]; }

    private GameObject[] hitTargets = new GameObject[0];
    public GameObject[] GetHitTargets() { return hitTargets; }

    private AbstractEnemy[] targetScripts = new AbstractEnemy[0];
    public AbstractEnemy[] GetTargetScripts() { return targetScripts; }


    // ===============================================================
    // DAMAGE SYSTEM
    // ===============================================================
    private string damageType = "none";
    public void SetDamageType(string type) { damageType = type; }

    private int spellPower;
    public int GetSpellPower() { return spellPower; }
    public void SetSpellPower(int newSP) { spellPower = newSP; }

    private float damageCalc;
    private int damageDealt;
    public int GetDamage() { return damageDealt; }

    [SerializeField] private GameObject hitSplashPrefab;


    // ===============================================================
    // VISUAL & RENDERING
    // ===============================================================
    [SerializeField] private Renderer spellRenderer;
    public void SetSpellColour(Material newMaterial) { spellRenderer.material = newMaterial; }
    public Material GetSpellMaterial() { return spellRenderer.material; }


    // ===============================================================
    // TIMING & COOLDOWNS
    // ===============================================================
    private float castStartTime = 0f;
    private float timeSinceCast = 0f;


    // ===============================================================
    // COMPLEX METHODS
    // ===============================================================
    public bool CheckTargets(GameObject hitTarget)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == hitTarget)
            {
                return true;
            }
        }
        return false;
    }
    public bool CheckIgnoredTargets(GameObject hitTarget)
    {
        for (int i = 0; i < ignoredTargets.Length; i++)
        {
            if (ignoredTargets[i] == hitTarget)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateRadius()
    {
        radius = 1f;
        radius *= shapeScript.radiusModifier;
        radius *= effectScript.radiusModifier;
        radius *= elementScript.radiusModifier;
        //this.gameObject.transform.localScale = Vector3.one * radius;
        //Debug.Log("SpellScript radius updated: " + radius);
        //Debug.Log("shape rmod: " + shapeScript.radiusModifier + ", effect rmod: " + effectScript.radiusModifier + ", element rmod: " + elementScript.radiusModifier);
    }

    public float GetSpellCooldownMax()
    {
        float maxCD = 10;
        maxCD *= shapeScript.cooldownModifier;
        maxCD *= effectScript.cooldownModifier;
        maxCD *= elementScript.cooldownModifier;
        return maxCD;
    }



    // ===============================================================
    // INITIALIZATION & CONSTRUCTION
    // ===============================================================
    public SpellScript StartSpellScript(AbstractSceneManager newASM)
    {
        ASM = newASM;
        castStartTime = Time.time;
        spellPower = 3;
        casted = false;

        playerObject = ASM.GetPlayerObject();
        PC = ASM.GetPlayerController();

        spellActive = true;

        return this;
    }


    // ===============================================================
    // COMPONENT SETUP & CONFIGURATION
    // ===============================================================
    public void UpdateSpellScriptEffect(string effectName)
    {
        this.effectName = effectName;
        effectScriptName = "Effect" + effectName;
        FindEffect();

        if (effectScript != null)
        {
            effectScript.StartEffectScript(this);
        }
    }
    private void FindEffect()
    {
        if (effectName != null)
        {
            System.Type effectType = System.Type.GetType(effectScriptName);
            effectScript = this.gameObject.AddComponent(effectType) as AbstractEffect;
            //Debug.Log("effect script: " + effectScript);
        }
    }

    public void UpdateSpellScriptElement(string elementName)
    {
        this.elementName = elementName;
        elementScriptName = "Element" + elementName;
        FindElement();

        if (elementScript != null)
        {
            elementScript.ApplyElement(this);
        }
    }
    private void FindElement()
    {
        if (elementName != null)
        {
            System.Type elementType = System.Type.GetType(elementScriptName);
            elementScript = this.gameObject.AddComponent(elementType) as AbstractElement;
            //Debug.Log("element script: " + elementScript);
        }
    }

    public void UpdateSpellScriptShape(string shapeName)
    {
        this.shapeName = shapeName;
        shapeScriptName = "Shape" + shapeName;
        FindShape();

        if (shapeScript != null)
        {
            shapeScript.StartShapeScript(this);
        }
    }
    private void FindShape()
    {
        if (shapeName != null)
        {
            System.Type shapeType = System.Type.GetType(shapeScriptName);
            shapeScript = this.gameObject.AddComponent(shapeType) as AbstractShape;
            //Debug.Log("shape script: " + shapeScript);
        }
    }

    // ===============================================================
    // RUNTIME UPDATE & AIMING
    // ===============================================================
    void FixedUpdate()
    {
        if (shapeScript != null && !casted && spellActive) { shapeScript.AimSpell(); }
        if (effectScript != null && effectScript.componentWeight == 0 && !casted) { effectScript.ApplyEffect(); }
    }

    // ===============================================================
    // SPELL CASTING & ACTIVATION
    // ===============================================================
    public void CastSpell()
    {
        if (shapeScript.castable && !casted)
        {
            Debug.Log("SpellScript cast spell, castable & not casted");
            if (shapeName.Contains("Ball")) 
            {
                this.transform.parent.transform.SetParent(null);
                shapeScript.EndAim();
            }
            else if (shapeName.Contains("Beam")) { /*keep parent*/ }

            elementScript.SetupCondition();

            if (effectScript.componentWeight == 1) { effectScript.ApplyEffect(); }

            UpdateRadius();
            casted = true;

            shapeScript.ApplyShape();
        }
        else if (casted)
        {
            Debug.Log("SpellScript cast spell, casted");
            shapeScript.ApplyShape();
        }
    }

    // ===============================================================
    // SPELL IMPACT & RESOLUTION
    // ===============================================================
    public void EndSpell()
    {
        Debug.Log("SpellScript end spell");

        targets = shapeScript.FindShapeTargets();
        targetScripts = new AbstractEnemy[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                Debug.Log("SpellScript spell target " + i + ": " + targets[i]);
                targetScripts[i] = targets[i].GetComponent<AbstractEnemy>();
            }
        }

        if (effectScript.componentWeight == 3) { effectScript.ApplyEffect(); }
        DealDamage();

        if (spellPersist && shapeName.Contains("Ball")) { CastSpell(); }
        else if (!spellPersist) { DestroySpell(); }
    }

    private void DealDamage()
    {
        Debug.Log("SpellScript deal dmg");
        int[] damagesDealt = new int[targets.Length];

        damageCalc = spellPower;
        damageCalc *= shapeScript.damageModifier;
        damageCalc *= effectScript.damageModifier;
        damageCalc *= elementScript.damageModifier;

        Debug.Log("targets.Length: " + targets.Length);
        if (targets != null)
        {
            for (int targetNum = 0; targetNum < targets.Length; targetNum++)
            {
                Debug.Log("target " + targetNum + ": " + targets[targetNum]);
                if (targets[targetNum] != null)
                {
                    Debug.Log("SpellScript dealing damage to: " + targets[targetNum].name);
                    int randDamage = Random.Range(0, 2);

                    damageDealt = Mathf.RoundToInt(damageCalc);
                    damageDealt += randDamage;

                    damagesDealt[targetNum] = damageDealt;

                    targetScripts[targetNum].DamageTarget(damageDealt, damageType);
                    ASM.GetADM().SpellSuccess();
                    ASM.GetADM().AddSpellDamageDealt(damageDealt);
                    Debug.Log("dmg calc on " + targets[targetNum].name + ": " + damageDealt);
                }
            }
        }

        ApplyConditions();
    }

    private void ApplyConditions()
    {
        elementScript.ApplyCondition();
    }

    // ===============================================================
    // CLEANUP & DESTRUCTION
    // ===============================================================
    public void SpellDestroy() { DestroySpell(); }

    private void DestroySpell()
    {
        Debug.Log("Spell Script destroy spell");

        if (triggerObjects != null)
        {
            for (int point = 0; point < triggerObjects.Length; point++)
            {
                Destroy(triggerObjects[point]);
            }
        }

        Destroy(this.transform.parent.gameObject);
    }

    // ===============================================================
    // DEBUG & UTILITY
    // ===============================================================
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, radius);
    }
}