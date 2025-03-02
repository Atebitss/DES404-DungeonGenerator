using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static UnityEditor.PlayerSettings;

public class RoomGeneration : MonoBehaviour
{
    //debug
    [SerializeField] private bool dbugEnabled = false;
    [SerializeField] private Material baseDbugMat, matDbugEmpty, matDbugWall, matDbugDoor;
    [SerializeField] private GameObject dbugMarker, dbugText;
    private GameObject[] dbugGrid;


    //relevant scripts
    private AbstractSceneManager ASM;
    private MapGeneration MG;
    private RoomColliderManager RCM;


    //generation info
    [SerializeField] private Collider roomCollider;
    private int roomID = -1, roomPosX = -1, roomPosZ = -1, roomBoundsX = -1, roomBoundsZ = -1; //room size
    private string roomSize = "", roomType = "";
    private Vector3[] roomGridPositions;
    private Vector3 literalPosition, roomCenter;
    private string[] roomGridStates; //current 'state' of grid position (state ie. Wall, Doorway, Corner, Table, Chair)


    //room info
    private bool entered = false, running = false;
    public bool GetRoomEntered() { return entered; }
    public void SetRoomEntered(bool entered) { this.entered = entered; }
    
    [SerializeField] private GameObject dbugFloorTile;
    private GameObject floorObject;
    private float tileXOffset, tileZOffset;

    private Vector2[] doorPositions;
    private GameObject[] doorObjects = new GameObject[0];
    public GameObject[] GetDoorObjects() { return doorObjects; }
    public void LockDoors()
    {
        for(int i = 0; i < doorObjects.Length; i++)
        {
            doorObjects[i].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().LockDoor();
        }
    }
    public void UnlockDoors()
    {
        for(int i = 0; i < doorObjects.Length; i++)
        {
            doorObjects[i].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().UnlockDoor();
        }
    }

    [SerializeField] private GameObject wallSection, doorwaySection, doorPrefab;
    private GameObject[] wallObjects = new GameObject[0]; //0 - bottom, 1 - top, 2 - left, 3 - right
    private float sectionXOffset, sectionZOffset;


    //enemy info
    private int enemyMin = 4, enemyMax = 7;
    public void SetEnemyMin(int min) { enemyMin = min; }
    public void SetEnemyMax(int max) { enemyMax = max; }



