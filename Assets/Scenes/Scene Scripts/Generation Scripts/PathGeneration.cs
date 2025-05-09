using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    //relevant scripts
    private MapGeneration MG;
    private DungeonGeneration DG;
    private AbstractSceneManager ASM;

    //debug info
    private bool dbugEnabled = false;
    private bool visualDemo = false;

    //map creation
    private int boundsX, boundsZ; //map generation
    private Vector2 startPos, targetPos; //start and end position of the path
    private int scale; //room scale

    //hallway creation
    private GameObject hallwayParent = null; //the current hallway parent
    private GameObject[] hallwayParents = new GameObject[0]; //every hallway parent
    public GameObject[] GetHallwayParents() { return hallwayParents; }
    [SerializeField] private GameObject hallwayFloorPrefab, hallwayWallPrefab;
    private GameObject[][] hallwayWalls = new GameObject[0][], hallwayFloors = new GameObject[0][]; //jagged 2D array to store hallway walls/floors per hallway (hallway count, wall/floor count)
    private Dictionary<int, Vector2[]> hallwayPaths = new Dictionary<int, Vector2[]>(); //holds hallwayIndex & hallway positions
    private int wallIndex = 0, floorIndex = 0; //how many walls/floors there are per hallway
    private int hallwayCount = 0; //how many hallways there are
    private int hallwayParentIndex = 0; //how many hallway parents there are (replacing this with count breaks things idk why)

    private void Awake()
    {
        //set up references
        ASM = this.gameObject.GetComponent<AbstractSceneManager>();
        MG = this.gameObject.GetComponent<MapGeneration>();
        DG = this.gameObject.GetComponent<DungeonGeneration>();

        dbugEnabled = ASM.GetDbugMode();
        visualDemo = ASM.GetVisualMode();

        //if (dbugEnabled) { MG.UpdateHUDDbugText("PG, Awake"); }
    }



    public IEnumerator BeginPathGeneration(Vector2 startPos, Vector2 targetPos, int boundsX, int boundsZ, int scale)
    {
        //Debug.Log("PathGeneration: BeginPathGeneration");
        if (dbugEnabled) { MG.UpdateHUDDbugText("Dungeon Generation: Beginning Path Generation"); }

        //update data
        this.boundsX = boundsX;
        this.boundsZ = boundsZ;

        this.startPos = startPos;
        this.targetPos = targetPos;
        this.scale = scale;

        if(hallwayParent == null)
        {
            //create hallway parent object
            hallwayParent = new GameObject("HallwayParent");
            hallwayParent.transform.parent = this.gameObject.transform;
        }

        hallwayCount++; //increase hallway count

        GameObject[] newHallwayParents = new GameObject[hallwayCount]; //create new hallway parent
        for (int parentIndex = 0; parentIndex < hallwayParents.Length; parentIndex++){ newHallwayParents[parentIndex] = hallwayParents[parentIndex]; } //copy existing hallways
        newHallwayParents[hallwayCount - 1] = new GameObject("HallwayParent" + hallwayParentIndex); //create new hallway parent
        newHallwayParents[hallwayCount - 1].transform.parent = hallwayParent.gameObject.transform; //parent new parent to parent parent
        hallwayParents = newHallwayParents; //update reference

        GameObject[][] newHallwayFloors = new GameObject[hallwayCount][]; //create new array to fill with previous array content
        for (int hallwayIndex = 0; hallwayIndex < hallwayFloors.Length; hallwayIndex++){ newHallwayFloors[hallwayIndex] = hallwayFloors[hallwayIndex]; } //copy existing hallways
        newHallwayFloors[hallwayCount - 1] = new GameObject[0]; //initialize new hallway's floor array
        hallwayFloors = newHallwayFloors; //update reference

        GameObject[][] newHallwayWalls = new GameObject[hallwayCount][]; //create new array to fill with previous array content
        for (int hallwayIndex = 0; hallwayIndex < hallwayWalls.Length; hallwayIndex++){ newHallwayWalls[hallwayIndex] = hallwayWalls[hallwayIndex]; } //copy existing hallways
        newHallwayWalls[hallwayCount - 1] = new GameObject[0]; //initialize new hallway's wall array
        hallwayWalls = newHallwayWalls; //update reference

        wallIndex = 0; //reset wall index for new hallway


        //begin path generation
        yield return StartCoroutine(GeneratePath(FindPath()));
        
        hallwayParentIndex++; //increase parent Index
        //Debug.Log("PathGeneration: Finished PathGeneration");
    }


    private Vector2[] FindPath()
    {
        //Debug.Log("PathGeneration: FindPath");
        //find connections between rooms using A*
        if (dbugEnabled) { MG.UpdateHUDDbugText("Dungeon Generation: Finding Path"); }

        int startPosX = (int)startPos.x; //beginning position
        int startPosZ = (int)startPos.y;
        int targetPosX = (int)targetPos.x; //targetted room position
        int targetPosZ = (int)targetPos.y; //cast to int to avoid issues with floats

        bool[,] openSet = new bool[boundsX, boundsZ]; //availible for path finding
        bool[,] closedSet = new bool[boundsX, boundsZ]; //closed from path finding
        Vector2[,] previousPos = new Vector2[boundsX, boundsZ]; //previous grid positions
        int[,] costToNext = new int[boundsX, boundsZ]; //cheapest cost to next position
        int[,] pathToEnd = new int[boundsX, boundsZ]; //cheapest cost path to end position

        string startPosState = MG.GetGridState(startPosX, startPosZ); //beginning position state
        string targetPosState = MG.GetGridState(targetPosX, targetPosZ);
        int startRoomIndex = int.Parse(startPosState.Substring(4)); //starting room Index
        int targetRoomIndex = int.Parse(targetPosState.Substring(4)); //targetted room Index
        if (dbugEnabled) { MG.UpdateHUDDbugText("Dungeon Generation: Finding Path from Room " + startRoomIndex + " to Room " + targetRoomIndex); }


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

        int iteration = 0;
        while (true) //loop until path is found
        {
            iteration++;
            //Debug.Log("PathGeneration: finding lowest cost position, " + iteration);
            Vector2 curPos = FindLowestCostPosition(openSet, pathToEnd, targetPos); //look through the grid to find the lowest cost position

            if (curPos.x == -1 && curPos.y == -1) { /*Debug.Log("PathGeneration: no path");*/ return null; } //if no lower costs found, return no path
            if (curPos == targetPos) { /*Debug.Log("PathGeneration: Finished FindPath");*/ return ConstructPath(previousPos, curPos); } //if current position is end position, build the path

            int curPosX = (int)curPos.x; //cast to int to avoid issues with floats
            int curPosZ = (int)curPos.y;
            
            //Debug.Log("PathGeneration: Processing position (" + curPosX + "," + curPosZ + ") - Cost to next: " + costToNext[curPosX, curPosZ] + " - Path to end: " + pathToEnd[curPosX, curPosZ]);

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
                    //Debug.Log("checking neighbor position x:" + neighborPosX + ", y:" + neighborPosZ);
                    //Debug.Log("grid state:" + gridStates[neighborPosX, neighborPosZ]);
                }*/

                if (neighborPosX < 0 || neighborPosX >= boundsX || neighborPosZ < 0 || neighborPosZ >= boundsZ) { continue; } //check if neighboring position is outside of area bounds

                int moveCost = GetMoveCost(neighborPosX, neighborPosZ); //finds the appropriate moving cost for that position
                //if (dbugEnabled) { //Debug.Log("move cost: " + moveCost);}
                if (moveCost == -1) { continue; } //if moving cost indicates a viable space

                if (closedSet[neighborPosX, neighborPosZ]) { continue; } //if position closed, skip

                if (dbugEnabled) { MG.UpdateDbugTileTextMoveCost(neighborPosX, neighborPosZ, moveCost); } //update checked tile debug text with new cost


                int tempCostToNext = costToNext[curPosX, curPosZ] + moveCost; //cost of current position
                if (tempCostToNext < costToNext[neighborPosX, neighborPosZ]) //if cost of current position is less than cost of reaching the neighbour position
                {
                    previousPos[neighborPosX, neighborPosZ] = curPos; //update the previous position with the current position
                    costToNext[neighborPosX, neighborPosZ] = tempCostToNext; //update neighbour cost with current cost

                    //update overall path with (neighbor cost + distance from target)
                    pathToEnd[neighborPosX, neighborPosZ] = costToNext[neighborPosX, neighborPosZ] + Heuristic(neighborPos, targetPos);
                    
                    //update open set with neighbor position
                    if (!openSet[neighborPosX, neighborPosZ]) { /*if (dbugEnabled) { MG.UpdateHUDDbugText("update open set with " + neighborPos); } */ openSet[neighborPosX, neighborPosZ] = true; }
                }
            }
        }
    }

    private Vector2 FindLowestCostPosition(bool[,] openSet, int[,] pathToEnd, Vector2 targetPos)
    {
        //Debug.Log("PathGeneration: FindingLowestCostPosition");
        //if (dbugEnabled) { MG.UpdateHUDDbugText("PG, Find Lowest Cost Position"); }

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

        //Debug.Log("PathGeneration: Finished FindingLowestCostPosition");
        return lowestCostPos; //return lowest cost position
    }

    [SerializeField] private int wallCost = 50, wallCornerCost = 500, hallwayCost = 1, doorwayCost = 1, emptyCost = 15, roomCost = 1;
    private int GetMoveCost(int x, int z)
    {
        //Debug.Log("PG, GetMoveCost");
        //find movement cost of requested position
        //if (dbugEnabled) { MG.UpdateHUDDbugText("PG, Get Move Cost"); }
        //if (dbugEnabled) { MG.UpdateHUDDbugText("grid state: " + MG.GetGridState(x, z)); }
        int moveCost = -1;

        //return cost depending on position state
        switch (MG.GetGridState(x, z))
        {
            case "Wall": moveCost = wallCost; break; //high cost, path will avoid this if possible
            case "WallCorner": moveCost = wallCornerCost; break; //absurd cost, path will avoid this at all costs
            case "Hallway": moveCost = hallwayCost; break; //low cost, path will prioritise this
            case "Doorway": moveCost = doorwayCost; break;
            case "Empty": moveCost = emptyCost; break; //moderate cost, path will use if needed
            default:
                if (MG.GetGridState(x, z).StartsWith("Room")) { moveCost = roomCost; break; } //low cost, allows path to traverse rooms easily
                else { moveCost = -1; break; } //should never occur
        }

        //check if current checked tile is adjacent to a corner to avoid strange hallways
        if(scale == 2 || scale == 1) { if(IsAdjacentToWallCorner(x, z)) { moveCost = wallCornerCost; } }

        //Debug.Log("PG, Finished GetMoveCost");
        return moveCost;
    }
    private bool IsAdjacentToWallCorner(int x, int z)
    {
        //Debug.Log("PG, IsAdjacentToWallCorner");
        //check 8 adjacent neighbour positions
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if(xOffset == 0 && zOffset == 0) { continue; } //skip current tile

                int neighbourX = (x + xOffset);
                int neighbourZ = (z + zOffset);

                //if neighbour is wihtin bounds and is not a hallway
                if (neighbourX >= 0 && neighbourX < boundsX && neighbourZ >= 0 && neighbourZ < boundsZ)
                {
                    //Debug.Log("offset within bounds");
                    //if neighbour is a wall corner, return true
                    if (MG.GetGridState(neighbourX, neighbourZ) == "WallCorner") { /*Debug.Log("PG, wall corner found");*/ return true; }
                }
                //else { Debug.Log("offset out of bounds"); } //if neighbour is out of bounds, skip
            }
        }

        //Debug.Log("PG, no wall corner found");
        //if no wall corner adjacent, return false
        return false;
    }

    //calculate the shortest distance between start position and target position
    private int Heuristic(Vector2 startPos, Vector2 endPos) { return (int)Mathf.Sqrt(Mathf.Pow(startPos.x - endPos.x, 2) + Mathf.Pow(startPos.y - endPos.y, 2)); }

    private Vector2[] ConstructPath(Vector2[,] previousPos, Vector2 curPos)
    {
        //Debug.Log("PathGeneration: ConstructPath");
        //puts together an array of the path findings previous steps
        //if (dbugEnabled) { MG.UpdateHUDDbugText("PG, Construct Path"); }

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

        //Debug.Log("PathGeneration: Finished ConstructPath");
        return path;
    }

    private IEnumerator GeneratePath(Vector2[] path)
    {
        //Debug.Log("PathGeneration: GeneratePath");
        //update map manager with new path
        //if (dbugEnabled) { MG.UpdateHUDDbugText("PG, Generate Path between " + startPos + " & " + targetPos); }

        if (path == null) { yield return null; } //if there's no path, skip

        Vector2 hallwayStart = path[0]; //set start position
        Vector2 hallwayEnd = path[path.Length - 1]; //set end position
        //Debug.Log("PG. hallwayStart: " +  hallwayStart + ", hallwayEnd: " + hallwayEnd);

        int hallwaySectionIndex = 0;
        Vector2[] hallwaySectionPositions = new Vector2[1];

        //iterate through path array to create hallway
        for (int pathSection = 0; pathSection < (path.Length - 1); pathSection++)
        {
            //Debug.Log("PG. path section: " + pathSection + " / " + (path.Length - 1));
            Vector2 start = path[pathSection]; //set start position to current path section
            Vector2 end = path[(pathSection + 1)]; //set end position to next path section
            //Debug.Log("start: " + start + ", end: " + end);
            int startX = (int)start.x; //cast to int to avoid float issues 
            int startZ = (int)start.y;
            int endX = (int)end.x;
            int endZ = (int)end.y;
            //Debug.Log("start pos: " + startX + "." + startZ + ", end pos: " + endX + "." + endZ);


            for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
            {
                //Debug.Log("pos x: " + x);
                for (int z = Mathf.Min(startZ, endZ); z <= Mathf.Max(startZ, endZ); z++)
                {
                    //Debug.Log("pos z: " + z);
                    if (MG.GetGridState(x, z).Contains("Room"))
                    {
                        //Debug.Log("PG, path in room @ pos: " + x + ", " + z);
                        continue;
                    }
                    else if (MG.GetGridState(x, z) == "Empty")
                    {
                        //Debug.Log("PG, path in empty space @ pos: " + x + ", " + z);
                        //set grid states and update material for hallways
                        //if (dbugEnabled) { MG.UpdateHUDDbugText("setting position x:" + x + ", y:" + z + " as hallway"); }
                        MG.UpdateDbugTileMat(x, z, "Hallway");
                        MG.UpdateDbugTileTextGridState(x, z, "Hallway");
                        //Debug.Log("PG, updating with hallway @ pos: " + x + ", " + z);
                        MG.UpdateGridState(x, z, "Hallway"); //mark the grid position as a hallway

                        Vector2[] newHallwaySectionPositions = new Vector2[hallwaySectionPositions.Length + 1]; //new array with increased size
                        for (int sectionIndex = 0; sectionIndex < hallwaySectionPositions.Length; sectionIndex++) { newHallwaySectionPositions[sectionIndex] = hallwaySectionPositions[sectionIndex]; } //copy old array to new arary
                        hallwaySectionPositions = newHallwaySectionPositions; //replace old array with new arary

                        hallwaySectionPositions[hallwaySectionIndex] = new Vector2(x, z); //update last new array position
                        hallwaySectionIndex++; //increase index

                        //Debug.Log("PG, running neighbour pos check");
                        //check 8 adjacent neighbour positions
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int zOffset = -1; zOffset <= 1; zOffset++)
                            {
                                //Debug.Log("PG, checking offset " + xOffset + ", " + zOffset);
                                if (xOffset == 0 && zOffset == 0) { /*Debug.Log("skipping center");*/ continue; } //skip current tile

                                int neighbourX = (x + xOffset);
                                int neighbourZ = (z + zOffset);

                                if (neighbourX >= 0 && neighbourX < boundsX && neighbourZ >= 0 && neighbourZ < boundsZ) //if adjacent position is within bounds
                                {
                                    //Debug.Log("PG, offset within bounds");
                                    if (MG.GetGridState(neighbourX, neighbourZ) == "Empty") //if adjacent position is a wall
                                    {
                                        //Debug.Log("PG, neighbour pos in empty space @ pos: " + x + ", " + z);
                                        MG.UpdateDbugTileMat(neighbourX, neighbourZ, "Hallway"); //update material
                                        MG.UpdateDbugTileTextGridState(neighbourX, neighbourZ, "Hallway"); //update debug text
                                        //Debug.Log("PG, updating with hallway @ neighbour pos: " + neighbourX + ", " + neighbourZ);
                                        MG.UpdateGridState(neighbourX, neighbourZ, "Hallway"); //update grid state

                                        newHallwaySectionPositions = new Vector2[hallwaySectionPositions.Length + 1]; //new array with increased size
                                        for (int sectionIndex = 0; sectionIndex < hallwaySectionPositions.Length; sectionIndex++) { newHallwaySectionPositions[sectionIndex] = hallwaySectionPositions[sectionIndex]; } //copy old array to new arary
                                        hallwaySectionPositions = newHallwaySectionPositions; //replace old array with new arary

                                        hallwaySectionPositions[hallwaySectionIndex] = new Vector2(neighbourX, neighbourZ); //update last new array position
                                        hallwaySectionIndex++; //increase index
                                    }
                                }
                            }
                        }
                    }
                    else if (MG.GetGridState(x, z) == "Wall")
                    {
                        //Debug.Log("PG, path in wall @ pos: " + x + ", " + z);
                        //set grid states and update material for doorways
                        //if (dbugEnabled) { MG.UpdateHUDDbugText("setting position x:" + x + ", y:" + z + " as doorway"); }
                        MG.UpdateDbugTileMat(x, z, "Doorway");
                        MG.UpdateDbugTileTextGridState(x, z, "Doorway");
                        //Debug.Log("PG, updating with doorway @ pos: " + x + ", " + z);
                        MG.UpdateGridState(x, z, "Doorway"); //mark the grid position as a doorway

                        //assign adjacent positions to doorway as doorway edges
                        int offsetX = 0;
                        int offsetZ = 0;

                        if (start.x != end.x) //if path is moving horizontally
                        {
                            //Debug.Log("PG, path is moving horizontally");
                            offsetZ = 1; //check above and below
                        }
                        else //if path is moving vertically
                        {
                            //Debug.Log("PG, path is moving vertically");
                            offsetX = 1; //check left and right
                        }
                        //Debug.Log("checking offset " + offsetX + ", " + offsetZ);

                        //mark adjacent walls as doorway edges
                        for (int i = -1; i <= 1; i += 2) //loop for two directions (-1 and 1)
                        {
                            int neighbourX = x + (offsetX * i); //find adjacent position
                            int neighbourZ = z + (offsetZ * i); //if offset is 0, will have no effect

                            if (neighbourX >= 0 && neighbourX < boundsX && neighbourZ >= 0 && neighbourZ < boundsZ) //if adjacent position is within bounds
                            {
                                //Debug.Log("PG, offset within bounds");
                                if (MG.GetGridState(neighbourX, neighbourZ) == "Wall") //if adjacent position is a wall
                                {
                                    //Debug.Log("PG, neighbour pos in wall @ pos: " + neighbourX + ", " + neighbourZ);
                                    MG.UpdateDbugTileMat(neighbourX, neighbourZ, "DoorwayEdge"); //update material
                                    MG.UpdateDbugTileTextGridState(neighbourX, neighbourZ, "DoorwayEdge"); //update debug text
                                    //Debug.Log("PG, updating with doorway edge @ pos: " + neighbourX + ", " + neighbourZ);
                                    MG.UpdateGridState(neighbourX, neighbourZ, "DoorwayEdge"); //update grid state
                                }
                            }
                            //else { Debug.Log("offset out of bounds"); }
                        }
                    }
                    else if (MG.GetGridState(x, z) == "Hallway" || MG.GetGridState(x, z) == "Doorway")
                    {
                        //Debug.Log("PG, path in hallway or doorway space @ pos: " + x + ", " + z);
                        //Debug.Log("PG, running neighbour pos check");
                        //check 8 adjacent neighbour positions
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int zOffset = -1; zOffset <= 1; zOffset++)
                            {
                                //Debug.Log("PG, checking offset " + xOffset + ", " + zOffset);
                                if (xOffset == 0 && zOffset == 0) { /*Debug.Log("skipping center");*/ continue; } //skip current tile

                                int neighbourX = (x + xOffset);
                                int neighbourZ = (z + zOffset);

                                if (neighbourX >= 0 && neighbourX < boundsX && neighbourZ >= 0 && neighbourZ < boundsZ) //if adjacent position is within bounds
                                {
                                    //Debug.Log("PG, offset within bounds");
                                    if (MG.GetGridState(neighbourX, neighbourZ) == "Empty") //if adjacent position is a wall
                                    {
                                        //Debug.Log("PG, neighbour pos in empty space @ pos: " + x + ", " + z);
                                        MG.UpdateDbugTileMat(neighbourX, neighbourZ, "Hallway"); //update material
                                        MG.UpdateDbugTileTextGridState(neighbourX, neighbourZ, "Hallway"); //update debug text
                                        //Debug.Log("PG, updating with hallway @ neighbour pos: " + neighbourX + ", " + neighbourZ);
                                        MG.UpdateGridState(neighbourX, neighbourZ, "Hallway"); //update grid state

                                        Vector2[] newHallwaySectionPositions = new Vector2[hallwaySectionPositions.Length + 1]; //new array with increased size
                                        for (int sectionIndex = 0; sectionIndex < hallwaySectionPositions.Length; sectionIndex++) { newHallwaySectionPositions[sectionIndex] = hallwaySectionPositions[sectionIndex]; } //copy old array to new arary
                                        hallwaySectionPositions = newHallwaySectionPositions; //replace old array with new arary

                                        hallwaySectionPositions[hallwaySectionIndex] = new Vector2(neighbourX, neighbourZ); //update last new array position
                                        hallwaySectionIndex++; //increase index
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        //Debug.Log("PG, path in ??? @ pos: " + x + ", " + z);
                        //Debug.Log("PG, position is: " + MG.GetGridState(x, z));
                    }
                }
            }

            //Debug.Log("PG. Finished path section: " + pathSection + " / " + (path.Length - 1));
            if (visualDemo) { yield return new WaitForSeconds(.1f); }
            else { yield return null; }
        }

        //Debug.Log("PathGeneration: Finished GeneratePath");
        hallwayPaths[hallwayParentIndex] = hallwaySectionPositions;
    }

    
    public IEnumerator CreateHallways()
    {
        //Debug.Log("PathGeneration: CreateHallways");
        hallwayParentIndex = 0; //reset hallway parent index

        //for each hallway previously identified
        for(int hallwayIndex = 0; hallwayIndex < hallwayPaths.Count; hallwayIndex++)
        {
            CreateHallway();
            hallwayParentIndex++;
            if (visualDemo) { yield return new WaitForSeconds(.1f); }
            else { yield return null; }
        }

        //Debug.Log("PathGeneration: Finished CreateHallways");
    }
    private void CreateHallway() //called for each hallway section
    {
        //Debug.Log("PathGeneration: CreateHallway");
        //for each section in hallway
        for (int sectionIndex = 0; sectionIndex < (hallwayPaths[hallwayParentIndex].Length - 1); sectionIndex++)
        {
            int xPos = (int)hallwayPaths[hallwayParentIndex][sectionIndex].x;
            int zPos = (int)hallwayPaths[hallwayParentIndex][sectionIndex].y;


            //create main hallway floor
            GameObject hallwayFloor = Instantiate(hallwayFloorPrefab, new Vector3(xPos, -0.5f, zPos), Quaternion.identity);
            hallwayFloor.transform.parent = hallwayParents[hallwayParentIndex].transform;
            hallwayFloor.name = "Hallway" + hallwayParentIndex + "Floor" + sectionIndex;

            //increase floor array size
            int hallwayIndex = hallwayCount - 1; //find current hallway index
            int oldSzie = hallwayWalls[hallwayIndex].Length; //find previous array length

            GameObject[] newFloors = new GameObject[(oldSzie + 1)]; //create new array +1 larger than previous
            for(int floorIndex = 0; floorIndex < oldSzie; floorIndex++){ newFloors[floorIndex] = hallwayFloors[hallwayIndex][floorIndex]; } //fill new array with old content

            newFloors[oldSzie] = hallwayFloor; //add new floor section to end of new array
            hallwayFloors[hallwayIndex] = newFloors; //update old array with new array

            floorIndex++; //increase floor section index
            


            //create hallway walls
            for(int adjacentIndex = 0; adjacentIndex < 4; adjacentIndex++)
            {
                int xOffset = 0, zOffset = 0, wallRotation = 0;
                Vector2 spawnOffset = new Vector2(0, 0);
                switch(adjacentIndex)
                {
                    case 0: //right
                        xOffset = 1;
                        zOffset = 0;
                        wallRotation = 180;
                        spawnOffset = new Vector2(-0.5f, 0);
                        break;
                    case 1: //left
                        xOffset = -1;
                        zOffset = 0;
                        wallRotation = 0;
                        spawnOffset = new Vector2(0.5f, 0);
                        break;
                    case 2: //up
                        xOffset = 0;
                        zOffset = 1;
                        wallRotation = 90;
                        spawnOffset = new Vector2(0, -0.5f);
                        break;
                    case 3: //down
                        xOffset = 0;
                        zOffset = -1;
                        wallRotation = -90;
                        spawnOffset = new Vector2(0, 0.5f);
                        break;
                }
                
                //Debug.Log("x: " + (xPos+xOffset) + ", z: " + (zPos+zOffset));
                //if tile is empty, create wall
                if(MG.GetGridState(xPos + xOffset, zPos + zOffset) == "Empty")
                {
                    //create additional hallway floors
                    hallwayFloor = Instantiate(hallwayFloorPrefab, new Vector3(xPos + xOffset, -0.5f, zPos + zOffset), Quaternion.identity);
                    hallwayFloor.transform.parent = hallwayParents[hallwayParentIndex].transform;
                    hallwayFloor.name = "Hallway" + hallwayParentIndex + "Floor" + sectionIndex + "Adjacent" + wallIndex;

                    //create wall section and add to array
                    GameObject hallwayWall = Instantiate(hallwayWallPrefab, new Vector3(xPos + xOffset + spawnOffset.x, 0, zPos + zOffset + spawnOffset.y), Quaternion.identity);
                    hallwayWall.transform.parent = hallwayParents[hallwayParentIndex].transform;
                    hallwayWall.name = "Hallway" + hallwayParentIndex + "Floor" + sectionIndex + "Wall" + wallIndex;
                    hallwayWall.transform.Rotate(0, wallRotation, 0, Space.Self);

                    //increase wall array size
                    GameObject[] newWalls = new GameObject[(oldSzie + 1)]; //create new array +1 larger than previous
                    for(int wallIndex = 0; wallIndex < oldSzie; wallIndex++){ newWalls[wallIndex] = hallwayWalls[hallwayIndex][wallIndex]; } //fill new array with old content

                    newWalls[oldSzie] = hallwayWall; //add new wall section to end of new array
                    hallwayWalls[hallwayIndex] = newWalls; //update old array with new array

                    wallIndex++; //increase wall section index
                }
            }
        }

        //Debug.Log("PathGeneration: Finished CreateHallway");
    }



    public void ResetHallways()
    {
        Destroy(hallwayParent);

        //reset the arrays
        hallwayPaths = new Dictionary<int, Vector2[]>();
        hallwayFloors = new GameObject[0][];
        hallwayWalls = new GameObject[0][];
        hallwayParents = new GameObject[0];

        //reset indexs
        hallwayParentIndex = 0;
        hallwayCount = 0;
        floorIndex = 0;
        wallIndex = 0;
    }
}