using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdaptiveDifficultyDbugManager : MonoBehaviour
{
    //misc
    private AdaptiveDifficultyManager ADM;
    public void SetADM(AdaptiveDifficultyManager newADM) { ADM = newADM; }

    //text
    [SerializeField] private TMP_Text dbugText;


    //player stats
    public float playerSkillScore = 0f;

    public int numOfAttacks = 0;
    public int numOfHits = 0;
    private float accuracy = 0f;

    public int combosPerformed = 0;

    public int totalDamageDealt = 0;
    public int totalDamageTaken = 0;
    public int totalSpellDamageDealt = 0;

    public int numOfMagicAttacks = 0;
    public int numOfMagicHits = 0;
    private float magicAccuracy = 0f;

    public int numOfDodges = 0;
    public int numOfHitsDodged = 0;
    private float dodgeEffectivness = 0f;

    public float[] timesDamageTaken = new float[0];
    public float avgTimeBetweenDamageTaken = 0f;


    //room stats
    public float currentDifficulty = 0f;
    public float startTime = 0f, endTime = 0f;

    public int consumablesUsed = 0;

    public int roomsCleared = 0;
    public float[] roomClearTimes = new float[0];
    public float avgRoomClearTime = 0f;


    //floor stats
    public int floorsCleared = 0;
    public float[] floorClearTimes = new float[0];
    public float avgFloorClearTime = 0f;


    public void ResetRoomStats()
    {
        numOfAttacks = 0;
        numOfHits = 0;
        accuracy = 0f;
        combosPerformed = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;
        totalSpellDamageDealt = 0;
        numOfMagicAttacks = 0;
        numOfMagicHits = 0;
        magicAccuracy = 0f;
        numOfDodges = 0;
        numOfHitsDodged = 0;
        dodgeEffectivness = 0f;
        consumablesUsed = 0;
        timesDamageTaken = new float[0];
        avgTimeBetweenDamageTaken = 0f;
    }



    public void SwitchVisible()
    {
        if (this.enabled) { this.gameObject.SetActive(false); }
        else if (!this.enabled) { this.gameObject.SetActive(true); }
    }

    private void FixedUpdate() 
    {
        if (ADM.IsStatTracking()) 
        {
            if (numOfAttacks > 0 && numOfHits > 0) { accuracy = (((float)numOfHits / (float)numOfAttacks)); }
            if (numOfMagicAttacks > 0 && numOfMagicHits > 0) { magicAccuracy = (((float)numOfMagicHits / (float)numOfMagicAttacks)); }
            if (numOfDodges > 0 && numOfHitsDodged > 0) { dodgeEffectivness = (((float)numOfHitsDodged / (float)numOfDodges)); }
            UpdateDisplayText(); 
        } 
    }
    private void UpdateDisplayText()
    {
        dbugText.text =
            "Skill Score: " + playerSkillScore +
            "   Difficulty: " + currentDifficulty +
            "\nRooms Cleared: " + roomsCleared +
            "   Avg Clear Time: " + avgRoomClearTime +
            "\nCur Room Start Time: " + startTime +
            "   End Time: " + endTime +
            "\nAttacks: " + numOfAttacks +
            "   Hits: " + numOfHits +
            "\nAccuracy: " + accuracy +
            "   Damage Dealt: " + totalDamageDealt +
            "\nCombos: " + combosPerformed +
            "\nMagic Attacks: " + numOfMagicAttacks +
            "   Magic Hits: " + numOfMagicHits +
            "\nMagic Accuracy: " + magicAccuracy +
            "   Spell Damage Dealt: " + totalSpellDamageDealt +
            "\nDodges: " + numOfDodges +
            "   Hits Dodged: " + numOfHitsDodged +
            "\nDodge Effectiveness: " + dodgeEffectivness +
            "\nHits Taken: " + timesDamageTaken.Length +
            "   Damage Taken: " + totalDamageTaken +
            "\nAvg Time Between Damage: " + avgTimeBetweenDamageTaken +
            "\nConsumables Used: " + consumablesUsed;
    }
}
