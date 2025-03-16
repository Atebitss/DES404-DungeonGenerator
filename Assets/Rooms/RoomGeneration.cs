using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static UnityEditor.PlayerSettings;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;

public class RoomGeneration : MonoBehaviour
{
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //relevant scripts
    private AbstractSceneManager ASM;
    private MapGeneration MG;
    private RoomColliderManager RCM;
    private AdaptiveDifficultyManager ADM;
    //~~~~~misc~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~running~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //room info
    [SerializeField] private bool entered = false, running = false;
    public bool GetRoomEntered() { return entered; }
    public void SetRoomEntered(bool entered) { this.entered = entered; }
    public void FixedUpdate()
    {
        if(running)
        {
            //Debug.Log("running: " + running + "   enemies: " + ASM.GetEnemyObjects().Length);
            if(ASM.GetEnemyObjects().Length == 0)
            {
                if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Room Complete"); }
                if (bossRoom) //if boss room over, spawn way down
                {
                    portalObject = Instantiate(portalPrefab, (literalPosition + roomCenter), Quaternion.identity);
                    portalObject.GetComponent<PortalManager>().SetASM(ASM);
                    portalObject.transform.parent = this.transform.GetChild(1);
                }
                UnlockDoors();
                //open doors locked pre combat
                if (doorClosedPreCombat != null) { doorClosedPreCombat.transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().InteractWithDoor(); }
                running = false;

                if (ADM != null) 
                {
                    ADM.RoomCleared();
                    ADM.RunDifficultyAdapter(); 
                }
            }
        }
    }
    //~~~~~running~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //generation info
    [SerializeField] private Collider roomCollider;
    [SerializeField] private int roomID = -1, roomPosX = -1, roomPosZ = -1, roomBoundsX = -1, roomBoundsZ = -1; //room size
    [SerializeField] private string roomSize = "", roomType = "";
    private string[] roomGridStates; //current 'state' of grid position (state ie. Wall, Doorway, Corner, Table, Chair)

    //floors
    [SerializeField] private GameObject floorPrefab;
    private GameObject floorObject;
    private float tileXOffset, tileZOffset;
    [SerializeField] private Vector3[] roomGridPositions;
    [SerializeField] private Vector3 literalPosition, roomCenter;

    //walls
    [SerializeField] private GameObject wallSectionPrefab;
    private GameObject[] wallObjects = new GameObject[0]; //0 - bottom, 1 - top, 2 - left, 3 - right
    private float sectionXOffset, sectionZOffset;

