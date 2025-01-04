using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class RoomGeneration : MonoBehaviour
{
    //Debug
    [SerializeField] private Material baseDbugMat, emptyDbugMat, wallDbugMat, wallCornerDbugMat;
    [SerializeField] private GameObject dbugMarker, dbugText;
    private GameObject[] dbugGrid;


    //relevant scripts
    private MapManager MM;


    //Generation Info
    private int roomID = 0, roomX = 3, roomZ = 5; //room size
    private string roomSize = "", roomType = "";
    private Vector3[] roomGridPositions;
    private string[] roomGridStates; //current 'state' of grid position (state ie. Wall, Doorway, Corner, Table, Chair)


    //Floor Info
    [SerializeField] private GameObject testFloorTile;
    private GameObject floorObject;
    private float tileXOffset, tileZOffset;


    //Wall Info
    [SerializeField] private GameObject testWallSection, testDoorwaySection;
    private GameObject[] wallObjects = new GameObject[4], doorObjects = new GameObject[4]; //0 - bottom, 1 - top, 2 - left, 3 - right
    private bool[] wallHasDoorway = new bool[4]; //0 - bottom, 1 - top, 2 - left, 3 - right
    private float sectionXOffset, sectionZOffset;



    void Awake()
    {
        this.roomX = 12;
        this.roomZ = 10;
        this.roomID = 0;
        this.roomSize = "Medium";
        this.roomType = "Study";

        //Begin();
    }
    public void Wake(int roomX, int roomZ, int roomID, string roomSize, string roomType)
    {
        this.roomX = roomX;
        this.roomZ = roomZ;
        this.roomID = roomID;
        this.roomSize = roomSize;
        this.roomType = roomType;

        MM = GameObject.Find("SceneManager").gameObject.GetComponent<MapManager>();

        Begin();
    }
    private void Begin()
    {
        Debug.Log("ID: " + roomID + "   size: " + roomSize + "   type: " + roomType + "   x: " + roomX + ", z: " + roomZ);

        //set grid positions size to (x*z)
        roomGridPositions = new Vector3[(roomX * roomZ)];

        //set tile x & z offsets
        tileXOffset = testFloorTile.transform.localScale.x;
        tileZOffset = testFloorTile.transform.localScale.z;

        //set first position to current position - this will be used as reference for future tiles
        roomGridPositions[0] = this.transform.position;

        //for(int i = 0; i < roomGridPositions.Length; i++) { Debug.Log("grid pos " + (i + 1) + ": " + roomGridPositions[i]); }
        GenerateFloor();
        UpdateTextDbug();
        GenerateWalls();
    }



    //generate room grid x by z
    private void GenerateFloor()
    {
        int pos = 0;
        GameObject dbugParent = new GameObject("DebugParent");
        dbugParent.transform.parent = this.transform;
        dbugGrid = new GameObject[(roomX * roomZ)];
        roomGridStates = new string[(roomX * roomZ)];
        for (int i = 0; i < roomGridStates.Length; i++) { /*Debug.Log("marking grid" + i + " as Empty");*/ roomGridStates[i] = "Empty"; }

        //for each position to be generated
        //for each room z
        for (int zPos = 0; zPos < roomZ; zPos++)
        {
            //for each room x
            for (int xPos = 0; xPos < roomX; xPos++)
            {
                //Debug.Log("creating tile " + (pos + 1) + " at x:" + tileXOffset * xPos + ", z:" + tileZOffset * ZPos);
                roomGridPositions[pos] = new Vector3(((tileXOffset * xPos) + this.transform.position.x), 0, ((tileZOffset * zPos) + this.transform.position.z));

                //dbug
                //dbugGrid[pos] = Instantiate(dbugMarker, roomGridPositions[pos], Quaternion.identity);
                //dbugGrid[pos].name = "DebugTile" + pos;
                //dbugGrid[pos].transform.parent = dbugParent.transform;

                pos++;
            }
        }

        float x = ((roomGridPositions[roomGridPositions.Length - 1].x + this.transform.position.x) / 2);
        float z = ((roomGridPositions[roomGridPositions.Length - 1].z + this.transform.position.z) / 2);
        //Debug.Log("X: " + x + ", Z: " + z);

        float boundsX = ((float)roomX / 2);
        float boundsZ = ((float)roomZ / 2);
        //Debug.Log("X: " + boundsX + ", Z: " + boundsZ);

        Vector3 tempPos = new Vector3(x, -1, z);
        floorObject = Instantiate(testFloorTile, tempPos, Quaternion.identity);
        floorObject.transform.localScale = new Vector3(boundsX, floorObject.transform.localScale.y, boundsZ);
        floorObject.transform.parent = this.transform;

        roomGridStates = new string[roomGridPositions.Length];
    }

    private void GenerateWalls()
    {
        //debug
        //Debug.Log(/*top left*/roomGridPositions[(roomGridPositions.Length - roomX)] + "\t" + roomGridPositions[(roomGridPositions.Length - 1)]/*top right*/);
        //Debug.Log(/*bottom left*/roomGridPositions[0] + "\t" + roomGridPositions[(roomX - 1)]/*bottom right*/);

        //variables
        Vector3 tempPos;
        float xScale = ((float)roomZ / 2), zScale = ((float)roomX / 2);


        //top wall
        tempPos = new Vector3((roomGridPositions[roomGridPositions.Length - 1].x + this.transform.position.x) / 2, 1, roomGridPositions[roomGridPositions.Length - 1].z + 0.375f);
        wallObjects[0] = Instantiate(testWallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[0].transform.localScale = new Vector3(wallObjects[0].transform.localScale.x, wallObjects[0].transform.localScale.y, zScale);   //sets the size of the wall
        wallObjects[0].transform.Rotate(0, 90, 0, Space.Self);   //rotates the wall

        //bottom wall
        tempPos = new Vector3((roomGridPositions[roomGridPositions.Length - 1].x + this.transform.position.x) / 2, 1, roomGridPositions[0].z - 0.375f);
        wallObjects[1] = Instantiate(testWallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[1].transform.localScale = new Vector3(wallObjects[1].transform.localScale.x, wallObjects[1].transform.localScale.y, zScale);   //sets the size of the wall
        wallObjects[1].transform.Rotate(0, -90, 0, Space.Self);   //rotates the wall


        //left wall
        tempPos = new Vector3(roomGridPositions[0].x - 0.375f, 1, (roomGridPositions[roomGridPositions.Length - 1].z + this.transform.position.z) / 2);
        wallObjects[2] = Instantiate(testWallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[2].transform.localScale = new Vector3(wallObjects[2].transform.localScale.x, wallObjects[2].transform.localScale.y, xScale);   //sets the size of the wall
        wallObjects[2].transform.Rotate(0, 0, 0, Space.Self);   //rotates the wall

        //right wall
        tempPos = new Vector3(roomGridPositions[roomGridPositions.Length - 1].x + 0.375f, 1, (roomGridPositions[roomGridPositions.Length - 1].z + this.transform.position.z) / 2);
        wallObjects[3] = Instantiate(testWallSection, tempPos, Quaternion.identity);   //creates the new wall
        wallObjects[3].transform.localScale = new Vector3(wallObjects[3].transform.localScale.x, wallObjects[3].transform.localScale.y, xScale);   //sets the size of the wall
        wallObjects[3].transform.Rotate(0, 180, 0, Space.Self);   //rotates the wall


        for (int gridPos = 0; gridPos < roomX; gridPos++){ /*Debug.Log("marking grid" + gridPos + " as Wall");*/ roomGridStates[gridPos] = "Wall"; } //update grid state with wall ID
        for (int wall = 0; wall < wallObjects.Length; wall++) { if (wallObjects[wall] != null) { wallObjects[wall].transform.parent = this.transform; } }
    }



    private void GenerateObjects()
    {

    }



    private void UpdateTextDbug()
    {
        Vector3 tempPos = new Vector3((roomX / 2), 0, (roomZ / 2));
        dbugText.transform.position = dbugText.transform.position + tempPos;
        TMP_Text curText = dbugText.GetComponent<TMP_Text>();
        //float space = MM.GetTotalSpace();
        //curText.fontSize = curText.fontSize + (space / 100000);
        curText.text = "ID: " + roomID + "\nsize: " + roomSize + "\ntype: " + roomType + "\nx: " + roomX + ", z: " + roomZ;
    }



    private void UpdateGridDbug()
    {
        for(int index = 0; index < roomGridStates.Length; index++)
        {
            Renderer curObjRend = dbugGrid[index].GetComponent<Renderer>();

            switch (roomGridStates[index])
            {
                case "Empty":
                    if (curObjRend.material != emptyDbugMat) { curObjRend.material = emptyDbugMat; }
                    break;
                case "Wall":
                    if (curObjRend.material != wallDbugMat) { curObjRend.material = wallDbugMat; }
                    break;
                case "CornerWall":
                    if (curObjRend.material != wallCornerDbugMat) { curObjRend.material = wallCornerDbugMat; }
                    break;
            }
        }
    }
}
