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


    //generation info
    [SerializeField] private Collider roomCollider;
    private int roomID = -1, roomPosX = -1, roomPosZ = -1, roomBoundsX = -1, roomBoundsZ = -1; //room size
    private string roomSize = "", roomType = "";
    private Vector3[] roomGridPositions;
    private Vector3 literalPosition, roomCenter;
    private string[] roomGridStates; //current 'state' of grid position (state ie. Wall, Doorway, Corner, Table, Chair)


    //room info
    private bool entered = false;

    [SerializeField] private GameObject dbugFloorTile;
    private GameObject floorObject;
    private float tileXOffset, tileZOffset;

    private Vector2[] doorPositions;
    private GameObject[] doorObjects = new GameObject[4];
    public GameObject[] GetDoorObjects() { return doorObjects; }

    [SerializeField] private GameObject wallSection, doorwaySection, doorPrefab;
    private GameObject[] wallObjects = new GameObject[4]; //0 - bottom, 1 - top, 2 - left, 3 - right
    private float sectionXOffset, sectionZOffset;


    //enemy info
    private int enemyCount = 0;
    public int GetEnemyCount() { return enemyCount; }
    private int enemyMin = 0, enemyMax = 4;
    public void SetEnemyMin(int min) { enemyMin = min; }
    public void SetEnemyMax(int max) { enemyMax = max; }



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
        roomCollider.transform.localScale = new Vector3(roomBoundsX, 0.5f, roomBoundsZ);

        //if(MG.isDbugEnabled()){dbugText.SetActive(true);}
        //else{dbugText.SetActive(false);}
        dbugText.SetActive(false);

        //set grid positions size to (x*z)
        roomGridPositions = new Vector3[(roomBoundsX * roomBoundsZ)];

        //set first position to current position - this will be used as reference for future tiles
        roomGridPositions[0] = this.transform.position;

        //for(int i = 0; i < roomGridPositions.Length; i++) { Debug.Log("grid pos " + (i + 1) + ": " + roomGridPositions[i]); }
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

                //find first null position in array and add doorway
                for(int i = 0; i < doorObjects.Length; i++) 
                {
                    if(doorObjects[i] == null) 
                    {
                        doorObjects[i] = door;
                        break;
                    }
                }
            }
        }


    }
    private void GenerateWalls()
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
                    wallPos = new Vector3(wallOffset, 1, (roomBoundsZ / 2f) - (tileZOffset / 2f));
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
            else // If doors exist, create wall segments
            {
                //if i == 0, create wall segment from start to first door
                //if i > 0, create wall segment from previous door to current door
                //if i == doorCount, create wall segment from last door to end of wall

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
                            Debug.Log("distToNext: " + distToNext + ", wallEndIndex: " + wallEndIndex + ", doorIndexArray[i]: " + doorIndexArray[i - 1]);
                        }
                        else
                        {
                            distToNext = ((doorIndexArray[i] - prevIndex) / roomBoundsX);
                            Debug.Log("distToNext: " + distToNext + ", prevIndex: " + prevIndex + ", doorIndexArray[i]: " + doorIndexArray[i]);
                        }
                    }
                    else
                    {
                        if(i == doorCount)
                        {
                            distToNext = (wallEndIndex - doorIndexArray[i - 1]);
                            Debug.Log("distToNext: " + distToNext + ", wallEndIndex: " + wallEndIndex + ", doorIndexArray[i]: " + doorIndexArray[i - 1]);
                        }
                        else
                        {
                            distToNext = (doorIndexArray[i] - prevIndex);
                            Debug.Log("distToNext: " + distToNext + ", prevIndex: " + prevIndex + ", doorIndexArray[i]: " + doorIndexArray[i]);
                        }
                    }


                    GameObject segmentWall = Instantiate(wallSection, Vector3.zero, Quaternion.identity);
                    segmentWall.name = "Room" + roomID + direction + "WallSegment" + i;
                    segmentWall.transform.parent = this.transform.GetChild(2);

                    switch(wallIndex)
                    {
                        case 0: //top wall (north)
                            wallPos = new Vector3(((distToNext / 2f) - tileXOffset), 1, roomBoundsZ - wallOffset);
                            scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset / 2f)));
                            break;
                        case 1: //bottom wall (south)
                            wallPos = new Vector3(((distToNext / 2f) - tileXOffset), 1, (wallOffset - 1));
                            scale = new Vector3(1, 1, ((distToNext / 2f) - (tileXOffset / 2f)));
                            break;
                        case 2: //right wall (east)
                            wallPos = new Vector3(roomBoundsX - wallOffset, 1, ((distToNext / 2f) - tileZOffset));
                            scale = new Vector3(1, 1, ((distToNext / 2f) - (tileZOffset / 2f)));
                            break;
                        case 3: //left wall (west)
                            wallPos = new Vector3((wallOffset - 1), 1, ((distToNext / 2f) - tileZOffset));
                            scale = new Vector3(1, 1, ((distToNext / 2f) - (tileZOffset / 2f)));
                            break;
                    }

                    segmentWall.transform.localPosition = wallPos;
                    segmentWall.transform.localScale = scale;
                    segmentWall.transform.Rotate(0, wallRotation, 0, Space.Self);

                    //add something that tracks previous distance used to use as reference for next start position
                }
            }
        }
    }



    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player" && !entered)
        {
            //lock doors and start combat
            entered = true;

            //lock doors
            for(int i = 0; i < doorObjects.Length; i++)
            {
                AbstractDoorScript curDoorScript = doorObjects[i].GetComponent<AbstractDoorScript>();
                curDoorScript.LockDoor();
                //run animation
            }

            //start combat
            enemyCount = Random.Range(enemyMin, enemyMax);
            Vector3 playerPos = ASM.GetPlayerPosition();
            Vector3[] enemyPositions = new Vector3[enemyCount];
            for(int i = 0; i < enemyCount; i++)
            {
                //find random position within room
                //check radius around player, if enemy is out with radius
                //check radius around position for walls, if no walls
                //check radius around position for other enemies, if no enemies
                //spawn enemy
            }
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
            }
        }
    }
}
