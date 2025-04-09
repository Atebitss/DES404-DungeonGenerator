using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AdaptiveDifficultyManager : MonoBehaviour
{
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private PlayerController PC;
    private AbstractSceneManager ASM;
    private AdaptiveDifficultyDisplayManager ADDM;
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
        ResetTrackers();

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

        Invoke("DisableStatWatch", 0.1f);
    }
    private void DisableStatWatch() { statTracking = false; }
    private void ResetTrackers()
    {
        timeIndexsOfDamageLastTaken = new float[0];
        numOfAttacks = 0;
        numOfHits = 0;
        accuracy = 0f;
        numOfDodges = 0;
        numOfHitsDodged = 0;
        dodgeEffectiveness = 0f;
        combosPerformed = 0;
        startTime = 0f;
        endTime = 0f;

        if (ADDM != null) { ADDM.ResetRoomStats(); }
    }



    private float[] timeIndexsOfDamageLastTaken = new float[0]; //time of when damage last taken
    private float avgTimeBetweenDamageTaken = 0f;
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
    public float GetAccuracy() { return accuracy; }
    public void AttackRan()
    {
        numOfAttacks++;
        if (ADDM != null) { ADDM.numOfAttacks = numOfAttacks; }
    }
    public void AttackSuccess()
    {
        numOfHits++;
        if (ADDM != null) { ADDM.numOfHits = numOfHits; }
    }


    private int numOfDodges = 0, numOfHitsDodged = 0; //number of dodges and hits dodged to determine dodge percision
    private float dodgeEffectiveness = 0f;
    public float GetDodgeEffectiveness() { return dodgeEffectiveness; }
    public void DodgeRan()
    {
        numOfDodges++;
        if (ADDM != null) { ADDM.numOfDodges = numOfDodges; }
    }
    public void DodgeSuccess()
    {
        numOfHitsDodged++;
        if (ADDM != null) { ADDM.numOfHitsDodged = numOfHitsDodged; }
    }


    private int combosPerformed = 0; //number of combos performed
    public int GetCombosPerformed() { return combosPerformed; }
    public void ComboPerformed()
    {
        combosPerformed++;
        if (ADDM != null) { ADDM.combosPerformed = combosPerformed; }
    }
    //~~~~~~track stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~difficulty change~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private float skillScore = 100f; //used to determine player skill level
    public float GetSkillScore() { return skillScore; }
    private int difficulty = 5; //difficulty level
    public int GetDifficulty() { return difficulty; }

    public void RunDifficultyAdapter()
    {
        //Debug.Log("Running difficulty adapter");

        //difficulty change based on stats
        //calculate average time between damage taken
        float maxExpectedTimeBetweenDamage = 15f;
        //Debug.Log("TimeBetweenDamageTaken score: " + (avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage));
        skillScore += (avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage);
        //Debug.Log("skillScore: " + skillScore);


        //calculate attack accuracy
        if (numOfAttacks > 0) { accuracy = (((float)numOfHits / (float)numOfAttacks)); }
        //Debug.Log("numOfHits: " + numOfHits + " / numOfAttacks: " + numOfAttacks);
        //Debug.Log("accuracy score: " + accuracy);
        skillScore += accuracy;
        //Debug.Log("skillScore: " + skillScore);


        //calculate dodge effectiveness
        if (numOfDodges > 0) { dodgeEffectiveness = (((float)numOfHitsDodged / (float)numOfDodges)); }
        //Debug.Log("numOfHitsDodged: " + numOfHitsDodged + " / numOfDodges: " + numOfDodges);
        //Debug.Log("dodge score: " + dodgeEffectiveness);
        skillScore += dodgeEffectiveness;
        //Debug.Log("skillScore: " + skillScore);


        //add combo useage
        //Debug.Log("combo scored: " + combosPerformed);
        skillScore += combosPerformed;
        //Debug.Log("skillScore: " + skillScore);


        //calculate average room clear time
        float totalRoomClearTime = 0f;
        for (int i = 0; i < roomClearTimes.Length; i++)
        {
            //Debug.Log("roomClearTimes[" + i + "]: " + roomClearTimes[i]);
            totalRoomClearTime += roomClearTimes[i];
            //Debug.Log("totalRoomClearTime: " + totalRoomClearTime);
        }
        avgRoomClearTime = (totalRoomClearTime / (float)roomClearTimes.Length);
        //Debug.Log("RoomClearTime score: " + avgRoomClearTime);
        if (roomClearTimes.Length > 0) { skillScore -= (avgRoomClearTime / 10f); }
        //Debug.Log("skillScore: " + skillScore);


        //add rooms cleared
        //Debug.Log("roomsCleared score: " + roomsCleared);
        skillScore += ((float)roomsCleared / 10f);
        //Debug.Log("skillScore: " + skillScore);


        //calculate average floor clear time
        float totalFloorClearTime = 0f;
        for (int i = 0; i < floorClearTimes.Length; i++)
        {
            //Debug.Log("floorClearTimes[" + i + "]: " + floorClearTimes[i]);
            totalFloorClearTime += floorClearTimes[i];
            //Debug.Log("totalFloorClearTime: " + totalFloorClearTime);
        }
        avgFloorClearTime = (totalFloorClearTime / floorClearTimes.Length);
        //Debug.Log("FloorClearTime score: " + avgFloorClearTime);
        if (floorClearTimes.Length > 0) { skillScore -= (avgFloorClearTime / 10f); }
        //Debug.Log("skillScore: " + skillScore);


        //add floors cleared
        //Debug.Log("floorsCleared score: " + floorsCleared);
        skillScore += ((float)floorsCleared * 10f);
        //Debug.Log("skillScore: " + skillScore);


        //difficulty change based on skillScore
        //Debug.Log("final skillScore: " + skillScore);
        if (skillScore <= 25f)
        {
            //Debug.Log("difficulty: beginner");
            difficulty = -1;
        }
        else if (skillScore > 25f && skillScore <= 75f)
        {
            //Debug.Log("difficulty: easy");
            difficulty = 0;
        }
        else if (skillScore > 75f && skillScore <= 125f)
        {
            //Debug.Log("difficulty: normal");
            difficulty = 1;
        }
        else if (skillScore > 125f && skillScore <= 175f)
        {
            //Debug.Log("difficulty: hard");
            difficulty = 2;
        }
        else if (skillScore > 175f)
        {
            //Debug.Log("difficulty: dire");
            difficulty = 3;
        }

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
                    "Dodges: " + numOfDodges + ", Hits Dodged: " + numOfHitsDodged + "\n" +
                    "Dodge Effectiveness: " + dodgeEffectiveness + "\n" +
                    "Combos Performed: " + combosPerformed + "\n\n" +
                    "Times Damage Taken: " + timeIndexsOfDamageLastTaken.Length + "\n" +
                    "Average Time Between Damage Taken: " + avgTimeBetweenDamageTaken + "\n\n" +
                    "Score Gained This Room: " + ((avgTimeBetweenDamageTaken / 15) + accuracy + dodgeEffectiveness + combosPerformed - (avgRoomClearTime / 10f) + ((float)roomsCleared / 10f) - (avgFloorClearTime / 10f) + ((float)floorsCleared * 10f)) + "\n" +
                    "Skill Score: " + skillScore + "\n\n" +
                    "~~~~~~~~~~~~~~~~~~~\n";
    }
    private void FillDataFileFloor()
    {
        //Debug.Log("Filling data file with floor " + floorsCleared + " data");

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
