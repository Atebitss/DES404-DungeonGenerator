using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpellDbugManager : MonoBehaviour
{
    //text
    [SerializeField] private TMP_Text dbugText;

    //spell
    public string spellShape = "", spellEffect = "", spellElement = "";
    public float spellCooldown = 0f, spellCooldownMax = 0f;
    public float radius = 0f, speed = 0f, damage = 0f, spellPower = 1f;
    public bool valid = false, casted = false, persistent = false;
    public int targetPoints = 0, triggerPoints = 0;
    public int targets = 0, aimingTargets = 0, ignoredTargets = 0;
    public Vector3 startPos = Vector3.zero, endPos = Vector3.zero;
    public Vector3 direction = Vector3.zero;
    public float distance = 0f;



    public void SwitchVisible()
    {
        if (this.enabled) { this.gameObject.SetActive(false); }
        else if (!this.enabled) { this.gameObject.SetActive(true); }
    }

    private void FixedUpdate() { UpdateDisplayText(); }
    private void UpdateDisplayText()
    {
        dbugText.text =
            "Shape: " + spellShape +
            "   Effect: " + spellEffect +
            "   Element: " + spellElement +
            "\nCooldown: " + spellCooldown +
            " / Cooldown Max: " + spellCooldownMax +
            "\nRadius: " + radius +
            "   Speed: " + speed +
            "   Damage: " + damage +
            "\nValid: " + valid +
            "   Casted: " + casted +
            "\nTarget Points: " + targetPoints +
            "   Trigger Points: " + triggerPoints +
            "\nTargets: " + targets +
            "   Aiming Targets: " + aimingTargets +
            "   Ignored Targets: " + ignoredTargets +
            "\nStart Position: " + startPos +
            "   End Position: " + endPos +
            "\nDirection: " + direction +
            "   Distance: " + distance;
    }
}
