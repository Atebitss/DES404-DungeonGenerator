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
    private GameObject[] aimingTargets = new GameObject[0];
    public void SetAimingTargets(GameObject[] aimTargets) { this.aimingTargets = aimTargets; }
    public GameObject[] GetAimingTargets() { return aimingTargets; }
    private GameObject[] ignoredTargets = new GameObject[1];
    public void SetIgnoredTargets(GameObject[] targets) { ignoredTargets = targets; }
    public void SetIgnoredTarget(GameObject target) { ignoredTargets[0] = target; }
    public GameObject[] GetIgnoredTargets() { return ignoredTargets; }
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
    public GameObject[] GettriggerObjects() { return triggerObjects; }

    //spell info
    private bool spellValid = false;
    public bool GetSpellValid() { return spellValid; }
    private bool casted = false;
    public bool GetCasted() { return casted; }
    private bool delayed = false;
    private bool spellActive = false;
    private float castStartTime = 0f;
    private float timeSinceCast = 0f;
    private float maxLength = 50f;
    public float GetMaxLength() { return maxLength; }
    public void SetMaxLength(float newMax) { maxLength = newMax; }
    private Vector3 startPos;
    public void SetStartPos(Vector3 startPos) { /*Debug.Log("set start pos: " + startPos);*/ this.startPos = startPos; }
    public Vector3 GetStartPos() { /*Debug.Log("get start pos: " + startPos);*/ return startPos; }
    private Vector3 endPos;
    public void SetEndPos(Vector3 endPos) { /*Debug.Log("set end pos: " + endPos);*/ this.endPos = endPos; }
    public Vector3 GetEndPos() { /*Debug.Log("get end pos: " + endPos);*/ return endPos; }
    private Vector3 dir;
    public Vector3 GetDir() { return dir; }
    private float journeyLength;
    public float GetJourneyLength() { return journeyLength; }
    private float startTime;
    public float GetStartTime() { return startTime; }
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
        //Debug.Log("SpellScript CastSpell");

        //Debug.Log("shape castable: " + shapeScript.castable);
        //Debug.Log("casted: " + casted);
        //Debug.Log(this.transform.position);
        if (shapeScript.castable && !casted) //if the spell can be cast and has not been cast yet
        {
            //Debug.Log("castable & !casted");
            //Debug.Log(this.transform.position);
            //Debug.Log(effectName + elementName + shapeName);

            this.transform.parent.transform.SetParent(null);

            targetPoints = shapeScript.pathPoints; //get appropriate aiming points from the shape script

            shapeScript.EndAim(); //end the aiming phase
            elementScript.SetupCondition(); //run any setup functions for the element script

            if(effectScript.componentWeight == 1) { effectScript.ApplyEffect(); } //if effect weight is 1, apply effect as spell is cast

            //if more than one trigger point & the spell will expire, shape is line
            if (shapeScript.GetTriggerPoints().Length > 0 && !spellPersist)
            {
                //Debug.Log("triggerPoints & !persistent");
                triggerPoints = shapeScript.GetTriggerPoints(); //update trigger points
                TravelSetup(); //determine spell movement
            }
            //if more than one trigger point & the spell will persist, shape is line and effect is chain
            /*else if (shapeScript.GetTriggerPoints().Length > 0 && spellPersist)
            {
                Debug.Log("triggerPoints & persistent");
                effectScript.ApplyEffect(); //find targets
                aimingTargets = effectScript.targets; //set spell targets to those found
                int numOfTargets = 0;
                for (int i = 0; i < aimingTargets.Length; i++) { if (aimingTargets[i] != null) { numOfTargets++; } }
                targetPoints = new Vector3[numOfTargets + 2]; //spell path +2 to include start and end 
                triggerPoints = new Vector3[numOfTargets + 1]; //where the spell should trigger +1 to include end point
                //for (int i = 0; i < targetPoints.Length; i++) { Debug.Log("Spell Script target point" + i + ": " + targetPoints[i]); }
                //triggerPoints = effectScript.pathPoints;
                TravelSetup(); //determine spell movement
            }*/

            UpdateRadius(); //apply component radius modifiers
            //Debug.Log(this.transform.position);

            casted = true;

            if (!delayed)
            {
                //move the spell along a line from first position to target position
                StartCoroutine(MoveToTarget());
            }
        }
        else if (!shapeScript.castable)
        {
            //Debug.Log("!castable");
            shapeScript.ApplyShape();
        }
        else if (spellPersist && casted)
        {
            //Debug.Log("persistent & casted");
            if (!effectName.Contains("Pierce"))
            {
                aimingTargets = effectScript.targets;
                //for (int i = 0; i < aimingTargets.Length; i++) { if (aimingTargets[i] == null) { Debug.Log("aiming targets null"); } else { Debug.Log(aimingTargets[i]); } }
                startPos = this.transform.position;
                if (effectScript.targets[0] != null) { endPos = effectScript.targets[0].transform.position; }
                targetPoints[0] = startPos;
                targetPoints[1] = endPos;
            }

            //for (int i = 0; i < targetPoints.Length; i++) { Debug.Log("Spell Script target point" + i + ": " + targetPoints[i]); }
            if (this.gameObject != null) { StartCoroutine(MoveToTarget()); }
        }
        else if (!spellPersist && casted)
        {
            //Debug.Log("!persistant & casted");
            targetPoints = effectScript.pathPoints;
            //for (int i = 0; i < targetPoints.Length; i++) { Debug.Log("Spell Script target point" + i + ": " + targetPoints[i]); }
            if (this.gameObject != null) { DestroySpell(); }
        }
    }
    public IEnumerator DelayCast(float delayTime)
    {
        //Debug.Log("Spell Script delay cast");
        delayed = true; //set delayed to true so the spell does not cast immediately
        yield return new WaitForSeconds(delayTime); //wait for the specified time
        delayed = false; //set delayed to false so the spell can be cast

        StartCoroutine(MoveToTarget());
    }

    public bool GetSpellCastable()
    {
        return shapeScript.castable;
    }
    private void TravelSetup()
    {
        //Debug.Log("Spell Script travel setup");

        //start & end positions are set on aim destroy - run at start of CastSpell()
        //the start and end point of the line are constant
        this.transform.position = startPos; //move spell to start pos

        //Debug.Log("travel setup end pos: " + endPos);

        targetPoints[0] = startPos; //path point 0 is start pos
        targetPoints[targetPoints.Length - 1] = endPos; //path point end is end pos

        triggerPoints[triggerPoints.Length - 1] = endPos; //trigger point end is end pos

        //if there are targeted objects and no defined trigger objects
        //set the trigger object pos' & trigger points to that of the targets
        //must be updatable later as the targets will constantly move - use UpdateTravel
        if (aimingTargets != null && triggerObjects == null)
        {
            //Debug.Log("aiming targets & no trigger objects");

            for (int i = 0; i < aimingTargets.Length; i++) 
            {
                if (aimingTargets[i] != null)
                {
                    targetPoints[i + 1] = aimingTargets[i].transform.position; //skips starting pos and doesnt go as far as end pos
                    triggerPoints[i] = aimingTargets[i].transform.position;
                }
                //Debug.Log("target points" + (i + 1) + ": " + targetPoints[i + 1]);
            }
        }

        //create trigger objects
        if (triggerObjects == null)
        {
            triggerObjects = new GameObject[triggerPoints.Length]; //new game object array to store triggers
            for (int point = 0; point < triggerPoints.Length; point++)
            {
                //Debug.Log("Adding trigger point" + point + " at " + triggerPoints[point]);
                triggerObjects[point] = Instantiate(lineMarkerPrefab, triggerPoints[point], Quaternion.identity);
                triggerObjects[point].name = "LineMarker" + (point + 1);
            }
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


    IEnumerator MoveToTarget()
    {
        //Debug.Log("Spell Script move to target: " + endPos);
        //Debug.Log(this.transform.position);
        int curTarget = 0;
        bool spellEnded = false;

        speed *= shapeScript.speedModifier; //apply shape speed modifier
        speed *= effectScript.speedModifier; //apply effect speed modifier
        speed *= elementScript.speedModifier; //apply element speed modifier

        //update life time counter for analytics
        timeSinceCast = Time.time - castStartTime;

        for (int step = 0; step < targetPoints.Length - 1; step++)
        {
            //Debug.Log(targetPoints.Length - 1); Debug.Log("step: " + step); Debug.Log(this.transform.position);
            //begining position, distance between start and end, time spell began travelling
            //for(int i = 0; i < targetPoints.Length; i++) { Debug.Log(targetPoints[i]); }

            Vector3 curStartPos = targetPoints[step];
            Vector3 curEndPos = targetPoints[step + 1];
            dir = (curEndPos - curStartPos).normalized;
            journeyLength = Vector3.Distance(curStartPos, curEndPos);
            startTime = Time.time;
            //Debug.Log(curStartPos); Debug.Log(curEndPos); Debug.Log(dir); Debug.Log(journeyLength); Debug.Log(startTime);

            if (aimingTargets != null && step > 0 && step < aimingTargets.Length && spellPersist) { curTarget++; }
            //Debug.Log(aimingTargets.Length);

            //while the spell is not at the target (with a small leeway) and not beyond the maximum travel distance, 
            while (Vector3.Distance(this.transform.position, curEndPos) > 0.01f && Vector3.Distance(curStartPos, this.transform.position) < maxLength)
            {
                //update target position for lerp if needed and move along interpolated lerp
                //if the spell is persistent and has aiming targets
                if (aimingTargets != null)
                {
                    if (spellPersist && aimingTargets.Length > 1 && step < aimingTargets.Length)
                    {
                        //update target position
                        //if the target isnt null
                        //Debug.Log(aimingTargets[curTarget]);
                        if (aimingTargets[curTarget] != null)
                        {
                            //update points with the targets position
                            targetPoints[curTarget + 1] = aimingTargets[curTarget].transform.position;
                            //Debug.Log("update target pos " + curTarget + ": " + targetPoints[curTarget] + " @ " + aimingTargets[curTarget].transform.position);
                            triggerPoints[curTarget] = aimingTargets[curTarget].transform.position;

                            triggerObjects[curTarget].transform.position = triggerPoints[curTarget]; //update the trigger positions accordingly
                        }
                        else
                        {
                            int nonNullIndex = -1;
                            for (int i = step + 2; i < targetPoints.Length; i++) { if (targetPoints[i] != null) { nonNullIndex = i; break; } }
                            if (nonNullIndex != -1)
                            {
                                //update the point with a middle ground between the last and next position
                                targetPoints[curTarget + 1] = Vector3.Lerp(curStartPos, targetPoints[nonNullIndex], 0.5f);
                                triggerPoints[curTarget] = targetPoints[curTarget];
                                triggerObjects[curTarget].transform.position = triggerPoints[curTarget];
                            }
                        }
                    }
                    else if(spellPersist && aimingTargets.Length == 1 && step < aimingTargets.Length)
                    {
                        //update target position
                        //if the target isnt null
                        //Debug.Log(aimingTargets[curTarget]);
                        if (aimingTargets[curTarget] != null)
                        {
                            //update points with the targets position
                            targetPoints[curTarget + 1] = aimingTargets[curTarget].transform.position;
                            //Debug.Log("update target pos " + curTarget + ": " + targetPoints[curTarget] + " @ " + aimingTargets[curTarget].transform.position);
                        }
                    }
                }

                //update spell with component impact
                if (effectScript.componentWeight == 2) { effectScript.ApplyEffect(); } //if effect weight is 2, apply effect during flight

                curEndPos = targetPoints[step + 1];
                dir = (curEndPos - curStartPos).normalized;
                journeyLength = Vector3.Distance(curStartPos, curEndPos);


                //Debug.Log(Vector3.Distance(this.transform.position, endPos) > 0.01f);
                float travelInterpolate = (Time.time - startTime) * speed / journeyLength;
                Vector3 nextPosition = Vector3.Lerp(curStartPos, curEndPos, travelInterpolate);


                Debug.DrawRay(transform.position, dir, Color.red, 10f);
                //check if the spell intersects with an object on the Enemy layer
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Vector3.Distance(transform.position, nextPosition) + 0.1f, LayerMask.GetMask("Enemy")) && !CheckIgnoredTargets(hit.collider.gameObject) && !HasAlreadyHitTarget(hit.collider.gameObject)) 
                {
                    //Debug.Log("Spell hit: " + hit.collider.gameObject.name);
                    spellEnded = true;
                    EndSpell();

                    if (!effectName.Contains("Pierce")) { break; } //exit
                }

                if (this.gameObject != null) { transform.position = nextPosition; }

                yield return null;
            }
        }

        if (effectName.Contains("Pierce")) { spellPersist = false; } //stop pierce persisting after reaching destination

        if (!spellEnded)
        {
            //if shape has no trigger points, run spell end on impact
            if (shapeScript.GetTriggerPoints().Length == 0) { EndSpell(); }
            else { DestroySpell(); }
        }
    }
    void OnTriggerEnter(Collider col)
    {
        //triggers when passing through a marker, triggering the spells effect
        if (col.gameObject.tag.Equals("PathMarker"))
        {
            //Debug.Log("spell collision with path marker");
            targets = FindTargets();
            DealDamage();
        }
    }

    private bool CheckIgnoredTargets(GameObject hitTarget)
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

    private void EndSpell()
    {
        //assuming the spell has no trigger points,
        //apply effect then find targets, deal damage, and destroy self
        //Debug.Log(this.transform.position);
        //Debug.Log("SpellScript end spell");

        targets = FindTargets(); //sets targets to those found within spell radius
        //for(int i = 0; i < targets.Length; i++) { Debug.Log("SpellScript spell target " + i + ": " + targets[i]); }
        if (effectScript.componentWeight == 3) { effectScript.ApplyEffect(); } //if effect weight is 3, apply effect upon impact
        DealDamage(); //deals damage to any targets found within said radius

        if (spellPersist && shapeScript.GetTriggerPoints().Length == 0) { CastSpell(); } //if spell continues after impact and has 0 triggers
        else if (!spellPersist) { DestroySpell(); } //destroys the spell if permited
    }

    private GameObject[] FindTargets()
    {
        //checked for target within range, send list of targets to spell for damage
        //Debug.Log("SpellScript find targets");
        //Debug.Log("Finding targets near " + this.transform.position + " within " + radius);

        int numOfTargets = 0;
        Collider[] collisions = Physics.OverlapSphere(this.transform.position, radius); //find all objects within radius of spell
        //for (int i = 0; i < collisions.Length; i++) { Debug.Log("collisions " + i + ": " + collisions[i]); }

        //ensure nothing's collided twice by removing multiple colliders attached to the same object
        for (int pos = 0; pos < collisions.Length; pos++) //for each collision
        {
            Collider curCol = collisions[pos]; //current collider
            //Debug.Log("cur: " + curCol);

            for (int check = pos + 1; check < collisions.Length; check++) //for each other collision
            {
                //Debug.Log("check: " + collisions[check]);
                if (collisions[check] != null && curCol != null && collisions[check].gameObject == curCol.gameObject) //if current collider equal to checked collider
                {
                    //Debug.Log("dupe found, nulling");
                    collisions[check] = null; //empty position at checked collider
                }
            }
        }

        GameObject[] newTargets = new GameObject[0]; //set new tracking array
        AbstractEnemy[] newTargetScripts = new AbstractEnemy[0]; //set new tracking array
        for (int check = 0; check < collisions.Length; check++) //for each found object
        {
            if (collisions[check] != null && collisions[check].gameObject.tag.Equals("Enemy") && !HasAlreadyHitTarget(collisions[check].gameObject)) //if their tag is enemy
            {
                //Debug.Log(collisions[check].name + " found at " + collisions[check].gameObject.transform.position);
                numOfTargets++; //increase the number of found targets by one

                GameObject[] tempNewTargets = newTargets; //prep to increase arrays size
                newTargets = new GameObject[numOfTargets]; //increase array size to number of found targets
                for(int i = 0; i < tempNewTargets.Length; i++) { newTargets[i] = tempNewTargets[i]; } //fill new array with previously found targets
                newTargets[numOfTargets - 1] = collisions[check].gameObject; //fill new arrays last position with enemy object
                //Debug.Log("new target " + (numOfTargets - 1) + ": " + newTargets[numOfTargets - 1]);

                AbstractEnemy[] tempNewTargetScripts = newTargetScripts; //prep to increase arrays size
                newTargetScripts = new AbstractEnemy[numOfTargets]; //increase array size to number of found targets
                for (int i = 0; i < tempNewTargetScripts.Length; i++) { newTargetScripts[i] = tempNewTargetScripts[i]; } //fill new array with previously found targets
                newTargetScripts[numOfTargets - 1] = collisions[check].gameObject.GetComponent<AbstractEnemy>(); //fill new arrays last position with enemy object
                //Debug.Log("new target script " + (numOfTargets - 1) + ": " + newTargetScripts[numOfTargets - 1]);

                AddHitTarget(collisions[check].gameObject); //add the enemy to the hit targets list
            }
        }

        //for(int i = 0; i < newTargets.Length; i++) { Debug.Log("new target " + i + ": " + newTargets[i]); }
        targetScripts = newTargetScripts; //update the target scripts to the new ones found
        return newTargets;
    }
    private bool HasAlreadyHitTarget(GameObject enemy)
    {
        for (int i = 0; i < hitTargets.Length; i++)
        {
            if (hitTargets[i] == enemy)
            {
                //Debug.Log("already hit enemy: " + enemy.name);
                return true;
            }
        }

        return false;
    }
    private void AddHitTarget(GameObject enemy)
    {
        GameObject[] newHitEnemies = new GameObject[hitTargets.Length + 1];
        for (int i = 0; i < hitTargets.Length; i++)
        {
            newHitEnemies[i] = hitTargets[i];
        }
        newHitEnemies[hitTargets.Length] = enemy;
        hitTargets = newHitEnemies;
    }

    private void DealDamage()
    {
        //Debug.Log("SpellScript deal dmg");
        int[] damagesDealt = new int[targets.Length];

        //calculation of damage, rounded <0.5>
        damageCalc = spellPower;
        damageCalc *= shapeScript.damageModifier;
        damageCalc *= effectScript.damageModifier;
        damageCalc *= elementScript.damageModifier;

        if (targets != null)
        {
            for (int targetNum = 0; targetNum < targets.Length; targetNum++)
            {
                if (targets[targetNum] != null)
                {
                    int randDamage = Random.Range(0, 2);

                    damageDealt = Mathf.RoundToInt(damageCalc);
                    damageDealt += randDamage;

                    damagesDealt[targetNum] = damageDealt;

                    targetScripts[targetNum].DamageTarget(damageDealt, damageType);
                    ASM.GetADM().SpellSuccess();
                    ASM.GetADM().AddSpellDamageDealt(damageDealt);
                    //Debug.Log("dmg calc on " + targets[targetNum].name + ": " + damageDealt);
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
        //Debug.Log("Spell Script destroy spell");

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