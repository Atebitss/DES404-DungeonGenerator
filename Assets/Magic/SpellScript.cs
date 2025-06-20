using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellScript : MonoBehaviour
{
    //scene controller
    private AbstractSceneManager ASM;
    public AbstractSceneManager GetASM() { return ASM; }

    //player refs
    private GameObject playerObject;
    private PlayerController PC;
    public PlayerController GetPlayerController() { return PC; }

    //component refs
    private string effectName, elementName, shapeName = "";
    public string GetEffectName() { return effectName; }
    public string GetElementName() { return elementName; }
    public string GetShapeName() { return shapeName; }
    private string effectScriptName, elementScriptName, shapeScriptName = "";
    private AbstractEffect effectScript;
    public AbstractEffect GetEffectScript() { return effectScript; }
    private AbstractElement elementScript;
    public AbstractElement GetElementScript() { return elementScript; }
    private AbstractShape shapeScript;
    public AbstractShape GetShapeScript() { return shapeScript; }

    //target info
    private Vector3[] targetPoints = new Vector3[0];
    public Vector3[] GetTargetPoints() { return targetPoints; }
    public void SetTargetPoint(int point, Vector3 pos) { targetPoints[point] = pos; } //set a specific target point
    private Vector3[] triggerPoints = new Vector3[0];
    public Vector3[] GetTriggerPoints() { return triggerPoints; }
    public void SetTriggerPoints(Vector3[] triggerPoints) { this.triggerPoints = triggerPoints; }
    private GameObject[] targets = new GameObject[0];
    public void SetSpellTargets(GameObject[] targets) { this.targets = targets; }
    public GameObject[] GetSpellTargets() { return targets; }
    private GameObject[] ignoredTargets = new GameObject[1];
    public void SetIgnoredTargets(GameObject[] targets) { ignoredTargets = targets; }
    public void SetIgnoredTarget(GameObject target) { ignoredTargets[0] = target; }
    public GameObject[] GetIgnoredTargets() { return ignoredTargets; }
    public bool CheckIgnoredTargets(GameObject hitTarget)
    {
        //Debug.Log("hitTarget: " + hitTarget);
        for (int i = 0; i < ignoredTargets.Length; i++)
        {
            //Debug.Log("ignoredTargets" + i + ": " + ignoredTargets[i]);
            if (ignoredTargets[i] == hitTarget)
            {
                //Debug.Log("ignored target found: " + ignoredTargets[i]);
                return true;
            }
        }

        return false;
    }
    public void ResetIgnoredTargets() { ignoredTargets = new GameObject[1]; }
    private AbstractEnemy[] targetScripts = new AbstractEnemy[0];
    public AbstractEnemy[] GetTargetScripts() { return targetScripts; }
    private GameObject[] hitTargets = new GameObject[0]; //targets hit by the spell
    public GameObject[] GetHitTargets() { return hitTargets; }


    //spell renderer
    [SerializeField] private Renderer spellRenderer;
    public void SetSpellColour(Material newMaterial) { spellRenderer.material = newMaterial; }

    //spell colliders
    [SerializeField] private SphereCollider spellSphereCollider;
    [SerializeField] private GameObject lineMarkerPrefab;
    private GameObject[] triggerObjects;
    public GameObject[] GetTriggerObjects() { return triggerObjects; }

    //spell info
    private bool spellValid = false;
    public bool GetSpellValid() { return spellValid; }
    private bool casted = false;
    public bool GetCasted() { return casted; }
    private bool delayed = false;
    public bool GetDelayed() { return delayed; }
    private bool spellActive = false;
    public bool GetSpellActive() { return spellActive; }
    private float castStartTime = 0f;
    private float timeSinceCast = 0f;
    private float maxLookLength = 500f;
    public float GetMaxLookLength() { return maxLookLength; }
    public void SetMaxLookLength(float newMax) { maxLookLength = newMax; }
    private float maxSpellLength = 500f;
    public float GetMaxSpellLength() { return maxSpellLength; }
    public void SetMaxSpellLength(float newMax) { maxSpellLength = newMax; }
    private Vector3 startPos;
    public void SetStartPos(Vector3 startPos) { /*Debug.Log("set start pos: " + startPos);*/ this.startPos = startPos; }
    public Vector3 GetStartPos() { /*Debug.Log("get start pos: " + startPos);*/ return startPos; }
    private Vector3 endPos;
    public void SetEndPos(Vector3 endPos) { /*Debug.Log("set end pos: " + endPos);*/ this.endPos = endPos; }
    public Vector3 GetEndPos() { /*Debug.Log("get end pos: " + endPos);*/ return endPos; }
    private bool spellPersist = false;
    public bool GetSpellPersist() { /*Debug.Log("GetSpellPersist");*/ return spellPersist; }
    public void SetSpellPersist(bool persist) { /*Debug.Log("SetSpellPersist: " + persist);*/ spellPersist = persist; }

    //damage type
    private string damageType = "none";
    public void SetDamageType(string type) { damageType = type; }

    
    //radius of the spell object   ADD UPDATE PHYSICAL SIZE
    private float radius = 1f;
    public float GetRadius() { return radius; }

    //speed
    private float speed = 10f;
    public float GetSpeed() { return speed; }
    public void SetSpeed(float change) { speed = change; }
    public void AlterSpeed(float change) { speed += change; }

    //damage
    private float damageCalc;
    private int damageDealt;
    public int GetDamage() { return damageDealt; }
    private int spellPower;
    public int GetSpellPower() { return spellPower; }
    public void SetSpellPower(int newSP) { spellPower = newSP; }
    [SerializeField] private GameObject hitSplashPrefab;



    //essentially a constructor
    public SpellScript StartSpellScript(AbstractSceneManager newASM)
    {
        //Debug.Log("SpellScript start");
        ASM = newASM;        
        castStartTime = Time.time;
        spellPower = 3;
        casted = false;

        //Debug.Log("ASM: " + ASM);
        playerObject = ASM.GetPlayerObject();
        PC = ASM.GetPlayerController();
        //Debug.Log("ASM.GetPlayerController(): " + ASM.GetPlayerController());
        //Debug.Log("PC: " + PC);

        spellActive = true;

        //return reference to this script
        return this;
    }


    void FixedUpdate()
    {
        //update this pos to aim pos
        if (shapeScript != null && !casted && spellActive) { shapeScript.AimSpell(); }
        if (effectScript != null && effectScript.componentWeight == 0 && !casted) { effectScript.ApplyEffect(); } //if effect weight is 0, assumes the effect impacts aiming
    }



    //find appropriate concrete classes
    //effect
    public void UpdateSpellScriptEffect(string effectName) 
    {
        this.effectName = effectName;
        effectScriptName = "Effect" + effectName; 
        FindEffect();

        //if script not null
        if (effectScript != null)
        {
            //inform spell data, effect weight and modifiers 
            effectScript.StartEffectScript(this);
        }
    }
    private void FindEffect()
    {
        //Debug.Log("SpellScript find effect");

        //if effect name not null
        if (effectName != null)
        {
            //get system type (script) by the name of effect name
            //then add relevant script
            System.Type effectType = System.Type.GetType(effectScriptName);
            effectScript = this.gameObject.AddComponent(effectType) as AbstractEffect;

            //Debug.Log(effectScript.GetType().ToString() + " found");
        }
    }


    //element
    public void UpdateSpellScriptElement(string elementName) 
    {
        this.elementName = elementName;
        elementScriptName = "Element" + elementName; 
        FindElement();

        //if script not null
        if (elementScript != null)
        {
            //apply elemental damage and visual
            elementScript.ApplyElement(this);
        }
    }
    private void FindElement()
    {
        //Debug.Log("SpellScript find element");

        //if element name not null
        if (elementName != null)
        {
            //get system type (script) by the name of element name
            //then add relevant script
            System.Type elementType = System.Type.GetType(elementScriptName);
            elementScript = this.gameObject.AddComponent(elementType) as AbstractElement;
            
            //Debug.Log(elementScript.GetType().ToString() + " found");
        }
    }


    //shape
    public void UpdateSpellScriptShape(string shapeName) 
    {
        this.shapeName = shapeName;
        shapeScriptName = "Shape" + shapeName; 
        FindShape();

        //if script not null
        if (shapeScript != null)
        {
            //inform modifiers, aiming and visual
            shapeScript.StartShapeScript(this);
        }
    }
    private void FindShape()
    {
        //Debug.Log("SpellScript find shape");

        //if shape name not null
        if (shapeName != null)
        {
            //get system type (script) by the name of shape name
            //then add relevant script
            System.Type shapeType = System.Type.GetType(shapeScriptName);
            shapeScript = this.gameObject.AddComponent(shapeType) as AbstractShape;

            //Debug.Log(shapeScript.GetType().ToString() + " found");
        }
    }



    //operation center
    public void CastSpell()
    {
        if (shapeScript.castable && !casted)
        {
            Debug.Log("SpellScript cast spell, castable & not casted");
            this.transform.parent.transform.SetParent(null);

            targetPoints = shapeScript.pathPoints;
            shapeScript.EndAim();
            elementScript.SetupCondition();

            if (effectScript.componentWeight == 1) { effectScript.ApplyEffect(); }

            UpdateRadius();
            casted = true;

            shapeScript.ApplyShape();
        }
        else if(effectName.Contains("Chain") && casted)
        {
            Debug.Log("SpellScript cast spell, chain & casted");
            shapeScript.ApplyShape();
        }
    }


    public void UpdateRadius()
    {
        //Debug.Log(radius + "*(" + effectScript.radiusModifier + "+" + elementScript.radiusModifier + "+" + shapeScript.radiusModifier + ")");
        radius = 1f;
        radius *= shapeScript.radiusModifier; //apply shape radius modifier
        radius *= effectScript.radiusModifier; //apply effect radius modifier
        radius *= elementScript.radiusModifier; //apply element radius modifier

        this.gameObject.transform.localScale = Vector3.one * radius;

        //if (shapeScript.GetTriggerPoints().Length > 0 && spellPersist || radius == 0) { radius = 1; }
        //Debug.Log(radius);
    }


    public void EndSpell()
    {
        //assuming the spell has no trigger points,
        //apply effect then find targets, deal damage, and destroy self
        Debug.Log("SpellScript end spell");

        targets = shapeScript.FindShapeTargets(); //sets targets to those found within shape area
        targetScripts = new AbstractEnemy[(targets.Length - 1)]; //reset target scripts array
        for (int i = 0; i < (targets.Length - 1); i++) 
        {
            Debug.Log("SpellScript spell target " + i + ": " + targets[i]);
            targetScripts[i] = targets[i].GetComponent<AbstractEnemy>(); //get the enemy scripts for each target
        }

        if (effectScript.componentWeight == 3) { effectScript.ApplyEffect(); } //if effect weight is 3, apply effect upon impact
        DealDamage(); //deals damage to any targets found within said radius

        if (spellPersist && shapeScript.GetTriggerPoints().Length == 0) { CastSpell(); } //if spell continues after impact and has 0 triggers
        else if (!spellPersist) { DestroySpell(); } //destroys the spell if permited
    }


    private void DealDamage()
    {
        Debug.Log("SpellScript deal dmg");
        int[] damagesDealt = new int[targets.Length];

        //calculation of damage, rounded <0.5>
        damageCalc = spellPower;
        damageCalc *= shapeScript.damageModifier;
        damageCalc *= effectScript.damageModifier;
        damageCalc *= elementScript.damageModifier;

        if (targets != null)
        {
            for (int targetNum = 0; targetNum < (targets.Length - 1); targetNum++)
            {
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

    public void SpellDestroy() { /*Debug.Log("Spell Script spell destroy called");*/ DestroySpell(); }
    private void DestroySpell()
    {
        //Debug.Log(this.transform.position);
        Debug.Log("Spell Script destroy spell");

        if (triggerObjects != null) { for (int point = 0; point < triggerObjects.Length; point++) { Destroy(triggerObjects[point]); } }

        Destroy(this.transform.parent.gameObject);
    }


    public float GetSpellCooldownMax() 
    {
        float maxCD = 10;

        maxCD *= shapeScript.cooldownModifier;
        maxCD *= effectScript.cooldownModifier;
        maxCD *= elementScript.cooldownModifier;

        //Debug.Log("get spell cooldown max: " + maxCD);
        return maxCD; 
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the gizmo
        Gizmos.DrawWireSphere(this.transform.position, radius); // Draw the wire sphere
    }
}