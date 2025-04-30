using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillVisualizationManager : MonoBehaviour
{
    [SerializeField] private GameObject graphPointPrefab;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private TMP_Text[] xAxisTitleTexts;

    private float[] skillScores = new float[0];
    private Vector3 containerCornerOffset;
    private GameObject[] graphPoints = new GameObject[0];
    int calledCounter = 0;

    private AdaptiveDifficultyManager ADM;
    public void SetADM(AdaptiveDifficultyManager newADM)
    {
        ADM = newADM;
    }

    private void Awake()
    {
        if (containerCornerOffset == Vector3.zero)
        {
            //find bottom left corner position of ADVisual rect transform
            Vector3[] corners = new Vector3[4];
            graphContainer.GetWorldCorners(corners);
            containerCornerOffset = corners[0];
            //Debug.Log("containerCornerOffset: " + containerCornerOffset);
        }
    }

    public void AddSkillScoreDataPoint(float skillScore)
    {
        calledCounter++;

        if (skillScores.Length == 10)
        {
            //if the skill score is full, shift the scores to the left
            for (int i = 0; i < skillScores.Length - 1; i++)
            {
                skillScores[i] = skillScores[i + 1];
            }
            skillScores[skillScores.Length - 1] = skillScore;
        }
        else
        {
            //otherwise, increase the array size and add the new skill score
            float[] newSkillScores = new float[skillScores.Length + 1];
            for (int i = 0; i < skillScores.Length; i++) { newSkillScores[i] = skillScores[i]; }
            newSkillScores[skillScores.Length] = skillScore;
            skillScores = newSkillScores;
        }

        UpdateChart();
    }
    private void UpdateChart()
    {
        if (ADM == null) { return; }

        //skill score length reference for later use
        int numDataPoints = skillScores.Length; //skillScores updated earlier, no need to increase array sizes later
        float minSkill = 0f;
        float maxSkill = 250f;
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;

        Vector3[] newPoints = new Vector3[numDataPoints];
        GameObject[] newGraphPoints = new GameObject[numDataPoints];
        

        for (int i = 0; i < numDataPoints; i++)
        {
            //distribute across graph segments
            float xPos = ((i / 10f) * graphWidth) + 100f;

            //calculate y position based on skill score
            float yPos = ((skillScores[i] - minSkill) / (maxSkill - minSkill)) * graphHeight;

            //store new point
            newPoints[i] = new Vector3(xPos, yPos, 0);

            if (i < graphPoints.Length && graphPoints[i] != null)
            {
                //if the graph point already exists, update its position
                graphPoints[i].transform.localPosition = newPoints[i];
                newGraphPoints[i] = graphPoints[i];

                //update number displayed on segment title
                xAxisTitleTexts[i].text = ((calledCounter - (numDataPoints - i)) + 1).ToString();
            }
            else
            {
                //otherwise, create a new graph point
                newGraphPoints[i] = Instantiate(graphPointPrefab, graphContainer);
                newGraphPoints[i].transform.localPosition = newPoints[i];
                newGraphPoints[i].name = "Graph Point " + (i + 1);
                graphPoints = newGraphPoints;
            }
        }
    }
}