using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MapGeneration : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //relevant scripts
    private AbstractSceneManager ASM;
    private DungeonGeneration DG;
    private PathGeneration PG;

    //camera
    [SerializeField] private Camera loadingCamera;
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //map generation data
    [Header("Generation Data")]
    [SerializeField] [Range(10, 250)] private int mapBoundsMax = 50;
    [SerializeField] [Range(10, 250)] private int mapBoundsMin = 50;
    [SerializeField] [Range(0, 5)] private int treasureRoomsMax = 3;
    [SerializeField] [Range(0, 5)] private int treasureRoomsMin = 3;
    [SerializeField] [Range(0, 5)] private int specialRoomsMax = 3;
    [SerializeField] [Range(0, 5)] private int specialRoomsMin = 3;

    //map creation
    private int genAttempts = 0;
    private int boundsX, boundsZ; //map width & length
    public int GetBoundsX() { return boundsX; } //get map width
    public int GetBoundsZ() { return boundsZ; } //get map length
    private int totalSpace; //map area
    public int GetTotalSpace() { return totalSpace; }
    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~grid~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //map grid
    private Vector2[,] gridPositions; //literal positions
    public Vector2 GetGridPosition(int posX, int posZ) { return gridPositions[posX, posZ]; }
    private string[,] gridStates; //what fills the grid square, if anything
    public void UpdateGridState(int posX, int posZ, string gridState) //update tile state
    {
        //Debug.Log("Map Generation: UpdateGridState");
        //Debug.Log("pos: " + posX + ", " + posZ);
        //Debug.Log((posX * posZ) + " / " + gridStates.Length);
        //Debug.Log("gridStates[x,z]: " + gridStates[posX, posZ]);
        gridStates[posX, posZ] = gridState;
    }
    public string GetGridState(int posX, int posZ) { /*Debug.Log(gridStates[posX, posZ]);*/ return gridStates[posX, posZ]; }


    public void ResetMap()
    {
        //Debug.Log("MG, Reset Map");
        if (dbugEnabled) { UpdateHUDDbugText("Dungeon Generation: Resetting Map"); }

        if (dbugEnabled && visualDemo)
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
                if (dbugEnabled && visualDemo) { if (gridDbugText[x, z] != null) { gridDbugText[x, z].text = ""; } }
            }
        }
    }
    //~~~~~grid~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~start~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void Start()
    {
        if (dbugEnabled) { UpdateHUDDbugText("Map Generation: Starting"); }
        //Debug.Log("MG Awake");

        //set up references
        ASM = this.gameObject.GetComponent<AbstractSceneManager>();
        DG = ASM.GetDG();
        PG = ASM.GetPG();

        dbugEnabled = ASM.GetDbugMode();
        visualDemo = ASM.GetVisualMode();

        singleGridSize = testTile.transform.localScale.x + 0.1f;

        if (dbugEnabled) { dbugTextObj.SetActive(true); }
        else if (!dbugEnabled) { dbugTextObj.SetActive(false); }
    }
    //~~~~~start~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void RegenerateDungeon()
    {
        if (dbugEnabled) { UpdateHUDDbugText("Map Generation: Regenerating"); }
        //Debug.Log("MG, Regenerating Dungeon");

        //prime & begin generation
        BeginMapGeneration();
    }

    private void BeginMapGeneration()
    {
        if (dbugEnabled) { UpdateHUDDbugText("Map Generation: Beginning"); }
        //Debug.Log("MG, Beginning Map Generation");

        //Debug.Log("New dungeon generation: " + genAttempts);
        genAttempts++;

        //generate dungeon area & fill space
        DefineBounds();
        DefineGrid();

        //update camera position based on map size
        if (dbugEnabled && visualDemo) { loadingCamera.transform.position = new Vector3((boundsX / 2), (((boundsX / 2) + (boundsZ / 2)) * 0.8f), (boundsZ / 2)); }

        //begin dungeon generation within bounds
        //if(genAttempts > 1) { ASM.RestartScene(); }
        Debug.Log("DG: " + DG);
        DG.BeginDungeonGeneration(treasureRoomsMax, treasureRoomsMin, specialRoomsMax, specialRoomsMin, boundsX, boundsZ, totalSpace, gridPositions);
    }
    private void DefineBounds()
    {
        if (dbugEnabled) { UpdateHUDDbugText("Map Generation: Defining Bounds"); }
        //Debug.Log("MG, Defining Dungeon Bounds");

        //define dungeon area
        boundsZ = Random.Range(mapBoundsMin, mapBoundsMax);   //z extent
        boundsX = (int)(boundsZ * 1.25f);   //x extent
        totalSpace = (boundsX * boundsZ); //interior mass
        //Debug.Log("X*Z = total");
        //Debug.Log(boundsX + "*" + boundsZ + " = " + totalSpace);
    }
    private void DefineGrid()
    {
        if (dbugEnabled) { UpdateHUDDbugText("Map Generation: Defining Grid"); }
        //Debug.Log("MG, Defining Dungeon Grid");

        //fill dungeon area with grid
        gridStates = new string[boundsX, boundsZ];      //states of grid positions
        gridPositions = new Vector2[boundsX, boundsZ];  //in world grid positions
        if (dbugEnabled && visualDemo)
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
                if (dbugEnabled && visualDemo)
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
    public int ConvertPosToPosID(Vector2 pos) //convert 2D position to grid position
    {
        //Debug.Log("vector2int: " + pos);
        //Debug.Log("ID: " + (pos.x * boundsZ + pos.y + 1));
        return ((int)pos.x * boundsZ + (int)pos.y) + 1;
    }
    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Header("Debug Settings")]
    private bool dbugEnabled = false;
    private bool visualDemo = false;

    //visual
    [Header("Debug Visuals")]
    [SerializeField] private GameObject testTile;
    [SerializeField] private Material baseDbugMat;
    [SerializeField] private Material matDbugEmpty, matDbugRoom, matDbugWall, matDbugHallway, matDbugDoorway;
    [SerializeField] private Material matDbugRoomBoss, matDbugRoomEntry, matDbugRoomTreasure, matDbugRoomSpecial;
    private GameObject dbugParent;
    private Renderer[,] gridDbugRenderer;
    private GameObject[,] gridDbug; //grid squares [x bounds, z bounds] (ie. 0,0 = tile 1   25,25 = tile 625)

    private float singleGridSize; //size of a single grid square (x or z) + 0.1f (for spacing)
    public float GetSingleGridSize() { return singleGridSize; } //get size of a single grid square (x or z) + 0.1f (for spacing)


    //text
    [Header("Debug Text")]
    private TMP_Text[,] gridDbugText;

    public void UpdateDbugTileTextMoveCost(int posX, int posZ, float moveCost) //update tile debug text with PathGeneration movement cost (for hallway pathfinding)
    {
        if (dbugEnabled && visualDemo) { gridDbugText[posX, posZ].text = Regex.Replace(gridDbugText[posX, posZ].text, @"\d+$", moveCost.ToString()); }
    }

    public void UpdateDbugTileTextGridState(int posX, int posZ, string gridState) //update tile debug text with tile state
    {
        if (dbugEnabled && visualDemo) 
        {
            //Debug.Log("pos: " + posX + ", " + posZ + " - state: " + gridState + ", legnth: " + gridDbugText.Length);
            //Debug.Log(gridDbugText[posX, posZ]);
            gridDbugText[posX, posZ].text = gridState; 
        }
    }

    public void UpdateDbugTileMat(int posX, int posZ, string matType) //update tile debug material
    {
        if (dbugEnabled && visualDemo)
        {
            Material newMat = null;
            //Debug.Log(matType);
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
                case "DoorwayEdge":
                    newMat = new Material(matDbugDoorway);
                    newMat.color = newMat.color / 4;
                    break;
                case "Boss": newMat = matDbugRoomBoss; break;
                case "Entry": newMat = matDbugRoomEntry; break;
                case "Treasure": newMat = matDbugRoomTreasure; break;
                case "Special": newMat = matDbugRoomSpecial; break;
                default: newMat = baseDbugMat; break;
            }

            gridDbugRenderer[posX, posZ].material = newMat;
        }
    }


    [SerializeField] private GameObject dbugTextObj;
    [SerializeField] private TMP_Text HUDDbugText;
    public void UpdateHUDDbugText(string newText) //update dbug text with DungeonGeneration state updates
    {
        if (dbugEnabled) { HUDDbugText.GetComponent<TMP_Text>().text = newText; }
    }
    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
