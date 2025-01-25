using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MapGeneration : MonoBehaviour
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
    [Header("Debug Settings")]
    [SerializeField] private bool dbugEnabled = false;
    [SerializeField] private bool showDbugTiles = false;
    public bool isDbugEnabled() { return dbugEnabled; }

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
    private int boundsX, boundsZ; //map width & length
    private int totalSpace; //map area
    public int GetTotalSpace() { return totalSpace; }

    //map grid
    private string[,] gridStates; //what fills the grid square, if anything
    private Vector2[,] gridPositions; //literal positions



    private void ResetMap()
    {
        Debug.Log("MG, Reset Map");

        if (dbugEnabled && showDbugTiles)
        {
            //clear the debug grid
            for (int x = 0; x < boundsX; x++) { for (int z = 0; z < boundsZ; z++) { if (gridDbug[x, z] != null) { Destroy(gridDbug[x, z]); } } }

            //reset the debug grid materials
            for (int x = 0; x < boundsX; x++) { for (int z = 0; z < boundsZ; z++) { gridDbugRenderer[x, z].material = baseDbugMat; } }

            //clear the debug parent
            if (dbugParent != null) { if (dbugParent != null) { Destroy(dbugParent); } }
        }

        //reset grid states and positions
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (gridStates[x, z] != null) { gridStates[x, z] = "Empty"; }
                if (gridPositions[x, z] != null) { gridPositions[x, z] = new Vector2(((testTile.transform.localScale.x + 0.1f) * x), ((testTile.transform.localScale.z + 0.1f) * z)); }

                //reset debug text
                if (dbugEnabled && showDbugTiles) { if (gridDbugText[x, z] != null) { gridDbugText[x, z].text = ""; } }
            }
        }
    }



    private void Awake() 
    {
        Debug.Log("MG Awake");

        //set up references
        DG = this.gameObject.GetComponent<DungeonGeneration>();
        PG = this.gameObject.GetComponent<PathGeneration>();
    }
    
    public void RegenerateDungeon()
    {
        Debug.Log("MG, Regenerating Dungeon");

        //prime & begin generation
        ResetMap();
        BeginMapGeneration(); 
    }

    private void BeginMapGeneration()
    {
        Debug.Log("MG, Beginning Map Generation");

        Debug.Log("New dungeon generation: " + genAttempts);
        genAttempts++;

        //generate dungeon area & fill space
        DefineBounds();
        DefineGrid();

        //update camera position based on map size
        defaultCamera.transform.position = new Vector3((boundsX / 2), (((boundsX / 2) + (boundsZ / 2)) * 0.8f), (boundsZ / 2));

        //begin dungeon generation within bounds
        if(genAttempts > 1) { DG.ResetDungeon(); }
        DG.BeginDungeonGeneration(treasureRoomsMax, treasureRoomsMin, specialRoomsMax, specialRoomsMin, boundsX, boundsZ, totalSpace, gridPositions);
    }

    private void DefineBounds()
    {
        Debug.Log("MG, Defining Dungeon Bounds");

        //define dungeon area
        boundsZ = Random.Range(mapBoundsMin, mapBoundsMax);   //z extent
        boundsX = (int)(boundsZ * 1.25f);   //x extent
        totalSpace = (boundsX * boundsZ); //interior mass
        //Debug.Log("X*Z = total");
        //Debug.Log(boundsX + "*" + boundsZ + " = " + totalSpace);
    }

    private void DefineGrid()
    {
        Debug.Log("MG, Defining Dungeon Grid");

        //fill dungeon area with grid
        gridStates = new string[boundsX, boundsZ];      //states of grid positions
        gridPositions = new Vector2[boundsX, boundsZ];  //in world grid positions
        if (dbugEnabled && showDbugTiles)
        {
            gridDbug = new GameObject[boundsX, boundsZ];    //game object array for debug grid
            gridDbugText = new TMP_Text[boundsX, boundsZ];  //text components attached to debug grid objects
            gridDbugRenderer = new Renderer[boundsX, boundsZ];
            dbugParent = new GameObject("DebugParent");     //debug parent for debug grid objects
        }

        //Debug.Log("boundsX:" + boundsX + ", boundsZ:" + boundsZ);
        //for each grid x
        for (int xPos = 0; xPos < boundsX; xPos++)
        {
            //for each grid z
            for (int zPos = 0; zPos < boundsZ; zPos++)
            {
                //Debug.Log("x: " + xPos + ", z: " + zPos);
                //Debug.Log("creating tile " + (pos + 1) + " at x:" + tileXOffset * xPos + ", z:" + tileZOffset * ZPos);
                gridPositions[xPos, zPos] = new Vector2(((testTile.transform.localScale.x + 0.1f) * xPos), ((testTile.transform.localScale.z + 0.1f) * zPos)); //spawn new grid tile
                gridStates[xPos, zPos] = "Empty"; //update tile state
                //Debug.Log("grid pos: " + gridPositions[xPos, zPos]);

                //debug
                if (dbugEnabled && showDbugTiles)
                {
                    gridDbug[xPos, zPos] = Instantiate(testTile, new Vector3(gridPositions[xPos, zPos].x, 0, gridPositions[xPos, zPos].y), Quaternion.identity); //spawn new debug grid tile
                    gridDbug[xPos, zPos].name = "DbugTile" + ConvertPosToPosID(new Vector2(xPos, zPos)); //name debug tiles with relevant position ID
                    gridDbug[xPos, zPos].transform.parent = dbugParent.transform; //add tile to parent
                    gridDbugText[xPos, zPos] = gridDbug[xPos, zPos].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>(); //set tile debug text

                    gridDbugRenderer[xPos, zPos] = gridDbug[xPos, zPos].GetComponent<Renderer>(); //set tile debug renderer
                    gridDbugRenderer[xPos, zPos].material = matDbugEmpty; //set tile debug material
                }
            }
        }
    }

    private int ConvertPosToPosID(Vector2 pos) //convert 2D position to grid position
    {
        //Debug.Log("vector2int: " + pos);
        //Debug.Log("ID: " + (pos.x * boundsZ + pos.y + 1));
        return ((int)pos.x * boundsZ + (int)pos.y) + 1;
    }


    public void UpdateGridState(int posX, int posZ, string gridState) //update tile state
    {
        gridStates[posX, posZ] = gridState;
    }
    public string GetGridState(int posX, int posZ) { /*Debug.Log(gridStates[posX, posZ]);*/ return gridStates[posX, posZ]; }


    public void UpdateDbugTileTextMoveCost(int posX, int posZ, float moveCost) //update tile debug text with PathGeneration movement cost (for hallway pathfinding)
    {
        if (dbugEnabled && showDbugTiles) { gridDbugText[posX, posZ].text = Regex.Replace(gridDbugText[posX, posZ].text, @"\d+$", moveCost.ToString()); }
    }

    public void UpdateDbugTileTextGridState(int posX, int posZ, string gridState) //update tile debug text with tile state
    {
        if (dbugEnabled && showDbugTiles) { gridDbugText[posX, posZ].text = gridState; }
    }

    public void UpdateDbugTileMat(int posX, int posZ, string matType) //update tile debug material
    {
        if (dbugEnabled && showDbugTiles)
        {
            Material newMat = null;

            switch (matType)
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
    }


    public void UpdateHUDDbugText(string newText) //update dbug text with DungeonGeneration state updates
    {
        if (dbugEnabled) { HUDDbugText.GetComponent<TMP_Text>().text = newText; }
    }
}
