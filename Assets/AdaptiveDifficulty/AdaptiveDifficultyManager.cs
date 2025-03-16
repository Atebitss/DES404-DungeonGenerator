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
    private RoomGeneration RG;

    public void Wake()
    {
        ASM = GameObject.Find("SceneManager").GetComponent<AbstractSceneManager>();
        PC = ASM.GetPlayerController();
        ADDM = PC.GetADDM();
        ADDM.SetADM(this);
    }
    //~~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~track stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //to be tracked throughout a run, reset on death/restart
    private int roomsCleared = 0;
    public void RoomCleared() 
    {
        roomsCleared++; 
        if(ADDM != null) { ADDM.roomsCleared = roomsCleared; }
    }

    private float avgRoomClearTime = 0f;
    private float[] roomClearTimes = new float[0];


    //to be tracked throughout a run, reset each room
    private bool statTracking = false;
    public bool IsStatTracking() { return statTracking; }
    private float startTime = 0f, endTime = 0f;
    public void StartStatWatch(RoomGeneration newRG)
    {
        Debug.Log("Starting stat watch");
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
                Debug.Log("roomClearTimes[" + i + "]: " + roomClearTimes[i]);
                totalRoomClearTime += roomClearTimes[i];
                Debug.Log("totalRoomClearTime: " + totalRoomClearTime);
            }
            avgRoomClearTime = (totalRoomClearTime / roomClearTimes.Length);
            Debug.Log("avgRoomClearTime: " + avgRoomClearTime);

            if(ADDM != null) 
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
        Debug.Log("Ending stat watch");

        endTime = Time.time;
        if(ADDM != null) 
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
        numOfDodges = 0;
        numOfHitsDodged = 0;
        combosPerformed = 0;
        startTime = 0f;
        endTime = 0f;

        if (ADDM != null) { ADDM.ResetStats(); }
    }



    private float[] timeIndexsOfDamageLastTaken = new float[0]; //time of when damage last taken
    private float avgTimeBetweenDamageTaken = 0f;
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
            Debug.Log("timeIndexsOfDamageLastTaken[" + i + "]: " + timeIndexsOfDamageLastTaken[i]);
            totalTime += timeIndexsOfDamageLastTaken[i];
            Debug.Log("totalTime: " + totalTime);
        }
        avgTimeBetweenDamageTaken = totalTime / timeIndexsOfDamageLastTaken.Length;
        Debug.Log("avgTimeBetweenDamageTaken: " + avgTimeBetweenDamageTaken);
        if (ADDM != null) 
        {
            ADDM.timesDamageTaken = timeIndexsOfDamageLastTaken; 
            ADDM.avgTimeBetweenDamageTaken = avgTimeBetweenDamageTaken;
        }
    }


    private int numOfAttacks = 0, numOfHits = 0; //number of swings and hits to detemine accuracy
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
    public void ComboPerformed() 
    {
        combosPerformed++; 
        if (ADDM != null) { ADDM.combosPerformed = combosPerformed; }
    }
    //~~~~~~track stats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~~difficulty change~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private float skillScore = 100f; //used to determine player skill level
    public void RunDifficultyAdapter()
    {
        Debug.Log("Running difficulty adapter");

        //difficulty change based on stats
        //calculate average time between damage taken
        Debug.Log("");
        float maxExpectedTimeBetweenDamage = 15f;
        Debug.Log("(avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage): " + (avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage));
        skillScore += (avgTimeBetweenDamageTaken / maxExpectedTimeBetweenDamage);
        Debug.Log("skillScore: " + skillScore);


        //calculate attack accuracy
        Debug.Log("");
        float accuracy = 0f;
        if (numOfAttacks > 0) { accuracy = (((float)numOfHits / (float)numOfAttacks)); }
        Debug.Log("numOfHits: " + numOfHits + " / numOfAttacks: " + numOfAttacks);
        Debug.Log("accuracy: " + accuracy);
        skillScore += accuracy;
        Debug.Log("skillScore: " + skillScore);


        //calculate dodge effectiveness
        Debug.Log("");
        float dodgeEffectiveness = 0f;
        if (numOfDodges > 0) { dodgeEffectiveness = (((float)numOfHitsDodged / (float)numOfDodges)); }
        Debug.Log("numOfHitsDodged: " + numOfHitsDodged + " / numOfDodges: " + numOfDodges);
        Debug.Log("dodgeEffectiveness: " + dodgeEffectiveness);
        skillScore += dodgeEffectiveness;
        Debug.Log("skillScore: " + skillScore);


        //add combo useage
        Debug.Log("");
        Debug.Log("combosPerformed: " + combosPerformed);
        skillScore += combosPerformed;
        Debug.Log("skillScore: " + skillScore);


        //calculate average room clear time
        Debug.Log("");
        float totalRoomClearTime = 0f;
        for (int i = 0; i < roomClearTimes.Length; i++)
        {
            Debug.Log("roomClearTimes[" + i + "]: " + roomClearTimes[i]);
            totalRoomClearTime += roomClearTimes[i];
            Debug.Log("totalRoomClearTime: " + totalRoomClearTime);
        }
        avgRoomClearTime = (totalRoomClearTime / roomClearTimes.Length);
        Debug.Log("avgRoomClearTime: " + avgRoomClearTime);
        if (roomClearTimes.Length > 0) { skillScore -= (avgRoomClearTime / 10f); }
        Debug.Log("skillScore: " + skillScore);


        //add rooms cleared
        Debug.Log("");
        Debug.Log("roomsCleared: " + roomsCleared);
        skillScore += ((float)roomsCleared / 10f);
        Debug.Log("skillScore: " + skillScore);


        //difficulty change based on skillScore
        Debug.Log("");
        Debug.Log("final skillScore: " + skillScore);
        if (skillScore <= 25f)
        {
            Debug.Log("difficulty: beginner");
            RG.SetRoomDifficulty(-1);
            RG.SetPlayerSkillScore(skillScore);
        }
        else if (skillScore > 25f && skillScore <= 75f)
        {
            Debug.Log("difficulty: easy");
            RG.SetRoomDifficulty(0);
            RG.SetPlayerSkillScore(skillScore);
        }
        else if (skillScore > 75f && skillScore <= 125f)
        {
            Debug.Log("difficulty: normal");
            RG.SetRoomDifficulty(1);
            RG.SetPlayerSkillScore(skillScore);
        }
        else if (skillScore > 125f && skillScore <= 175f)
        {
            Debug.Log("difficulty: hard");
            RG.SetRoomDifficulty(2);
            RG.SetPlayerSkillScore(skillScore);
        }
        else if (skillScore > 175f)
        {
            Debug.Log("difficulty: dire");
            RG.SetRoomDifficulty(3);
            RG.SetPlayerSkillScore(skillScore);
        }

        if (ADDM != null) { ADDM.playerSkillScore = skillScore; }

        if (statTracking)
        {
            EndStatWatch();
        }
    }
    //~~~~~~difficulty change~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
