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
    private bool[] wallHasDoorway = new bool[4]; //0 - bottom, 1 - top, 2 - left, 3 - right
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

        roomCenter = new Vector3(roomPosX + (roomBoundsX / 2f), 0, roomPosZ + (roomBoundsZ / 2f));
        //https://stackoverflow.com/questions/2304052/check-if-a-number-has-a-decimal-place-is-a-whole-number
        //using modulo (%) 1 checks if there is a decimal component
        //if so, add half a tile to the center to offset the collider
        if((roomBoundsX / 2f) % 1 != 0){roomCenter.x -= (tileXOffset / 2f);}
        if((roomBoundsZ / 2f) % 1 != 0){roomCenter.z -= (tileZOffset / 2f);}

        roomCollider.transform.localPosition = roomCenter;
        roomCollider.transform.localScale = new Vector3(roomBoundsX, 0.5f, roomBoundsZ);

        //if(MG.isDbugEnabled()){dbugText.SetActive(true);}
        //else{dbugText.SetActive(false);}
        dbugText.SetActive(false);

        //set grid positions size to (x*z)
        roomGridPositions = new Vector3[(roomBoundsX * roomBoundsZ)];

        //set tile x & z offsets
        tileXOffset = dbugFloorTile.transform.localScale.x;
        tileZOffset = dbugFloorTile.transform.localScale.z;

        //set first position to current position - this will be used as reference for future tiles
        roomGridPositions[0] = this.transform.position;

        //for(int i = 0; i < roomGridPositions.Length; i++) { Debug.Log("grid pos " + (i + 1) + ": " + roomGridPositions[i]); }
        if (dbugEnabled)
        {
            UpdateGridDbug();
            UpdateTextDbug();
        }

        GenerateFloor();
        GenerateDoorways();
        //GenerateWalls();
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
                        door.transform.position += new Vector3(0, 0, 0);
                        break;
                    case 1:
                        direction = "North"; 
                        doorway.transform.Rotate(0, 90, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0, 1, 0.25f);
                        door.transform.position += new Vector3(0, 0, 0);
                        break;
                }
                switch(edgeID.y) 
                {
                    case -1:
                        direction = "West";
                        doorway.transform.Rotate(0, 0, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(-0.25f, 1, 0);
                        door.transform.position += new Vector3(0, 0, 0);
                        break;
                    case 1:
                        direction = "East";
                        doorway.transform.Rotate(0, 180, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0.25f, 1, 0);
                        door.transform.position += new Vector3(0, 0, 0);
                        break;
                }

                doorway.name = "Room" + roomID + direction + "Doorway";
                door.name = "Room" + roomID + direction + "Door";

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
        //debug
        //Debug.Log(/*top left*/roomGridPositions[(roomGridPositions.Length - roomBoundsX)] + "\t" + roomGridPositions[(roomGridPositions.Length - 1)]/*top right*/);
        //Debug.Log(/*bottom left*/roomGridPositions[0] + "\t" + roomGridPositions[(roomBoundsX - 1)]/*bottom right*/);

        //variables
        Vector3 tempPos;
        float xScale = ((float)roomBoundsZ / 2), zScale = ((float)roomBoundsX / 2);


        //top wall
        tempPos = new Vector3((roomGridPositions[roomGridPositions.Length - 1].x + this.transform.position.x) / 2, 1, roomGridPositions[roomGridPositions.Length - 1].z + 0.375f);
        wallObjects[0] = Instantiate(wallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[0].transform.localScale = new Vector3(wallObjects[0].transform.localScale.x, wallObjects[0].transform.localScale.y, zScale);   //sets the size of the wall
        wallObjects[0].transform.Rotate(0, 90, 0, Space.Self);   //rotates the wall

        //bottom wall
        tempPos = new Vector3((roomGridPositions[roomGridPositions.Length - 1].x + this.transform.position.x) / 2, 1, roomGridPositions[0].z - 0.375f);
        wallObjects[1] = Instantiate(wallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[1].transform.localScale = new Vector3(wallObjects[1].transform.localScale.x, wallObjects[1].transform.localScale.y, zScale);   //sets the size of the wall
        wallObjects[1].transform.Rotate(0, -90, 0, Space.Self);   //rotates the wall


        //left wall
        tempPos = new Vector3(roomGridPositions[0].x - 0.375f, 1, (roomGridPositions[roomGridPositions.Length - 1].z + this.transform.position.z) / 2);
        wallObjects[2] = Instantiate(wallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[2].transform.localScale = new Vector3(wallObjects[2].transform.localScale.x, wallObjects[2].transform.localScale.y, xScale);   //sets the size of the wall
        wallObjects[2].transform.Rotate(0, 0, 0, Space.Self);   //rotates the wall

        //right wall
        tempPos = new Vector3(roomGridPositions[roomGridPositions.Length - 1].x + 0.375f, 1, (roomGridPositions[roomGridPositions.Length - 1].z + this.transform.position.z) / 2);
        wallObjects[3] = Instantiate(wallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[3].transform.localScale = new Vector3(wallObjects[3].transform.localScale.x, wallObjects[3].transform.localScale.y, xScale);   //sets the size of the wall
        wallObjects[3].transform.Rotate(0, 180, 0, Space.Self);   //rotates the wall


        for (int gridPos = 0; gridPos < roomBoundsX; gridPos++){ /*Debug.Log("marking grid" + gridPos + " as Wall");*/ roomGridStates[gridPos] = "Wall"; } //update grid state with wall ID
        for (int wall = 0; wall < wallObjects.Length; wall++) { if (wallObjects[wall] != null) { wallObjects[wall].transform.parent = this.transform; } }
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
