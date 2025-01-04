using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    //relevant scripts
    private MapManager MM;
    private DungeonGeneration DG;

    //map creation
    private int boundsX, boundsZ; //map generation
    private Vector2 startPos, targetPos;



    private void Awake()
    {
        Debug.Log("PG, Awake");

        MM = this.gameObject.GetComponent<MapManager>();
        DG = this.gameObject.GetComponent<DungeonGeneration>();
    }



    public void BeginPathGeneration(Vector2 startPos, Vector2 targetPos, int boundsX, int boundsZ)
    {
        Debug.Log("PG, Begin Path Generation");

        this.boundsX = boundsX;
        this.boundsZ = boundsZ;

        this.startPos = startPos;
        this.targetPos = targetPos;

        GeneratePath(FindPath());
    }


    private Vector2[] FindPath()
    {
        //Debug.Log("PG, Find Path");

        //connect rooms using A*
        int startPosX = (int)startPos.x; //cast to int to avoid issues with floats
        int startPosZ = (int)startPos.y;
        int targetPosX = (int)targetPos.x;
        int targetPosZ = (int)targetPos.y;

        bool[,] openSet = new bool[boundsX, boundsZ]; //open grid positions
        bool[,] closedSet = new bool[boundsX, boundsZ]; //closed grid positions
        Vector2[,] previousPos = new Vector2[boundsX, boundsZ]; //previous grid positions
        int[,] costToNext = new int[boundsX, boundsZ]; //cheapest cost to next position
        int[,] pathToEnd = new int[boundsX, boundsZ]; //cheapest cost path to end position

        //number identifying room
        //Debug.Log("x:" + startPosX + ", z:" + startPosZ);
        //Debug.Log("x:" + targetPosX + ", z:" + targetPosZ);
        //Debug.Log(MM.GetGridState(startPosX, startPosZ));
        string startPosState = MM.GetGridState(startPosX, startPosZ);
        string targetPosState = MM.GetGridState(targetPosX, targetPosZ);
        int startRoomID = int.Parse(startPosState.Substring(4));
        int targetRoomID = int.Parse(targetPosState.Substring(4));
        //Debug.Log("startRoomID: " + startRoomID);
        //Debug.Log("targetRoomID: " + targetRoomID);


        //initialise scores for all grid positions
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                costToNext[x, z] = int.MaxValue;
                pathToEnd[x, z] = int.MaxValue;
                previousPos[x, z] = new Vector2(-1, -1);
            }
        }

        costToNext[startPosX, startPosZ] = 0;
        pathToEnd[startPosX, startPosZ] = Heuristic(startPos, targetPos); //find optimal path between start and end positions
        openSet[startPosX, startPosZ] = true;

        while (true) //infinite loop
        {
            Vector2 curPos = FindLowestCostPosition(openSet, pathToEnd, targetPos);

            if (curPos.x == -1 && curPos.y == -1) { return null; } //if no lower costs found, return no path
            if (curPos == targetPos) { return ConstructPath(previousPos, curPos); } //if current position is end position, build the path

            int curPosX = (int)curPos.x; //cast to int to avoid issues with floats
            int curPosZ = (int)curPos.y;

            //update positions so they're only checked once
            openSet[curPosX, curPosZ] = false;
            closedSet[curPosX, curPosZ] = true;

            //check neighbor positions
            for (int i = 0; i < 4; i++)
            {
                Vector2 neighborPos = curPos;
                switch (i)
                {
                    case 0: neighborPos += new Vector2Int(1, 0); break;
                    case 1: neighborPos += new Vector2Int(-1, 0); break;
                    case 2: neighborPos += new Vector2Int(0, 1); break;
                    case 3: neighborPos += new Vector2Int(0, -1); break;
                }
                int neighborPosX = (int)neighborPos.x;
                int neighborPosZ = (int)neighborPos.y;

                //Debug.Log("checking neighbor position x:" + neighborPosX + ", y:" + neighborPosZ);
                //Debug.Log("grid state:" + gridStates[neighborPosX, neighborPosZ]);

                if (neighborPosX < 0 || neighborPosX >= boundsX || neighborPosZ < 0 || neighborPosZ >= boundsZ) { continue; } //neighbor position outside of bounds


                int moveCost = GetMoveCost(neighborPosX, neighborPosZ);
                //Debug.Log("move cost: " + moveCost);
                if (moveCost == -1) { continue; }


                MM.UpdateDbugTileTextMoveCost(neighborPosX, neighborPosZ, moveCost);


                if (closedSet[neighborPosX, neighborPosZ]) { continue; } //if position closed, skip

                int tempCostToNext = costToNext[curPosX, curPosZ] + moveCost; //cost of current position
                if (tempCostToNext < costToNext[neighborPosX, neighborPosZ]) //if cost of current position is less than cost of reaching the neighbour position
                {
                    previousPos[neighborPosX, neighborPosZ] = curPos; //update the previous position with the current position
                    costToNext[neighborPosX, neighborPosZ] = tempCostToNext; //update neighbour cost with current cost
                    //update overall path with (neighbor cost + distance from target)
                    pathToEnd[neighborPosX, neighborPosZ] = costToNext[neighborPosX, neighborPosZ] + Heuristic(neighborPos, targetPos);
                    //update open set with neighbor position
                    if (!openSet[neighborPosX, neighborPosZ]) { /*Debug.Log("update open set with " + neighborPos);*/ openSet[neighborPosX, neighborPosZ] = true; }
                }
            }
        }
    }

    private Vector2 FindLowestCostPosition(bool[,] openSet, int[,] pathToEnd, Vector2 targetPos)
    {
        //Debug.Log("PG, Find Lowest Cost Position");

        //initialise lowest cost trackers
        Vector2 lowestCostPos = new Vector2(-1, -1);
        int lowestCost = int.MaxValue;

        //for each room
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (openSet[x, z] && pathToEnd[x, z] < lowestCost) //if open set and path to end less than lowest current cost
                {
                    //update lowest cost trackers
                    lowestCost = pathToEnd[x, z];
                    lowestCostPos = new Vector2(x, z);
                }
                else if (openSet[x, z] && pathToEnd[x, z] == lowestCost) //if open set and path to end equal to lowest current cost
                {
                    //update lowest cost position tracker
                    Vector2 tempPos = new Vector2(x, z);
                    if (Vector2.Distance(tempPos, targetPos) < Vector2.Distance(lowestCostPos, targetPos))
                    {
                        lowestCostPos = tempPos;
                    }
                }
            }
        }

        return lowestCostPos; //return lowest cost position
    }

    private int GetMoveCost(int x, int z)
    {
        //Debug.Log("PG, Get Move Cost");
        //Debug.Log("grid state: " + MM.GetGridState(x, z));

        switch (MM.GetGridState(x, z))
        {
            case "Wall": return 50;
            case "WallCorner": return 500;
            case "Hallway": return 1;
            case "Doorway": return 1;
            case "Empty": return 15;
            default:
                if (MM.GetGridState(x, z).StartsWith("Room")) { return 5; }
                else { return -1; }
        }
    }

    //calculate the shortest distance between start position and target position
    private int Heuristic(Vector2 startPos, Vector2 endPos) { return (int)Mathf.Sqrt(Mathf.Pow(startPos.x - endPos.x, 2) + Mathf.Pow(startPos.y - endPos.y, 2)); }

    private Vector2[] ConstructPath(Vector2[,] previousPos, Vector2 curPos)
    {
        //Debug.Log("PG, Construct Path");

        int pathLength = 0;
        Vector2 tempPos = curPos;
        int tempPosX = (int)tempPos.x;
        int tempPosZ = (int)tempPos.y;

        //count path length
        while (previousPos[tempPosX, tempPosZ].x != -1 && previousPos[tempPosX, tempPosZ].y != -1)
        {
            tempPos = previousPos[tempPosX, tempPosZ];
            pathLength++;
            tempPosX = (int)tempPos.x;
            tempPosZ = (int)tempPos.y;
        }

        //create path array
        Vector2[] path = new Vector2[pathLength + 1];
        int index = pathLength;

        //populate path array in reverse
        path[index] = curPos;
        tempPos = curPos;
        tempPosX = (int)tempPos.x;
        tempPosZ = (int)tempPos.y;

        while (previousPos[tempPosX, tempPosZ].x != -1 && previousPos[tempPosX, tempPosZ].y != -1)
        {
            tempPos = previousPos[tempPosX, tempPosZ];
            index--;
            path[index] = tempPos;
            tempPosX = (int)tempPos.x;
            tempPosZ = (int)tempPos.y;
        }

        return path;
    }

    private void GeneratePath(Vector2[] path)
    {
        Debug.Log("PG, Generate Path");

        if (path == null) { return; } //if there's no path, skip

        Vector2 hallwayStart = path[0];
        Vector2 hallwayEnd = path[path.Length - 1];

        //iterate through path array to create hallway
        for (int pathSection = 0; pathSection < path.Length - 1; pathSection++)
        {
            Vector2 start = path[pathSection];
            Vector2 end = path[pathSection + 1];
            int startX = (int)start.x;
            int startZ = (int)start.y;
            int endX = (int)end.x;
            int endZ = (int)end.y;


            //set grid states and update material for hallways
            for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
            {
                for (int z = Mathf.Min(startZ, endZ); z <= Mathf.Max(startZ, endZ); z++)
                {
                    if (MM.GetGridState(x, z) == "Empty")
                    {
                        //Debug.Log("setting position x:" + x + ", y:" + z + " as hallway");
                        MM.UpdateDbugTileMat(x, z, "Hallway");
                        MM.UpdateDbugTileTextGridState(x, z, "Hallway");
                        MM.UpdateGridState(x, z, "Hallway"); //mark the grid position as a hallway
                    }
                    else if (MM.GetGridState(x, z) == "Wall")
                    {
                        //Debug.Log("setting position x:" + x + ", y:" + z + " as doorway");
                        MM.UpdateDbugTileMat(x, z, "Doorway");
                        MM.UpdateDbugTileTextGridState(x, z, "Doorway");
                        MM.UpdateGridState(x, z, "Doorway"); //mark the grid position as a doorway
                    }
                }
            }
        }
    }
}
