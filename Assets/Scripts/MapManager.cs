using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MapManager : MonoBehaviour
{
    //relevant scripts
    private DungeonGeneration DG;
    private PathGeneration PG;

    //map generation data
    [Header("Generation Data")]
    private int genAttempts = 0;
    [SerializeField] [Range(100, 150)] private int mapBoundsMax = 100;
    [SerializeField] [Range(50, 100)] private int mapBoundsMin = 100;
    [SerializeField] [Range(3, 5)] private int treasureRoomsMax = 3;
    [SerializeField] [Range(1, 3)] private int treasureRoomsMin = 3;
    [SerializeField] [Range(3, 5)] private int specialRoomsMax = 3;
    [SerializeField] [Range(1, 3)] private int specialRoomsMin = 3;

    //debug info
    [Header("Debug Materials")]
    [SerializeField] private Camera defaultCamera;
    [SerializeField] private Material baseDbugMat;
    [SerializeField] private Material matDbugEmpty, matDbugRoom, matDbugWall, matDbugHallway, matDbugDoorway;
    [SerializeField] private Material matDbugRoomBoss, matDbugRoomEntry, matDbugRoomTreasure, matDbugRoomSpecial;

    [Header("Debug Objects")]
    [SerializeField] private GameObject testTile, HUDDbugText;
    private GameObject[,] gridDbug; //grid squares [x bounds, z bounds] (ie. 0,0 = tile 1   25,25 = tile 625)
    private TMP_Text[,] gridDbugText;
    private GameObject dbugParent;
    private Renderer[,] gridDbugRenderer;

    //map creation
    private int boundsX, boundsZ; //map generation
    private int totalSpace; //room generation
    public int GetTotalSpace() { return totalSpace; }

    //map grid
    private string[,] gridStates; //what fills the grid square, if anything
    private Vector2[,] gridPositions; //literal position



    private void ResetMap()
    {
        Debug.Log("MM, Reset Map");

        //clear the debug grid
        for (int x = 0; x < boundsX; x++) { for (int z = 0; z < boundsZ; z++) { if (gridDbug[x, z] != null) { Destroy(gridDbug[x, z]); } } }

        //reset the debug grid materials
        for (int x = 0; x < boundsX; x++) { for (int z = 0; z < boundsZ; z++) { gridDbugRenderer[x, z].material = baseDbugMat; } }

        //clear the debug parent
        if (dbugParent != null) { if (dbugParent != null) { Destroy(dbugParent); } }

        //reset grid states and positions
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (gridStates[x, z] != null) { gridStates[x, z] = "Empty"; }
                if (gridPositions[x, z] != null) { gridPositions[x, z] = new Vector2(((testTile.transform.localScale.x + 0.1f) * x), ((testTile.transform.localScale.z + 0.1f) * z)); }

                //reset debug text
                if (gridDbugText[x, z] != null) { gridDbugText[x, z].text = ""; }
            }
        }
    }



    private void Awake() 
    {
        Debug.Log("MM Awake");

        DG = this.gameObject.GetComponent<DungeonGeneration>();
        PG = this.gameObject.GetComponent<PathGeneration>();
    }
    
    public void RegenerateDungeon()
    {
        Debug.Log("MM, Regenerating Dungeon");

        ResetMap();
        BeginMapGeneration(); 
    }

    private void BeginMapGeneration()
    {
        Debug.Log("MM, Beginning Map Generation");

        Debug.Log("New dungeon generation: " + genAttempts);
        genAttempts++;

        DefineBounds();
        DefineGrid();

        //update camera position based on map size
        defaultCamera.transform.position = new Vector3((boundsX / 2), (((boundsX / 2) + (boundsZ / 2)) * 0.8f), (boundsZ / 2));

        if(genAttempts > 1) { DG.ResetDungeon(); }
        DG.BeginDungeonGeneration(treasureRoomsMax, treasureRoomsMin, specialRoomsMax, specialRoomsMin, boundsX, boundsZ, totalSpace, gridPositions);
    }

    private void DefineBounds()
    {
        Debug.Log("MM, Defining Dungeon Bounds");

        //define bounds
        boundsZ = Random.Range(mapBoundsMin, mapBoundsMax);   //z extent
        boundsX = (int)(boundsZ * 1.25f);   //x extent
        totalSpace = (boundsX * boundsZ); //interior mass
        //Debug.Log("X*Z = total");
        //Debug.Log(boundsX + "*" + boundsZ + " = " + totalSpace);
    }

    private void DefineGrid()
    {
        Debug.Log("MM, Defining Dungeon Grid");

        //define grid & debug grid
        gridStates = new string[boundsX, boundsZ];      //states of grid positions
        gridPositions = new Vector2[boundsX, boundsZ];  //in world grid positions
        gridDbug = new GameObject[boundsX, boundsZ];    //game object array for debug grid
        gridDbugText = new TMP_Text[boundsX, boundsZ];  //text components attached to debug grid objects
        gridDbugRenderer = new Renderer[boundsX, boundsZ];
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

                gridDbugRenderer[xPos, zPos] = gridDbug[xPos, zPos].GetComponent<Renderer>();
                gridDbugRenderer[xPos, zPos].material = matDbugEmpty;
            }
        }
    }

    private int ConvertPosToPosID(Vector2 pos) //convert 2D position to grid position
    {
        //Debug.Log("vector2int: " + pos);
        //Debug.Log("ID: " + (pos.x * boundsZ + pos.y + 1));
        return ((int)pos.x * boundsZ + (int)pos.y) + 1;
    }


    public void UpdateDbugTileTextMoveCost(int posX, int posZ, float moveCost)
    {
        gridDbugText[posX, posZ].text = Regex.Replace(gridDbugText[posX, posZ].text, @"\d+$", moveCost.ToString());
    }

    public void UpdateGridState(int posX, int posZ, string gridState)
    {
        gridStates[posX, posZ] = gridState;
    }
    public string GetGridState(int posX, int posZ) { /*Debug.Log(gridStates[posX, posZ]);*/ return gridStates[posX, posZ]; }

    public void UpdateDbugTileTextGridState(int posX, int posZ, string gridState)
    {
        gridDbugText[posX, posZ].text = gridState;
    }

    public void UpdateDbugTileMat(int posX, int posZ, string matType)
    {
        Material newMat = null;

        switch(matType)
        {
            case null: newMat = baseDbugMat; break;
            case "Empty": newMat = matDbugEmpty; break;
            case "Room": newMat = matDbugRoom; break;
            case "Wall": newMat = matDbugWall; break;
            case "WallCorner": 
                newMat = new Material(matDbugWall);
                newMat.color = newMat.color / 4;
                break;
            case "Hallway": newMat = matDbugHallway; break;
            case "Doorway": newMat = matDbugDoorway; break;
            case "Boss": newMat = matDbugRoomBoss; break;
            case "Entry": newMat = matDbugRoomEntry; break;
            case "Treasure": newMat = matDbugRoomTreasure; break;
            case "Special": newMat = matDbugRoomSpecial; break;
            default: newMat = baseDbugMat; break;
        }

        gridDbugRenderer[posX, posZ].material = newMat;
    }


    public void UpdateHUDDbugText(string newText)
    {
        HUDDbugText.GetComponent<TMP_Text>().text = newText;
    }
}