    //doors
    [SerializeField] private GameObject doorwaySectionPrefab, doorPrefab;
    private Vector2[] doorPositions;
    private GameObject[] doorObjects = new GameObject[0];
    public GameObject[] GetDoorObjects() { return doorObjects; }
    private GameObject doorClosedPreCombat;
    public void LockDoors()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Locking Doors"); }
        for (int i = 0; i < doorObjects.Length; i++)
        {
            doorObjects[i].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().LockDoor();
        }
    }
    public void UnlockDoors()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Unlocking Doors"); }
        for (int i = 0; i < doorObjects.Length; i++)
        {
            doorObjects[i].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>().UnlockDoor();
        }
    }

    //portal
    [SerializeField] private GameObject portalPrefab;
    private GameObject portalObject;
    private Vector2 portalPos;
    public GameObject GetPortalObject() { return portalObject; }



    public void Wake(int roomID, int roomPosX, int roomPosZ, int roomBoundsX, int roomBoundsZ, string roomSize, string roomType, Vector3 literalPosition)
    {
        //if (dbugEnabled) { Debug.Log("ID: " + roomID + "   size: " + roomSize + "   type: " + roomType + "   x: " + roomBoundsX + ", z: " + roomBoundsZ); }

        this.roomID = roomID;
        this.roomPosX = roomPosX;
        this.roomPosZ = roomPosZ;
        this.roomBoundsX = roomBoundsX;
        this.roomBoundsZ = roomBoundsZ;
        this.roomSize = roomSize;
        this.roomType = roomType;
        this.literalPosition = literalPosition;

        ASM = GameObject.Find("SceneManager").gameObject.GetComponent<AbstractSceneManager>();
        MG = ASM.gameObject.GetComponent<MapGeneration>();
        ADM = ASM.gameObject.GetComponent<AdaptiveDifficultyManager>();

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

        GetValidEnemyTypes();
        GetValidBossTypes();

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
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Generating Floor"); }
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
        floorObject = Instantiate(floorPrefab, tempPos, Quaternion.identity);
        floorObject.transform.parent = this.transform.GetChild(1);
        floorObject.transform.localScale = new Vector3(((float)roomBoundsX / 2), floorObject.transform.localScale.y, ((float)roomBoundsZ / 2));
    }
    private void GenerateDoorways()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Generating Doorways"); }
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
                GameObject doorway = Instantiate(doorwaySectionPrefab, roomGridPositions[pos], Quaternion.identity);
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
                        doorway.transform.position += new Vector3(0, 1, -0.75f);
                        door.transform.position += new Vector3(0.75f, -1, 0.35f);
                        break;
                    case 1:
                        direction = "North"; 
                        doorway.transform.Rotate(0, 90, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0, 1, 0.75f);
                        door.transform.position += new Vector3(-0.75f, -1, -0.35f);
                        break;
                }
                switch(edgeID.y) 
                {
                    case -1:
                        direction = "West";
                        doorway.transform.Rotate(0, 0, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(-0.75f, 1, 0);
                        door.transform.position += new Vector3(0.35f, -1, -0.75f);
                        break;
                    case 1:
                        direction = "East";
                        doorway.transform.Rotate(0, 180, 0, Space.Self);
                        door.transform.Rotate(0, 90, 0, Space.Self);
                        doorway.transform.position += new Vector3(0.75f, 1, 0);
                        door.transform.position += new Vector3(-0.35f, -1, 0.75f);
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
    private void GenerateWalls()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Generating Walls"); }
        for (int wallIndex = 0; wallIndex < 4; wallIndex++) // loop through 4 walls
        {
            string direction = "";
            int wallStartIndex = 0, wallEndIndex = 0, tileStep = 1, wallRotation = 0;
            Vector3 wallOffset = Vector2.zero;

            switch (wallIndex)
            {
                case 0: // Top Wall (North)
                    direction = "North";
                    wallStartIndex = roomBoundsX * (roomBoundsZ - 1);
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    wallOffset = new Vector3(0, 0, 0.25f);
                    wallRotation = 90;
                    tileStep = 1;
                    break;
                case 1: // Bottom Wall (South)
                    direction = "South";
                    wallStartIndex = 0;
                    wallEndIndex = roomBoundsX;
                    wallOffset = new Vector3(0, 0, -0.25f);
                    wallRotation = -90;
                    tileStep = 1;
                    break;
                case 2: // Right Wall (East)
                    direction = "East";
                    wallStartIndex = roomBoundsX - 1;
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    wallOffset = new Vector3(0.25f, 0, 0);
                    wallRotation = 180;
                    tileStep = roomBoundsX;
                    break;
                case 3: // Left Wall (West)
                    direction = "West";
                    wallStartIndex = 0;
                    wallEndIndex = roomBoundsX * roomBoundsZ;
                    wallOffset = new Vector3(-0.25f, 0, 0);
                    wallRotation = 0;
                    tileStep = roomBoundsX;
                    break;
            }

            //create wall sections along the wall
            for (int tileIndex = wallStartIndex; tileIndex < wallEndIndex; tileIndex += tileStep)
            {
                if (roomGridStates[tileIndex] == "Wall" || roomGridStates[tileIndex] == "WallCorner")
                {
                    //create wall segment
                    GameObject wallSection = Instantiate(wallSectionPrefab, (roomGridPositions[tileIndex] + wallOffset), Quaternion.identity);
                    wallSection.name = "Room" + roomID + direction + "WallSegment" + tileIndex;
                    wallSection.transform.parent = this.transform.GetChild(2);
                    wallSection.transform.Rotate(0, wallRotation, 0, Space.Self);
                }
            }
        }
    }
    //~~~~~generation~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~room interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private bool playerInRoom = false; //used to check if player is still in room before starting combat
    public void SetPlayerInRoom(bool inRoom) { playerInRoom = inRoom; } //updated by RoomColliderManager
    public IEnumerator RoomEntered() //called by RoomColliderManager when player enters room
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Room Entered"); }
        yield return new WaitForSeconds(0.25f); //wait for player to enter room
        //Debug.Log("playerInRoom: " + playerInRoom + ", entered: " + entered);

        if (ASM.GetADDM() != null) { ASM.GetADDM().currentDifficulty = roomDifficulty; }

        if (playerInRoom && !entered)
        {
            //if player is in room and room is not entered, start appropriate room event
            //Debug.Log("roomType: " + roomType);
            if(!roomType.Contains("Entry") && !roomType.Contains("Treasure") && !roomType.Contains("Special") && !roomType.Contains("Boss")) {StartCombat();} 
            else if(roomType.Contains("Treasure")){StartTreasure(); }
            else if(roomType.Contains("Special")){StartSpecial();}
            else if(roomType.Contains("Entry")){StartEntry(); }
            else if(roomType.Contains("Boss")){StartBoss();}
        } 
    }


    private void StartCombat()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Starting Combat Room"); }
        //update room state
        entered = true;
        running = true;

        //lock doors
        for(int doorIndex = 0; doorIndex < doorObjects.Length; doorIndex++)
        {
            AbstractDoorScript curADS = doorObjects[doorIndex].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>();

            if (curADS.GetIsOpen()) { doorClosedPreCombat = doorObjects[doorIndex]; }
            curADS.LockDoor();
        }

        //start adaptive difficulty stat watcher
        if (ADM != null) { ADM.StartStatWatch(this); }

        //generate enemies
        GenerateEnemies();
    }

    private void StartBoss()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Starting Boss Room"); }
        //update room state
        entered = true;
        running = true;
        bossRoom = true;

        //lock doors
        for (int doorIndex = 0; doorIndex < doorObjects.Length; doorIndex++)
        {
            AbstractDoorScript curADS = doorObjects[doorIndex].transform.GetChild(0).transform.GetChild(0).GetComponent<AbstractDoorScript>();

            if (curADS.GetIsOpen()) { doorClosedPreCombat = doorObjects[doorIndex]; }
            curADS.LockDoor();
        }

        //start adaptive difficulty stat watcher
        if (ADM != null) { ADM.StartStatWatch(this); }

        //generate boss
        GenerateBoss();

        //generate enemies
        GenerateEnemies();
    }  


    private void StartTreasure()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Starting Treasure Room"); }
        //update room state
        entered = true;

        //spawn treasure
    }

    private void StartSpecial()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Starting Special Room"); }
        //update room state
        entered = true;

        //spawn special
    }


    private void StartEntry()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Starting Entry Room"); }
        //update room state
        entered = true;

        //run intro
    }
    //~~~~~room interaction~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~enemies~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //adaptive difficulty
    [SerializeField] private int roomDifficulty = 0;
    public void SetRoomDifficulty(int newDifficulty) { roomDifficulty = newDifficulty; }
    [SerializeField] private float playerSkillScore = 0f;
    public void SetPlayerSkillScore(float newScore) { playerSkillScore = newScore; }

    //enemy info
    [SerializeField] private bool bossRoom = false;
    [SerializeField] private int enemyMin = 4, enemyMax = 7;
    public void SetEnemyMin(int min) { enemyMin = min; }
    public void SetEnemyMax(int max) { enemyMax = max; }

    //enemy types
    [SerializeField] private GameObject[] validEnemyTypes, validBossTypes;
    private void GetValidEnemyTypes() { validEnemyTypes = ASM.GetDG().GetEnemyTypes(); }
    private void GetValidBossTypes() { validBossTypes = ASM.GetDG().GetBossTypes(); }



    private void GenerateBoss()
    {
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Generating Boss Enemy"); }
        //find random boss type
        GameObject bossType = validBossTypes[Random.Range(0, (validBossTypes.Length - 1))];
        //Debug.Log("bossType: " + bossType.name);
        ASM.SpawnEnemy(bossType, (literalPosition + roomCenter));
    }


    private void GenerateEnemies()
    {
        //start combat
        int enemyCount = Random.Range(enemyMin, enemyMax);
        if (dbugEnabled) { MG.UpdateHUDDbugText("Room Generation: Generating " + enemyCount + " Enemies"); }
        Vector3[] enemyPositions = new Vector3[0];
        GameObject[] enemyTypes = new GameObject[0];
        //Debug.Log("enemyCount: " + enemyCount);

        //for each enemy to be spawned
        for(int currentEnemyIndex = 0; currentEnemyIndex < enemyCount; currentEnemyIndex++)
        {
            //Debug.Log("curEnemyIndex: " + currentEnemyIndex);
            int attempts = 0, maxAttempts = 3;
            bool posValid = false;
            while(attempts < maxAttempts && !posValid)
            {
                //find random position within room
                Vector3 spawnPos = new Vector3((Random.Range(0, roomBoundsX) + literalPosition.x), 0.5f, (Random.Range(0, roomBoundsZ) + literalPosition.z));

                float checkDistance = tileXOffset;

                //find if distance between generated position and player is less than checkDistance
                Vector3 playerPos = ASM.GetPlayerPosition(); //get current player position from abstract scene manager (run every attempt incase player moves)
                bool playerNearBy = Vector3.Distance(spawnPos, playerPos) < checkDistance; //check if distance between spawn pos & player pos is less than check distance (if false, no player near; if true, player near)

                //check radius around position for walls & other enemies
                bool wallInRadius = false, enemyInRadius = false;
                Collider[] hitColliders = Physics.OverlapSphere(spawnPos, checkDistance); //check for colliders within radius of check distance around spawn pos
                for(int colliderIndex = 0; colliderIndex < hitColliders.Length; colliderIndex++) //for each collider found
                {
                    if(hitColliders[colliderIndex].gameObject.CompareTag("Wall"))
                    {
                        //if its a wall, set bool to true
                        wallInRadius = true;
                        break;
                    }
                    else if(hitColliders[colliderIndex].gameObject.CompareTag("Enemy"))
                    {
                        //if its an enemy, set bool to true
                        enemyInRadius = true;
                        break;
                    }
                }

                //if player is not near by, enemy is not in radius, and there are no walls in radius, spawn enemy
                if(!playerNearBy && !enemyInRadius && !wallInRadius)
                {
                    //expand enemy positions array and add new pos
                    Vector3[] tempEnemyPositions = new Vector3[enemyPositions.Length + 1]; //new increased array
                    for (int enemyIndex = 0; enemyIndex < enemyPositions.Length; enemyIndex++) { tempEnemyPositions[enemyIndex] = enemyPositions[enemyIndex]; } //copy old content
                    tempEnemyPositions[(tempEnemyPositions.Length - 1)] = spawnPos; //add new data to last position
                    enemyPositions = tempEnemyPositions; //update old array with new array

                    //expand enemy types array and add new type
                    GameObject[] tempEnemyTypes = new GameObject[enemyTypes.Length + 1]; //new increased array
                    for (int enemyIndex = 0; enemyIndex < enemyTypes.Length; enemyIndex++) { tempEnemyTypes[enemyIndex] = enemyTypes[enemyIndex]; } //copy old content
                    GameObject randType;
                    if (Random.Range(0, 10) >= 8) { randType = validEnemyTypes[1]; }
                    else { randType = validEnemyTypes[0]; }
                    tempEnemyTypes[(tempEnemyTypes.Length - 1)] = randType;
                    enemyTypes = tempEnemyTypes; //update old array with new array

                    posValid = true; //ensure while break
                    break;
                }
                //otherwise increase attempts
                else { attempts++; }
                //if max attempts reached & posValid false, exit loop, move to next enemy
            }
        }

        if (enemyPositions.Length > 0){ ASM.SpawnEnemies(enemyTypes, enemyPositions); }
    }
    //~~~~~enemies~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //debug
    [SerializeField] private bool dbugEnabled = false;
    [SerializeField] private Material baseDbugMat, matDbugEmpty, matDbugWall, matDbugDoor;
    [SerializeField] private GameObject dbugFloorTile, dbugMarker, dbugText;
    private GameObject[] dbugGrid;



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
    //~~~~~debug~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
}