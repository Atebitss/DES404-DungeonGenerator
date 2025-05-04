using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AdaptiveDifficultyManager : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private PlayerController PC;
    private AbstractSceneManager ASM;
    private AdaptiveDifficultyDbugManager ADDM;
    private SkillVisualizationManager SVM;
    private RoomGeneration RG;


    private void Awake()
    {
        //where to export the file
        filePath = Application.persistentDataPath + "/player_stats.txt";
        //Debug.Log("filePath: " + filePath);
    }
    public void Wake(AbstractSceneManager newASM)
    {
        //Debug.Log("Waking up Adaptive Difficulty Manager");

        //set references
        ASM = newASM;
        PC = ASM.GetPlayerController();
        ADDM = PC.GetADDM();
        ADDM.SetADM(this);
        SVM = PC.GetSVM();
        SVM.SetADM(this);

        StartDataFile(); //start data file
    }
    public void End()
    {
        //Debug.Log("Ending Adaptive Difficulty Manager");

        //update data with final stats
        statsData += "\n~~~PLAYER DEATH~~~\n\n";
        FillDataFileRoom(); //fill room data
        FillDataFileFloor(); //fill floor data
        EndDataFile(); //end data file
        HardResetStats();
        SVM.ResetSVM();
    }
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~track stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //to be tracked throughout a run, reset on death/restart
    //room clears
    private int roomsCleared = 0;
    public int GetRoomsCleared() { return roomsCleared; }
    public void RoomCleared()
    {
        roomsCleared++;
        if (ADDM != null) { ADDM.roomsCleared = roomsCleared; }
    }

    private float avgRoomClearTime = 0f;
    public float GetAvgRoomClearTime() { return avgRoomClearTime; }
    private float[] roomClearTimes = new float[0];

    //floor clears
    private int floorsCleared = 0;
    public int GetFloorsCleared() { return floorsCleared; }
    public void FloorCleared(float clearTime)
    {
        floorsCleared++;
        if (ADDM != null) { ADDM.floorsCleared = floorsCleared; }

        //increase floorClearTimes array and add new data
        float[] newFloorClearTimes = new float[floorClearTimes.Length + 1];
        for (int i = 0; i < floorClearTimes.Length; i++) { newFloorClearTimes[i] = floorClearTimes[i]; }
        newFloorClearTimes[floorClearTimes.Length] = clearTime; //new time
        floorClearTimes = newFloorClearTimes;

        FillDataFileFloor();
    }

    private float avgFloorClearTime = 0f;
    public float GetAvgFloorClearTime() { return avgFloorClearTime; }
    private float[] floorClearTimes = new float[0];


    //to be tracked throughout a run, reset each room
    private bool statTracking = false;
    public bool IsStatTracking() { return statTracking; }
    private float startTime = 0f, endTime = 0f;
    public void StartStatWatch(RoomGeneration newRG)
    {
        //Debug.Log("Starting stat watch");
        RG = newRG;
        ResetRoomStats();

        if (!statTracking)
        {
            startTime = Time.time;
            statTracking = true;

            //find average room clear time and return to display manager
            float totalRoomClearTime = 0f;
            for (int i = 0; i < roomClearTimes.Length; i++)
            {
                //Debug.Log("roomClearTimes[" + i + "]: " + roomClearTimes[i]);
                totalRoomClearTime += roomClearTimes[i];
                //Debug.Log("totalRoomClearTime: " + totalRoomClearTime);
            }
            avgRoomClearTime = (totalRoomClearTime / roomClearTimes.Length);
            //Debug.Log("avgRoomClearTime: " + avgRoomClearTime);

            if (ADDM != null)
            {
                ADDM.SetADM(this);
                ADDM.startTime = startTime;
                ADDM.playerSkillScore = skillScore;
                ADDM.avgRoomClearTime = avgRoomClearTime;
            }
        }
    }

    public void EndStatWatch()
    {
        //Debug.Log("Ending stat watch");

        endTime = Time.time;
        if (ADDM != null)
        {
            ADDM.endTime = endTime;
            ADDM.roomsCleared = roomsCleared;
        }

        //track room clear time
        //increase roomClearTimes array size by 1
        //copy old array to new, larger array
        //add the current time to the end
        //update old array
        float[] newRoomClearTimes = new float[roomClearTimes.Length + 1];
        for (int i = 0; i < roomClearTimes.Length; i++) { newRoomClearTimes[i] = roomClearTimes[i]; }
        newRoomClearTimes[roomClearTimes.Length] = (endTime - startTime); //new time
        roomClearTimes = newRoomClearTimes;
        totalRoomsCleared++;

        Invoke("DisableStatWatch", 0.1f);
    }
    private void DisableStatWatch() { statTracking = false; }
    public void HardResetStats()
    {
        //reset all variables
        numOfAttacks = 0;
        numOfHits = 0;
        accuracy = 0f;
        combosPerformed = 0;

        numOfSpellsCast = 0;
        numOfSpellsHit = 0;
        spellAccuracy = 0f;

        numOfDodges = 0;
        numOfHitsDodged = 0;
        dodgeEffectiveness = 0f;

        totalDamageDealt = 0;
        totalDamageTaken = 0;
        totalSpellDamageDealt = 0;

        consumablesUsed = 0;

        timeIndexsOfDamageLastTaken = new float[0];
        avgTimeBetweenDamageTaken = 0f;

        roomsCleared = 0;
        floorsCleared = 0;
        roomClearTimes = new float[0];
        floorClearTimes = new float[0];
        avgRoomClearTime = 0f;
        avgFloorClearTime = 0f;

        skillScore = 100f;
        difficulty = 1;

        if (ADDM != null) { ADDM.ResetRoomStats(); }
    }
    private void ResetRoomStats()
    {
        startTime = 0f;
        endTime = 0f;

        numOfAttacks = 0;
        numOfHits = 0;
        accuracy = 0f;
        combosPerformed = 0;

        numOfSpellsCast = 0;
        numOfSpellsHit = 0;
        spellAccuracy = 0f;

        numOfDodges = 0;
        numOfHitsDodged = 0;
        dodgeEffectiveness = 0f;
    }



    private float[] timeIndexsOfDamageLastTaken = new float[0]; //time of when damage last taken
    private float avgTimeBetweenDamageTaken = 0f;
    public float[] GetTimesDamageTaken() { return timeIndexsOfDamageLastTaken; }
    public float GetAvgTimeBetweenDamageTaken() { return avgTimeBetweenDamageTaken; }
    public void DamageTaken()
    {
        //increase timeIdexsOfDamageLastTaken array size by 1
        //copy old array to new, larger array
        //add the current time to the end
        //update old array
        float[] newTimeIndexsOfDamageLastTaken = new float[timeIndexsOfDamageLastTaken.Length + 1];
        for (int i = 0; i < timeIndexsOfDamageLastTaken.Length; i++) { newTimeIndexsOfDamageLastTaken[i] = timeIndexsOfDamageLastTaken[i]; }
        newTimeIndexsOfDamageLastTaken[timeIndexsOfDamageLastTaken.Length] = (Time.time - startTime); //new time
        timeIndexsOfDamageLastTaken = newTimeIndexsOfDamageLastTaken;


        float totalTime = 0f;
        for (int i = 0; i < timeIndexsOfDamageLastTaken.Length - 1; i++)
        {
            //Debug.Log("timeIndexsOfDamageLastTaken[" + i + "]: " + timeIndexsOfDamageLastTaken[i]);
            totalTime += timeIndexsOfDamageLastTaken[i];
            //Debug.Log("totalTime: " + totalTime);
        }

        avgTimeBetweenDamageTaken = totalTime / timeIndexsOfDamageLastTaken.Length;

        //Debug.Log("avgTimeBetweenDamageTaken: " + avgTimeBetweenDamageTaken);
        if (ADDM != null)
        {
            ADDM.timesDamageTaken = timeIndexsOfDamageLastTaken;
            ADDM.avgTimeBetweenDamageTaken = avgTimeBetweenDamageTaken;
        }
    }


    private int numOfAttacks = 0, numOfHits = 0; //number of swings and hits to detemine accuracy
    private float accuracy = 0f;
    public int GetNumOfAttacks() { return numOfAttacks; }
    public int GetNumOfHits() { return numOfHits; }
    public float GetAccuracy() { return accuracy; }
    public void AttackRan()
    {
        numOfAttacks++;
        totalMeleeAttacks++;
        if (ADDM != null) { ADDM.numOfAttacks = numOfAttacks; }
    }
    public void AttackSuccess()
    {
        numOfHits++;
        totalMeleeHits++;
        if (ADDM != null) { ADDM.numOfHits = numOfHits; }
    }


    private int combosPerformed = 0; //number of combos performed
    public int GetCombosPerformed() { return combosPerformed; }
    public void ComboPerformed()
    {
        combosPerformed++;
        totalCombosPerformed++;
        if (ADDM != null) { ADDM.combosPerformed = combosPerformed; }
    }


    private int numOfSpellsCast = 0, numOfSpellsHit; //number of casts and hits to determine spell accuracy
    private float spellAccuracy = 0f, spellStrength = 0f;
    public int GetNumOfSpellAttacks() { return numOfSpellsCast; }
    public int GetNumOfSpellHits() { return numOfSpellsHit; }
    public float GetSpellAccuracy() { return spellAccuracy; }
    public void SetSpellStrength(float newSpellStrength) { spellStrength = newSpellStrength; }
    public void SpellRan()
    {
        numOfSpellsCast++;
        totalSpellAttacks++;
        if (ADDM != null) { ADDM.numOfMagicAttacks = numOfSpellsCast; }
    }
    public void SpellSuccess()
    {
        //Debug.Log("Spell success");
        numOfSpellsHit++;
        totalSpellHits++;
        if (ADDM != null) { ADDM.numOfMagicHits = numOfSpellsHit; }
    }


    private int numOfDodges = 0, numOfHitsDodged = 0; //number of dodges and hits dodged to determine dodge percision
    private float dodgeEffectiveness = 0f;
    public int GetNumOfDodges() { return numOfDodges; }
    public int GetNumOfHitsDodged() { return numOfHitsDodged; }
    public float GetDodgeEffectiveness() { return dodgeEffectiveness; }
    public void DodgeRan()
    {
        numOfDodges++;
        totalDodges++;
        if (ADDM != null) { ADDM.numOfDodges = numOfDodges; }
    }
    public void DodgeSuccess()
    {
        numOfHitsDodged++;
        totalDodgesSuccessful++;
        if (ADDM != null) { ADDM.numOfHitsDodged = numOfHitsDodged; }
    }


    private int consumablesUsed = 0; //number of consumables used
    public int GetConsumablesUsed() { return consumablesUsed; }
    public void ConsumableUsed()
    {
        consumablesUsed++;
        totalConsumablesUsed++;
        if (ADDM != null) { ADDM.consumablesUsed = consumablesUsed; }
    }


    //session stats
    private int totalMeleeAttacks = 0;
    private int totalMeleeHits = 0;
    private int totalCombosPerformed = 0;

    private int totalSpellAttacks = 0;
    private int totalSpellHits = 0;

    private int totalDodges = 0;
    private int totalDodgesSuccessful = 0;

    private int totalConsumablesUsed = 0;

    private int totalRoomsCleared = 0;
    private int totalFloorsCleared = 0;

    private int totalDamageDealt = 0;
    private int totalDamageTaken = 0;

    private int totalSpellDamageDealt = 0;

    public int GetTotalMeleeAttacks() { return totalMeleeAttacks; }
    public int GetTotalMeleeHits() { return totalMeleeHits; }
    public float GetTotalMeleeAccuracy()
    {
        if (totalMeleeAttacks > 0) { return ((float)totalMeleeHits / (float)totalMeleeAttacks); }
        else { return 0f; }
    }
    public int GetTotalCombosPerformed() { return totalCombosPerformed; }
    public int GetTotalSpellAttacks() { return totalSpellAttacks; }
    public int GetTotalSpellHits() { return totalSpellHits; }
    public float GetTotalSpellAccuracy()
    {
        if (totalSpellAttacks > 0) { return ((float)totalSpellHits / (float)totalSpellAttacks); }
        else { return 0f; }
    }
    public int GetTotalDodges() { return totalDodges; }
    public int GetTotalDodgesSuccessful() { return totalDodgesSuccessful; }
    public float GetTotalDodgeEffectiveness()
    {
        if (totalDodges > 0) { return ((float)totalDodgesSuccessful / (float)totalDodges); }
        else { return 0f; }
    }
    public int GetTotalConsumablesUsed() { return totalConsumablesUsed; }
    public int GetTotalRoomsCleared() { return totalRoomsCleared; }
    public int GetTotalFloorsCleared() { return totalFloorsCleared; }

    public int GetTotalDamageDealt() { return totalDamageDealt; }
    public void AddDamageDealt(int damage)
    {
        totalDamageDealt += damage;
        if (ADDM != null) { ADDM.totalDamageDealt = totalDamageDealt; }
    }

    public int GetTotalDamageTaken() { return totalDamageTaken; }
    public void AddDamageTaken(int damage)
    {
        totalDamageTaken += damage;
        if (ADDM != null) { ADDM.totalDamageTaken = totalDamageTaken; }
    }

    public int GetTotalSpellDamageDealt() { return totalSpellDamageDealt; }
    public void AddSpellDamageDealt(int damage)
    {
        totalSpellDamageDealt += damage;
        if (ADDM != null) { ADDM.totalSpellDamageDealt = totalSpellDamageDealt; }
    }
    //~~~~~~track stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~difficulty change~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private float skillScore = 100f; //used to determine player skill level
    public float GetSkillScore() { return skillScore; }
    private int difficulty = 1; //difficulty level
    public int GetDifficulty() { return difficulty; }

    public void RunDifficultyAdapter()
    {
        //difficulty change based on stats
        //calculate average time between damage taken
        float maxExpectedTimeBetweenDamage = 15f;
        if (avgTimeBetweenDamageTaken == 0) 
        {
            skillScore += (Time.time - startTime);

            if (ADDM != null)
            {
                ADDM.timesDamageTaken = timeIndexsOfDamageLastTaken;
                ADDM.avgTimeBetweenDamageTaken = (Time.time - startTime);
            }
        }
        else { skillScore += (avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage); }

        //calculate attack accuracy
        if (numOfAttacks > 0) { accuracy = (((float)numOfHits / (float)numOfAttacks)); }
        skillScore += accuracy;

        //add combo useage
        skillScore += combosPerformed;

        //add spell useage
        if (numOfSpellsCast > 0) { spellAccuracy = (((float)numOfSpellsHit / (float)numOfSpellsCast)); }
        skillScore += (spellAccuracy * spellStrength);

        //calculate dodge effectiveness
        if (numOfDodges > 0) { dodgeEffectiveness = (((float)numOfHitsDodged / (float)numOfDodges)); }
        skillScore += dodgeEffectiveness;

        //calculate average room clear time
        float totalRoomClearTime = 0f;
        for (int i = 0; i < roomClearTimes.Length; i++)
        {
            totalRoomClearTime += roomClearTimes[i];
        }

        avgRoomClearTime = (totalRoomClearTime / (float)roomClearTimes.Length);
        if (roomClearTimes.Length > 0) { skillScore -= (avgRoomClearTime / 10f); }

        //add rooms cleared
        skillScore += ((float)roomsCleared / 10f);

        //calculate average floor clear time
        float totalFloorClearTime = 0f;
        for (int i = 0; i < floorClearTimes.Length; i++)
        {
            totalFloorClearTime += floorClearTimes[i];
        }

        avgFloorClearTime = (totalFloorClearTime / floorClearTimes.Length);
        if (floorClearTimes.Length > 0) { skillScore -= (avgFloorClearTime / 10f); }

        //add floors cleared
        skillScore += ((float)floorsCleared * 10f);

        //difficulty change based on skillScore
        if (skillScore <= 25f) { difficulty = -1; }
        else if (skillScore > 25f && skillScore <= 75f) { difficulty = 0; }
        else if (skillScore > 75f && skillScore <= 125f) { difficulty = 1; }
        else if (skillScore > 125f && skillScore <= 175f) { difficulty = 2; }
        else if (skillScore > 175f && skillScore <= 225f) { difficulty = 3; }
        else if (skillScore > 225f && skillScore <= 275f) { difficulty = 4; }
        else if (skillScore > 275f) { difficulty = 5; }

        RG.SetPlayerSkillScore(skillScore);
        RG.SetRoomDifficulty(difficulty);

        if (ADDM != null) { ADDM.playerSkillScore = skillScore; }
        if (SVM != null) { SVM.AddSkillScoreDataPoint(skillScore); }
        FillDataFileRoom();

        if (statTracking)
        {
            EndStatWatch();
        }
    }
    //~~~~~~difficulty change~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~data export~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private string filePath = "";
    private string statsData = "";
    private void StartDataFile()
    {
        //Debug.Log("Starting data file");

        //start data file header
        //if file already exists, add to it
        if (System.IO.File.Exists(filePath))
        {
            //Debug.Log("File found. Adding to existing data file, " + System.DateTime.Now.ToString());

            statsData = System.IO.File.ReadAllText(filePath);

            //player stats string to be added to file
            statsData += "======================================\n\n";
            statsData += "\n\n\n--- New Report ---\n\n";
            statsData += "Player Stats Report - " + System.DateTime.Now.ToString() + "\n\n";
            statsData += "===================\n\n";
            FillDataFileRoom();
        }
        else
        {
            //Debug.Log("File not found. Creating new data file, " + System.DateTime.Now.ToString());

            //if no file exists, create new file at path location
            System.IO.File.Create(filePath);

            //player stats string to be added to file
            statsData = "\n\n--- Starting New Data File ---\n\n";
            statsData += "Player Stats Report - " + System.DateTime.Now.ToString() + "\n\n";
            statsData += "===================\n\n";
            FillDataFileRoom();
        }
    }
    private void FillDataFileRoom()
    {
        //Debug.Log("Filling data file with room " + roomsCleared + " data");

        //update statsData with room data
        statsData += "Rooms Cleared: " + roomsCleared + "\n" +
                    "Average Room Clear Time: " + avgRoomClearTime + "\n\n" +
                    "Attacks: " + numOfAttacks + ", Hits: " + numOfHits + "\n" +
                    "Accuracy: " + accuracy + "\n" +
                    "Combos Performed: " + combosPerformed + "\n\n" +
                    "Magic Attacks: " + numOfSpellsCast + ", Magic Hits: " + numOfSpellsHit + "\n" +
                    "Magic Accuracy: " + spellAccuracy + "\n\n" +
                    "Dodges: " + numOfDodges + ", Hits Dodged: " + numOfHitsDodged + "\n" +
                    "Dodge Effectiveness: " + dodgeEffectiveness + "\n\n" +
                    "Times Damage Taken: " + timeIndexsOfDamageLastTaken.Length + "\n" +
                    "Average Time Between Damage Taken: " + avgTimeBetweenDamageTaken + "\n\n" +
                    "Consumables Used: " + consumablesUsed + "\n\n" +
                    "Score Gained This Room: " + ((avgTimeBetweenDamageTaken / 15) + accuracy + dodgeEffectiveness + combosPerformed - (avgRoomClearTime / 10f) + ((float)roomsCleared / 10f) - (avgFloorClearTime / 10f) + ((float)floorsCleared * 10f)) + "\n" +
                    "Skill Score: " + skillScore + "\n\n" +
                    "~~~~~~~~~~~~~~~~~~~\n";
    }
    private void FillDataFileFloor()
    {
        //Debug.Log("Filling data file with floor " + floorsCleared + " data");

        totalFloorsCleared++;
        //update statsData with floor data
        statsData += "Floor: " + floorsCleared + "\n" +
                    "Average Floor Clear Time: " + avgFloorClearTime + "\n" +
                    "===================\n";
    }
    private void EndDataFile()
    {
        //Debug.Log("Ending data file");

        //update statsData with footer
        statsData += "\n\n\nEnd of Run Stats Report - " + System.DateTime.Now.ToString() + "\n";
        statsData += "======================================\n\n";

        //add statsData to file
        System.IO.File.WriteAllText(filePath, statsData);
    }
    //~~~~~~data export~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
