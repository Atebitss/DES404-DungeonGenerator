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
        if (ADM == null) { return; }

        //skill score length reference for later use
        int numDataPoints = skillScores.Length; //skillScores updated earlier, no need to increase array sizes later
        float minSkill = 0f;
        float maxSkill = 200f;
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;


        if (numDataPoints > 5) { numDataPoints = 5; } //only display last 5 data points
        Debug.Log("numDataPoints: " + numDataPoints);

        //calculate new point position
        float xPos = containerCornerOffset.x;
        float yPos = containerCornerOffset.y;
        Debug.Log("xPos: " + xPos + ", yPos: " + yPos);


        //if less than 5 data points have been created
        if (points.Length < 5)
        {
            Debug.Log("numDataPoint   " + numDataPoints + " < 5");

            //increment xPos by number of data points
            xPos += (numDataPoints * (graphWidth / 25)); //number of data points times 1/25th of graph width

            //set yPos based on skill score
            yPos += ((skillScores[(numDataPoints - 1)] / maxSkill) * (graphHeight / 5)); //skill score divided by max skill times 1/10th of graph height
            Debug.Log("new xPos: " + xPos + ", new yPos: " + yPos);


            //increase point array size and add new point to point array
            Vector3[] newPoints = new Vector3[numDataPoints];
            for (int i = 0; i < (numDataPoints - 1); i++) { newPoints[i] = points[i]; }
            newPoints[(numDataPoints - 1)] = new Vector3(xPos, yPos, 0);
            points = newPoints;
            Debug.Log("new point: " + points[(numDataPoints - 1)]);


            //increase graph point array size and add new graph point to graph point array
            GameObject[] newGraphPoints = new GameObject[numDataPoints];
            for (int i = 0; i < (numDataPoints - 1); i++) { newGraphPoints[i] = graphPoints[i]; }

            //create a new data point
            newGraphPoints[(numDataPoints - 1)] = Instantiate(graphPointPrefab, points[(numDataPoints - 1)], Quaternion.identity);
            newGraphPoints[(numDataPoints - 1)].transform.SetParent(graphContainer);
            newGraphPoints[(numDataPoints - 1)].name = "Graph Point " + numDataPoints;

            //update graph point array
            graphPoints = newGraphPoints;
            Debug.Log("new graph point: " + graphPoints[(numDataPoints - 1)]);
        }
        //otherwise, if there are 5 data points
        else
        {
            Debug.Log("numDataPoint " + numDataPoints + "  == 5");

            //move all points to the left, overwriting the first point and leaving the last point empty
            for (int i = 0; i < 4; i++)
            {
                Debug.Log("old point " + i + ": " + points[i]);
                Debug.Log("old point " + (i + 1) + ": " + points[(i + 1)]);

                float pointXPos = points[i].x; //store x position of current point
                points[i] = points[(i + 1)]; //set current point to next point
                points[i].x = pointXPos; //set x position of current point to stored x position
            }
            points[4] = Vector3.zero; //empty last point


            //calculate new point position
            //increment xPos by number of data points
            xPos += (numDataPoints * (graphWidth / 25)); //number of data points times 1/25th of graph width

            //set yPos based on skill score
            yPos += ((skillScores[(numDataPoints - 1)] / maxSkill) * (graphHeight / 5)); //skill score divided by max skill times 1/10th of graph height
            Debug.Log("new xPos: " + xPos + ", new yPos: " + yPos);

            //update last point
            points[4] = new Vector3(xPos, yPos, 0);
            Debug.Log("new point: " + points[(numDataPoints - 1)]);


            //update graph point objects with updated positions
            for (int i = 0; i < (numDataPoints - 1); i++) 
            {
                Debug.Log("updating graph point: " + graphPoints[i]);
                graphPoints[i].transform.position = points[i]; 
            }
        }
    }
}