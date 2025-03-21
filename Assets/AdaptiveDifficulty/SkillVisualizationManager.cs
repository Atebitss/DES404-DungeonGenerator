using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillVisualizationManager : MonoBehaviour
{
    [SerializeField] private GameObject graphPointPrefab;
    [SerializeField] private RectTransform graphContainer;

    private float[] skillScores = new float[0];
    private Vector3 containerCornerOffset;
    private Vector3[] points = new Vector3[0];
    private GameObject[] graphPoints = new GameObject[0];

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
            Debug.Log("containerCornerOffset: " + containerCornerOffset);
        }
    }

    public void AddSkillScoreDataPoint(float skillScore)
    {
        //increase score array size and add new skill score to score array
        float[] newSkillScores = new float[skillScores.Length + 1];
        for (int i = 0; i < skillScores.Length; i++) { newSkillScores[i] = skillScores[i]; }
        newSkillScores[skillScores.Length] = skillScore; //new score
        skillScores = newSkillScores;

        UpdateChart();
    }
    private void UpdateChart()
    {
        Debug.Log("containerCornerOffset: " + containerCornerOffset);
        if (ADM == null) { return; }

        //skill score length reference for later use
        int numDataPoints = skillScores.Length;
        float minSkill = 0f;
        float maxSkill = 200f;
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;


        //if there are 5 or more data points, remove the first point and shift all other points left
        if (numDataPoints > 5)
        {
            //decrease point array size and remove first point from point array
            Vector3[] movedPoints = new Vector3[numDataPoints - 1];
            for (int i = 1; i < numDataPoints; i++) { movedPoints[i - 1] = points[i]; }
            points = movedPoints;

            //decrease graph point array size and remove first graph point from graph point array
            GameObject[] movedGraphPoints = new GameObject[numDataPoints - 1];
            for (int i = 1; i < numDataPoints; i++) { movedGraphPoints[i - 1] = graphPoints[i]; }
            Destroy(graphPoints[0]); //destroy first graph point before updating array
            graphPoints = movedGraphPoints;

            //lower number of data points
            numDataPoints--;
        }


        //calculate new point position
        float xPos = containerCornerOffset.x;
        float yPos = containerCornerOffset.y;
        //Debug.Log("xPos: " + xPos + ", yPos: " + yPos);

        //increment xPos by number of data points
        xPos += (numDataPoints * (graphWidth / 25)); //number of data points times 1/25th of graph width

        //set yPos based on skill score
        yPos += ((skillScores[(numDataPoints - 1)] / maxSkill) * (graphHeight / 5)); //skill score divided by max skill times 1/10th of graph height
        //Debug.Log("new xPos: " + xPos + ", new yPos: " + yPos);


        //increase point array size and add new point to point array
        Vector3[] newPoints = new Vector3[numDataPoints + 1];
        for (int i = 0; i < (numDataPoints - 1); i++) { newPoints[i] = points[i]; }
        newPoints[numDataPoints] = new Vector3(xPos, yPos, 0);
        points = newPoints;
        //Debug.Log("new point: " + points[numDataPoints]);

        //increase graph point array size and add new graph point to graph point array
        GameObject[] newGraphPoints = new GameObject[numDataPoints + 1];
        for (int i = 0; i < (numDataPoints - 1); i++) { newGraphPoints[i] = graphPoints[i]; }
        newGraphPoints[numDataPoints] = Instantiate(graphPointPrefab, points[numDataPoints], Quaternion.identity);
        newGraphPoints[numDataPoints].transform.SetParent(graphContainer);
        newGraphPoints[numDataPoints].name = "Graph Point " + numDataPoints;
        graphPoints = newGraphPoints;
    }
}