using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    //relevant scripts
    private MapManager MM;
    private DungeonGeneration DG;

    //debug info
    [SerializeField] private bool dbugEnabled = false;
    public bool isDbugEnabled() { return dbugEnabled; }

    //map creation
    private int boundsX, boundsZ; //map generation
    private Vector2 startPos, targetPos;



    private void Awake()
    {
        //set up references
        MM = this.gameObject.GetComponent<MapManager>();
        DG = this.gameObject.GetComponent<DungeonGeneration>();

        if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Awake"); }
    }



    public void BeginPathGeneration(Vector2 startPos, Vector2 targetPos, int boundsX, int boundsZ)
    {
        if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Begin Path Generation"); }

        //update data
        this.boundsX = boundsX;
        this.boundsZ = boundsZ;

        this.startPos = startPos;
        this.targetPos = targetPos;

        //begin path generation
        GeneratePath(FindPath());
    }


    private Vector2[] FindPath()
    {
        //find connections between rooms using A*
        if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Find Path"); }

        int startPosX = (int)startPos.x; //beginning position
        int startPosZ = (int)startPos.y;
        int targetPosX = (int)targetPos.x; //targetted room position
        int targetPosZ = (int)targetPos.y; //cast to int to avoid issues with floats

        bool[,] openSet = new bool[boundsX, boundsZ]; //availible for path finding
        bool[,] closedSet = new bool[boundsX, boundsZ]; //closed from path finding
        Vector2[,] previousPos = new Vector2[boundsX, boundsZ]; //previous grid positions
        int[,] costToNext = new int[boundsX, boundsZ]; //cheapest cost to next position
        int[,] pathToEnd = new int[boundsX, boundsZ]; //cheapest cost path to end position

        string startPosState = MM.GetGridState(startPosX, startPosZ); //beginning position state
        string targetPosState = MM.GetGridState(targetPosX, targetPosZ);
        int startRoomID = int.Parse(startPosState.Substring(4)); //starting room ID
        int targetRoomID = int.Parse(targetPosState.Substring(4)); //targetted room ID
        if (dbugEnabled) { MM.UpdateHUDDbugText("startRoomID: " + startRoomID + " / targetRoomID: " + targetRoomID); }


        //initialise movement costs for all grid positions
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                costToNext[x, z] = int.MaxValue;
                pathToEnd[x, z] = int.MaxValue;
                previousPos[x, z] = new Vector2(-1, -1);
            }

        }

        costToNext[startPosX, startPosZ] = 0; //set cost of current tile to 0
        pathToEnd[startPosX, startPosZ] = Heuristic(startPos, targetPos); //find optimal path between start and end positions
        openSet[startPosX, startPosZ] = true; //set current tile to open

        while (true) //loop until path is found
        {
            Vector2 curPos = FindLowestCostPosition(openSet, pathToEnd, targetPos); //look through the grid to find the lowest cost position

            if (curPos.x == -1 && curPos.y == -1) { return null; } //if no lower costs found, return no path
            if (curPos == targetPos) { return ConstructPath(previousPos, curPos); } //if current position is end position, build the path

            int curPosX = (int)curPos.x; //cast to int to avoid issues with floats
            int curPosZ = (int)curPos.y;

            //update positions so its only checked once
            openSet[curPosX, curPosZ] = false; //no longer availible for path finding
            closedSet[curPosX, curPosZ] = true; //no longer availible for path finding

            //check neighbor positions
            for (int i = 0; i < 4; i++)
            {
                Vector2 neighborPos = curPos;
                switch (i) //set direction
                {
                    case 0: neighborPos += new Vector2Int(1, 0); break;
                    case 1: neighborPos += new Vector2Int(-1, 0); break;
                    case 2: neighborPos += new Vector2Int(0, 1); break;
                    case 3: neighborPos += new Vector2Int(0, -1); break;
                }
                int neighborPosX = (int)neighborPos.x; //cast to int to avoid float issues
                int neighborPosZ = (int)neighborPos.y;

                /*if (dbugEnabled)
                {
                    Debug.Log("checking neighbor position x:" + neighborPosX + ", y:" + neighborPosZ);
                    Debug.Log("grid state:" + gridStates[neighborPosX, neighborPosZ]);
                }*/

                if (neighborPosX < 0 || neighborPosX >= boundsX || neighborPosZ < 0 || neighborPosZ >= boundsZ) { continue; } //check if neighboring position is outside of area bounds

                int moveCost = GetMoveCost(neighborPosX, neighborPosZ); //finds the appropriate moving cost for that position
                //if (dbugEnabled) { Debug.Log("move cost: " + moveCost);}
                if (moveCost == -1) { continue; } //if moving cost indicates a viable space

                if (closedSet[neighborPosX, neighborPosZ]) { continue; } //if position closed, skip

                if (MM.isDbugEnabled()) { MM.UpdateDbugTileTextMoveCost(neighborPosX, neighborPosZ, moveCost); } //update checked tile debug text with new cost


                int tempCostToNext = costToNext[curPosX, curPosZ] + moveCost; //cost of current position
                if (tempCostToNext < costToNext[neighborPosX, neighborPosZ]) //if cost of current position is less than cost of reaching the neighbour position
                {
                    previousPos[neighborPosX, neighborPosZ] = curPos; //update the previous position with the current position
                    costToNext[neighborPosX, neighborPosZ] = tempCostToNext; //update neighbour cost with current cost

                    //update overall path with (neighbor cost + distance from target)
                    pathToEnd[neighborPosX, neighborPosZ] = costToNext[neighborPosX, neighborPosZ] + Heuristic(neighborPos, targetPos);

                    //update open set with neighbor position
                    if (!openSet[neighborPosX, neighborPosZ]) { /*if (dbugEnabled) { MM.UpdateHUDDbugText("update open set with " + neighborPos); } */ openSet[neighborPosX, neighborPosZ] = true; }
                }
            }
        }
    }

    private Vector2 FindLowestCostPosition(bool[,] openSet, int[,] pathToEnd, Vector2 targetPos)
    {
        //if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Find Lowest Cost Position"); }

        //initialise lowest cost trackers
        Vector2 lowestCostPos = new Vector2(-1, -1); //set to -1 to be invalid until fixed
        int lowestCost = int.MaxValue; //set to max value so any found position is lower

        //for every grid tile
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (openSet[x, z] && pathToEnd[x, z] < lowestCost) //if current grid tile is open and path to end is less than lowest current cost
                {
                    //update lowest cost trackers
                    lowestCost = pathToEnd[x, z]; //update with lower cost
                    lowestCostPos = new Vector2(x, z); //update with new position
                }
                else if (openSet[x, z] && pathToEnd[x, z] == lowestCost) //if current grid tile is open and path to end is equal to lowest current cost
                {
                    //update lowest cost position tracker
                    Vector2 tempPos = new Vector2(x, z); //set current position
                    if (Vector2.Distance(tempPos, targetPos) < Vector2.Distance(lowestCostPos, targetPos)) //compare current and lowest cost distances
                    {
                        lowestCostPos = tempPos; //update with lower cost
                    }
                }
            }
        }

        return lowestCostPos; //return lowest cost position
    }

    private int GetMoveCost(int x, int z)
    {
        //find movement cost of requested position
        //if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Get Move Cost"); }
        //if (dbugEnabled) { MM.UpdateHUDDbugText("grid state: " + MM.GetGridState(x, z)); }

        //return cost depending on position state
        switch (MM.GetGridState(x, z))
        {
            case "Wall": return 50; //high cost, path will avoid this if possible
            case "WallCorner": return 500; //absurd cost, path will avoid this at all costs
            case "Hallway": return 1; //low cost, path will prioritise this
            case "Doorway": return 1;
            case "Empty": return 15; //moderate cost, path will use if needed
            default:
                if (MM.GetGridState(x, z).StartsWith("Room")) { return 5; } //low cost, allows path to traverse rooms easily
                else { return -1; } //should never occur
        }
    }

    //calculate the shortest distance between start position and target position
    private int Heuristic(Vector2 startPos, Vector2 endPos) { return (int)Mathf.Sqrt(Mathf.Pow(startPos.x - endPos.x, 2) + Mathf.Pow(startPos.y - endPos.y, 2)); }

    private Vector2[] ConstructPath(Vector2[,] previousPos, Vector2 curPos)
    {
        //puts together an array of the path findings previous steps
        if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Construct Path"); }

        int pathLength = 0; //track number of tiles in path
        Vector2 tempPos = curPos; //used to track previous position as the path iterates
        int tempPosX = (int)tempPos.x; //cast to int to avoid float issues 
        int tempPosZ = (int)tempPos.y; //(should only ever return full values)

        //count path length
        while (previousPos[tempPosX, tempPosZ].x != -1 && previousPos[tempPosX, tempPosZ].y != -1) //if previous position is still valid
        {
            tempPos = previousPos[tempPosX, tempPosZ]; //update with previous position
            pathLength++; //increase number of tiles
            tempPosX = (int)tempPos.x;  //cast to int to avoid float issues 
            tempPosZ = (int)tempPos.y;
        }

        //create path array
        Vector2[] path = new Vector2[pathLength + 1]; //initiate path array
        int index = pathLength; //start path at end

        //populate path array in reverse
        path[index] = curPos; //add current position to end of path
        tempPos = curPos; //update with current position
        tempPosX = (int)tempPos.x; //cast to int to avoid float issues 
        tempPosZ = (int)tempPos.y;

        //fill path array
        while (previousPos[tempPosX, tempPosZ].x != -1 && previousPos[tempPosX, tempPosZ].y != -1)
        {
            tempPos = previousPos[tempPosX, tempPosZ]; //update with previous posiiton
            index--; //lower index
            path[index] = tempPos; //add position to path array
            tempPosX = (int)tempPos.x; //cast to int to avoid float issues 
            tempPosZ = (int)tempPos.y;
        }

        return path;
    }

    private void GeneratePath(Vector2[] path)
    {
        //update map manager with new path
        if (dbugEnabled) { MM.UpdateHUDDbugText("PG, Generate Path between " + startPos + " & " + targetPos); }

        if (path == null) { return; } //if there's no path, skip

        Vector2 hallwayStart = path[0]; //set start position
        Vector2 hallwayEnd = path[path.Length - 1]; //set end position

        //iterate through path array to create hallway
        for (int pathSection = 0; pathSection < path.Length - 1; pathSection++)
        {
            Vector2 start = path[pathSection]; //set start position to current path section
            Vector2 end = path[pathSection + 1]; //set end position to next path section
            int startX = (int)start.x; //cast to int to avoid float issues 
            int startZ = (int)start.y;
            int endX = (int)end.x;
            int endZ = (int)end.y;


            for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
            {
                for (int z = Mathf.Min(startZ, endZ); z <= Mathf.Max(startZ, endZ); z++)
                {
                    if (MM.GetGridState(x, z) == "Empty")
                    {
                        //set grid states and update material for hallways
                        //if (dbugEnabled) { MM.UpdateHUDDbugText("setting position x:" + x + ", y:" + z + " as hallway"); }
                        MM.UpdateDbugTileMat(x, z, "Hallway");
                        MM.UpdateDbugTileTextGridState(x, z, "Hallway");
                        MM.UpdateGridState(x, z, "Hallway"); //mark the grid position as a hallway
                    }
                    else if (MM.GetGridState(x, z) == "Wall")
                    {
                        //set grid states and update material for doorways
                        //if (dbugEnabled) { MM.UpdateHUDDbugText("setting position x:" + x + ", y:" + z + " as doorway"); }
                        MM.UpdateDbugTileMat(x, z, "Doorway");
                        MM.UpdateDbugTileTextGridState(x, z, "Doorway");
                        MM.UpdateGridState(x, z, "Doorway"); //mark the grid position as a doorway
                    }
                }
            }
        }
    }
}
