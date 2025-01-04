using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DungeonGeneration : MonoBehaviour
{
    //relevant scripts
    private MapManager MM;
    private PathGeneration PG;

    //generation data
    private Vector2 mapBoundsMax, mapBoundsMin;
    private int treasureRoomsMax, treasureRoomsMin;
    private int specialRoomsMax, specialRoomsMin;

    //map creation
    private int boundsX, boundsZ; //map generation
    private int totalSpace, numOfRooms, posNumOfRooms, curRoomsSpawned = 0, scale = 2; //room generation
    private int largeRoomNum, mediumRoomNum, smallRoomNum; //number of room sizes

    //map grid
    private Vector2[,] gridPositions; //literal position

    //room identification
    private int bossRoomID, entryRoomID; //important room ids
    private int[] treasureRoomIDs, specialRoomIDs; //unique room ids
    private int numOfTreasureRooms, numOfSpecialRooms; //number of unique rooms
    Vector2 boundsCenter, bossRoomCenter, entryRoomCenter;
    Vector2[] roomCenters, treasureRoomCenters, specialRoomCenters;
    private Vector2[] roomPositions; //bottom left corners of rooms

    //room info
    [Header("Room Objects")]
    [SerializeField] private GameObject basicRoom, dbugTile;
    private GameObject[] roomObjects;
    private int[] roomPosX, roomPosZ, roomBoundsX, roomBoundsZ;
    private string[] roomStates, roomScales;

    //dungeon types
    private string dungeonType;
    private Dictionary<string, Dictionary<int, List<int>>> dungeonTypes = new Dictionary<string, Dictionary<int, List<int>>>();

    //general room types
    private Dictionary<string, int> largeRoomTypes = new Dictionary<string, int>();
    private Dictionary<string, int> mediumRoomTypes = new Dictionary<string, int>(); 
    private string[] smallRoomTypes = { "Crypts", "Shrine", "PortalNook", "Storage", "Lavatory", "Pantry", "GuardPost", "Cell", "Alcove", "TrapRoom", "SpiderDen", "RubbleRoom", "FungusNook", "IllusionRoom", "SecretRoom" };
    private Dictionary<string, List<int>> parentRoomTypes = new Dictionary<string, List<int>>();
    private int[] usedTypeLargeIDs, usedTypeMediumIDs; 

    //POI room types
    private string[] treasureRoomType = { "Melee", "Range", "Magic", "Armour"}; //spawns rewards
    private string[] specialRoomType = { "Food", "Potion", "Enchantment", "Gold", "Gem" }; //spawns helpful resources
    private int[] usedSpecialRoomIDs, usedTreasureRoomIDs;
    private int specialRoomsFound = 0, treasureRoomsFound = 0;



    void Awake() 
    {
        MM = this.gameObject.GetComponent<MapManager>();
        PG = this.gameObject.GetComponent<PathGeneration>();

        MM.UpdateHUDDbugText("DG, Awake");

        InitialiseDungeonTypes();
        InitialiseLargeRoomTypes();
        InitialiseMediumRoomTypes();
        InitialiseSmallParentRoomTypes();
    }
    private void InitialiseDungeonTypes()
    {
        MM.UpdateHUDDbugText("DG, Initialising Room Types");

        dungeonTypes.Add("Crypt", new Dictionary<int, List<int>>
        {
            //scale, valid room ids
            {2, new List<int>{ 0 } },
            {1, new List<int>{ 0, 13 } },
        });

        dungeonTypes.Add("Bandit", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 2, 4, 6, 7, 8, 9, 11, 12 } },
            { 1, new List<int> { 1, 2, 3, 5, 6, 7, 8, 9, 10 } },
        });

        dungeonTypes.Add("Goblin", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 2, 4, 6, 8, 10, 11, 12 } },
            { 1, new List<int> { 2, 3, 7, 8, 11, 12 } },
        });

        dungeonTypes.Add("Ruins", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 2, 4, 6, 7, 8, 9, 10, 13, 14 } },
            { 1, new List<int> { 1, 2, 5, 6, 10, 11, 12, 13, 14 } },
        });

        dungeonTypes.Add("Wizard", new Dictionary<int, List<int>>
        {
            { 2, new List<int> { 1, 9, 10, 13 } },
            { 1, new List<int> { 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 } },
        });

    }
    private void InitialiseLargeRoomTypes()
    {
        MM.UpdateHUDDbugText("DG, Initialising Large Room Types");

        //room type, room focus'     0 - anywhere     1 - entry     2 - inbetween     3 - boss
        largeRoomTypes.Add("Crypts", 0); //0
        largeRoomTypes.Add("Library", 2); //1
        largeRoomTypes.Add("ThroneRoom", 3); //2
        largeRoomTypes.Add("Prison", 2); //3
        largeRoomTypes.Add("Temple", 1); //4
        largeRoomTypes.Add("Armoury", 2); //5
        largeRoomTypes.Add("Forge", 2); //6
        largeRoomTypes.Add("TrophyRoom", 2); //7
        largeRoomTypes.Add("DiningHall", 1); //8
        largeRoomTypes.Add("AudienceChamber", 1); //9
        largeRoomTypes.Add("RitualChamber", 2); //10
        largeRoomTypes.Add("FightingPit", 1); //11
        largeRoomTypes.Add("TrainingGrounds", 0); //12
        largeRoomTypes.Add("GardenRoom", 2); //13
        largeRoomTypes.Add("BallRoom", 3); //14
    }
    private void InitialiseMediumRoomTypes()
    {
        MM.UpdateHUDDbugText("DG, Initialising Medium Room Types");

        //room type, room focus'     0 - anywhere     1 - entry     2 - inbetween     3 - boss
        mediumRoomTypes.Add("Chapel", 1);
        mediumRoomTypes.Add("Barracks", 1);
        mediumRoomTypes.Add("TortureRoom", 3);
        mediumRoomTypes.Add("StorageRoom", 0);
        mediumRoomTypes.Add("Study", 1);
        mediumRoomTypes.Add("Infirmary", 2);
        mediumRoomTypes.Add("Kennel", 0);
        mediumRoomTypes.Add("Workshop", 2);
        mediumRoomTypes.Add("MapRoom", 2);
        mediumRoomTypes.Add("ServantsQuarters", 0);
        mediumRoomTypes.Add("Laboratory", 3);
        mediumRoomTypes.Add("SummoningChamber", 3);
        mediumRoomTypes.Add("RelicRoom", 3);
        mediumRoomTypes.Add("Antechamber", -1);
    }
    private void InitialiseSmallParentRoomTypes()
    {
        MM.UpdateHUDDbugText("DG, Initialising Small Parent Room Types");

        //room type, valid small rooms
        parentRoomTypes.Add("Crypts", new List<int> { 0 });
        parentRoomTypes.Add("Library", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("ThroneRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Prison", new List<int> { 7 });
        parentRoomTypes.Add("Temple", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Armoury", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Forge", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("TrophyRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("DiningHall", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("AudienceChamber", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("RitualChamber", new List<int> { 1, 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("FightingPit", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("TrainingGrounds", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("GardenRoom", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("BallRoom", new List<int> { 3, 4, 6, 8, 9, 10, 11, 12 });

        parentRoomTypes.Add("Chapel", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Barracks", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("TortureRoom", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("StorageRoom", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Study", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("Infirmary", new List<int> { 1, 2, 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Kennel", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("Workshop", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("MapRoom", new List<int> { 2, 3, 4, 9, 10, 11, 12 });
        parentRoomTypes.Add("ServantsQuarters", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Laboratory", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("SummoningChamber", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("RelicRoom", new List<int> { 1, 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Antechamber", new List<int> { 3, 4, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("EntryAntechamber", new List<int> { 3, 4, 8, 9, 10, 11, 12 });

        parentRoomTypes.Add("Food", new List<int> { 5 });
        parentRoomTypes.Add("Potion", new List<int> { 2, 3, 11, 12 });
        parentRoomTypes.Add("Enchantment", new List<int> { 2, 3, 11, 12 });
        parentRoomTypes.Add("Gold", new List<int> { 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Gem", new List<int> { 3, 8, 9, 10, 11, 12 });
        parentRoomTypes.Add("Boss", new List<int> { 1, 3, 6, 8 });
        parentRoomTypes.Add("Entry", new List<int> { 3, 4, 5, 6, 8, 10, 11, 12 });
    }

    public void ResetDungeon()
    {
        MM.UpdateHUDDbugText("DG, Reset Dungeon");

        //clear the room objects
        for (int i = 0; i < numOfRooms; i++) { if (roomObjects[i] != null) { Destroy(roomObjects[i]); } }

        //reset room positions and bounds
        roomPositions = new Vector2[0];
        roomCenters = new Vector2[0];
        roomPosX = new int[0];
        roomPosZ = new int[0];
        roomBoundsX = new int[0];
        roomBoundsZ = new int[0];
        roomStates = new string[0];

        //reset room type trackers
        for (int i = 0; i < usedTypeLargeIDs.Length; i++) { if (usedTypeLargeIDs[i] != 0) { usedTypeLargeIDs[i] = -1; } }
        for (int i = 0; i < usedTypeMediumIDs.Length; i++) { if (usedTypeMediumIDs[i] != 0) { usedTypeMediumIDs[i] = -1; } }
        for (int i = 0; i < usedSpecialRoomIDs.Length; i++) { if (usedSpecialRoomIDs[i] != 0) { usedSpecialRoomIDs[i] = -1; } }
        for (int i = 0; i < usedTreasureRoomIDs.Length; i++) { if (usedTreasureRoomIDs[i] != 0) { usedTreasureRoomIDs[i] = -1; } }

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
        scale = 2;
        mapBoundsMin = new Vector2();
        mapBoundsMax = new Vector2();
    }

    public void BeginDungeonGeneration(int treasureRoomsMax, int treasureRoomsMin, int specialRoomsMax, int specialRoomsMin, int boundsX, int boundsZ, int totalSpace, Vector2[,] gridPositions)
    {
        MM.UpdateHUDDbugText("DG, Beginning Dungeon Generation");

        this.treasureRoomsMax = treasureRoomsMax;
        this.treasureRoomsMin = treasureRoomsMin;
        this.specialRoomsMax = specialRoomsMax;
        this.specialRoomsMin = specialRoomsMin;
        this.boundsX = boundsX;
        this.boundsZ = boundsZ;
        this.totalSpace = totalSpace;
        this.gridPositions = gridPositions;
        mapBoundsMin = new Vector2(0, 0);
        mapBoundsMax = new Vector2(boundsX, boundsZ);

        StartCoroutine(GenerateDungeon());
    }
    private IEnumerator GenerateDungeon()
    {
        MM.UpdateHUDDbugText("DG, Generating Dungeon");

        dungeonType = "Bandit";//dungeonTypes[Random.Range(0, 2)];
        MM.UpdateHUDDbugText("dungeon type: " + dungeonType);

        //floor plan
        DetermineNumOfRooms();

        yield return StartCoroutine(PlotRooms());
        ReinitialiseWithNewLimit();
        MM.UpdateHUDDbugText("L: " + largeRoomNum + ", M: " + mediumRoomNum + ", S: " + smallRoomNum + ", TOTAL: " + (largeRoomNum + mediumRoomNum + smallRoomNum) + "/" + numOfRooms);
        //yield return new WaitForSeconds(50f);

        yield return StartCoroutine(DefineImportantRooms());
        //yield return new WaitForSeconds(50f);
        yield return StartCoroutine(DefineRoomTypes());
        //yield return new WaitForSeconds(50f);

        //build
        yield return StartCoroutine(GenerateRooms());
    }



    private void DetermineNumOfRooms()
    {
        MM.UpdateHUDDbugText("DG, Defining Initial Number Of Rooms");

        //determine number of rooms to be spawned
        posNumOfRooms = Random.Range(((totalSpace / boundsX) / 2), ((totalSpace / boundsZ) / 2));
        //numOfRooms = 5;
        MM.UpdateHUDDbugText("number of rooms to spawn: " + posNumOfRooms);

        float tempLargeRoomNum = ((float)posNumOfRooms / 100) * 15;
        float tempMediumRoomNum = ((float)posNumOfRooms / 100) * 35;
        float tempSmallRoomNum = ((float)posNumOfRooms / 100) * 50;
        largeRoomNum = (int)tempLargeRoomNum;
        mediumRoomNum = (int)tempMediumRoomNum;
        smallRoomNum = (int)tempSmallRoomNum;
        //MM.UpdateHUDDbugText("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);

        numOfRooms = largeRoomNum + mediumRoomNum + smallRoomNum;
        MM.UpdateHUDDbugText("new number of rooms to spawn: " + numOfRooms);

        roomPositions = new Vector2[numOfRooms];
        roomPosX = new int[numOfRooms];
        roomPosZ = new int[numOfRooms];
        roomBoundsX = new int[numOfRooms];
        roomBoundsZ = new int[numOfRooms];
        roomStates = new string[numOfRooms];
        roomScales = new string[numOfRooms];
        roomCenters = new Vector2[numOfRooms];
        //MM.UpdateHUDDbugText("largest possible size of rooms: " + totalSpace / posNumOfRooms);
    }



    private IEnumerator PlotRooms()
    {
        /*
        ~ set room bounds
        ~ select random position within map bounds
        ~ ensure cells are unoccupied
          ~ if cells are occupied and max attempts not reached, find a new position and recheck cells
          ~ if max attempts has been reached, set room bounds to be smaller and restart the cell check
        ~ update cells
        */

        MM.UpdateHUDDbugText("DG, Plot Room Positions");


        //plot room positions within grid
        for (int roomSpawnAttempts = 0; roomSpawnAttempts < numOfRooms; roomSpawnAttempts++)
        {
            //MM.UpdateHUDDbugText("room " + curRoomsSpawned + ", " + roomSpawnAttempts + "/" + numOfRooms);

            if (scale == 2) //large rooms
            {
                MM.UpdateHUDDbugText("large: " + curRoomsSpawned + "/" + largeRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 2), ((boundsX / 2) / 3));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 2), ((boundsZ / 2) / 3));
            }
            else if (scale == 1) //medium rooms
            {
                //break;
                MM.UpdateHUDDbugText("medium: " + (curRoomsSpawned - largeRoomNum) + "/" + mediumRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 4), ((boundsX / 2) / 5));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 4), ((boundsZ / 2) / 5));
            }
            else if (scale == 0) //small rooms
            {
                //break;
                MM.UpdateHUDDbugText("small: " + (curRoomsSpawned - (largeRoomNum + mediumRoomNum)) + "/" + smallRoomNum);

                //define room bounds
                roomBoundsX[curRoomsSpawned] = Random.Range(((boundsX / 2) / 6), ((boundsX / 2) / 7));
                roomBoundsZ[curRoomsSpawned] = Random.Range(((boundsZ / 2) / 6), ((boundsZ / 2) / 7));
            }
            else { /*MM.UpdateHUDDbugText("scale past room gen");*/ break; } //no rooms found

            //roomBoundsX[curRoomsSpawned] = 7;
            //roomBoundsZ[curRoomsSpawned] = 4;
            //MM.UpdateHUDDbugText("room X:" + roomBoundsX + ", room Z: " + roomBoundsZ);
            int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                //MM.UpdateHUDDbugText("placement attempt " + attempts);

                //generate room positions
                if (scale == 0) //if making small rooms, find positions adjacent to large or medium rooms
                {
                    int targetRoomID = curRoomsSpawned - (largeRoomNum + mediumRoomNum);
                    int wallSide = Random.Range(0, 4);
                    //MM.UpdateHUDDbugText("(" + largeRoomNum + " + " + mediumRoomNum + ") - " + curRoomsSpawned + " = " + targetRoomID);

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
                //MM.UpdateHUDDbugText("x" + roomPosX[curRoomsSpawned] + ", z" + roomPosZ[curRoomsSpawned]);
                //MM.UpdateHUDDbugText("room pos: " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);

                //ensure reasonable distance and no overlap
                if (AreCellsUnoccupied(roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned], roomBoundsX[curRoomsSpawned], roomBoundsZ[curRoomsSpawned]))
                {
                    //MM.UpdateHUDDbugText("grid pos' clear, creating room @ " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);
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
                                //MM.UpdateHUDDbugText("grid pos' clear, creating room @ " + gridPositions[roomPosX[curRoomsSpawned], roomPosZ[curRoomsSpawned]]);
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
            if (attempts == maxAttempts) { /*MM.UpdateHUDDbugText("room" + curRoomsSpawned + " removed");*/ }
            else if (attempts != maxAttempts)
            {
                //update grid states
                //MM.UpdateHUDDbugText("room" + curRoomsSpawned + " added @ x" + roomPosX[curRoomsSpawned] + ", z" + roomPosZ[curRoomsSpawned]);
                for (int x = roomPosX[curRoomsSpawned]; x < (roomPosX[curRoomsSpawned] + roomBoundsX[curRoomsSpawned]); x++)
                {
                    for (int z = roomPosZ[curRoomsSpawned]; z < (roomPosZ[curRoomsSpawned] + roomBoundsZ[curRoomsSpawned]); z++)
                    {
                        //MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        MM.UpdateDbugTileTextGridState(x, z, "Room");
                        MM.UpdateDbugTileMat(x, z, "Room");
                        MM.UpdateGridState(x, z, ("Room" + curRoomsSpawned));
                        roomStates[curRoomsSpawned] = "Empty";
                    }
                }

                //debug room display
                /*GameObject roomDbugTile = Instantiate(dbugTile, new Vector3((roomPosX[curRoomsSpawned] + ((float)roomBoundsX[curRoomsSpawned]/2) - 0.5f), 1, (roomPosZ[curRoomsSpawned] + ((float)roomBoundsZ[curRoomsSpawned] / 2)) - 0.5f), Quaternion.identity);
                roomDbugTile.transform.localScale = new Vector3(roomBoundsX[curRoomsSpawned], 0.1f, roomBoundsZ[curRoomsSpawned]);
                roomDbugTile.transform.GetChild(0).gameObject.SetActive(true);
                roomDbugTile.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Room " + curRoomsSpawned;*/


                switch (scale)
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

                roomCenters[curRoomsSpawned] = new Vector2(roomPosX[curRoomsSpawned] + (roomBoundsX[curRoomsSpawned] / 2), roomPosZ[curRoomsSpawned] + (roomBoundsZ[curRoomsSpawned] / 2));
                curRoomsSpawned++;
                yield return new WaitForSeconds(.1f);
            }


            //MM.UpdateHUDDbugText("current room spawn attempts: " + roomSpawnAttempts);
            if (roomSpawnAttempts == (largeRoomNum - 1))
            {
                largeRoomNum = curRoomsSpawned;
                //MM.UpdateHUDDbugText("large rooms plotted");

                yield return StartCoroutine(DefineWalls()); ;
                //MM.UpdateHUDDbugText("large rooms walled");
                yield return StartCoroutine(DefineHallways());
                //MM.UpdateHUDDbugText("large rooms hallwayed");

                scale--;
                //yield return new WaitForSeconds(10f);
                //MM.UpdateHUDDbugText("lowering scale, " + scale);
                //MM.UpdateHUDDbugText("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
            }
            else if (roomSpawnAttempts == ((largeRoomNum + mediumRoomNum) - 1))
            {
                mediumRoomNum = curRoomsSpawned - largeRoomNum;
                //MM.UpdateHUDDbugText("medium rooms plotted");

                yield return StartCoroutine(DefineWalls()); ;
                //MM.UpdateHUDDbugText("medium rooms walled");
                yield return StartCoroutine(DefineHallways());
                //MM.UpdateHUDDbugText("medium rooms hallwayed");


                scale--;
                //yield return new WaitForSeconds(10f);
                //MM.UpdateHUDDbugText("lowering scale, " + scale);
                //MM.UpdateHUDDbugText("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
            }
            else if (roomSpawnAttempts == ((largeRoomNum + mediumRoomNum + smallRoomNum) - 1))
            {
                smallRoomNum = curRoomsSpawned - (largeRoomNum + mediumRoomNum);
                //MM.UpdateHUDDbugText("small rooms plotted");

                yield return StartCoroutine(DefineWalls()); ;
                //MM.UpdateHUDDbugText("small rooms walled");
                yield return StartCoroutine(DefineHallways());
                //MM.UpdateHUDDbugText("small rooms hallwayed");


                scale--;
                //yield return new WaitForSeconds(10f);
                //MM.UpdateHUDDbugText("lowering scale, " + scale);
                //MM.UpdateHUDDbugText("large: " + largeRoomNum + ", medium: " + mediumRoomNum + ", small: " + smallRoomNum);
                //MM.UpdateHUDDbugText("total: " + curRoomsSpawned);
                break;
            }
        }
    }
    private bool AreCellsUnoccupied(int posX, int posZ, int roomBoundsX, int roomBoundsZ)
    {
        if (posX < 0 || posX + roomBoundsX > boundsX || posZ < 0 || posZ + roomBoundsZ > boundsZ)
        {
            //Debug.Log(posX + ", " + posZ + ", " + (posX + roomBoundsX) + "/" + boundsX + ", " + (posZ + roomBoundsZ) + "/" + boundsZ);
            return false;
        }
        for (int x = posX; x < (posX + roomBoundsX); x++)
        {
            for (int z = posZ; z < (posZ + roomBoundsZ); z++)
            {
                MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " - " + MM.GetGridState(x, z));
                if (MM.GetGridState(x, z) != "Empty") { /*MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " not empty");*/ return false; }
            }
        }
        return true;
    }

    private IEnumerator DefineWalls()
    {
        MM.UpdateHUDDbugText("DG, Define Walls");

        /*
         * get edges of rooms
         * set grid position state to wall
         * update material colour with half hue
         */

        //MM.UpdateHUDDbugText("adding walls");
        int roomIDStart = 0;
        if (scale == 1) { roomIDStart = largeRoomNum; }
        else if (scale == 0) { roomIDStart = (largeRoomNum + mediumRoomNum); }
        //MM.UpdateHUDDbugText("scale: " + scale);
        MM.UpdateHUDDbugText("total curRoomsSpawned: " + (curRoomsSpawned - roomIDStart));
        //MM.UpdateHUDDbugText("roomIDStart: " + roomIDStart + ", roomIDEnd: " + curRoomsSpawned);

        //Debug.Log(largeRoomNum + " " + mediumRoomNum + " " + smallRoomNum);
        //Debug.Log(roomIDStart + " " + curRoomsSpawned);
        for (int roomID = roomIDStart; roomID < curRoomsSpawned; roomID++)//for each room
        {
            //MM.UpdateHUDDbugText("adding walls to room" + roomID + " @ x" + roomPosX[roomID] + ", z" + roomPosZ[roomID]);
            int newX = -1;
            int newZ = -1;

            for (int x = roomPosX[roomID]; x < (roomPosX[roomID] + roomBoundsX[roomID]); x++)//top row of wall
            {
                newX = x;
                newZ = roomPosZ[roomID] + (roomBoundsZ[roomID] - 1);
                //MM.UpdateHUDDbugText("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (x == roomPosX[roomID] || x == roomPosX[roomID] + roomBoundsX[roomID] - 1)
                {
                    MM.UpdateGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileMat(newX, newZ, "WallCorner");
                }
                else if (MM.GetGridState(newX, newZ) != "Wall")
                {
                    MM.UpdateGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileMat(newX, newZ, "Wall");
                }
            }
            for (int x = roomPosX[roomID]; x < (roomPosX[roomID] + roomBoundsX[roomID]); x++)//bottom row of wall
            {
                newX = x;
                newZ = roomPosZ[roomID];
                //MM.UpdateHUDDbugText("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (x == roomPosX[roomID] || x == roomPosX[roomID] + roomBoundsX[roomID] - 1)
                {
                    MM.UpdateGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileMat(newX, newZ, "WallCorner");
                }
                else if (MM.GetGridState(newX, newZ) != "Wall")
                {
                    MM.UpdateGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileMat(newX, newZ, "Wall");
                }
            }

            for (int z = roomPosZ[roomID]; z < (roomPosZ[roomID] + roomBoundsZ[roomID]); z++)//left row of wall
            {
                newX = roomPosX[roomID];
                newZ = z;
                //MM.UpdateHUDDbugText("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (z == roomPosZ[roomID] || z == roomPosZ[roomID] + roomBoundsZ[roomID] - 1)
                {
                    MM.UpdateGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileMat(newX, newZ, "WallCorner");
                }
                else if (MM.GetGridState(newX, newZ) != "Wall")
                {
                    MM.UpdateGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileMat(newX, newZ, "Wall");
                }
            }
            for (int z = roomPosZ[roomID]; z < (roomPosZ[roomID] + roomBoundsZ[roomID]); z++)//right row of wall
            {
                newX = roomPosX[roomID] + (roomBoundsX[roomID] - 1);
                newZ = z;
                //MM.UpdateHUDDbugText("x:" + x + ", z:" + z);
                //Debug.Log(gridStates[x, z]);

                if (z == roomPosZ[roomID] || z == roomPosZ[roomID] + roomBoundsZ[roomID] - 1)
                {
                    MM.UpdateGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "WallCorner");
                    MM.UpdateDbugTileMat(newX, newZ, "WallCorner");
                }
                else if (MM.GetGridState(newX, newZ) != "Wall")
                {
                    MM.UpdateGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileTextGridState(newX, newZ, "Wall");
                    MM.UpdateDbugTileMat(newX, newZ, "Wall");
                }
            }

            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator DefineHallways()
    {
        MM.UpdateHUDDbugText("DG, Define Hallways");

        //define rooms to be connected using A* and Prims
        if (scale == 2)
        {
            //for each large room, connect it to each other large room - creating primary hallways
            for (int roomID = 0; roomID < curRoomsSpawned - 1; roomID++)
            {
                //MM.UpdateHUDDbugText("cur:" + roomID);

                for (int targetRoomID = roomID + 1; targetRoomID < curRoomsSpawned; targetRoomID++) //for each other room
                {
                    //if the current room and next room are different IDs
                    if (roomID != targetRoomID)
                    {
                        //yield return new WaitForSeconds(.1f);
                        //MM.UpdateHUDDbugText("cur:" + roomID + ", target:" + targetRoomID);
                        //calculate distance
                        Vector2 curRoomCenter = roomCenters[roomID];
                        Vector2 nextRoomCenter = roomCenters[targetRoomID];
                        float distance = Vector2.Distance(curRoomCenter, nextRoomCenter);

                        Vector2 startPos = roomCenters[roomID];
                        Vector2 targetPos = roomCenters[targetRoomID];

                        MM.UpdateHUDDbugText("joining cur:" + roomID + " & target:" + targetRoomID);

                        PG.BeginPathGeneration(startPos, targetPos, boundsX, boundsZ);
                        yield return new WaitForSeconds(.1f);
                    }
                }
            }
        }
        else if (scale == 1)
        {
            //for each medium room, connect it to the nearest large room
            for (int roomID = largeRoomNum; roomID < curRoomsSpawned; roomID++)
            {
                //MM.UpdateHUDDbugText("cur:" + roomID);

                //initialise tracking vars
                int nearestRoomID = -1;
                float minDistance = float.MaxValue;

                for (int targetRoomID = 0; targetRoomID < largeRoomNum; targetRoomID++)
                {
                    //if the current room and next room are different IDs
                    if (roomID != targetRoomID)
                    {
                        //MM.UpdateHUDDbugText("cur:" + roomID + ", target:" + targetRoomID);

                        //calculate distance
                        Vector2 curRoomCenter = roomCenters[roomID];
                        Vector2 nextRoomCenter = roomCenters[targetRoomID];
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
                    Vector2 startPos = roomCenters[roomID];
                    Vector2 targetPos = roomCenters[nearestRoomID];

                    MM.UpdateHUDDbugText("joining cur:" + roomID + " & target:" + nearestRoomID);

                    PG.BeginPathGeneration(startPos, targetPos, boundsX, boundsZ);
                    yield return new WaitForSeconds(.1f);
                }
            }
        }
        else if (scale == 0)
        {
            for (int roomID = (largeRoomNum + mediumRoomNum); roomID < curRoomsSpawned; roomID++)
            {
                //connect each small room to its parent
                int parentRoomID = roomID - (largeRoomNum + mediumRoomNum);
                Vector2 startPos = roomCenters[roomID];
                Vector2 targetPos = roomCenters[parentRoomID];
                //MM.UpdateHUDDbugText("start: " + startPos + ", target: " + targetPos);

                MM.UpdateHUDDbugText("joining cur:" + roomID + " & target:" + parentRoomID);
                PG.BeginPathGeneration(startPos, targetPos, boundsX, boundsZ);
                yield return new WaitForSeconds(.1f);
            }
        }
        else { /*MM.UpdateHUDDbugText("scale past hallway gen");*/ }
    }



    private void ReinitialiseWithNewLimit()
    {
        MM.UpdateHUDDbugText("DG, Reinitialise With New Number Of Rooms");

        numOfRooms = curRoomsSpawned;
        //MM.UpdateHUDDbugText("new number of rooms: " + numOfRooms);

        usedTypeLargeIDs = new int[largeRoomNum];
        usedTypeMediumIDs = new int[mediumRoomNum];
        usedSpecialRoomIDs = new int[specialRoomType.Length];
        usedTreasureRoomIDs = new int[treasureRoomType.Length];
        roomObjects = new GameObject[numOfRooms];

        for (int i = 0; i < usedSpecialRoomIDs.Length; i++) { usedSpecialRoomIDs[i] = -1; }
        for (int i = 0; i < usedTreasureRoomIDs.Length; i++) { usedTreasureRoomIDs[i] = -1; }
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



    private IEnumerator DefineImportantRooms()
    {
        //set room types
        //find large room furthest away from bounds center to define as Boss room
        //find medium or small room furthest away from Boss room to define as Entry room
        //find small rooms furthest from Boss & Entry & other Treasure Rooms to define as Treasure Rooms
        //find medium or small rooms furthest from Boss & Entry & Treasure Rooms & other Special Rooms to define as Special Rooms
        MM.UpdateHUDDbugText("DG, Defining Important Rooms");

        FindBossRoom();
        yield return new WaitForSeconds(.1f);
        FindEntryRoom();
        yield return new WaitForSeconds(.1f);
        FindTreasureRooms();
        yield return new WaitForSeconds(.1f);
        FindSpecialRooms();
        yield return new WaitForSeconds(.1f);
    }

    private void FindBossRoom()
    {
        MM.UpdateHUDDbugText("DG, Finding Boss Room");
 
        //find boss room
        boundsCenter = new Vector2((boundsX / 2), (boundsZ / 2));
        //Debug.Log(boundsCenter);
        float distFromCenter = 0;
        bossRoomID = -1;

        for (int roomID = 0; roomID < largeRoomNum; roomID++) //for each large room
        {
            //init room center and distance from bounds center
            Vector2 roomCenter = roomCenters[roomID];
            float distRoomToCenter = Vector2.Distance(boundsCenter, roomCenter);
            //MM.UpdateHUDDbugText("roomID: " + roomID + " - " + roomCenter + ", " + distRoomToCenter);

            //if room further than last nearest
            if (distRoomToCenter > distFromCenter)
            {
                //update trackers
                distFromCenter = distRoomToCenter;
                bossRoomID = roomID;
                //MM.UpdateHUDDbugText("new boss room id: " + bossRoomID);
            }
        }

        if (bossRoomID != -1) //if boss room found
        {
            MM.UpdateHUDDbugText("boss room id: " + bossRoomID);

            //update room state tracker
            roomStates[bossRoomID] = "Boss";

            //for each pos inside the room
            for (int x = roomPosX[bossRoomID]; x < (roomPosX[bossRoomID] + roomBoundsX[bossRoomID]); x++)
            {
                for (int z = roomPosZ[bossRoomID]; z < (roomPosZ[bossRoomID] + roomBoundsZ[bossRoomID]); z++)
                {
                    string gridState = MM.GetGridState(x, z);
                    //only if pos is empty room
                    if (gridState != "Wall" && gridState != "Doorway" && gridState != "WallCorner")
                    {
                        //update grid state, debug text, and debug material
                        //MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        MM.UpdateGridState(x, z, "BossRoom");
                        MM.UpdateDbugTileTextGridState(x, z, "Boss");
                        MM.UpdateDbugTileMat(x, z, "Boss");
                    }
                }
            }
        }
    }

    private void FindEntryRoom()
    {
        MM.UpdateHUDDbugText("DG, Finding Entry Room");

        //find entry room
        bossRoomCenter = roomCenters[bossRoomID];
        float distFromBoss = 0;
        entryRoomID = -1;

        for (int roomID = largeRoomNum; roomID < (largeRoomNum + mediumRoomNum + smallRoomNum - 1); roomID++) //for each medium and small room
        {
            //init room center and distance from boss room center
            //Debug.Log(roomID);
            Vector2 roomCenter = roomCenters[roomID];
            float distRoomToBoss = Vector2.Distance(bossRoomCenter, roomCenter);
            //MM.UpdateHUDDbugText("roomID: " + roomID + " - " + roomCenter + ", " + distRoomToBoss);

            //if room further than last nearest
            if (distRoomToBoss > distFromBoss)
            {
                //update trackers
                distFromBoss = distRoomToBoss;
                entryRoomID = roomID;
                //MM.UpdateHUDDbugText("new entry room id: " + entryRoomID);
            }
        }

        if (entryRoomID != -1) //if entry room found
        {
            MM.UpdateHUDDbugText("entry room id: " + entryRoomID);

            //update room state tracker
            roomStates[entryRoomID] = "Entry";

            //for each pos inside the room
            for (int x = roomPosX[entryRoomID]; x < (roomPosX[entryRoomID] + roomBoundsX[entryRoomID]); x++)
            {
                for (int z = roomPosZ[entryRoomID]; z < (roomPosZ[entryRoomID] + roomBoundsZ[entryRoomID]); z++)
                {
                    string gridState = MM.GetGridState(x, z);
                    //only if pos is empty room
                    if (gridState != "Wall" && gridState != "Doorway" && gridState != "WallCorner")
                    {
                        //update grid state, debug text, and debug material
                        //MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                        MM.UpdateGridState(x, z, "EntryRoom");
                        MM.UpdateDbugTileTextGridState(x, z, "Entry");
                        MM.UpdateDbugTileMat(x, z, "Entry");
                    }
                }
            }
        }
    }

    private void FindTreasureRooms()
    {
        MM.UpdateHUDDbugText("DG, Finding Treasure Rooms");

        //find treasure rooms
        entryRoomCenter = roomCenters[entryRoomID];
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
                Vector2 roomCenter = roomCenters[roomID];
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
                //MM.UpdateHUDDbugText("roomID: " + roomID + " - " + roomCenter + ", " + distFromRoomToEntryAndBoss);

                //if room further than last nearest & room is empty
                if (distFromRoomToEntryAndBoss > distFromEntryAndBoss && roomStates[roomID] == "Empty")
                {
                    //Debug.Log(distFromRoomToEntryAndBoss + " > " + distFromEntryAndBoss);
                    //update trackers
                    distFromEntryAndBoss = distFromRoomToEntryAndBoss;
                    treasureRoomIDs[tR] = roomID;
                    //MM.UpdateHUDDbugText("new treasure room id: " + treasureRoomIDs[tR]);
                }
            }

            if (treasureRoomIDs[tR] != -1) //if treasure room found
            {
                MM.UpdateHUDDbugText("treasure room id: " + treasureRoomIDs[tR]);

                //update room state tracker
                roomStates[treasureRoomIDs[tR]] = "Treasure"; 
                treasureRoomCenters[tR] = roomCenters[treasureRoomIDs[tR]];

                //for each pos inside the room
                for (int x = roomPosX[treasureRoomIDs[tR]]; x < (roomPosX[treasureRoomIDs[tR]] + roomBoundsX[treasureRoomIDs[tR]]); x++)
                {
                    for (int z = roomPosZ[treasureRoomIDs[tR]]; z < (roomPosZ[treasureRoomIDs[tR]] + roomBoundsZ[treasureRoomIDs[tR]]); z++)
                    {
                        string gridState = MM.GetGridState(x, z);
                        //only if pos is empty room
                        if (gridState != "Wall" && gridState != "Doorway" && gridState != "WallCorner")
                        {
                            //update grid state, debug text, and debug material
                            //MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                            MM.UpdateGridState(x, z, "TreasureRoom");
                            MM.UpdateDbugTileTextGridState(x, z, "Treasure");
                            MM.UpdateDbugTileMat(x, z, "Treasure");
                        }
                    }
                }

                distFromEntryAndBoss = 0;
            }
        }
    }

    private void FindSpecialRooms()
    {
        MM.UpdateHUDDbugText("DG, Finding Special Rooms");

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
                Vector2 roomCenter = roomCenters[roomID];
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
                //MM.UpdateHUDDbugText("roomID: " + roomID + " - " + roomCenter + ", " + distFromRoomToEntryAndBoss);

                //if room further than last nearest & room is empty
                if (distFromRoomToEntryAndBoss > distFromEntryAndBoss && roomStates[roomID] == "Empty")
                {
                    //Debug.Log(distFromRoomToEntryAndBoss + " > " + distFromEntryAndBoss);
                    //update trackers
                    distFromEntryAndBoss = distFromRoomToEntryAndBoss;
                    specialRoomIDs[sR] = roomID;
                    //MM.UpdateHUDDbugText("new special room id: " + specialRoomIDs[sR]);
                }
            }

            if (specialRoomIDs[sR] != -1) //if special room found
            {
                MM.UpdateHUDDbugText("special room id: " + specialRoomIDs[sR]);

                //update room state tracker
                roomStates[specialRoomIDs[sR]] = "Special";
                specialRoomCenters[sR] = roomCenters[specialRoomIDs[sR]];

                //for each pos inside the room
                for (int x = roomPosX[specialRoomIDs[sR]]; x < (roomPosX[specialRoomIDs[sR]] + roomBoundsX[specialRoomIDs[sR]]); x++)
                {
                    for (int z = roomPosZ[specialRoomIDs[sR]]; z < (roomPosZ[specialRoomIDs[sR]] + roomBoundsZ[specialRoomIDs[sR]]); z++)
                    {
                        string gridState = MM.GetGridState(x, z);
                        //only if pos is empty room
                        if (gridState != "Wall" && gridState != "Doorway" && gridState != "WallCorner")
                        {
                            //update grid state, debug text, and debug material
                            //MM.UpdateHUDDbugText("grid pos @ X: " + x + ", Z: " + z + " is now part of room" + curRoomsSpawned);
                            MM.UpdateGridState(x, z, "SpecialRoom");
                            MM.UpdateDbugTileTextGridState(x, z, "Special");
                            MM.UpdateDbugTileMat(x, z, "Special");
                        }
                    }
                }

                distFromEntryAndBoss = 0;
            }
        }
    }



    private IEnumerator DefineRoomTypes()
    {
        MM.UpdateHUDDbugText("DG, Defining Room Types");

        //if specific dungeon type, layout specific
        //otherwise, assign appropriate rooms

        if (dungeonType == "Crypt")
        {
            MM.UpdateHUDDbugText("DG, Assigning dungeon type: Crypts");
            //assign all crypt rooms
            for (int roomID = 0; roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++)
            {
                //select random room
                string roomStateTemp = roomStates[roomID];
                roomStates[roomID] = "Crypt" + roomStateTemp;
                //MM.UpdateHUDDbugText("crypt room" + roomID + " set as " + roomTypeLarge[roomTypeID]);
                yield return new WaitForSeconds(.1f);
            }
        }
        else
        {
            MM.UpdateHUDDbugText("DG, Assigning dungeon type: " + dungeonType);
            string futureBossRoomType = "";
            for (int scale = 2; scale >= 0; scale--) //for each size of room
            {
                MM.UpdateHUDDbugText("scale: " + scale);
                if (scale == 2)
                {
                    MM.UpdateHUDDbugText("DG, Assigning " + largeRoomNum + " large rooms");
                    //assign large room types
                    string[] largeRoomTypeStrings = new string[largeRoomTypes.Count];
                    largeRoomTypes.Keys.CopyTo(largeRoomTypeStrings, 0);
                    for (int roomID = 0; roomID < largeRoomNum; roomID++)
                    {
                        Debug.Log(roomID + "/" + usedTypeLargeIDs.Length);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int[] curValidRoomIDs = FindThresholdRooms(scale, roomID);
                        int index = Random.Range(0, curValidRoomIDs.Length);
                        int roomTypeID = curValidRoomIDs[index]; //find random room type

                        while (inc == 0) { inc = Random.Range(-1, 1); }

                        while (!valid) //while room invalid
                        {
                            //MM.UpdateHUDDbugText("room type id: " + roomTypeID + ", " + largeRoomTypeStrings[roomTypeID]);
                            valid = IsRoomValid(scale, roomTypeID, roomID, curValidRoomIDs.Length); //set valid to room check
                            //MM.UpdateHUDDbugText("valid: " + valid);

                            if (valid) { break; }
                            else if (!valid) //find unused room type, then loop
                            {
                                //Debug.Log(index + "/" + curValidRoomIDs.Length);
                                if (inc == 1 && index >= curValidRoomIDs.Length) //wrap around if exceeding array length
                                {
                                    index = 0;
                                }
                                else if (inc == -1 && index <= 0) //wrap around if exceeding array length
                                {
                                    index = (curValidRoomIDs.Length - 1);
                                }
                                else //increment to find the next room type
                                {
                                    index += inc;
                                }

                                roomTypeID = curValidRoomIDs[index];
                            }
                        }

                        //MM.UpdateHUDDbugText("roomID: " + roomID);
                        //MM.UpdateHUDDbugText("roomTypeID: " + roomTypeID);
                        //MM.UpdateHUDDbugText("largeRoomTypeStrings[roomTypeID]: " + largeRoomTypeStrings[roomTypeID]);
                        //MM.UpdateHUDDbugText("pre usedTypeLargeIDs[roomID]: " + usedTypeLargeIDs[roomID]);
                        usedTypeLargeIDs[roomID] = roomTypeID;
                        //MM.UpdateHUDDbugText("post usedTypeLargeIDs[roomID]: " + usedTypeLargeIDs[roomID]);
                        MM.UpdateHUDDbugText("room " + roomID + ": large room " + roomID + "/" + largeRoomNum + " set as " + largeRoomTypeStrings[roomTypeID]);

                        if (roomStates[roomID] == "Empty") { roomStates[roomID] = largeRoomTypeStrings[roomTypeID]; }
                        else if(roomStates[roomID] == "Boss") { futureBossRoomType = largeRoomTypeStrings[roomTypeID]; continue; }
                        else { string roomStateTemp = roomStates[roomID]; roomStates[roomID] = roomStateTemp + largeRoomTypeStrings[roomTypeID]; }

                        yield return new WaitForSeconds(.1f);
                    }
                }
                else if (scale == 1)
                {
                    MM.UpdateHUDDbugText("DG, Assigning " + mediumRoomNum + " medium rooms");
                    //assign medium room types
                    string[] mediumRoomTypeStrings = new string[mediumRoomTypes.Count];
                    mediumRoomTypes.Keys.CopyTo(mediumRoomTypeStrings, 0);
                    for (int roomID = largeRoomNum; roomID < (largeRoomNum + mediumRoomNum); roomID++)
                    {
                        //Debug.Log((roomID - largeRoomNum) + "/" + usedTypeMediumIDs.Length);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int[] curValidRoomIDs = FindThresholdRooms(scale, roomID);
                        int index = Random.Range(0, curValidRoomIDs.Length);
                        int roomTypeID = curValidRoomIDs[index]; //find random room type

                        while (inc == 0) { inc = Random.Range(-1, 1); }

                        if (roomStates[roomID] == "Special")
                        {
                            //MM.UpdateHUDDbugText("special medium room");
                            roomTypeID = Random.Range(0, specialRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                //MM.UpdateHUDDbugText("room type id: " + roomTypeID);
                                valid = IsRoomValid(scale, roomTypeID, roomID, curValidRoomIDs.Length); //set valid to room check
                                //MM.UpdateHUDDbugText("valid: " + valid);

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
                            if (specialRoomsFound < usedSpecialRoomIDs.Length) { usedSpecialRoomIDs[specialRoomsFound] = roomTypeID; }
                            usedTypeMediumIDs[(roomID - largeRoomNum)] = 20;
                            MM.UpdateHUDDbugText("room " + roomID + ": medium special room " + (roomID - largeRoomNum) + "/" + mediumRoomNum + " set as " + specialRoomType[roomTypeID]);
                            specialRoomsFound++;
                            yield return new WaitForSeconds(.1f);
                            continue;
                        }
                        else if (roomStates[roomID] == "Entry")
                        {
                            MM.UpdateHUDDbugText("room " + roomID + ": medium room is already " + roomStates[roomID]);
                            roomStates[roomID] = "EntryAntechamber";
                            usedTypeMediumIDs[(roomID - largeRoomNum)] = 21;
                            yield return new WaitForSeconds(.1f);
                            continue;
                        }
                        else
                        {
                            while (!valid) //while room invalid
                            {
                                //MM.UpdateHUDDbugText("room type id: " + roomTypeID + ", " + mediumRoomTypeStrings[roomTypeID]);
                                valid = IsRoomValid(scale, roomTypeID, roomID, curValidRoomIDs.Length); //set valid to room check
                                //MM.UpdateHUDDbugText("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    //Debug.Log(index + "/" + curValidRoomIDs.Length);
                                    if (inc == 1 && index >= curValidRoomIDs.Length) //wrap around if exceeding array length
                                    {
                                        index = 0;
                                    }
                                    else if (inc == -1 && index <= 0) //wrap around if exceeding array length
                                    {
                                        index = (curValidRoomIDs.Length - 1);
                                    }
                                    else //increment to find the next room type
                                    {
                                        index += inc;
                                    }

                                    roomTypeID = curValidRoomIDs[index];
                                }
                            }
                        }

                        if (roomStates[roomID] == "Empty") { roomStates[roomID] = mediumRoomTypeStrings[roomTypeID]; }
                        else { string roomStateTemp = roomStates[roomID]; roomStates[roomID] = roomStateTemp + mediumRoomTypeStrings[roomTypeID]; }
                        //MM.UpdateHUDDbugText("usedTypeMediumIDs.Length: " + usedTypeMediumIDs.Length);
                        //MM.UpdateHUDDbugText("roomID: " + roomID);
                        //MM.UpdateHUDDbugText("roomID-largeRoomNum: " + (roomID- largeRoomNum));
                        //MM.UpdateHUDDbugText("largeRoomNum+mediumRoomNum" + (largeRoomNum + mediumRoomNum));
                        usedTypeMediumIDs[(roomID - largeRoomNum)] = roomTypeID;
                        MM.UpdateHUDDbugText("room " + roomID + ": medium room " + roomID + "/" + (largeRoomNum + mediumRoomNum) + " set as " + mediumRoomTypeStrings[roomTypeID]);
                        yield return new WaitForSeconds(.1f);
                    }
                }
                else if (scale == 0)
                {
                    MM.UpdateHUDDbugText("DG, Assigning " + smallRoomNum + " small rooms");
                    //assign small room types
                    for (int roomID = (largeRoomNum + mediumRoomNum); roomID < (largeRoomNum + mediumRoomNum + smallRoomNum); roomID++)
                    {
                        //Debug.Log((roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum);
                        //select random room
                        bool valid = false;
                        int inc = 0;
                        int roomTypeID = 0; //find random room type

                        while (inc == 0) { inc = Random.Range(-1, 1); }

                        if (roomStates[roomID] == "Special")
                        {
                            //Debug.Log("special small room");
                            roomTypeID = Random.Range(0, specialRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                //Debug.Log("room type id: " + roomTypeID);
                                valid = IsRoomValid(scale, roomTypeID, roomID, 0); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= specialRoomType.Length)
                                    {
                                        //Debug.Log("roomTypeID = " + roomTypeID + ", looping");
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID < 0)
                                    {
                                        //Debug.Log("roomTypeID = " + roomTypeID + ", looping");
                                        roomTypeID = (specialRoomType.Length - 1);
                                    }
                                }
                            }

                            //Debug.Log("adding room state: " + roomTypeID);
                            roomStates[roomID] = specialRoomType[roomTypeID];
                            if (specialRoomsFound < usedSpecialRoomIDs.Length) { usedSpecialRoomIDs[specialRoomsFound] = roomTypeID; }
                            MM.UpdateHUDDbugText("room " + roomID + ": small special room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + specialRoomType[roomTypeID]);
                            specialRoomsFound++;
                            yield return new WaitForSeconds(.1f);
                            continue;
                        }
                        else if (roomStates[roomID] == "Treasure")
                        {
                            //Debug.Log("treasure small room");
                            roomTypeID = Random.Range(0, treasureRoomType.Length); //find random room type
                            while (!valid) //while room invalid
                            {
                                valid = IsRoomValid(scale, roomTypeID, roomID, 0); //set valid to room check
                                //Debug.Log("valid: " + valid);

                                if (valid) { break; }
                                else if (!valid) //find unused room type, then loop
                                {
                                    roomTypeID += inc; //increment to find the next room type
                                    if (inc == 1 && roomTypeID >= treasureRoomType.Length)
                                    {
                                        //Debug.Log("roomTypeID = " + roomTypeID + ", looping");
                                        roomTypeID = 0; //wrap around if exceeding array length
                                    }
                                    else if (inc == -1 && roomTypeID < 0)
                                    {
                                        //Debug.Log("roomTypeID = " + roomTypeID + ", looping");
                                        roomTypeID = (treasureRoomType.Length - 1);
                                    }
                                }
                            }

                            //Debug.Log("adding room state: " + roomTypeID);
                            roomStates[roomID] = treasureRoomType[roomTypeID];
                            if (treasureRoomsFound < usedTreasureRoomIDs.Length) { usedTreasureRoomIDs[treasureRoomsFound] = roomTypeID; }
                            MM.UpdateHUDDbugText("room " + roomID + ": small treasure room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + treasureRoomType[roomTypeID]);
                            treasureRoomsFound++;
                            yield return new WaitForSeconds(.1f);
                            continue;
                        }
                        else if (roomStates[roomID] == "Entry")
                        {
                            MM.UpdateHUDDbugText("room " + roomID + ": small room is already " + roomStates[roomID]);
                            roomStates[roomID] = "Entry";
                            yield return new WaitForSeconds(.1f);
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
                                    MM.UpdateHUDDbugText("room " + roomID + ": medium room is already " + roomStates[roomID]);
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
                                case "EntryAntechamber":

                                case "Food":
                                case "Potion":
                                case "Enchantment":
                                case "Gold":
                                case "Gem":
                                case "Boss":
                                    //if parent room types contains provided parent room, provide int list of valid small room IDs
                                    if (parentRoomTypes.TryGetValue(parentRoomType, out List<int> smallRoomTypesForParent))
                                    {
                                        int[] validSmallRoomIDs = new int[smallRoomTypesForParent.Count];
                                        smallRoomTypesForParent.CopyTo(validSmallRoomIDs, 0);
                                        string[] validSmallRoomTypes = new string[smallRoomTypesForParent.Count]; //init new string array, length of valid small room IDs
                                        int validIndex = 0; //counting index for found strings of valid IDs
                                        for(int i = 0; i < smallRoomTypes.Length; i++) //for each small room type
                                        {
                                            //MM.UpdateHUDDbugText("smallRoomTypes" + i + ": " + smallRoomTypes[i]);
                                            if(smallRoomTypesForParent.Contains(i)) //if the ID is contained in the list
                                            {
                                                //Debug.Log("valid small room " + validIndex + " found: " + smallRoomTypes[i]);
                                                validSmallRoomTypes[validIndex] = smallRoomTypes[i]; //add small room type string to valid string array
                                                validIndex++; //then increase array pos until next room is found
                                            }
                                        }
                                        //MM.UpdateHUDDbugText("validSmallRoomTypes.Length: " + validSmallRoomTypes.Length);

                                        //find random ID from valid IDs array
                                        roomTypeID = Random.Range(0, (validSmallRoomTypes.Length - 1));
                                        //Debug.Log("found room ID: " + roomTypeID);
                                        //Debug.Log("found room type: " + validSmallRoomTypes[roomTypeID]);

                                        //if found ID is storage and parent room is boss
                                        if(validSmallRoomTypes[roomTypeID] == "Storage" && roomStates[(roomID - (largeRoomNum + mediumRoomNum))] == "Boss")
                                        {
                                            //concatonate storage and boss strings & update room state
                                            roomStateTemp = futureBossRoomType;
                                            roomStates[roomID] = roomStateTemp + validSmallRoomTypes[roomTypeID];
                                        }
                                        //otherwise if found ID is storage
                                        else if (validSmallRoomTypes[roomTypeID] == "Storage")
                                        {
                                            //concatonate storage and parent strings & update room state
                                            roomStateTemp = parentRoomType;
                                            roomStates[roomID] = roomStateTemp + validSmallRoomTypes[roomTypeID];
                                        }
                                        //otherwise update room state with found ID
                                        else { roomStates[roomID] = validSmallRoomTypes[roomTypeID]; }

                                        MM.UpdateHUDDbugText("room " + roomID + ": small room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + validSmallRoomTypes[roomTypeID]);
                                    }
                                    yield return new WaitForSeconds(.1f);
                                    break;
                                default:
                                    //Debug.Log("creating secret room");
                                    roomStateTemp = roomStates[(roomID - (largeRoomNum + mediumRoomNum))];
                                    roomStates[(roomID - (largeRoomNum + mediumRoomNum))] = roomStateTemp + "IllusionRoom";
                                    roomStates[roomID] = "SecretRoom";
                                    //Debug.Log("room " + (roomID - (largeRoomNum + mediumRoomNum)) + ": room set as " + roomStates[(roomID - (largeRoomNum + mediumRoomNum))]);
                                    MM.UpdateHUDDbugText("room " + roomID + ": small room " + (roomID - largeRoomNum - mediumRoomNum) + "/" + smallRoomNum + " set as " + roomStates[roomID]);
                                    yield return new WaitForSeconds(.1f);
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
    private bool IsRoomValid(int scale, int typeID, int roomID, int length)
    {
        //MM.UpdateHUDDbugText("checking if room is valid - scale: " + scale + ", room type id: " + typeID);

        if (roomStates[roomID] == "Special") //if room id is special
        {
            //Debug.Log("type id: " + typeID);

            //check if last used room ID has been updated, if so return true
            //Debug.Log("last usedSpecialRoomIDs[" + (usedSpecialRoomIDs.Length - 1) +"]: " + usedSpecialRoomIDs[usedSpecialRoomIDs.Length - 1]);
            if (usedSpecialRoomIDs[usedSpecialRoomIDs.Length - 1] != -1) { return true; }

            //for each special room type
            for (int i = 0; i < usedSpecialRoomIDs.Length; i++)
            {
                //Debug.Log("usedSpecialRoomIDs[" + i + "]: " + usedSpecialRoomIDs[i]);
                if (usedSpecialRoomIDs[i] == typeID) { return false; } //if type id has been used, return false
            }

            return true; //break out true
        }
        else if (roomStates[roomID] == "Treasure")
        {
            //Debug.Log("type id: " + typeID);

            //check if last used room ID has been updated, if so return true
            //Debug.Log("last usedTreasureRoomIDs[" + (usedTreasureRoomIDs.Length - 1) + "]: " + usedTreasureRoomIDs[usedTreasureRoomIDs.Length - 1]);
            if (usedTreasureRoomIDs[usedTreasureRoomIDs.Length - 1] != -1) { return true; }

            //for each treasure room type
            for (int i = 0; i < usedTreasureRoomIDs.Length; i++)
            {
                //Debug.Log("usedTreasureRoomIDs[" + i + "]: " + usedTreasureRoomIDs[i]);

                //if used room ID equals room ID, return false
                if (usedTreasureRoomIDs[i] == typeID) { return false; }
            }

            //if no used room IDs equal room ID, return true
            return true;
        }

        switch(scale)
        {
            case 2:
                for (int i = 0; i < length; i++)
                {
                    //MM.UpdateHUDDbugText("used room type " + i + ": " + usedTypeLargeIDs[i]);
                    if (usedTypeLargeIDs[i] == -1) { return true; }
                    else if (usedTypeLargeIDs[i] == typeID) { return false; }
                    else if (i == length - 1) { return true; }
                }
                return false;

            case 1:
                for (int i = 0; i < length; i++) //for each room type
                {
                    //MM.UpdateHUDDbugText("used room type " + i + ": " + usedTypeMediumIDs[i]);
                    if(usedTypeMediumIDs[i] == -1) { return true; }
                    else if (usedTypeMediumIDs[i] == typeID) { return false; }
                    else if (i == length - 1) { return true; }
                }
                return false;
        }

        return false;
    }
    private int[] FindThresholdRooms(int scale, int roomID)
    {
        MM.UpdateHUDDbugText("DG, finding rooms within threshold");

        List<int> curValidRoomIndices = new List<int>(); //valid threshold room indicies
        int threshold = -1;     //room focuses   0 - anywhere     1 - entry     2 - inbetween     3 - boss
        float quarterMapDist = (Vector2.Distance(mapBoundsMin, mapBoundsMax) * 0.25f); //1/4 of the maps total distance for later comparisons

        //MM.UpdateHUDDbugText("roomID: " + roomID);
        if (Vector2.Distance(roomCenters[roomID], entryRoomCenter) < quarterMapDist) //if distance between cur room and entry room less than a map quarter
        {
            //MM.UpdateHUDDbugText("dist from room to entry: " + Vector2.Distance(roomCenters[roomID], entryRoomCenter));
            //MM.UpdateHUDDbugText("dist from entry below 25% threshold");
            threshold = 1;
        }
        else if (Vector2.Distance(roomCenters[roomID], bossRoomCenter) < quarterMapDist) //if distance between cur room and boss room less than a map quarter
        {
            //MM.UpdateHUDDbugText("dist from room to boss: " + Vector2.Distance(roomCenters[roomID], bossRoomCenter));
            //MM.UpdateHUDDbugText("dist from boss below 25% threshold");
            threshold = 3;
        }
        else //if distance between cur room and entry & boss room more than a map quarter
        {
            //MM.UpdateHUDDbugText("dist inbetween entry and boss");
            threshold = 2;
        }


        int index = 0; //for tracking room indices
        switch (scale)
        {
            case 2:
                foreach (var roomType in largeRoomTypes)
                {
                    if (roomType.Value == threshold || roomType.Value == 0 && roomType.Key != "Crypts")
                    {
                        curValidRoomIndices.Add(index);
                    }
                    index++;
                }
                break;
            case 1:
                foreach (var roomType in mediumRoomTypes)
                {
                    if (roomType.Value == threshold || roomType.Value == 0 && roomType.Key != "Crypts")
                    {
                        curValidRoomIndices.Add(index);
                    }
                    index++;
                }
                break;
            default:
                //MM.UpdateHUDDbugText("invalid scale");
                break;
        }

        return curValidRoomIndices.ToArray();
    }



    private IEnumerator GenerateRooms()
    {
        MM.UpdateHUDDbugText("DG, Generate Room");

        //generate rooms
        for (int roomID = 0; roomID < numOfRooms; roomID++)
        {
            //spawn room
            MM.UpdateHUDDbugText("Spawning room " + roomID + " @ " + roomPositions[roomID] + ", size: " + roomBoundsX[roomID] + "*" + roomBoundsZ[roomID]);
            //MM.UpdateHUDDbugText("extents");
            //Debug.Log(/*top left*/(roomPositions[roomID] + new Vector2(0, 0, roomBoundsZ[roomID])) + "\t" + (roomPositions[roomID] + new Vector2(roomBoundsX[roomID], 0, roomBoundsZ[roomID]))/*top right*/);
            //Debug.Log(/*bottom left*/roomPositions[roomID] + "\t" + (roomPositions[roomID] + new Vector2(roomBoundsX[roomID], 0,0))/*bottom right*/);
            Vector3 instantiatePos = new Vector3(roomPositions[roomID].x, 0, roomPositions[roomID].y);
            roomObjects[roomID] = Instantiate(basicRoom, instantiatePos, Quaternion.identity);
            roomObjects[roomID].name = roomScales[roomID] + roomStates[roomID] + roomID;
            roomObjects[roomID].GetComponent<RoomGeneration>().Wake(roomBoundsX[roomID], roomBoundsZ[roomID], roomID, roomScales[roomID], roomStates[roomID]);
            yield return new WaitForSeconds(.1f);
        }
    }
    private bool IsRoomSpecial(int roomID)
    {
        //for each treasure room, if room id is treasure room id, return true
        for (int i = 0; i < treasureRoomIDs.Length; i++)
        {
            if (roomID == treasureRoomIDs[i]) { return true; }
        }
        //for each special room, if room id is special room id, return true
        for (int i = 0; i < specialRoomIDs.Length; i++)
        {
            if (roomID == specialRoomIDs[i]) { return true; }
        }

        //otherwise return false
        return false;
    }
}
