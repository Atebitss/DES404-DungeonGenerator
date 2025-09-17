using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [SerializeField] private GameObject MinimapGridParent;
    [SerializeField] private GameObject MinimapGridPrefab;

    //grid variables
    private int boundsX = 0, boundsZ = 0; //size
    private GameObject[] gridObjects;
    private string[,] gridStates;

    public void Wake(int boundsX, int boundsZ, string[,] gridStates)
    {
        //set variables
        this.boundsX = boundsX;
        this.boundsZ = boundsZ;
        this.gridStates = gridStates;
        gridObjects = new GameObject[(boundsX * boundsZ)];

        GenerateMinimapGrid(); //create grid of appropraite size
        ApplyTileType(); //apply tile types
    }
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void GenerateMinimapGrid()
    {
        if (MinimapGridParent != null)
        {
            //clear old grid
            if (gridObjects[0] != null)
            {
                for (int gridIndex = 0; gridIndex < gridObjects.Length; gridIndex++)
                {
                    if (gridObjects[gridIndex] != null)
                    {
                        Destroy(gridObjects[gridIndex]);
                    }
                }
            }

            //determine scale for grid icons
            RectTransform parentRect = MinimapGridParent.GetComponent<RectTransform>();
            float scaleX = (parentRect.rect.width / boundsX);
            float scaleZ = (parentRect.rect.height / boundsZ);
            MinimapGridParent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(scaleZ, scaleX);

            //generate new grid
            int indexCounter = 0;
            for (int x = 0; x < boundsX; x++)
            {
                for (int z = 0; z < boundsZ; z++)
                {
                    gridObjects[indexCounter] = Instantiate(MinimapGridPrefab, MinimapGridParent.transform);
                    gridObjects[indexCounter].name = "MinimapTile-" + x + "," + z;
                    indexCounter++;
                }
            }
        }
    }


    private void ApplyTileType()
    {
        //assign tile types to grid icons
        int indexCounter = 0;
        for (int x = 0; x < boundsX; x++)
        {
            for (int z = 0; z < boundsZ; z++)
            {
                if (gridObjects[indexCounter] != null)
                {
                    //if grid state entry starts with "Room" followed by a number, simplify to just "Room"
                    if (Regex.IsMatch(gridStates[x, z], @"^Room\d+$")) { gridStates[x, z] = "Room"; } //simplify room types for minimap

                    switch (gridStates[x, z])
                    {
                        case "Empty":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
                            break;
                        case "Room":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0.75f, 1f, 0.75f);
                            break;
                        case "EntryRoom":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0f, 1f, 0f);
                            break;
                        case "BossRoom":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(1f, 0f, 0f);
                            break;
                        case "SpecialRoom":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0.25f, 1f, 1f);
                            break;
                        case "TreasureRoom":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(1f, 1f, 0f);
                            break;
                        case "Doorway":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0f, 0f, 1f);
                            break;
                        case "DoorwayEdge":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0f, 0f, 0.5f);
                            break;
                        case "Wall":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                            break;
                        case "WallCorner":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
                            break;
                        case "Hallway":
                            gridObjects[indexCounter].GetComponent<Image>().color = new Color(0.65f, 0.65f, 0.65f);
                            break;
                        default: //error
                            Debug.Log("missing entry case: " + gridStates[x, z]);
                            gridObjects[indexCounter].GetComponent<Image>().color = Color.magenta;
                            break;
                    }
                }

                indexCounter++;
            }
        }
    }
    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~player display~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void UpdatePlayerRoom(int roomID)
    {
        //---UPDATE TO DISPLAY PLAYER ICON---//
    }
    //~~~~~player display~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}
