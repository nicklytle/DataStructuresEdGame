using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class WorldGenerationBehavior : MonoBehaviour {

    public GameController gameController;

    // world generation properties.
    public int levelFileIndex;
    public TextAsset[] levelDescriptionJsonFiles;
    public bool generateWorld;
    public List<Transform> levelEntities; // all of the entities that were generated for the level.
    public List<LinkBlockBehavior> levelLinkBlocks;

    // references to PreFabs
    public Transform groundPreFab;
    public Transform groundTopPreFab;
    public Transform playerPreFab;
    public Transform goalPortalPreFab;
    public Transform objectiveBlockPreFab;
    public Transform linkBlockPreFab;
    public Transform singleLinkedListPreFab;
    public Transform helicopterRobotPreFab;

    // references to Sprite
    public Sprite startLinkBlockSprite;
    public Sprite nullStartLinkBlockSprite;

    // level backdrop
    public BackgroundBehavior background;

    // Use this for initialization
    void Start () {

    }

    public void ManualStartGenerator()
    {
        if (generateWorld)
        {
            CreateWorldFromLevelDescription();
            //Debug.Log("CREATED WORLD");
        }
    }

    /**
     * This function maps a string value to a PreFab value for level generation.
     */
    public Transform GetAssocInstanceFromType(string type)
    {
        if (type.Equals("GROUND"))
        {
            return groundPreFab;
        }

        return null;
    }

    public GameController.WinCondition GetWinConditionFromString(string str)
    {
        if (str.Equals("SortListAscending"))
        {
            return GameController.WinCondition.SortListAscending;
        } else if (str.Equals("SortListDescending"))
        {
            return GameController.WinCondition.SortListDescending;
        }
        return GameController.WinCondition.None;
    }

    /**
     * Generate a world using the given .JSON file in 'levelDescriptionJson'
     */
    public void CreateWorldFromLevelDescription()
    {
        LevelEntities level = JsonUtility.FromJson<LevelEntities>(levelDescriptionJsonFiles[levelFileIndex].text);
        gameController.winConditon = GetWinConditionFromString(level.winCondition); 
        // while generating the level, add things to levelEntities list so it can be destroyed for the next level generated.
        for (int i = 0; i < level.blocks.Length; i++)
        {
            Vector2 blockPos = new Vector2((float)(level.blocks[i].x + (level.blocks[i].width / 2f)),
                                        (float)(level.blocks[i].y - (level.blocks[i].height / 2f)));
            Transform objToInstances = GetAssocInstanceFromType(level.blocks[i].type);
            if (objToInstances != null)
            {
                if (objToInstances == groundPreFab && level.blocks[i].height == 1)
                {
                    objToInstances = groundTopPreFab; // render it with details on top of the block.
                }
                Transform obj = Instantiate(objToInstances, blockPos, Quaternion.identity);
                Vector2 sizeOfBlock = new Vector3((int)level.blocks[i].width, (int)level.blocks[i].height); 
                obj.GetComponent<SpriteRenderer>().size = sizeOfBlock;
                obj.GetComponent<BoxCollider2D>().size = sizeOfBlock;
                if (objToInstances == groundPreFab || objToInstances == groundTopPreFab)
                {
                    obj.GetComponent<GroundBehavior>().logId = level.blocks[i].logId; // ground block

                    //DateTime date1 = DateTime.Now;
                    string timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    Debug.Log("Ground logID: " + obj.GetComponent<GroundBehavior>().logId + " " + timestamp1);
                }
                levelEntities.Add(obj);
            }
        }
        // create the player
        if (level.player != null)
        {
            Vector2 loc = new Vector2((float)(level.player.x + (1 / 2f)), (float)(level.player.y - (1 / 2f)));
            gameController.playerRef = Instantiate(playerPreFab, loc, Quaternion.identity);
            gameController.playerRef.GetComponent<PlayerBehavior>().gameController = gameController;
            gameController.playerRef.GetComponent<PlayerBehavior>().logId = level.player.logId;
            Debug.Log("Player logID " + gameController.playerRef.GetComponent<PlayerBehavior>().logId);
            levelEntities.Add(gameController.playerRef);
            // move the backdrop right behind the player initially.
            background.initialPosition = gameController.playerRef.position + new Vector3(0, 0, -10);
            background.transform.position = background.initialPosition;
        }
        if (level.goalPortal != null)
        {
            Vector2 loc = new Vector2((float)(level.goalPortal.x + (1 / 2f)), (float)(level.goalPortal.y - (1 / 2f)));
            Transform goal = Instantiate(goalPortalPreFab, loc, Quaternion.identity);
            goal.GetComponent<GoalBehavior>().logId = level.goalPortal.logId;

            //DateTime date2 = DateTime.Now;
            string timestamp2 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Goal logID " + goal.GetComponent<GoalBehavior>().logId + " " + timestamp2);

            levelEntities.Add(goal);
        }
        if (level.helicopterRobot != null)
        {
            Vector2 loc = new Vector2((float)(level.helicopterRobot.x + (1 / 2f)), (float)(level.helicopterRobot.y - (1 / 2f)));
            Transform robot = Instantiate(helicopterRobotPreFab, loc, Quaternion.identity);
            HelicopterRobotBehavior robotBehavior = robot.GetComponent<HelicopterRobotBehavior>();
            robotBehavior.gameController = gameController;
            robotBehavior.targetLocation = robot.position;
            robotBehavior.logId = level.helicopterRobot.logId;

            //DateTime date3 = DateTime.Now;
            string timestamp3 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Robot itself: " + robotBehavior.logId + " " + timestamp3);


            gameController.helicopterRobotRef = robot;
            robotBehavior.childLink = robot.Find("LinkBlock").gameObject;
            robotBehavior.childLink.GetComponent<LinkBlockBehavior>().logId = level.helicopterRobot.logId + "Link";

            //DateTime date4 = DateTime.Now;
            string timestamp4 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Helicopter robo LINK logID " + robotBehavior.childLink.GetComponent<LinkBlockBehavior>().logId + " " + timestamp4);
            levelEntities.Add(robot);
        }
        
        // list of link blocks we are creating
        levelLinkBlocks = new List<LinkBlockBehavior>();
        // corresponding list of IDs telling the link blocks what they should point to when the level is generated
        List<string> levelLinkBlocksConnIds = new List<string>();
        List<ObjectiveBlockBehavior> levelObjectiveBlocks = new List<ObjectiveBlockBehavior>();
        List<PlatformBehavior> levelPlatformEntities = new List<PlatformBehavior>();
        gameController.platformsToAdd = new List<PlatformBehavior>();
        // create the start link
        if (level.startLink != null)
        {
            Vector2 loc = new Vector2((float)(level.startLink.x + (1 / 2f)), (float)(level.startLink.y - (1 / 2f)));
            Transform startLinkTran = Instantiate(linkBlockPreFab, loc, Quaternion.identity);
            LinkBlockBehavior startLinkBehavior = startLinkTran.GetComponent<LinkBlockBehavior>();
            startLinkTran.position = startLinkTran.position + (new Vector3(0, 0, -5));
            startLinkBehavior.gameController = gameController;
            startLinkBehavior.isStartingLink = true;  // mark link as start.
            startLinkBehavior.defaultSprite = startLinkBlockSprite;
            startLinkBehavior.nullLinkSprite = nullStartLinkBlockSprite;
            startLinkBehavior.logId = level.startLink.logId;

            //DateTime date5 = DateTime.Now;
            string timestamp5 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Head link LogID " + startLinkBehavior.logId + " " + timestamp5);
            startLinkBehavior.GetComponent<SpriteRenderer>().sprite = startLinkBlockSprite;
            gameController.startingLink = startLinkBehavior; // set start link reference
            levelLinkBlocks.Add(startLinkBehavior);
            levelLinkBlocksConnIds.Add(level.startLink.objIDConnectingTo);

            //DateTime date6 = DateTime.Now;
            string timestamp6 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Head link connection " + level.startLink.objIDConnectingTo + " " + timestamp6);
            levelEntities.Add(startLinkTran);
        }

        // create the indiv link blocks.
        for (int i = 0; i < level.linkBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.linkBlocks[i].x + (1 / 2f)), (float)(level.linkBlocks[i].y - (1 / 2f)));
            Transform newLink = Instantiate(linkBlockPreFab, loc, Quaternion.identity);
            newLink.position = newLink.position + (new Vector3(0,0,-5));
            LinkBlockBehavior lb = newLink.GetComponent<LinkBlockBehavior>();
            lb.gameController = gameController;
            lb.logId = level.linkBlocks[i].logId;

            //DateTime date7 = DateTime.Now;
            string timestamp7 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("each link " + i + " block " + lb.logId + " " + timestamp7);
            levelLinkBlocks.Add(lb);
            levelLinkBlocksConnIds.Add(level.linkBlocks[i].objIDConnectingTo);
            levelEntities.Add(newLink);
        }

        // create the objective blocks (fire)
        for (int i = 0; i < level.objectiveBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.objectiveBlocks[i].x + (1 / 2f)), (float)(level.objectiveBlocks[i].y - (1 / 2f)));
            Transform newOBlock = Instantiate(objectiveBlockPreFab, loc, Quaternion.identity);
            ObjectiveBlockBehavior ob = newOBlock.GetComponent<ObjectiveBlockBehavior>();
            ob.logId = level.objectiveBlocks[i].logId;

            //DateTime date8 = DateTime.Now;
            string timestamp8 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("fire blocks " + ob.logId + timestamp8);
            levelObjectiveBlocks.Add(ob);
            levelEntities.Add(newOBlock);
        }

        // create the platforms.
        Dictionary<string, PlatformBehavior> listPlatformMap = new Dictionary<string, PlatformBehavior>();
        for (int i = 0; i < level.singleLinkedListPlatforms.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.singleLinkedListPlatforms[i].x + (3 / 2f)), (float)(level.singleLinkedListPlatforms[i].y - (1 / 2f)));
            Transform newLLPlatform = Instantiate(singleLinkedListPreFab, loc, Quaternion.identity);
            PlatformBehavior newPlat = newLLPlatform.GetComponent<PlatformBehavior>();
            LinkBlockBehavior innerLink = newLLPlatform.Find("LinkBlock").GetComponent<LinkBlockBehavior>();
            innerLink.gameController = gameController;
            innerLink.parentPlatform = newPlat;
            newPlat.gameController = gameController;
            newPlat.childLink = innerLink.gameObject;
            newPlat.childValueBlock = newLLPlatform.Find("ValueBlock").gameObject;
            newPlat.setValue(level.singleLinkedListPlatforms[i].value);
            newPlat.logId = level.singleLinkedListPlatforms[i].logId;

            //DateTime date9 = DateTime.Now;
            string timestamp9 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("platform logID " + newPlat.logId + timestamp9);

            innerLink.logId = level.singleLinkedListPlatforms[i].logId + "Link";
            //DateTime date10 = DateTime.Now;
            string timestamp10 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("link inside the platform logID " + innerLink.logId + " "  + timestamp10);


            listPlatformMap.Add(level.singleLinkedListPlatforms[i].objId, newPlat);
            //DateTime date11 = DateTime.Now;
            string timestamp11 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("platform object ID " + level.singleLinkedListPlatforms[i].objId + " " + timestamp11);


            levelLinkBlocks.Add(innerLink); // add it to the list of blocks for references
            levelLinkBlocksConnIds.Add(level.singleLinkedListPlatforms[i].childLinkBlockConnectId);

            //DateTime date12 = DateTime.Now;
            string timestamp12 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Connecting block " + level.singleLinkedListPlatforms[i].childLinkBlockConnectId + "  " + level.singleLinkedListPlatforms.Length + ": " + timestamp12);

            levelEntities.Add(newLLPlatform);
            levelPlatformEntities.Add(newPlat);
            if (level.singleLinkedListPlatforms[i].toAdd == true)
            {
                //Debug.Log("Platform needs to be added");
                newLLPlatform.gameObject.SetActive(false);
                newPlat.isInLevel = false;
                gameController.platformsToAdd.Add(newPlat);
            } else
            {
                newPlat.isInLevel = true;
            }
            Debug.Log("size of addable platform list: " + gameController.platformsToAdd.Count);
        }

        // establishing links for the link blocks with the platforms
        for (int i = 0; i < levelLinkBlocksConnIds.Count; i++) {
            if (levelLinkBlocksConnIds[i].Length > 0) // if this link has a connection.
            {
                string platformId = levelLinkBlocksConnIds[i]; 
                if (listPlatformMap[platformId].isInLevel == true)
                {
                    // establish the connection
                    levelLinkBlocks[i].connectingPlatform = listPlatformMap[platformId];
                    levelLinkBlocks[i].connectingPlatform.addIncomingConnectingLink(levelLinkBlocks[i]);
                    levelLinkBlocks[i].renderArrow = true;
                }
            }
        }

        // update the win conditions for the objective blocks
        gameController.setLevelObjectiveBlocksList(levelObjectiveBlocks);
        gameController.setLevelPlatformEntitiesList(levelPlatformEntities);
        gameController.updateObjectiveHUDAndBlocks();
        gameController.updatePlatformEntities();
    }

    /**
     * Reset the level, rebuilding the level that is in the levelDescriptionFile value.
     */
    public void resetLevel()
    {
        Debug.Log("RESET LEVEL");
        foreach (LinkBlockBehavior lb in levelLinkBlocks)
        {
            lb.removeArrowBetween();
        }

        foreach (Transform t in levelEntities)
        {
            // delete any arrows associated with link blocks.
            if (t.GetComponent<HelicopterRobotBehavior>() != null && t.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<LinkBlockBehavior>() != null)
            {
                t.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<LinkBlockBehavior>().removeArrowBetween();
            } 
            Destroy(t.gameObject);
        }
        gameController.clearReferenceLists(); // clear the references here
        levelEntities.Clear();

        // make sure there is a level file for this
        if (levelFileIndex < levelDescriptionJsonFiles.Length)
        {
            gameController.setStatusText("");
            CreateWorldFromLevelDescription();
        } else
        {
            gameController.setStatusText("You have won!");
            /// .Log("GAME IS WON!");
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