    public void FixedUpdate()
    {
        if(running)
        {
            //Debug.Log("running: " + running + "   enemies: " + ASM.GetEnemyObjects().Length);
            if(ASM.GetEnemyObjects().Length == 0)
            {
                UnlockDoors();
                running = false;
            }
        }
    }
    public void Wake(int roomID, int roomPosX, int roomPosZ, int roomBoundsX, int roomBoundsZ, string roomSize, string roomType, Vector3 literalPosition)
    {
        if (dbugEnabled) { Debug.Log("ID: " + roomID + "   size: " + roomSize + "   type: " + roomType + "   x: " + roomBoundsX + ", z: " + roomBoundsZ); }

        this.roomID = roomID;
        this.roomPosX = roomPosX;
        this.roomPosZ = roomPosZ;
        this.roomBoundsX = roomBoundsX;
        this.roomBoundsZ = roomBoundsZ;
        this.roomSize = roomSize;
        this.roomType = roomType;
        this.literalPosition = literalPosition;

        ASM = GameObject.Find("SceneManager").gameObject.GetComponent<AbstractSceneManager>();
        MG = GameObject.Find("SceneManager").gameObject.GetComponent<MapGeneration>();

        //set tile x & z offsets
        tileXOffset = dbugFloorTile.transform.localScale.x;
        tileZOffset = dbugFloorTile.transform.localScale.z;

        //set room center
        roomCenter = new Vector3(((roomBoundsX / 2f) - (tileXOffset / 2f)), 0, ((roomBoundsZ / 2f) - (tileZOffset / 2f)));

        //move and scale room collider
        roomCollider.transform.localPosition = roomCenter;
        roomCollider.transform.localScale = new Vector3((roomBoundsX - (tileXOffset * 2f)), 0.5f, (roomBoundsZ - (tileZOffset * 2f)));
        roomCollider.GetComponent<RoomColliderManager>().SetColliderVariables(this);

        //if(MG.isDbugEnabled()){dbugText.SetActive(true);}
        //else{dbugText.SetActive(false);}
        dbugText.SetActive(false);

        //set grid positions size to (x*z)
        roomGridPositions = new Vector3[(roomBoundsX * roomBoundsZ)];

        //set first position to current position - this will be used as reference for future tiles
        roomGridPositions[0] = this.transform.position;



        GenerateFloor();
        GenerateDoorways();
        GenerateWalls();


        if (dbugEnabled)
        {
            UpdateGridDbug();
            UpdateTextDbug();
        }
    }
    //generate room grid x by z
    private void GenerateFloor()
    {
        int pos = 0;
        GameObject dbugParent = null;
        roomGridStates = new string[(roomBoundsX * roomBoundsZ)];

        if (dbugEnabled)
        {
            dbugParent = new GameObject("DebugParent");
            dbugParent.transform.parent = this.transform;
            dbugGrid = new GameObject[(roomBoundsX * roomBoundsZ)];
        }

        //for each position to be generated
        //for each room z
        for (int zPos = 0; zPos < roomBoundsZ; zPos++)
        {
            //for each room x
            for (int xPos = 0; xPos < roomBoundsX; xPos++)
            {
                //Debug.Log("creating tile " + (pos + 1) + " at x:" + tileXOffset * xPos + ", z:" + tileZOffset * ZPos);
                roomGridPositions[pos] = new Vector3(literalPosition.x + (tileXOffset * xPos), 0, literalPosition.z + (tileZOffset * zPos));
                roomGridStates[pos] = MG.GetGridState((roomPosX + xPos), (roomPosZ + zPos));

                if (dbugEnabled)
                { 
                    //dbug
                    dbugGrid[pos] = Instantiate(dbugMarker, roomGridPositions[pos], Quaternion.identity);
                    dbugGrid[pos].name = "DebugTile" + pos;
                    dbugGrid[pos].transform.parent = dbugParent.transform;
                }

                pos++;
            }
        }

        //Debug.Log("literal x: " + literalPosition.x + ", literal z: " + literalPosition.z); //x: 139, z: 32
        //Debug.Log("bounds x: " + roomBoundsX + ", bounds z: " + roomBoundsZ); //x: 13, z: 10
        //Debug.Log("bounds x/2: " + ((float)roomBoundsX /2) + ", bounds z/2: " + ((float)roomBoundsZ /2)); //x: 6.5, z: 5
        float x = (literalPosition.x + ((float)(roomBoundsX - 1) / 2)); //145.5
        float z = (literalPosition.z + ((float)(roomBoundsZ - 1) / 2)); //37
        //Debug.Log("x: " + x + ", z: " + z); //x: 145.5, z: 37


        Vector3 tempPos = new Vector3(x, -1, z);
        //Debug.Log("tempPos: " + tempPos);
        floorObject = Instantiate(dbugFloorTile, tempPos, Quaternion.identity);
        floorObject.transform.parent = this.transform.GetChild(1);
        floorObject.transform.localScale = new Vector3(((float)roomBoundsX / 2), floorObject.transform.localScale.y, ((float)roomBoundsZ / 2));
    }
    private void GenerateDoorways()
    {
        int doorCount = 0;

        //check each grid position along room bounds for doorways
        for (int pos = 0; pos < roomGridStates.Length; pos++)
        {
            //check if position is on room edge
            Vector2 edgeID = new Vector2(0, 0);
            if(pos < roomBoundsX){edgeID = new Vector2(-1, 0);} //bottom
            else if(pos >= (roomGridStates.Length - roomBoundsX)){edgeID = new Vector2(1, 0);} //top
            else if(pos % roomBoundsX == 0){edgeID = new Vector2(0, -1);} //left
            else if(pos % roomBoundsX == (roomBoundsX - 1)){edgeID = new Vector2(0, 1);} //right


            //if position is edge and state is doorway
            if (edgeID != new Vector2(0, 0) && roomGridStates[pos] == "Doorway")
            {
                //create door at position
                GameObject doorway = Instantiate(doorwaySection, roomGridPositions[pos], Quaternion.identity);
                doorway.transform.parent = this.transform.GetChild(3); //parent it to the doorway parent

                //Vector3 doorPosition = roomGridPositions[pos] + new Vector3(0.5f, 0, 0.5f); // Offset by 0.5 to get center
                GameObject door = Instantiate(doorPrefab, roomGridPositions[pos], Quaternion.identity);
                door.transform.parent = doorway.transform; //parent it to the door parent

                //name door based on room number and direction
                string direction = "";
                switch(edgeID.x)
                {
                    case -1:
                        direction = "South";
                        doorway.transform.Rotate(0, -90, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0, 1, -0.25f);
                        door.transform.position += new Vector3(0.75f, -1, 0);
                        break;
                    case 1:
                        direction = "North"; 
                        doorway.transform.Rotate(0, 90, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0, 1, 0.25f);
                        door.transform.position += new Vector3(-0.75f, -1, 0);
                        break;
                }
                switch(edgeID.y) 
                {
                    case -1:
                        direction = "West";
                        doorway.transform.Rotate(0, 0, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(-0.25f, 1, 0);
                        door.transform.position += new Vector3(0, -1, -0.75f);
                        break;
                    case 1:
                        direction = "East";
                        doorway.transform.Rotate(0, 180, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0.25f, 1, 0);
                        door.transform.position += new Vector3(0, -1, 0.75f);
                        break;
                }

                doorway.name = "Room" + roomID + direction + "Doorway";
                door.transform.GetChild(0).GetChild(0).name = "Room" + roomID + direction + "Door" + doorCount;
                doorCount++;

                //increase array size by 1 and add door
                GameObject[] newDoorObjects = new GameObject[doorObjects.Length + 1];
                for(int i = 0; i < doorObjects.Length; i++)
                {
                    newDoorObjects[i] = doorObjects[i];
                }
                newDoorObjects[doorObjects.Length] = door;
                doorObjects = newDoorObjects;
            }
        }


    }
    private void GenerateWalls() //~~~FIX THIS LATER, IDK WHAT TO DO WITH THIS~~~
    {
        for (int wallIndex = 0; wallIndex < 4; wallIndex++) // loop through 4 walls
        {
            int[] doorIndexArray = new int[0]; 
            int doorCount = 0; 
            string direction = ""; 

            int wallStartIndex = 0, wallEndIndex = 0, tileStep = 1, wallRotation = 0;
            bool isHorizontal = false;
            float wallOffset = 0.75f;

            Vector3 wallPos = Vector3.zero;
            Vector3 scale = Vector3.one;

            switch (wallIndex)
            {
                case 0: // Top Wall (North)
                    direction = "North";
                    wallStartIndex = roomBoundsX * (roomBoundsZ - 1);
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    isHorizontal = true;
                    wallPos = new Vector3((roomBoundsX / 2f) - (tileXOffset / 2f), 1, roomBoundsZ - wallOffset);
                    scale = new Vector3(1, 1, roomBoundsX / 2f);
                    wallRotation = 90;
                    break;
                case 1: // Bottom Wall (South)
                    direction = "South";
                    wallStartIndex = 0;
                    wallEndIndex = roomBoundsX;
                    isHorizontal = true;
                    wallPos = new Vector3((roomBoundsX / 2f) - (tileXOffset / 2f), 1, (wallOffset - 1));
                    scale = new Vector3(1, 1, roomBoundsX / 2f);
                    wallRotation = -90;
                    break;
                case 2: // Right Wall (East)
                    direction = "East";
                    wallStartIndex = roomBoundsX - 1;
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    tileStep = roomBoundsX;
                    wallPos = new Vector3(roomBoundsX - wallOffset, 1, (roomBoundsZ / 2f) - (tileZOffset / 2f));
                    scale = new Vector3(1, 1, roomBoundsZ / 2f);
                    wallRotation = 180;
                    break;
                case 3: // Left Wall (West)
                    direction = "West";
                    wallStartIndex = 0;
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    tileStep = roomBoundsX;
                    wallPos = new Vector3((wallOffset - 1), 1, (roomBoundsZ / 2f) - (tileZOffset / 2f));
                    scale = new Vector3(1, 1, roomBoundsZ / 2f);
                    wallRotation = 0;
                    break;
            }

            // Find doors in the wall
            for (int tileIndex = wallStartIndex; tileIndex < wallEndIndex; tileIndex += tileStep)
            {
                if (roomGridStates[tileIndex] == "Doorway")
                {
                    int[] tempDoorIndexArray = new int[doorCount + 1];
                    for (int doorID = 0; doorID < doorCount; doorID++)
                    {
                        tempDoorIndexArray[doorID] = doorIndexArray[doorID];
                    }
                    tempDoorIndexArray[doorCount] = tileIndex;
                    doorIndexArray = tempDoorIndexArray;
                    doorCount++;

                    //Debug.Log("door " + doorCount + " found at: " + tileIndex);
                }
            }

            if (doorCount == 0) // If no doors, create a full wall
            {
                GameObject fullWall = Instantiate(wallSection, Vector3.zero, Quaternion.identity);
                fullWall.name = "Room" + roomID + direction + "Wall";
                fullWall.transform.parent = this.transform.GetChild(2);
                fullWall.transform.localPosition = wallPos;
                fullWall.transform.localScale = scale;
                fullWall.transform.Rotate(0, wallRotation, 0, Space.Self);
            }
            /*else // If doors exist, create wall segments
            {
                //if i == 0, create wall segment from start to first door
                //if i > 0, create wall segment from previous door to current door
                //if i == doorCount, create wall segment from last door to end of wall

                float prevEndPos = 0;
                int prevIndex = wallStartIndex;
                int distToNext = 0;

                for(int i = 0; i < doorCount + 1; i++) //for each door on the wall + 1 for the end of the wall
                {
                    Debug.Log("i: " + i + ", doorCount: " + doorCount + ", wallIndex: " + wallIndex);
                    if(wallIndex == 2 || wallIndex == 3)
                    {
                        if(i == doorCount)
                        {
                            distToNext = ((wallEndIndex - doorIndexArray[i - 1]) / roomBoundsX); 
                        }
                        else
                        {
                            distToNext = ((doorIndexArray[i] - prevIndex) / roomBoundsX);
                        }
                    }
                    else
                    {
                        if(i == doorCount)
                        {
                            distToNext = (wallEndIndex - doorIndexArray[i - 1]);
                        }
                        else
                        {
                            distToNext = (doorIndexArray[i] - prevIndex);
                        }
                    }

                    GameObject segmentWall = Instantiate(wallSection, Vector3.zero, Quaternion.identity);
                    segmentWall.name = "Room" + roomID + direction + "WallSegment" + i;
                    segmentWall.transform.parent = this.transform.GetChild(2);

                    switch(wallIndex)
                    {
                        case 0: //top wall (north)
                            if(i != 0 && i != doorCount)
                            {
                                Debug.Log("doorIndexArray[i]: " + doorIndexArray[i] + ", prevIndex: " + prevIndex);
                                if((doorIndexArray[i] - prevIndex) == 3)
                                {
                                    Debug.Log("wall segment " + i + " destroyed");
                                    Destroy(segmentWall);
                                    break;
                                }
                            }

                            if(i == 0) //if first segment
                            {
                                segmentWall.name = "i == 0";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, roomBoundsZ - wallOffset);
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset / 2f))); 
                                prevIndex = doorIndexArray[i]; 
                            }
                            else if(i > 0 && i < doorCount && doorCount > 1 ) //if middle segment and there is more than one door
                            {
                                //~~~POSITION (7, -1, 4) & SCALE (7.5, 1, 4.5), SPAWNS WALLS FINE~~~
                                segmentWall.name = "i > 0 && i < doorCount && doorCount > 1";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - (tileXOffset * 1.5f)), 1, roomBoundsZ - wallOffset);
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset * 1.5f))); 
                                prevIndex = doorIndexArray[i];
                            }
                            else if(i == doorCount && doorCount == 1) //if last segment and there is only one door
                            {
                                segmentWall.name = "i == doorCount && doorCount == 1";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, roomBoundsZ - wallOffset);
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = wallEndIndex; 
                            }
                            else if(i == doorCount && doorCount > 1) //if last segment and there is more than one door
                            {
                                //~~~POSITION (7, -1, 4) & SCALE (7.5, 1, 4.5), SPAWNS WALLS FINE~~~
                                //~~~POSITION (6.5, -1, 4) & SCALE (7, 1, 4.5), SPAWNS WALL WITHOUT OFFSET~~~
                                //~~~POSITION (7, -1, 4) & SCALE (7.5, 1, 4.5), SPAWNS WALL HALF WAY THROUGH DOOR~~~
                                segmentWall.name = "i == doorCount && doorCount > 1";
                                if(doorIndexArray[i-1] - doorIndexArray[i-2] == 3) 
                                {
                                    // Adjacent doors, adjust wall center position
                                    //wallPos = new Vector3((prevEndPos - (distToNext / 2f)) + tileXOffset, 1, roomBoundsZ - wallOffset);
                                } 
                                else 
                                {
                                    wallPos = new Vector3((prevEndPos - (distToNext / 2f)) + (tileXOffset / 2f), 1, roomBoundsZ - wallOffset);
                                }
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = wallEndIndex; 
                            }
                            else //if not first or segment and there is less than one door (shouldnt happen)
                            {
                                segmentWall.name = "else";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, roomBoundsZ - wallOffset);
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = doorIndexArray[i]; 
                            }
                            break;
                        case 1: //bottom wall (south)
                            if(i != 0 && i != doorCount)
                            {
                                Debug.Log("doorIndexArray[i]: " + doorIndexArray[i] + ", prevIndex: " + prevIndex);
                                if((doorIndexArray[i] - prevIndex) == 3)
                                {
                                    Debug.Log("wall segment " + i + " destroyed");
                                    Destroy(segmentWall);
                                    break;
                                }
                            }

                            
                            
                            wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, (wallOffset - 1));
                            if(i == 0){ scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset / 2f))); prevIndex = doorIndexArray[i]; }
                            else if(i == doorCount) { scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); prevIndex = wallEndIndex; }
                            else{ scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); prevIndex = doorIndexArray[i]; }


                            if(i == 0) //if first segment
                            {
                                segmentWall.name = "i == 0";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, (wallOffset - 1));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset / 2f))); 
                                prevIndex = doorIndexArray[i]; 
                            }
                            else if(i == doorCount && doorCount == 1) //if last segment and there is only one door
                            {
                                segmentWall.name = "i == doorCount && doorCount == 1";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, (wallOffset - 1));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = wallEndIndex; 
                            }
                            else if(i == doorCount && doorCount > 1) //if last segment and there is more than one door
                            {
                                segmentWall.name = "i == doorCount && doorCount > 1";
                                Debug.Log("distToNext: " + distToNext + ", prevEndPos: " + prevEndPos + ", tileXOffset: " + tileXOffset);
                                wallPos = new Vector3((prevEndPos - (distToNext / 2f) + (tileXOffset / 2f)), 1, (wallOffset - 1));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = wallEndIndex; 
                            }
                            else if(i > 0 && i < doorCount && doorCount > 1 ) //if middle segment and there is more than one door
                            {
                                segmentWall.name = "i > 0 && i < doorCount && doorCount > 1";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - (tileXOffset * 1.5f)), 1, (wallOffset - 1));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset * 1.5f))); 
                                prevIndex = wallEndIndex;
                            }
                            else //if not first or segment and there is less than one door (shouldnt happen)
                            {
                                segmentWall.name = "else";
                                wallPos = new Vector3((prevEndPos + (distToNext / 2f) - tileXOffset), 1, (wallOffset - 1));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileXOffset)); 
                                prevIndex = doorIndexArray[i]; 
                            }
                            break;
                        case 2: //right wall (east)
                            if(i == doorCount) 
                            {
                                wallPos = new Vector3(roomBoundsX - wallOffset, 1, (prevEndPos + ((distToNext / 2f) - (tileZOffset / 2f))));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileZOffset / 2f))); 
                                prevIndex = wallEndIndex;
                            }
                            else
                            {
                                wallPos = new Vector3(roomBoundsX - wallOffset, 1, (prevEndPos + ((distToNext / 2f) - tileZOffset)));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileZOffset / 2f))); 
                                prevIndex = doorIndexArray[i];
                            }
                            break;
                        case 3: //left wall (west)
                            if(i == doorCount) 
                            {
                                wallPos = new Vector3((wallOffset - 1), 1, (prevEndPos + ((distToNext / 2f) - tileZOffset)));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - tileZOffset)); 
                                prevIndex = wallEndIndex;
                            }
                            else
                            {
                                wallPos = new Vector3((wallOffset - 1), 1, (prevEndPos + ((distToNext / 2f) - tileZOffset)));
                                scale = new Vector3(1, 1, ((distToNext / 2f) - (tileZOffset / 2f))); 
                                prevIndex = doorIndexArray[i];
                            }
                            break;
                    }

                    segmentWall.transform.localPosition = wallPos;
                    segmentWall.transform.localScale = scale;
                    segmentWall.transform.Rotate(0, wallRotation, 0, Space.Self);

                    prevEndPos += (distToNext + (tileXOffset * 1.5f));
                }
            }*/
        }
    }



    private void GenerateObjects()
    {

    }



    private void UpdateTextDbug()
    {
        Vector3 tempPos = new Vector3((roomBoundsX / 2), 0, (roomBoundsZ / 2));
        dbugText.transform.position = dbugText.transform.position + tempPos;
        TMP_Text curText = dbugText.GetComponent<TMP_Text>();
        //float space = MG.GetTotalSpace();
        //curText.fontSize = curText.fontSize + (space / 100000);
        curText.text = "ID: " + roomID + "\nsize: " + roomSize + "\ntype: " + roomType + "\nx: " + roomBoundsX + ", z: " + roomBoundsZ;
    }
    private void UpdateGridDbug()
    {
        for (int index = 0; index < roomGridStates.Length; index++)
        {
            Renderer curObjRend = dbugGrid[index].GetComponent<Renderer>();

            switch (roomGridStates[index])
            {
                case "Empty":
                    if (curObjRend.material != matDbugEmpty) { curObjRend.material = matDbugEmpty; }
                    break;
                case "Wall":
                    if (curObjRend.material != matDbugWall) { curObjRend.material = matDbugWall; }
                    break;
                case "WallCorner":
                    if (curObjRend.material != matDbugWall)
                    {
                        Material newMat = new Material(matDbugWall);
                        newMat.color = newMat.color / 4; 
                        curObjRend.material = newMat; 
                    }
                    break;
                case "Doorway":
                    if (curObjRend.material != matDbugDoor) { curObjRend.material = matDbugDoor; }
                    break;
                case "DoorwayEdge":
                    if (curObjRend.material != matDbugDoor)
                    {
                        Material newMat = new Material(matDbugDoor);
                        newMat.color = newMat.color / 4; 
                        curObjRend.material = newMat; 
                    }
                    break;
            }
        }
    }



    private bool playerInRoom = false; //used to check if player is still in room before starting combat
    public void SetPlayerInRoom(bool inRoom) { playerInRoom = inRoom; } //updated by RoomColliderManager
    public IEnumerator RoomEntered() //called by RoomColliderManager when player enters room
    {
        yield return new WaitForSeconds(0.25f); //wait for player to enter room
        //Debug.Log("playerInRoom: " + playerInRoom + ", entered: " + entered);
        if(playerInRoom && !entered)
        {
            //if player is in room and room is not entered, start appropriate room event
            Debug.Log("roomType: " + roomType);
            if(!roomType.Contains("Entry") && !roomType.Contains("Treasure") && !roomType.Contains("Special") && !roomType.Contains("Boss")) {StartCombat();} 
            else if(roomType.Contains("Treasure")){StartTreasure(); }
            else if(roomType.Contains("Special")){StartSpecial();}
            else if(roomType.Contains("Entry")){StartEntry(); }
            else if(roomType.Contains("Boss")){StartBoss();}
        } 
    }



    private void StartCombat()
    {
        //update room state
        entered = true;
        running = true;

        //lock doors
        for(int i = 0; i < doorObjects.Length; i++)
        {
            doorObjects[i].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().LockDoor();
        }

        //generate enemies
        GenerateEnemies();
    }

    private void StartTreasure()
    {
        //update room state
        entered = true;

        //spawn treasure
    }

    private void StartSpecial()
    {
        //update room state
        entered = true;

        //spawn special
    }

    private void StartEntry()
    {
        //update room state
        entered = true;

        //run intro
    }

    private void StartBoss()
    {
        //update room state
        entered = true;

        //generate boss

        //generate enemies
        GenerateEnemies();
    }    
    


    private void GenerateEnemies()
    {
        //start combat
        int enemyCount = Random.Range(enemyMin, enemyMax);
        Vector3[] enemyPositions = new Vector3[enemyCount];

        for(int currentEnemyIndex = 0; currentEnemyIndex < (enemyCount - 1); currentEnemyIndex++)
        {
            int attempts = 0, maxAttempts = 3;
            bool enemySpawned = false;
            while(attempts < maxAttempts && !enemySpawned)
            {
                //find random position within room
                Vector3 randomPos = new Vector3(Random.Range(0, roomBoundsX), 0.5f, Random.Range(0, roomBoundsZ));
                randomPos.x += literalPosition.x;
                randomPos.z += literalPosition.z;

                //find if distance between generated position and player is less than 1/100th of the room size
                Vector3 playerPos = ASM.GetPlayerPosition();
                bool playerNearBy = Vector3.Distance(randomPos, playerPos) > ((roomBoundsX * roomBoundsZ) * 0.25f);

                //check radius around position for walls
                bool wallInRadius = false;
                float checkRadius = 1f;
                Collider[] hitColliders = Physics.OverlapSphere(randomPos, checkRadius);
                for(int colliderIndex = 0; colliderIndex < hitColliders.Length; colliderIndex++)
                {
                    if(hitColliders[colliderIndex].gameObject.CompareTag("Wall"))
                    {
                        wallInRadius = true;
                        break;
                    }
                }

                //check radius around position for other enemies
                bool enemyInRadius = false;
                Collider[] enemyColliders = Physics.OverlapSphere(randomPos, checkRadius);
                for(int enemyIndex = 0; enemyIndex < enemyColliders.Length; enemyIndex++) 
                {
                    if(enemyColliders[enemyIndex].gameObject.CompareTag("Enemy"))
                    {
                        enemyInRadius = true;
                        break;
                    }
                }

                //if player is not near by, enemy is not in radius, and there are no walls in radius, spawn enemy
                if(!playerNearBy && !enemyInRadius && !wallInRadius)
                {
                    //Debug.Log("spawning enemy at: " + randomPos);
                    //Debug.Log("playerNearBy: " + playerNearBy + ", enemyInRadius: " + enemyInRadius + ", wallInRadius: " + wallInRadius);
                    enemyPositions[currentEnemyIndex] = randomPos;
                    enemySpawned = true;
                    break;
                }
                else
                {
                    //Debug.Log("failed to spawn enemy, attempts: " + attempts);
                    //Debug.Log("playerNearBy: " + playerNearBy + ", enemyInRadius: " + enemyInRadius + ", wallInRadius: " + wallInRadius);
                    attempts++;
                }
            }

            if(!enemySpawned)
            {
                //Debug.Log("failed to spawn enemy, attempts: " + attempts);
                
                // Create new array with one less position
                Vector3[] newPositions = new Vector3[enemyPositions.Length - 1];
                
                // Copy all positions before current index
                for(int positionIndex = 0; positionIndex < currentEnemyIndex; positionIndex++)
                {
                    newPositions[positionIndex] = enemyPositions[positionIndex];
                }
                
                enemyPositions = newPositions;
                currentEnemyIndex--; // Decrement index to try this position again with new array
            }
        }

        //Debug.Log("enemyPositions: " + enemyPositions.Length);
        ASM.SpawnEnemies(enemyPositions);
    }
}