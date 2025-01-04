using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BACKUP_Dungeon_MapGeneration : MonoBehaviour
{
    //debug info
    [Header("Debug Materials")]
    [SerializeField] private Material baseDbugMat;
    [SerializeField] private Material emptyDbugMat, roomDbugMat, wallDbugMat, hallwayDbugMat, doorwayDbugMat;
    [SerializeField] private Material roomBossDbugMat, roomEntryDbugMat, roomTreasureDbugMat, roomSpecialDbugMat;

    [Header("Debug Objects")]
    [SerializeField] private GameObject testTile;
    [SerializeField] private TMP_Text dbugText;
    private GameObject[,] gridDbug; //grid squares [x bounds, z bounds] (ie. 0,0 = tile 1   25,25 = tile 625)
    private TMP_Text[,] gridDbugText;
    private GameObject dbugParent;
    private int genAttempts = 1;


    //map generation data
    [Header("Generation Data")]
    [SerializeField] [Range(100, 150)] private int mapBoundsMax = 100;
    [SerializeField] [Range(50, 100)] private int mapBoundsMin = 100;
    [SerializeField] [Range(3, 5)] private int treasureRoomsMax = 3;
    [SerializeField] [Range(1, 3)] private int treasureRoomsMin = 3;
    [SerializeField] [Range(3, 5)] private int specialRoomsMax = 3;
    [SerializeField] [Range(1, 3)] private int specialRoomsMin = 3;

    //map creation
    private int boundsX, boundsZ; //map generation
    private int totalSpace, numOfRooms, posNumOfRooms, curRoomsSpawned = 0; //room generation
    private int largeRoomNum, mediumRoomNum, smallRoomNum; //number of room sizes

    //map grid
    private string[,] gridStates; //what fills the grid square, if anything
    private Vector2[,] gridPositions; //literal position

    //room identification
    private int bossRoomID, entryRoomID; //important room ids
    private int[] treasureRoomIDs, specialRoomIDs; //unique room ids
    private int numOfTreasureRooms, numOfSpecialRooms; //number of unique rooms
    Vector2 boundsCenter, bossRoomCenter, entryRoomCenter;
    Vector2[] treasureRoomCenters, specialRoomCenters;
    private Vector2[] roomPositions; //bottom left corners of rooms


    //room info
    [Header("Room Objects")]
    [SerializeField] private GameObject basicRoom;
    private GameObject[] roomObjects;
    private int[] roomPosX, roomPosZ, roomBoundsX, roomBoundsZ;
    private string[] roomStates, roomScales;


    //dungeon types
    private string dungeonType;
    private string[] dungeonTypes = { "Crypt", "Goblin", "Bandit", "Ruins", "Wizard" };

    //general room types
    private string[] roomTypeLarge = { "Crypts", "Library", "ThroneRoom", "Prison",  "Temple", "Armoury", "Forge", "TrophyRoom", "DiningHall", "AudienceChamber", "RitualChamber", "FightingPit", "TrainingGrounds", "GardenRoom", "BallRoom" };
    private string[] roomTypeMedium = { "Crypts", "Chapel", "Barracks", "TortureRoom","StorageRoom", "Study", "Infirmary", "Kennel", "Workshop", "MapRoom", "ServantsQuarters", "Laboratory", "SummoningChamber", "RelicRoom", "Antechamber" };
    private string[] roomTypeSmall = { "Crypts", "Shrine", "PortalNook", "Storage", "Lavatory", "Pantry", "GuardPost", "Cell", "Alcove", "TrapRoom", "SpiderDen", "RubbleRoom", "FungusNook", "IllusionRoom", "SecretRoom" };
                                       //0       //1       //2           //3        //4         //5       //6          //7     //8       //9         //10         //11          //12          //13            //14     
    private Dictionary<string, Dictionary<int, List<int>>> validRoomTypes = new Dictionary<string, Dictionary<int, List<int>>>();
    private Dictionary<string, List<int>> validSmallRoomTypes = new Dictionary<string, List<int>>();
    private int[] usedTypeLargeIDs, usedTypeMediumIDs;

    //POI room types
    private string[] treasureRoomType = { "Melee", "Range", "Magic", "Armour"}; //spawns rewards
    private string[] specialRoomType = { "Food", "Potion", "Enchantment", "Gold", "Gem" }; //spawns helpful resources
    private int[] usedSpecialRoomIDs, usedTreasureRoomIDs;
    private int specialRoomsFound = 0, treasureRoomsFound = 0;



    public void RegenerateDungeon() { ResetDungeon(); GenerateDungeon(); }
    public void ResetDungeon()
    {
        //Debug.Log("Dungeon resetting");

        //clear the room objects
        for (int i = 0; i < numOfRooms; i++) { if (roomObjects[i] != null) { Destroy(roomObjects[i]); } }

        //clear the debug grid
        for (int x = 0; x < boundsX; x++) { for (int z = 0; z < boundsZ; z++) { if (gridDbug[x, z] != null) { Destroy(gridDbug[x, z]); } } }

        //clear the debug parent
        if (dbugParent != null) { if (dbugParent != null) { Destroy(dbugParent); } }

        //reset grid states and positions
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (gridStates[x, z] != null) { gridStates[x, z] = "Empty"; }
                if (gridPositions[x, z] != null) { gridPositions[x, z] = new Vector2(((testTile.transform.localScale.x + 0.1f) * x), ((testTile.transform.localScale.z + 0.1f) * z)); }
            }
        }

        //reset room positions and bounds
        for (int i = 0; i < numOfRooms; i++)
        {
            if (roomPositions[i] != null) { roomPositions[i] = Vector2.zero; }
            if (roomPosX[i] != 0) { roomPosX[i] = -1; }
            if (roomPosZ[i] != 0) { roomPosZ[i] = -1; }
            if (roomBoundsX[i] != 0) { roomBoundsX[i] = -1; }
            if (roomBoundsZ[i] != 0) { roomBoundsZ[i] = -1; }
            if (roomStates[i] != null) { roomStates[i] = null; }
        }

        //reset room type trackers
        for (int i = 0; i < usedTypeLargeIDs.Length; i++) { if (usedTypeLargeIDs[i] != 0) { usedTypeLargeIDs[i] = -1; } }
        for (int i = 0; i < usedTypeMediumIDs.Length; i++) { if (usedTypeMediumIDs[i] != 0) { usedTypeMediumIDs[i] = -1; } }
        for (int i = 0; i < usedSpecialRoomIDs.Length; i++) { if (usedSpecialRoomIDs[i] != 0) { usedSpecialRoomIDs[i] = -1; } }
        for (int i = 0; i < usedTreasureRoomIDs.Length; i++) { if (usedTreasureRoomIDs[i] != 0) { usedTreasureRoomIDs[i] = -1; } }

        //reset debug text
        if (dbugText != null) { dbugText.text = ""; }


        curRoomsSpawned = 0;
        largeRoomNum = 0;
        mediumRoomNum = 0;
        smallRoomNum = 0;
        curRoomsSpawned = 0;
        posNumOfRooms = 0;
        numOfRooms = 0;
        totalSpace = 0;
        boundsX = 0;
        boundsZ = 0;
        specialRoomsFound = 0;
        treasureRoomsFound = 0;
    }



    void Awake() 
    { 
        //Debug.Log("Generator awake"); 
        InitialiseRoomTypes(); 
        InitialiseSmallRoomTypes(); 
        GenerateDungeon(); 
    }
    private void InitialiseRoomTypes()
    {
        //Debug.Log("Initialising room types");

        validRoomTypes.Add("Crypt", new Dictionary<int, List<int>>
        {
            //scale         //valid room ids
            {2, new List<int>{ 0 } },
            {1, new List<int>{ 0, 13 } },
        });

        validRoomTypes.Add("Bandit", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 2, 4, 6, 7, 8, 9, 11, 12 } },
            { 1, new List<int> { 1, 2, 3, 5, 6, 7, 8, 9, 10 } },
        });

        validRoomTypes.Add("Goblin", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 2, 4, 6, 8, 10, 11, 12 } },
            { 1, new List<int> { 2, 3, 7, 8, 11, 12 } },
        });

        validRoomTypes.Add("Ruins", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 2, 4, 6, 7, 8, 9, 10, 13, 14 } },
            { 1, new List<int> { 1, 2, 5, 6, 10, 11, 12, 13, 14 } },
        });

        validRoomTypes.Add("Wizard", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 9, 10, 13 } },
            { 1, new List<int> { 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 } },
        });

    }
    private void InitialiseSmallRoomTypes()
    {
        //Debug.Log("Initialising small room types");

                              //room type                   //valid small rooms
        validSmallRoomTypes.Add("Crypts", new List<int> { 0 });
        validSmallRoomTypes.Add("Library", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("ThroneRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Prison", new List<int> { 7 });
        validSmallRoomTypes.Add("Temple", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Armoury", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Forge", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("TrophyRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("DiningHall", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("AudienceChamber", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("RitualChamber", new List<int> { 1, 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("FightingPit", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("TrainingGrounds", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("GardenRoom", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("BallRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });

        validSmallRoomTypes.Add("Chapel", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Barracks", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("TortureRoom", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("StorageRoom", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Study", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Infirmary", new List<int> { 1, 2, 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Kennel", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Workshop", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("MapRoom", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("ServantsQuarters", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Laboratory", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("SummoningChamber", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("RelicRoom", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Antechamber", new List<int> { 3, 4, 8, 9, 10, 11, 12 });

        validSmallRoomTypes.Add("Food", new List<int> { 5 });
        validSmallRoomTypes.Add("Potion", new List<int> { 2, 3, 11, 12 });
        validSmallRoomTypes.Add("Enchantment", new List<int> { 2, 3, 11, 12 });
        validSmallRoomTypes.Add("Gold", new List<int> { 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Gem", new List<int> { 3, 8, 9, 10, 11, 12 });
        validSmallRoomTypes.Add("Boss", new List<int> { 1, 3, 6, 8 });
        validSmallRoomTypes.Add("Entry", new List<int> { 3, 4, 5, 6, 8, 10, 11, 12 });
    }

    private void GenerateDungeon()
    {
        Debug.Log("New dungeon generation: " + genAttempts);
        genAttempts++;

        dungeonType = "Bandit";//dungeonTypes[Random.Range(0, 2)];
        //Debug.Log("dungeon type: " + dungeonType);

        //foundation
        DefineBounds();
        DefineGrid();

        //floor plan
        DetermineNumOfRooms();
                
        PlotRooms();
        ReinitialiseWithNewLimit();
        Debug.Log("L: " + largeRoomNum + ", M: " + mediumRoomNum + ", S: " + smallRoomNum + ", TOTAL: " + (largeRoomNum + mediumRoomNum + smallRoomNum) + "/" + numOfRooms);

        DefineImportantRooms();
        DefineRoomTypes();

        //build
        GenerateRooms();
    }



    private void DefineBounds()
    {
        Debug.Log("Defining dungeon bounds");

        //define bounds
        boundsX = Random.Range(mapBoundsMin, mapBoundsMax);   //x extent
        boundsZ = (int)(boundsX * 1.25f);   //z extent
        totalSpace = (boundsX * boundsZ); //interior mass
        //Debug.Log("X*Z = total");
        //Debug.Log(boundsX + "*" + boundsZ + " = " + totalSpace);
    }

    private void DefineGrid()
    {
        Debug.Log("Defining dungeon grid");

        //define grid & debug grid
        gridStates = new string[boundsX, boundsZ];      //states of grid positions
        gridPositions = new Vector2[boundsX, boundsZ];  //in world grid positions
        gridDbug = new GameObject[boundsX, boundsZ];    //game object array for debug grid
        gridDbugText = new TMP_Text[boundsX, boundsZ];  //text components attached to debug grid objects
        dbugParent = new GameObject("DebugParent");     //debug parent for debug grid objects

        //Debug.Log("boundsX:" + boundsX + ", boundsZ:" + boundsZ);
        //for each grid x
        for (int xPos = 0; xPos < boundsX; xPos++)
        {
            //for each grid z
            for (int zPos = 0; zPos < boundsZ; zPos++)
            {
                //Debug.Log("x: " + xPos + ", z: " + zPos);
                //Debug.Log("creating tile " + (pos + 1) + " at x:" + tileXOffset * xPos + ", z:" + tileZOffset * ZPos);
                gridPositions[xPos, zPos] = new Vector2(((testTile.transform.localScale.x + 0.1f) * xPos), ((testTile.transform.localScale.z + 0.1f) * zPos));
                gridStates[xPos, zPos] = "Empty";
                //Debug.Log("grid pos: " + gridPositions[xPos, zPos]);

                //debug
                gridDbug[xPos, zPos] = Instantiate(testTile, new Vector3(gridPositions[xPos, zPos].x, 0, gridPositions[xPos, zPos].y), Quaternion.identity);
                gridDbug[xPos, zPos].name = "DbugTile" + ConvertPosToPosID(new Vector2(xPos, zPos)); //name debug tiles with relevant position ID
                gridDbug[xPos, zPos].transform.parent = dbugParent.transform;
                gridDbugText[xPos, zPos] = gridDbug[xPos, zPos].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();

                Renderer curObjRend = gridDbug[xPos, zPos].GetComponent<Renderer>();
                curObjRend.material = emptyDbugMat;
            }
        }
    }

    private void DetermineNumOfRooms()
    {
        Debug.Log("Defining initial number of rooms");

        //determine number of rooms to be spawned
        posNumOfRooms = Random.Range(((totalSpace / boundsX) / 2), ((totalSpace / boundsZ) / 2));
        //numOfRooms = 5;
        //Debug.Log("number of rooms to spawn: " + posNumOfRooms);

        float tempLargeRoomNum = ((float)posNumOfRooms / 100) * 15;
        float tempMediumRoomNum = ((float)posNumOfRooms / 100) * 35;
        float tempSmallRoomNum = ((float)posNumOfRooms / 100) * 50;
        largeRoomNum = (int)tempLargeRoomNum;
        mediumRoomNum = (int)tempMediumRoomNum;
        smallRoomNum = (int)tempSmallRoomNum;
        //Debug.Log("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);

        numOfRooms = largeRoomNum + mediumRoomNum + smallRoomNum;
        //Debug.Log("new number of rooms to spawn: " + numOfRooms);

        roomPositions = new Vector2[numOfRooms];
        roomPosX = new int[numOfRooms];
        roomPosZ = new int[numOfRooms];
        roomBoundsX = new int[numOfRooms];
        roomBoundsZ = new int[numOfRooms];
        roomStates = new string[numOfRooms];
        roomScales = new string[numOfRooms];
        //Debug.Log("largest possible size of rooms: " + totalSpace / posNumOfRooms);
    }



    private void PlotRooms()
    {
        /*
        ~ set room bounds
        ~ select random position within map bounds
        ~ ensure cells are unoccupied
          ~ if cells are occupied and max attempts not reached, find a new position and recheck cells
          ~ if max attempts has been reached, set room bounds to be smaller and restart the cell check
        ~ update cells
        */

        Debug.Log("Plotting room positions");

        int scale = 2;

        //plot room positions within grid
        for (int roomSpawnAttempts = 0; roomSpawnAttempts < numOfRooms; roomSpawnAttempts++)
        {
            //Debug.Log("room " + curRoomsSpawned + ", " + roomSpawnAttempts + "/" + numOfRooms);

            if (scale == 2) //large rooms
            {
                //Debug.Log("large: " + curRoomsSpawned + "/" + largeRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 2), ((boundsX / 2) / 3));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 2), ((boundsZ / 2) / 3));
            }
            else if (scale == 1) //medium rooms
            {
                //break;
                //Debug.Log("medium: " + (curRoomsSpawned - largeRoomNum) + "/" + mediumRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 4), ((boundsX / 2) / 5));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 4), ((boundsZ / 2) / 5));
            }
            else if (scale == 0) //small rooms
            {
                //break;
                //Debug.Log("small: " + (curRoomsSpawned - (largeRoomNum + mediumRoomNum)) + "/" + smallRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 6), ((boundsX / 2) / 7));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 6), ((boundsZ / 2) / 7));
            }
            else { /*Debug.Log("scale past room gen");*/ break; } //no rooms found

            //roomBoundsX[curRoomsSpawned] = 7;
            //roomBoundsZ[curRoomsSpawned] = 4;
            //Debug.Log("room X:" + roomBoundsX + ", room Z: " + roomBoundsZ);
            int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                //Debug.Log("placement attempt " + attempts);

                //generate room positions
                if (scale == 0) //if making small rooms, find positions adjacent to large or medium rooms
                {
                    int targetRoomID = curRoomsSpawned - (largeRoomNum + mediumRoomNum);
                    int wallSide = Random.Range(0, 4);
                    //Debug.Log("(" + largeRoomNum + " + " + mediumRoomNum + ") - " + curRoomsSpawned + " = " + targetRoomID);

                    switch (wallSide)
                    {
                        case 0: //top wall
                            roomPosX[curRoomsSpawned] = Random.Range(roomPosX[targetRoomID], roomPosX[targetRoomID] + roomBoundsX[targetRoomID] - roomBoundsX[curRoomsSpawned]);
                            roomPosZ[curRoomsSpawned] = roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID];
                            break;
                        case 1: //bottom wall 
                            roomPosX[curRoomsSpawned] = Random.Range(roomPosX[targetRoomID], roomPosX[targetRoomID] + roomBoundsX[targetRoomID] - roomBoundsX[curRoomsSpawned]);
                            roomPosZ[curRoomsSpawned] = roomPosZ[targetRoomID] - roomBoundsZ[curRoomsSpawned];
                            break;
                        case 2: //left wall
                            roomPosX[curRoomsSpawned] = roomPosX[targetRoomID] - roomBoundsX[curRoomsSpawned];
                            roomPosZ[curRoomsSpawned] = Random.Range(roomPosZ[targetRoomID], roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID] - roomBoundsZ[curRoomsSpawned]);
                            break;
                        case 3: //right wall
                            roomPosX[curRoomsSpawned] = roomPosX[targetRoomID] + roomBoundsX[targetRoomID];
                            roomPosZ[curRoomsSpawned] = Random.Range(roomPosZ[targetRoomID], roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID] - roomBoundsZ[curRoomsSpawned]);
                            break;
                    }
                }
                else //otherwise, find a random position
                {
                    roomPosX[curRoomsSpawned] = Random.Range(0, boundsX - roomBoundsX[curRoomsSpawned] + 1);
                    roomPosZ[curRoomsSpawned] = Random.Range(0, boundsZ - roomBoundsZ[curRoomsSpawned] + 1);
                }
                //Debug.Log("x" + roomPosX[curRoomsSpawned] + ", z" + roomPosZ[curRoomsSpawned]);
                //Debug.Log("room pos: " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);

                //ensure reasonable distance and no overlap
                if (AreCellsUnoccupied(roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned], roomBoundsX[curRoomsSpawned], roomBoundsZ[curRoomsSpawned]))
                {
                    //Debug.Log("grid pos' clear, creating room @ " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);
                    roomPositions[curRoomsSpawned] = new Vector2(gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]].x, gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]].y);
                    break;
                }
                else
                {
                    //move room within limited area
                    bool placed = false;
                    for (int offsetX = -5; offsetX <= 5 && !placed; offsetX++)
                    {
                        for (int offsetZ = -5; offsetZ <= 5 && !placed; offsetZ++)
                        {
                            if (offsetX == 0 && offsetZ == 0) { continue; } //skip original position

                            int newPosX = roomPosX[curRoomsSpawned] + offsetX;
                            int newPosZ = roomPosZ[curRoomsSpawned] + offsetZ;
                            //Debug.Log(newPosX + "   " + (newPosX + roomBoundsX[curRoomsSpawned]) + "   / " + boundsX);
                            //Debug.Log(newPosZ + "   " + (newPosZ + roomBoundsZ[curRoomsSpawned]) + "   / " + boundsZ);

                            if (AreCellsUnoccupied(newPosX, newPosZ, roomBoundsX[curRoomsSpawned], roomBoundsZ[curRoomsSpawned]))
                            {
                                roomPosX[curRoomsSpawned] = newPosX;
                                roomPosZ[curRoomsSpawned] = newPosZ;
                                //Debug.Log("grid pos' clear, creating room @ " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);
                                roomPositions[curRoomsSpawned] = new Vector3(gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]].x, 0, gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]].y);
                                placed = true;
                                break;
                            }
                            
                        }

                        if (placed) { break; }
                    }

                    if (!placed) { attempts++; } //if still unable to place, iterate loop
                }
            }

            //Debug.Log(attempts + " v " + maxAttempts);
            if (attempts == maxAttempts) { /*Debug.Log("room" + curRoomsSpawned + " removed");*/ }
            else if (attempts != maxAttempts)
            {
                //update grid states
                //Debug.Log("room" + curRoomsSpawned + " added @ x" + roomPosX[curRoomsSpawned] + ", z" + roomPosZ[curRoomsSpawned]);
                for (int x = roomPosX[curRoomsSpawned]; x < (roomPosX[curRoomsSpawned] + roomBoundsX[curRoomsSpawned]); x++)
                {
                    for (int z = roomPosZ[curRoomsSpawned]; z < (roomPosZ[curRoomsSpawned] + roomBoundsZ[curRoomsSpawned]); z++)
                    {
                        //Debug.Log("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        roomStates[curRoomsSpawned] = "Empty";
                        gridStates[x, z] = "Room" + curRoomsSpawned;

                        gridDbugText[x, z].text = gridStates[x, z] + "\n2";
                        Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                        curObjRend.material = roomDbugMat;
                    }
                }

                switch(scale)
                {
                    case 2:
                        roomScales[curRoomsSpawned] = "Large";
                        break;
                    case 1:
                        roomScales[curRoomsSpawned] = "Medium";
                        break;
                    case 0:
                        roomScales[curRoomsSpawned] = "Small";
                        break;
                }

                curRoomsSpawned++;
            }


            //Debug.Log("current room spawn attempts: " + roomSpawnAttempts);
            if (roomSpawnAttempts == (largeRoomNum - 1))
            {
                largeRoomNum = curRoomsSpawned;

                DefineWalls(curRoomsSpawned, scale);
                DefineHallways(curRoomsSpawned, scale);

                scale--; 
                //Debug.Log("lowering scale, " + scale);
                //Debug.Log("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
            }
            else if (roomSpawnAttempts == ((largeRoomNum + mediumRoomNum) - 1))
            {
                mediumRoomNum = curRoomsSpawned - largeRoomNum;

                DefineWalls(curRoomsSpawned, scale);
                DefineHallways(curRoomsSpawned, scale);

                scale--;
                //Debug.Log("lowering scale, " + scale);
                //Debug.Log("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
            }
            else if (roomSpawnAttempts == ((largeRoomNum + mediumRoomNum + smallRoomNum) - 1))
            {
                smallRoomNum = curRoomsSpawned - (largeRoomNum + mediumRoomNum);

                DefineWalls(curRoomsSpawned, scale);
                DefineHallways(curRoomsSpawned, scale);

                scale--;
                //Debug.Log("lowering scale, " + scale);
                //Debug.Log("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
                //Debug.Log("total: " + curRoomsSpawned);
                break;
            }
        }
    }

    private bool AreCellsUnoccupied(int posX, int posZ, int roomBoundsX, int roomBoundsZ)
    {
        if (posX < 0 || posX + roomBoundsX > boundsX || posZ < 0 || posZ + roomBoundsZ > boundsZ)
        {
            return false;
        }
        for (int x = posX; x < (posX + roomBoundsX); x++)
        {
            for (int z = posZ; z < (posZ + roomBoundsZ); z++)
            {
                //Debug.Log("grid pos @ X: " + x + ", Z: " + z);
                if (gridStates[x, z] != "Empty") { /*Debug.Log("grid pos @ X: " + x + ", Z: " + z + " not empty");*/ return false; }
            }
        }
        return true;
    }



    private void ReinitialiseWithNewLimit()
    {
        Debug.Log("Reinitialising variables with new number of rooms");

        numOfRooms = curRoomsSpawned;
        //Debug.Log("new number of rooms: " + numOfRooms);

        usedTypeLargeIDs = new int[largeRoomNum];
        usedTypeMediumIDs = new int[mediumRoomNum];
        usedSpecialRoomIDs = new int[specialRoomType.Length];
        usedTreasureRoomIDs = new int[treasureRoomType.Length];
        roomObjects = new GameObject[numOfRooms];

        for (int i = 0; i < largeRoomNum; i++) { usedTypeLargeIDs[i] = -1; }
        for (int i = 0; i < mediumRoomNum; i++) { usedTypeMediumIDs[i] = -1; }

        Vector2[] tempRoomPositions = roomPositions;
        roomPositions = new Vector2[numOfRooms];

        int[] tempRoomPosX = roomPosX;
        int[] tempRoomPosZ = roomPosZ;
        roomPosX = new int[numOfRooms];
        roomPosZ = new int[numOfRooms];

        int[] tempRoomBoundsX = roomBoundsX;
        int[] tempRoomBoundsZ = roomBoundsZ;
        roomBoundsX = new int[numOfRooms];
        roomBoundsZ = new int[numOfRooms];

        string[] tempRoomStates = roomStates;
        roomStates = new string[numOfRooms];

        string[] tempRoomSizes = roomScales;
        roomScales = new string[numOfRooms];

        for (int i = 0; i < numOfRooms; i++)
        {
            roomPositions[i] = tempRoomPositions[i];

            roomPosX[i] = tempRoomPosX[i];
            roomPosZ[i] = tempRoomPosZ[i];

            roomBoundsX[i] = tempRoomBoundsX[i];
            roomBoundsZ[i] = tempRoomBoundsZ[i];

            roomStates[i] = tempRoomStates[i];

            roomScales[i] = tempRoomSizes[i];
        }
    }



    private void DefineImportantRooms()
    {
        //set room types
        //find large room furthest away from bounds center to define as Boss room
        //find medium or small room furthest away from Boss room to define as Entry room
        //find small rooms furthest from Boss & Entry & other Treasure Rooms to define as Treasure Rooms
        //find medium or small rooms furthest from Boss & Entry & Treasure Rooms & other Special Rooms to define as Special Rooms
        Debug.Log("Defining important rooms");

        FindBossRoom();
        FindEntryRoom();
        FindTreasureRooms();
        FindSpecialRooms();
    }

    private void FindBossRoom()
    {
        Debug.Log("finding boss room");

        //find boss room
        boundsCenter = new Vector2((boundsX / 2), (boundsZ / 2));
        //Debug.Log(boundsCenter);
        float distFromCenter = 0;
        bossRoomID = -1;

        for (int roomID = 0; roomID < largeRoomNum; roomID++) //for each large room
        {
            //init room center and distance from bounds center
            Vector2 roomCenter = new Vector2((roomBoundsX[roomID] / 2) + roomPositions[roomID].x, (roomBoundsZ[roomID] / 2) + roomPositions[roomID].y);
            float distRoomToCenter = Vector2.Distance(boundsCenter, roomCenter);
            //Debug.Log("roomID: " + roomID + " - " + roomCenter + ", " + distRoomToCenter);

            //if room further than last nearest
            if (distRoomToCenter > distFromCenter)
            {
                //update trackers
                distFromCenter = distRoomToCenter;
                bossRoomID = roomID;
                //Debug.Log("new boss room id: " + bossRoomID);
            }
        }

        if (bossRoomID != -1) //if boss room found
        {
            //Debug.Log("boss room id: " + bossRoomID);

            //update room state tracker
            roomStates[bossRoomID] = "Boss";

            //for each pos inside the room
            for (int x = roomPosX[bossRoomID]; x < (roomPosX[bossRoomID] + roomBoundsX[bossRoomID]); x++)
            {
                for (int z = roomPosZ[bossRoomID]; z < (roomPosZ[bossRoomID] + roomBoundsZ[bossRoomID]); z++)
                {
                    //only if pos is empty room
                    if (gridStates[x, z] != "Wall" && gridStates[x, z] != "Doorway" && gridStates[x, z] != "WallCorner")
                    {
                        //update grid state, debug text, and debug material
                        //Debug.Log("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        gridStates[x, z] = "BossRoom";

                        gridDbugText[x, z].text = gridStates[x, z];
                        Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                        curObjRend.material = roomBossDbugMat;
                    }
                }
            }
        }
    }

    private void FindEntryRoom()
    {
        Debug.Log("finding entry room");

        //find entry room
        bossRoomCenter = new Vector2((roomBoundsX[bossRoomID] / 2) + roomPositions[bossRoomID].x, (roomBoundsZ[bossRoomID] / 2) + roomPositions[bossRoomID].y);
        float distFromBoss = 0;
        entryRoomID = -1;

        for (int roomID = largeRoomNum; roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++) //for each medium and small room
        {
            //init room center and distance from boss room center
            Vector2 roomCenter = new Vector2((roomBoundsX[roomID] / 2) + roomPositions[roomID].x, (roomBoundsZ[roomID] / 2) + roomPositions[roomID].y);
            float distRoomToBoss = Vector2.Distance(bossRoomCenter, roomCenter);
            //Debug.Log("roomID: " + roomID + " - " + roomCenter + ", " + distRoomToBoss);

            //if room further than last nearest
            if (distRoomToBoss > distFromBoss)
            {
                //update trackers
                distFromBoss = distRoomToBoss;
                entryRoomID = roomID;
                //Debug.Log("new entry room id: " + entryRoomID);
            }
        }

        if (entryRoomID != -1) //if entry room found
        {
            //Debug.Log("entry room id: " + entryRoomID);

            //update room state tracker
            roomStates[entryRoomID] = "Entry";

            //for each pos inside the room
            for (int x = roomPosX[entryRoomID]; x < (roomPosX[entryRoomID] + roomBoundsX[entryRoomID]); x++)
            {
                for (int z = roomPosZ[entryRoomID]; z < (roomPosZ[entryRoomID] + roomBoundsZ[entryRoomID]); z++)
                {
                    //only if pos is empty room
                    if (gridStates[x, z] != "Wall" && gridStates[x, z] != "Doorway" && gridStates[x, z] != "WallCorner")
                    {
                        //update grid state, debug text, and debug material
                        //Debug.Log("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        gridStates[x, z] = "EntryRoom";

                        gridDbugText[x, z].text = gridStates[x, z];
                        Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                        curObjRend.material = roomEntryDbugMat;
                    }
                }
            }
        }
    }

    private void FindTreasureRooms()
    {
        Debug.Log("finding treasure rooms");

        //find treasure rooms
        entryRoomCenter = new Vector2((roomBoundsX[entryRoomID] / 2) + roomPositions[entryRoomID].x, (roomBoundsZ[entryRoomID] / 2) + roomPositions[entryRoomID].y);
        float distFromEntryAndBoss = 0;
        numOfTreasureRooms = Random.Range(treasureRoomsMin, treasureRoomsMax);
        treasureRoomIDs = new int[numOfTreasureRooms];
        treasureRoomCenters = new Vector2[numOfTreasureRooms];
        for (int i = 0; i < numOfTreasureRooms; i++) { treasureRoomIDs[i] = -1; } //init each treasure room id

        for (int tR = 0; tR < numOfTreasureRooms; tR++) //for each treasure room, find a room
        {
            //Debug.Log(tR + "/" + numOfTreasureRooms);

            for (int roomID = (largeRoomNum + mediumRoomNum); roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++) //for each small room
            {
                //init room center and distance from entry and boss room center
                Vector2 roomCenter = new Vector2((roomBoundsX[roomID] / 2) + roomPositions[roomID].x, (roomBoundsZ[roomID] / 2) + roomPositions[roomID].y);
                float distRoomToBoss = Vector2.Distance(bossRoomCenter, roomCenter); //distance from current room to boss room
                //Debug.Log(distRoomToBoss);
                if (distRoomToBoss < 25) { continue; } //if current room close to boss room, skip
                float distRoomToEntry = Vector2.Distance(entryRoomCenter, roomCenter); //distance from current room to entry room
                //Debug.Log(distRoomToEntry);
                if (distRoomToEntry < 25) { continue; } //if current room close to entry room, skip
                float distFromRoomToEntryAndBoss = (distRoomToBoss + distRoomToEntry); //combined distance

                for (int i = 0; i < treasureRoomIDs.Length; i++) //for each treasure room
                {
                    //if the room has been designated
                    if (treasureRoomIDs[i] != -1)
                    {
                        //Debug.Log(treasureRoomIDs[i]);
                        float distRoomToTreasaure = Vector2.Distance(treasureRoomCenters[i], roomCenter); //distance from current room to current treasure room
                        //Debug.Log(distRoomToTreasaure);
                        if (distRoomToTreasaure < 25) { continue; } //if current room close to current treasure room, skip
                        distFromRoomToEntryAndBoss += distRoomToTreasaure; //otherwise, add distance to combination
                    }
                }
                //Debug.Log("roomID: " + roomID + " - " + roomCenter + ", " + distFromRoomToEntryAndBoss);

                //if room further than last nearest & room is empty
                if (distFromRoomToEntryAndBoss > distFromEntryAndBoss && roomStates[roomID] == "Empty")
                {
                    //Debug.Log(distFromRoomToEntryAndBoss + " > " + distFromEntryAndBoss);
                    //update trackers
                    distFromEntryAndBoss = distFromRoomToEntryAndBoss;
                    treasureRoomIDs[tR] = roomID;
                    //Debug.Log("new treasure room id: " + treasureRoomIDs[tR]);
                }
            }

            if (treasureRoomIDs[tR] != -1) //if treasure room found
            {
                //Debug.Log("treasure room id: " + treasureRoomIDs[tR]);

                //update room state tracker
                roomStates[treasureRoomIDs[tR]] = "Treasure"; 
                treasureRoomCenters[tR] = new Vector2((roomBoundsX[treasureRoomIDs[tR]] / 2) + roomPositions[treasureRoomIDs[tR]].x, (roomBoundsZ[treasureRoomIDs[tR]] / 2) + roomPositions[treasureRoomIDs[tR]].y);

                //for each pos inside the room
                for (int x = roomPosX[treasureRoomIDs[tR]]; x < (roomPosX[treasureRoomIDs[tR]] + roomBoundsX[treasureRoomIDs[tR]]); x++)
                {
                    for (int z = roomPosZ[treasureRoomIDs[tR]]; z < (roomPosZ[treasureRoomIDs[tR]] + roomBoundsZ[treasureRoomIDs[tR]]); z++)
                    {
                        //only if pos is empty room
                        if (gridStates[x, z] != "Wall" && gridStates[x, z] != "Doorway" && gridStates[x, z] != "WallCorner")
                        {
                            //update grid state, debug text, and debug material
                            //Debug.Log("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                            gridStates[x, z] = "TreasureRoom";

                            gridDbugText[x, z].text = gridStates[x, z];
                            Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                            curObjRend.material = roomTreasureDbugMat;
                        }
                    }
                }

                distFromEntryAndBoss = 0;
            }
        }
    }

    private void FindSpecialRooms()
    {
        Debug.Log("finding special room");

        //find special rooms
        float distFromEntryAndBoss = 0;
        numOfSpecialRooms = Random.Range(specialRoomsMin, specialRoomsMax);
        specialRoomIDs = new int[numOfSpecialRooms];
        specialRoomCenters = new Vector2[numOfSpecialRooms];
        for (int i = 0; i < numOfSpecialRooms; i++) { specialRoomIDs[i] = -1; } //init each special room id

        for (int sR = 0; sR < numOfSpecialRooms; sR++) //for each special room, find a room
        {
            //Debug.Log(sR + "/" + numOfSpecialRooms);

            for (int roomID = largeRoomNum; roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++) //for each medium and small room
            {
                //init room center and distance from entry and boss room center
                Vector2 roomCenter = new Vector2((roomBoundsX[roomID] / 2) + roomPositions[roomID].x, (roomBoundsZ[roomID] / 2) + roomPositions[roomID].y);
                float distRoomToBoss = Vector2.Distance(bossRoomCenter, roomCenter); //distance from current room to boss room
                //Debug.Log(distRoomToBoss);
                if (distRoomToBoss < 25) { continue; } //if current room close to boss room, skip
                float distRoomToEntry = Vector2.Distance(entryRoomCenter, roomCenter); //distance from current room to entry room
                //Debug.Log(distRoomToEntry);
                if (distRoomToEntry < 25) { continue; } //if current room close to entry room, skip
                float distFromRoomToEntryAndBoss = (distRoomToBoss + distRoomToEntry); //combined distance

                for (int i = 0; i < treasureRoomIDs.Length; i++) //for each treasure room
                {
                    //if the room has been designated
                    if (treasureRoomIDs[i] != -1)
                    {
                        //Debug.Log(treasureRoomIDs[i]);
                        float distRoomToTreasure = Vector2.Distance(treasureRoomCenters[i], roomCenter); //distance from current room to current treasure room
                        //Debug.Log(distRoomToTreasure);
                        if (distRoomToTreasure < 25) { continue; } //if current room close to current treasure room, skip
                        distFromRoomToEntryAndBoss += distRoomToTreasure; //otherwise, add distance to combination
                    }
                }

                for (int i = 0; i < specialRoomIDs.Length; i++) //for each special room
                {
                    //if the room has been designated
                    if (specialRoomIDs[i] != -1)
                    {
                        //Debug.Log(specialRoomIDs[i]);
                        float distRoomToSpecial = Vector2.Distance(specialRoomCenters[i], roomCenter); //distance from current room to current special room
                        //Debug.Log(distRoomToSpecial);
                        if (distRoomToSpecial < 25) { continue; } //if current room close to current special room, skip
                        distFromRoomToEntryAndBoss += distRoomToSpecial; //otherwise, add distance to combination
                    }
                }
                //Debug.Log("roomID: " + roomID + " - " + roomCenter + ", " + distFromRoomToEntryAndBoss);

                //if room further than last nearest & room is empty
                if (distFromRoomToEntryAndBoss > distFromEntryAndBoss && roomStates[roomID] == "Empty")
                {
                    //Debug.Log(distFromRoomToEntryAndBoss + " > " + distFromEntryAndBoss);
                    //update trackers
                    distFromEntryAndBoss = distFromRoomToEntryAndBoss;
                    specialRoomIDs[sR] = roomID;
                    //Debug.Log("new special room id: " + specialRoomIDs[sR]);
                }
            }

            if (specialRoomIDs[sR] != -1) //if special room found
            {
                //Debug.Log("special room id: " + specialRoomIDs[sR]);

                //update room state tracker
                roomStates[specialRoomIDs[sR]] = "Special";
                specialRoomCenters[sR] = new Vector2((roomBoundsX[specialRoomIDs[sR]] / 2) + roomPositions[specialRoomIDs[sR]].x, (roomBoundsZ[specialRoomIDs[sR]] / 2) + roomPositions[specialRoomIDs[sR]].y);

                //for each pos inside the room
                for (int x = roomPosX[specialRoomIDs[sR]]; x < (roomPosX[specialRoomIDs[sR]] + roomBoundsX[specialRoomIDs[sR]]); x++)
                {
                    for (int z = roomPosZ[specialRoomIDs[sR]]; z < (roomPosZ[specialRoomIDs[sR]] + roomBoundsZ[specialRoomIDs[sR]]); z++)
                    {
                        //only if pos is empty room
                        if (gridStates[x, z] != "Wall" && gridStates[x, z] != "Doorway" && gridStates[x, z] != "WallCorner")
                        {
                            //update grid state, debug text, and debug material
                            //Debug.Log("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                            gridStates[x, z] = "SpecialRoom";

                            gridDbugText[x, z].text = gridStates[x, z];
                            Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                            curObjRend.material = roomSpecialDbugMat;
                        }
                    }
                }

                distFromEntryAndBoss = 0;
            }
        }
    }



    private void DefineRoomTypes()
    {
        Debug.Log("defining room types");

        //if specific dungeon type, layout specific
        //otherwise, assign appropriate rooms

        if (dungeonType == "Crypt")
        {
            Debug.Log("Assigning crypts");
            //assign all crypt rooms
            for (int roomID = 0; roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++)
            {
                //select random room
                bool valid = false;
                int roomTypeID = 0; //find random room type

                while (!valid) //while room invalid
                {
                    valid = IsRoomValid(2, roomTypeID, roomID); //set valid to room check
                    //Debug.Log("valid: " + valid);

                    if (valid) { break; }
                    else if (!valid) { roomTypeID = 0; } //find new random room type, then loop
                }

                string roomStateTemp = roomStates[roomID];
                roomStates[roomID] = roomTypeLarge[roomTypeID] + roomStateTemp;
                //Debug.Log("crypt room" + roomID + " set as " + roomTypeLarge[roomTypeID]);
            }
        }
        else
        {
            //Debug.Log("dungeon type: " + dungeonType);
            string futureBossRoomType = "";
            for (int scale = 2; scale >= 0; scale--) //for each size of room
            {
                //Debug.Log("scale: " + scale);
                if (scale == 2)
                {
                    Debug.Log("Assigning " + largeRoomNum + " large rooms");
                    //assign large room types
                    for (int roomID = 0; roomID < largeRoomNum; roomID++)
                    {
                        Debug.Log(roomID + "/" + usedTypeLargeIDs.Length);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int roomTypeID = Random.Range(1, roomTypeLarge.Length); //find random room type

                        while (inc == 0) { inc = Random.Range(-1, 1); }

                        while (!valid) //while room invalid
                        {
                            //Debug.Log("room type id: " + roomTypeID);
                            valid = IsRoomValid(scale, roomTypeID, roomID); //set valid to room check
                            //Debug.Log("valid: " + valid);

                            if (valid) { break; }
                            else if (!valid) //find unused room type, then loop
                            {
                                roomTypeID += inc; //increment to find the next room type
                                if (inc == 1 && roomTypeID >= roomTypeLarge.Length)
                                {
                                    roomTypeID = 0; //wrap around if exceeding array length
                                }
                                else if (inc == -1 && roomTypeID <= 0)
                                {
                                    roomTypeID = (roomTypeLarge.Length - 1);
                                }
                            }
                        }

                        if (roomStates[roomID] == "Empty") { roomStates[roomID] = roomTypeLarge[roomTypeID]; }
                        else if(roomStates[roomID] == "Boss") { futureBossRoomType = roomTypeLarge[roomTypeID]; continue; }
                        else { string roomStateTemp = roomStates[roomID]; roomStates[roomID] = roomStateTemp + roomTypeLarge[roomTypeID]; }
                        usedTypeLargeIDs[roomID] = roomTypeID;
                        //Debug.Log("room " + roomID + ": large room " + roomID + "/" + largeRoomNum + " set as " + roomTypeLarge[roomTypeID]);
                    }
                }
                else if (scale == 1)
                {
                    Debug.Log("Assigning " + mediumRoomNum + " medium rooms");
                    //assign medium room types
                    for (int roomID = largeRoomNum; roomID < (largeRoomNum + mediumRoomNum); roomID++)
                    {
                        Debug.Log((roomID - largeRoomNum) + "/" + usedTypeMediumIDs.Length);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int roomTypeID = Random.Range(1, roomTypeMedium.Length); //find random room type

                        while(inc == 0) { inc = Random.Range(-1, 1); }

                        if (roomStates[roomID] == "Special")
                        {
                            //Debug.Log("special medium room");
                            roomTypeID = Random.Range(0, specialRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                //Debug.Log("room type id: " + roomTypeID);
                                valid = IsRoomValid(scale, roomTypeID, roomID); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= specialRoomType.Length)
                                    {
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID <= 0)
                                    {
                                        roomTypeID = (specialRoomType.Length - 1);
                                    }
                                }
                            }

                            roomStates[roomID] = specialRoomType[roomTypeID];
                            usedSpecialRoomIDs[specialRoomsFound] = roomTypeID;
                            usedTypeMediumIDs[(roomID - largeRoomNum)] = 20;
                            //Debug.Log("room " + roomID + ": medium special room " + (roomID - largeRoomNum) + "/" + mediumRoomNum + " set as " + specialRoomType[roomTypeID]);
                            specialRoomsFound++;
                            continue;
                        }
                        else if(roomStates[roomID] == "Entry")
                        {
                            //Debug.Log("room " + roomID + ": medium room is already " + roomStates[roomID]);
                            Debug.Log("parent room: " + (roomID - largeRoomNum));
                            usedTypeMediumIDs[(roomID - largeRoomNum)] = 21; 
                            continue; 
                        }
                        else
                        {
                            while (!valid) //while room invalid
                            {
                                //Debug.Log("room type id: " + roomTypeID);
                                valid = IsRoomValid(scale, roomTypeID, roomID); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= roomTypeMedium.Length)
                                    {
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID <= 0)
                                    {
                                        roomTypeID = (roomTypeMedium.Length - 1);
                                    }
                                }
                            }

                            if (roomStates[roomID] == "Empty") { roomStates[roomID] = roomTypeMedium[roomTypeID]; }
                            else { string roomStateTemp = roomStates[roomID]; roomStates[roomID] = roomTypeMedium[roomTypeID]; }
                            usedTypeMediumIDs[(roomID - largeRoomNum)] = roomTypeID;
                            //Debug.Log("room " + roomID + ": medium room " + (roomID - largeRoomNum) + "/" + mediumRoomNum + " set as " + roomTypeMedium[roomTypeID]);
                        }
                    }
                }
                else if (scale == 0)
                {
                    Debug.Log("Assigning " + smallRoomNum + " small rooms");
                    //assign small room types
                    for (int roomID = (largeRoomNum + mediumRoomNum); roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++)
                    {
                        Debug.Log((roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int roomTypeID = Random.Range(1, roomTypeSmall.Length); //find random room type

                        while (inc == 0) { inc = Random.Range(-1, 1); }

                        if (roomStates[roomID] == "Special")
                        {
                            //Debug.Log("special medium room");
                            roomTypeID = Random.Range(0, specialRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                //Debug.Log("room type id: " + roomTypeID);
                                valid = IsRoomValid(scale, roomTypeID, roomID); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= specialRoomType.Length)
                                    {
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID <= 0)
                                    {
                                        roomTypeID = (specialRoomType.Length - 1);
                                    }
                                }
                            }

                            roomStates[roomID] = specialRoomType[roomTypeID];
                            usedSpecialRoomIDs[specialRoomsFound] = roomTypeID;
                            //Debug.Log("room " + roomID + ": small special room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + specialRoomType[roomTypeID]);
                            specialRoomsFound++;
                            continue;
                        }
                        else if (roomStates[roomID] == "Treasure")
                        {
                            //Debug.Log("treasure small room");
                            roomTypeID = Random.Range(0, treasureRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                valid = IsRoomValid(scale, roomTypeID, roomID); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= treasureRoomType.Length)
                                    {
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID <= 0)
                                    {
                                        roomTypeID = (treasureRoomType.Length - 1);
                                    }
                                }
                            }

                            roomStates[roomID] = treasureRoomType[roomTypeID];
                            usedTreasureRoomIDs[treasureRoomsFound] = roomTypeID;
                            //Debug.Log("room " + roomID + ": small treasure room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + treasureRoomType[roomTypeID]);
                            treasureRoomsFound++;
                            continue;
                        }
                        else if(roomStates[roomID] == "Entry")
                        {
                            //Debug.Log("room " + roomID + ": small room is already " + roomStates[roomID]);
                            Debug.Log("parent room: " + (roomID - (largeRoomNum + mediumRoomNum)));
                            roomStates[roomID - (largeRoomNum + mediumRoomNum)] = "Antechamber";
                            Debug.Log((roomID - (largeRoomNum + mediumRoomNum)) + "/" + smallRoomNum);
                            continue;
                        }
                        else
                        {
                            //select random room
                            string parentRoomType = roomStates[(roomID - (largeRoomNum + mediumRoomNum))]; //find related room type
                            string roomStateTemp = "";
                            //Debug.Log("room" + roomID + " parent room type: " + parentRoomType);

                            switch (parentRoomType)
                            {
                                case "Entry":
                                    //Debug.Log("room " + roomID + ": medium room is already " + roomStates[roomID]);
                                    break;
                                case "Crypts":
                                case "Library":
                                case "ThroneRoom":
                                case "Prison":
                                case "Temple":
                                case "Armoury":
                                case "Forge":
                                case "TrophyRoom":
                                case "DiningHall":
                                case "AudienceChamber":
                                case "RitualChamber":
                                case "FightingPit":
                                case "TrainingGrounds":
                                case "GardenRoom":
                                case "BallRoom":

                                case "Chapel":
                                case "Barracks":
                                case "TortureRoom":
                                case "StorageRoom":
                                case "Study":
                                case "Infirmary":
                                case "Kennel":
                                case "Workshop":
                                case "MapRoom":
                                case "ServantsQuarters":
                                case "Laboratory":
                                case "SummoningChamber":
                                case "RelicRoom":
                                case "Antechamber":

                                case "Food":
                                case "Potion":
                                case "Enchantment":
                                case "Gold":
                                case "Gem":
                                case "Boss":
                                    if (validSmallRoomTypes.TryGetValue(parentRoomType, out List<int> validSmallRoomTypesForParent))
                                    {
                                        roomTypeID = Random.Range(1, roomTypeSmall.Length);
                                        while (!validSmallRoomTypesForParent.Contains(roomTypeID))
                                        {
                                            //Debug.Log("room type ID: " + roomTypeID);
                                            //Debug.Log("valid: " + validSmallRoomTypesForParent.Contains(roomTypeID));
                                            //for(int i = 0; i < validSmallRoomTypesForParent.Count; i++) { Debug.Log("valid small room type " + i + ": " + validSmallRoomTypesForParent[i]); }

                                            roomTypeID += inc; //increment to find the next room type
                                            if (inc == 1 && roomTypeID >= roomTypeSmall.Length)
                                            {
                                                roomTypeID = 0; //wrap around if exceeding array length
                                            }
                                            else if(inc == -1 && roomTypeID <= 0)
                                            {
                                                roomTypeID = (roomTypeSmall.Length - 1);
        }
                                        }

                                        if(roomTypeSmall[roomTypeID] == "Storage" && roomStates[(roomID - (largeRoomNum + mediumRoomNum))] == "Boss")
                                        {
                                            roomStateTemp = futureBossRoomType;
                                            roomStates[roomID] = roomStateTemp + roomTypeSmall[roomTypeID];
                                        }
                                        else if (roomTypeSmall[roomTypeID] == "Storage")
                                        {
                                            roomStateTemp = parentRoomType;
                                            roomStates[roomID] = roomStateTemp + roomTypeSmall[roomTypeID];
                                        }
                                        else { roomStates[roomID] = roomTypeSmall[roomTypeID]; }

                                        //Debug.Log("room " + roomID + ": small room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + roomTypeSmall[roomTypeID]);
                                    }
                                    break;
                                default:
                                    //Debug.Log("creating secret room");
                                    roomStateTemp = roomStates[(roomID - (largeRoomNum + mediumRoomNum))];
                                    roomTypeID = 13; //illusion room
                                    roomStates[(roomID - (largeRoomNum + mediumRoomNum))] = roomStateTemp + roomTypeSmall[roomTypeID];
                                    //Debug.Log("room " + (roomID - (largeRoomNum + mediumRoomNum)) + ": small room number " + (roomID - (largeRoomNum + mediumRoomNum)) + "/" + smallRoomNum + " set as " + roomTypeSmall[roomTypeID]);
                                    roomTypeID = 14; //secret room
                                    roomStates[roomID] = roomTypeSmall[roomTypeID];
                                    //Debug.Log("room " + roomID + ": small room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + roomTypeSmall[roomTypeID]);
                                    break;
                            }
                        }
                    }

                    break;
                }
                else { break; }
            }

            string bossRoomStateTemp = roomStates[bossRoomID]; 
            roomStates[bossRoomID] = bossRoomStateTemp + futureBossRoomType;
        }
    }
    private bool IsRoomValid(int scale, int typeID, int roomID)
    {
        //Debug.Log("checking if room is valid - scale: " + scale + ", room type id: " + typeID);

        if (roomStates[roomID] == "Special") //if room id is special
        {
            //for each special room type
            for (int i = 0; i < specialRoomType.Length; i++)
            {
                if (usedSpecialRoomIDs[i] == typeID) { return false; } //if type id has been used, return false
                else if (i == specialRoomType.Length - 1) { return true; } //if loop has reached end, return true
            }

            return true; //break out true
        }
        else if (roomStates[roomID] == "Treasure")
        {
            //Debug.Log("type id: " + typeID);
            for (int i = 0; i < treasureRoomType.Length; i++)
            {
                //Debug.Log("used treasure room ids: " + usedTreasureRoomIDs[i]);
                if (usedTreasureRoomIDs[i] == typeID) { return false; }
                else if (i == treasureRoomType.Length - 1) { return true; }
            }

            return true;
        }

        switch(scale)
        {
            case 2:
                if (validRoomTypes.TryGetValue(dungeonType, out Dictionary<int, List<int>> validLargeRoomsForDungeonType)) //get dungeon type dictionary
                {
                    if (validLargeRoomsForDungeonType.TryGetValue(scale, out List<int> validLargeRoomIDsForScale)) //get scale dictionary
                    {
                        for (int i = 0; i < largeRoomNum; i++)
                        {
                            //Debug.Log("used room type " + i + ": " + usedTypeLargeIDs[i]);
                            if (usedTypeLargeIDs[i] == -1) { return true; }
                            else if (usedTypeLargeIDs[i] == typeID) { return false; }
                            else if (i == largeRoomNum - 1) { return true; }
                        }
                    }
                }

                return false;

            case 1:
                if(validRoomTypes.TryGetValue(dungeonType, out Dictionary<int, List<int>> validMediumRoomsForDungeonType)) //get dungeon type dictionary
                {
                    if (validMediumRoomsForDungeonType.TryGetValue(scale, out List<int> validMediumRoomIDsForScale)) //get scale dictionary
                    {
                        for (int i = 0; i < mediumRoomNum; i++) //for each room type
                        {
                            //Debug.Log("used room type " + i + ": " + usedTypeMediumIDs[i]);
                            if(usedTypeMediumIDs[i] == -1) { return true; }
                            else if (usedTypeMediumIDs[i] == typeID) { return false; }
                            else if (i == mediumRoomNum - 1) { return true; }
                        }
                    }
                }

                return false;
        }

        return false;
    }



    private void DefineWalls(int roomsToWall, int scale)
    {
        //Debug.Log("Adding walls");

        /*
         * get edges of rooms
         * set grid position state to wall
         * update material colour with half hue
         */

        //Debug.Log("adding walls");
        int roomIDStart = 0;
        if(scale == 1) { roomIDStart = largeRoomNum; }
        else if(scale == 0) { roomIDStart = (largeRoomNum + mediumRoomNum); }
        //Debug.Log("scale: " + scale);
        //Debug.Log("total roomsToWall: " + (roomsToWall - roomIDStart));
        //Debug.Log("roomIDStart: " + roomIDStart + ", roomIDEnd: " + roomsToWall);

        //Debug.Log(largeRoomNum + " " + mediumRoomNum + " " + smallRoomNum);
        //Debug.Log(roomIDStart + " " + roomsToWall);
        for (int roomID = roomIDStart; roomID < roomsToWall; roomID++)//for each room
        {
            //Debug.Log("adding walls to room" + roomID + " @ x" + roomPosX[roomID] + ", z" + roomPosZ[roomID]);
            int newX = -1;
            int newZ = -1;

            for (int x = roomPosX[roomID]; x < (roomPosX[roomID] + roomBoundsX[roomID]); x++)//top row of wall
            {
                newX = x;
                newZ = roomPosZ[roomID] + (roomBoundsZ[roomID] - 1);
                //Debug.Log("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (x == roomPosX[roomID] || x == roomPosX[roomID] + roomBoundsX[roomID] - 1)
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;
                    Color newObjColour = curObjRend.material.color / 4;
                    curObjRend.material.color = newObjColour;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\n100";
                    gridStates[newX, newZ] = "WallCorner";
                }
                else if (gridStates[newX, newZ] != "Wall")
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\nWall\n10";
                    gridStates[newX, newZ] = "Wall";
                }
            }
            for (int x = roomPosX[roomID]; x < (roomPosX[roomID] + roomBoundsX[roomID]); x++)//bottom row of wall
            {
                newX = x;
                newZ = roomPosZ[roomID];
                //Debug.Log("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (x == roomPosX[roomID] || x == roomPosX[roomID] + roomBoundsX[roomID] - 1)
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;
                    Color newObjColour = curObjRend.material.color / 4;
                    curObjRend.material.color = newObjColour;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\n100";
                    gridStates[newX, newZ] = "WallCorner";
                }
                else if (gridStates[newX, newZ] != "Wall")
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\nWall\n10";
                    gridStates[newX, newZ] = "Wall";
                }
            }

            for (int z = roomPosZ[roomID]; z < (roomPosZ[roomID] + roomBoundsZ[roomID]); z++)//left row of wall
            {
                newX = roomPosX[roomID];
                newZ = z;
                //Debug.Log("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (z == roomPosZ[roomID] || z == roomPosZ[roomID] + roomBoundsZ[roomID] - 1)
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;
                    Color newObjColour = curObjRend.material.color / 4;
                    curObjRend.material.color = newObjColour;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\n100";
                    gridStates[newX, newZ] = "WallCorner";
                }
                else if (gridStates[newX, newZ] != "Wall")
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\nWall\n10";
                    gridStates[newX, newZ] = "Wall";
                }
            }
            for (int z = roomPosZ[roomID]; z < (roomPosZ[roomID] + roomBoundsZ[roomID]); z++)//right row of wall
            {
                newX = roomPosX[roomID] + (roomBoundsX[roomID] - 1);
                newZ = z;
                //Debug.Log("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (z == roomPosZ[roomID] || z == roomPosZ[roomID] + roomBoundsZ[roomID] - 1)
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;
                    Color newObjColour = curObjRend.material.color / 4;
                    curObjRend.material.color = newObjColour;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\n100";
                    gridStates[newX, newZ] = "WallCorner";
                }
                else if (gridStates[newX, newZ] != "Wall")
                {
                    Renderer curObjRend = gridDbug[newX, newZ].GetComponent<Renderer>();
                    curObjRend.material = wallDbugMat;

                    gridDbugText[newX, newZ].text = gridStates[newX, newZ] + "\nWall\n10";
                    gridStates[newX, newZ] = "Wall";
                }
            }

        }
    }



    private void DefineHallways(int roomsToConnect, int scale)
    {
        //Debug.Log("adding hallways");

        //define rooms to be connected using A* and Prims
        if (scale == 2)
        {
            //for each large room, connect it to each other large room - creating primary hallways
            for (int roomID = 0; roomID < roomsToConnect; roomID++)
            {
                //Debug.Log("cur:" + roomID);

                //initialise tracking vars
                Vector2 bestStartPos = Vector2.zero;
                Vector2 bestTargetPos = Vector2.zero;

                for (int targetRoomID = roomID + 1; targetRoomID < roomsToConnect; targetRoomID++) //for each other room
                {
                    //if the current room and next room are different IDs
                    if (roomID != targetRoomID)
                    {
                        //Debug.Log("cur:" + roomID + ", target:" + targetRoomID);
                        //calculate distance
                        Vector2 curRoomCenter = new Vector2(roomPosX[roomID] + roomBoundsX[roomID] / 2, roomPosZ[roomID] + roomBoundsZ[roomID] / 2);
                        Vector2 nextRoomCenter = new Vector2(roomPosX[targetRoomID] + roomBoundsX[targetRoomID] / 2, roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID] / 2);
                        float distance = Vector2.Distance(curRoomCenter, nextRoomCenter);

                        Vector2 startPos = new Vector2(roomPosX[roomID] + roomBoundsX[roomID] / 2, roomPosZ[roomID] + roomBoundsZ[roomID] / 2);
                        Vector2 targetPos = new Vector2(roomPosX[targetRoomID] + roomBoundsX[targetRoomID] / 2, roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID] / 2);

                        //Debug.Log("joining cur:" + roomID + " & target:" + targetRoomID);

                        GenerateHallway(FindPath(startPos, targetPos));
                    }
                }
            }
        }
        else if (scale == 1)
        {
            //for each medium room, connect it to the nearest large room
            for (int roomID = largeRoomNum; roomID < roomsToConnect; roomID++)
            {
                //Debug.Log("cur:" + roomID);

                //initialise tracking vars
                int nearestRoomID = -1;
                float minDistance = float.MaxValue;
                Vector2 bestStartPos = Vector2.zero;
                Vector2 bestTargetPos = Vector2.zero;

                for (int targetRoomID = 0; targetRoomID < largeRoomNum; targetRoomID++)
                {
                    //if the current room and next room are different IDs
                    if (roomID != targetRoomID)
                    {
                        //Debug.Log("cur:" + roomID + ", target:" + targetRoomID);
                        //calculate distance
                        Vector2 curRoomCenter = new Vector2(roomPosX[roomID] + roomBoundsX[roomID] / 2, roomPosZ[roomID] + roomBoundsZ[roomID] / 2);
                        Vector2 nextRoomCenter = new Vector2(roomPosX[targetRoomID] + roomBoundsX[targetRoomID] / 2, roomPosZ[targetRoomID] + roomBoundsZ[targetRoomID] / 2);
                        float distance = Vector2.Distance(curRoomCenter, nextRoomCenter);

                        //if calculated distance is less than the minimum distance
                        if (distance < minDistance)
                        {
                            minDistance = distance; //update minimum distance
                            nearestRoomID = targetRoomID; //update selected room ID
                        }
                    }
                }

                //generate the hallway between medium room and closest room
                if (nearestRoomID != -1)
                {
                    Vector2 startPos = new Vector2(roomPosX[roomID] + roomBoundsX[roomID] / 2, roomPosZ[roomID] + roomBoundsZ[roomID] / 2);
                    Vector2 targetPos = new Vector2(roomPosX[nearestRoomID] + roomBoundsX[nearestRoomID] / 2, roomPosZ[nearestRoomID] + roomBoundsZ[nearestRoomID] / 2);

                    //Debug.Log("joining cur:" + roomID + " & target:" + nearestRoomID);

                    GenerateHallway(FindPath(startPos, targetPos));
                }
            }
        }
        else if (scale == 0)
        {
            for (int roomID = (largeRoomNum + mediumRoomNum); roomID < roomsToConnect; roomID++)
            {
                //connect each small room to its parent
                int parentRoomID = roomID - (largeRoomNum + mediumRoomNum);
                Vector2 startPos = new Vector2(roomPosX[roomID] + roomBoundsX[roomID] / 2, roomPosZ[roomID] + roomBoundsZ[roomID] / 2);
                Vector2 targetPos = new Vector2(roomPosX[parentRoomID] + roomBoundsX[parentRoomID] / 2, roomPosZ[parentRoomID] + roomBoundsZ[parentRoomID] / 2);
                //Debug.Log("start: " + startPos + ", target: " + targetPos);

                //Debug.Log("joining cur:" + roomID + " & target:" + parentRoomID);
                GenerateHallway(FindPath(startPos, targetPos));
            }
        }
        else { Debug.Log("scale past hallway gen"); }
    }

    private Vector2[] FindPath(Vector2 startPos, Vector2 targetPos)
    {
        //Debug.Log("finding path");

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
        //Debug.Log(gridStates[startPosX, startPosZ]);
        //Debug.Log("x:" + targetPosX + ", z:" + targetPosZ);
        //Debug.Log(gridStates[targetPosX, targetPosZ]);
        int startRoomID = int.Parse(gridStates[startPosX, startPosZ].Substring(4));
        int targetRoomID = int.Parse(gridStates[targetPosX, targetPosZ].Substring(4));
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
                if (moveCost == -1) { continue; }


                gridDbugText[neighborPosX, neighborPosZ].text = Regex.Replace(gridDbugText[neighborPosX, neighborPosZ].text, @"\d+$", moveCost.ToString());


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
        //Debug.Log("finding lowest cost positions");

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
        //Debug.Log("getting move cost");

        switch (gridStates[x, z])
        {
            case "Wall": return 50;
            case "WallCorner": return 500;
            case "Hallway": return 5;
            case "Doorway": return 5;
            case "Empty": return 25;
            default:
                if (gridStates[x, z].StartsWith("Room")) { return 5; }
                else { return -1; }
        }
    }

    //calculate the shortest distance between start position and target position
    private int Heuristic(Vector2 startPos, Vector2 endPos) { return (int)Mathf.Sqrt(Mathf.Pow(startPos.x - endPos.x, 2) + Mathf.Pow(startPos.y - endPos.y, 2)); }

    private int ConvertPosToPosID(Vector2 pos) //convert 2D position to grid position
    {
        //Debug.Log("vector2int: " + pos);
        //Debug.Log("ID: " + (pos.x * boundsZ + pos.y + 1));
        return ((int)pos.x * boundsZ + (int)pos.y) + 1;
    }

    private Vector2[] ConstructPath(Vector2[,] previousPos, Vector2 curPos)
    {
        //Debug.Log("constructing path");

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

    private void GenerateHallway(Vector2[] path)
    {
        //Debug.Log("generating hallway");

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
                    if (gridStates[x, z] == "Empty")
                    {
                        //Debug.Log("setting position x:" + x + ", y:" + z + " as hallway");
                        Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                        curObjRend.material = hallwayDbugMat;

                        gridStates[x, z] = "Hallway"; //mark the grid position as a hallway
                        gridDbugText[x, z].text = gridStates[x, z] + "\n15";
                    }
                    else if(gridStates[x, z] == "Wall")
                    {
                        //Debug.Log("setting position x:" + x + ", y:" + z + " as doorway");
                        Renderer curObjRend = gridDbug[x, z].GetComponent<Renderer>();
                        curObjRend.material = doorwayDbugMat;

                        gridStates[x, z] = "Doorway"; //mark the grid position as a doorway
                        gridDbugText[x, z].text = gridStates[x, z] + "\n15";
                    }
                }
            }
        }
    }



    private void GenerateRooms()
    {
        //Debug.Log("generating room");

        //generate rooms

        for (int roomID = 0; roomID < numOfRooms; roomID++)
        {
            //spawn room
            //Debug.Log("Spawning room " + roomID + " @ " + roomPositions[roomID] + ", size: " + roomBoundsX[roomID] + "*" + roomBoundsZ[roomID]);
            //Debug.Log("extents");
            //Debug.Log(/*top left*/(roomPositions[roomID] + new Vector2(0, 0, roomBoundsZ[roomID])) + "\t" + (roomPositions[roomID] + new Vector2(roomBoundsX[roomID], 0, roomBoundsZ[roomID]))/*top right*/);
            //Debug.Log(/*bottom left*/roomPositions[roomID] + "\t" + (roomPositions[roomID] + new Vector2(roomBoundsX[roomID], 0,0))/*bottom right*/);
            Vector3 instantiatePos = new Vector3(roomPositions[roomID].x, 0, roomPositions[roomID].y);
            roomObjects[roomID] = Instantiate(basicRoom, instantiatePos, Quaternion.identity);
            //roomObjects[roomID].GetComponent<RoomGeneration>().Wake(roomBoundsX[roomID], roomBoundsZ[roomID]);
            roomObjects[roomID].name = roomScales[roomID] + roomStates[roomID] + roomID;
        }
    }
}
