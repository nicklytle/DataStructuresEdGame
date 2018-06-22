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
    public Transform playerPreFab;
    public Transform goalPortalPreFab;
    public Transform objectiveBlockPreFab;
    public Transform linkBlockPreFab;
    public Transform singleLinkedListPreFab;
    public Transform helicopterRobotPreFab;

    // references to Sprite
    public Sprite startLinkBlockSprite;
    public Sprite nullStartLinkBlockSprite;

    //reference to list of platformBehavior objects to add
    public PlatformBehavior listOfPlatsToAdd;

    // Use this for initialization
    void Start () {
        // ManualStartGenerator();
    }

    public void ManualStartGenerator()
    {
        if (generateWorld)
        {
            CreateWorldFromLevelDescription();
            Debug.Log("CREATED WORLD");
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
                Transform obj = Instantiate(objToInstances, blockPos, Quaternion.identity);
                Vector3 sizeOfBlock = new Vector3((int)level.blocks[i].width, (int)level.blocks[i].height, 1);
                Debug.Log(sizeOfBlock);
                obj.localScale = sizeOfBlock;
                levelEntities.Add(obj);
            }
        }
        // create the player
        if (level.player != null)
        {
            Vector2 loc = new Vector2((float)(level.player.x + (1 / 2f)), (float)(level.player.y - (1 / 2f)));
            gameController.playerRef = Instantiate(playerPreFab, loc, Quaternion.identity);
            gameController.playerRef.GetComponent<PlayerBehavior>().gameController = gameController;
            levelEntities.Add(gameController.playerRef);
        }
        if (level.goalPortal != null)
        {
            Vector2 loc = new Vector2((float)(level.goalPortal.x + (1 / 2f)), (float)(level.goalPortal.y - (1 / 2f)));
            Transform goal = Instantiate(goalPortalPreFab, loc, Quaternion.identity);
            levelEntities.Add(goal);
        }
        if (level.helicopterRobot != null)
        {
            Vector2 loc = new Vector2((float)(level.helicopterRobot.x + (1 / 2f)), (float)(level.helicopterRobot.y - (1 / 2f)));
            Transform robot = Instantiate(helicopterRobotPreFab, loc, Quaternion.identity);
            HelicopterRobotBehavior robotBehavior = robot.GetComponent<HelicopterRobotBehavior>();
            robotBehavior.gameController = gameController;
            robotBehavior.targetLocation = robot.position;
            gameController.helicopterRobotRef = robot;
            robotBehavior.childLink = robot.Find("LinkBlock").gameObject;
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
            startLinkBehavior.gameController = gameController;
            startLinkBehavior.isStartingLink = true;  // mark link as start.
            startLinkBehavior.defaultSprite = startLinkBlockSprite;
            startLinkBehavior.nullLinkSprite = nullStartLinkBlockSprite;
            startLinkBehavior.GetComponent<SpriteRenderer>().sprite = startLinkBlockSprite;
            gameController.startingLink = startLinkBehavior; // set start link reference
            levelLinkBlocks.Add(startLinkBehavior);
            levelLinkBlocksConnIds.Add(level.startLink.objIDConnectingTo);
            levelEntities.Add(startLinkTran);
        }

        // create the link blocks
        for (int i = 0; i < level.linkBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.linkBlocks[i].x + (1 / 2f)), (float)(level.linkBlocks[i].y - (1 / 2f)));
            Transform newLink = Instantiate(linkBlockPreFab, loc, Quaternion.identity);
            LinkBlockBehavior lb = newLink.GetComponent<LinkBlockBehavior>();
            lb.gameController = gameController;
            levelLinkBlocks.Add(lb);
            Debug.Log("Made me some level link blocks");
            levelLinkBlocksConnIds.Add(level.linkBlocks[i].objIDConnectingTo);
            levelEntities.Add(newLink);
        }

        // create the objective blocks
        Debug.Log(level.objectiveBlocks);
        for (int i = 0; i < level.objectiveBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.objectiveBlocks[i].x + (1 / 2f)), (float)(level.objectiveBlocks[i].y - (1 / 2f)));
            Transform newOBlock = Instantiate(objectiveBlockPreFab, loc, Quaternion.identity);
            ObjectiveBlockBehavior ob = newOBlock.GetComponent<ObjectiveBlockBehavior>();
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
            listPlatformMap.Add(level.singleLinkedListPlatforms[i].objId, newPlat);
            levelLinkBlocks.Add(innerLink); // add it to the list of blocks for references
            levelLinkBlocksConnIds.Add(level.singleLinkedListPlatforms[i].childLinkBlockConnectId);
            levelEntities.Add(newLLPlatform);
            levelPlatformEntities.Add(newPlat); 
            if (level.singleLinkedListPlatforms[i].toAdd == true)
            {
                Debug.Log("It needs to be added");
                newLLPlatform.gameObject.SetActive(false);
                newPlat.isInLevel = false;
                gameController.platformsToAdd.Add(newPlat);
            } else
            {
                newPlat.isInLevel = true;
            }
        }

        // establishing links for the link blocks with the platforms
        for (int i = 0; i < levelLinkBlocksConnIds.Count; i++) {
            if (levelLinkBlocksConnIds[i].Length > 0) // if this link has a connection.
            {
                string platformId = levelLinkBlocksConnIds[i];
                // establish the connection
                levelLinkBlocks[i].connectingPlatform = listPlatformMap[platformId];
                levelLinkBlocks[i].connectingPlatform.addIncomingConnectingLink(levelLinkBlocks[i]);
                levelLinkBlocks[i].renderArrow = true;
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
