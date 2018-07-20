using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenerationBehavior : MonoBehaviour {

    private GameController gameController;

    [Header("World generation files and options")]
    public int levelFileIndex;
    public TextAsset[] levelDescriptionJsonFiles;
    public bool generateWorld; // whether to generate the world or not

    [Header("Internal references to game objects")]
    public List<Transform> levelEntities; // all of the entities that were generated for the level.
    private List<LinkBlockBehavior> levelLinkBlocks; // reference to link blocks used during world generation
    // references to Sprite
    public Sprite startLinkBlockSprite;
    public Sprite nullStartLinkBlockSprite;
    public Transform backgroundRef;

    [Header("PreFabs used in World Generation")]
    public Transform groundPreFab;
    public Transform groundTopPreFab;
    public Transform playerPreFab;
    public Transform goalPortalPreFab;
    public Transform objectiveBlockPreFab;
    public Transform instructionViewBlockPreFab;
    public Transform linkBlockPreFab;
    public Transform singleLinkedListPreFab;
    public Transform helicopterRobotPreFab;    

    public void ManualStartGenerator()
    {
        if (generateWorld)
        {
            CreateWorldFromLevelDescription();
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
        else if(str.Equals("SortListDuplicatesNotAllBlocks"))
        {
            return GameController.WinCondition.SortListDuplicatesNotAllBlocks;
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
            levelEntities.Add(gameController.playerRef);
            // move the backdrop right behind the player initially.
            backgroundRef.position = gameController.playerRef.position + new Vector3(0, 0, -10);
        }
        if (level.goalPortal != null)
        {
            Vector2 loc = new Vector2((float)(level.goalPortal.x + (1 / 2f)), (float)(level.goalPortal.y - (1 / 2f)));
            Transform goal = Instantiate(goalPortalPreFab, loc, Quaternion.identity);
            goal.GetComponent<GoalBehavior>().logId = level.goalPortal.logId;
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

            gameController.helicopterRobotRef = robot;
            robotBehavior.childLink = robot.Find("LinkBlock").gameObject;
            robotBehavior.childLink.GetComponent<LinkBlockBehavior>().logId = level.helicopterRobot.logId + "Link";

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

            startLinkBehavior.GetComponent<SpriteRenderer>().sprite = startLinkBlockSprite;
            gameController.startingLink = startLinkBehavior; // set start link reference
            levelLinkBlocks.Add(startLinkBehavior);
            levelLinkBlocksConnIds.Add(level.startLink.objIDConnectingTo);

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

            levelObjectiveBlocks.Add(ob);
            levelEntities.Add(newOBlock);
        }
        
        // create the instruction blocks (question marks)
        for (int i = 0; i < level.instructionBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.instructionBlocks[i].x + (1 / 2f)), (float)(level.instructionBlocks[i].y - 1 ));
            Transform newInstructBlock = Instantiate(instructionViewBlockPreFab, loc, Quaternion.identity);
            newInstructBlock.GetComponent<InstructionViewBlockBehavior>().screenId = level.instructionBlocks[i].screenId;
            levelEntities.Add(newInstructBlock);
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
            innerLink.containerPlatform = newPlat;
            innerLink.logId = level.singleLinkedListPlatforms[i].logId + "Link";
            newPlat.gameController = gameController;
            newPlat.childLink = innerLink.gameObject;
            newPlat.childValueBlock = newLLPlatform.Find("ValueBlock").gameObject;
            newPlat.setValue(level.singleLinkedListPlatforms[i].value);
            newPlat.logId = level.singleLinkedListPlatforms[i].logId;

            listPlatformMap.Add(level.singleLinkedListPlatforms[i].objId, newPlat);
            levelLinkBlocks.Add(innerLink); // add it to the list of blocks for references
            levelLinkBlocksConnIds.Add(level.singleLinkedListPlatforms[i].childLinkBlockConnectId);
            levelEntities.Add(newLLPlatform);
            levelPlatformEntities.Add(newPlat);
            if (level.singleLinkedListPlatforms[i].toAdd == true)
            {
                newLLPlatform.gameObject.SetActive(false);
                newPlat.isInLevel = false;
                gameController.platformsToAdd.Add(newPlat);
                gameController.hudBehavior.setPlatformsToAddText(gameController.platformsToAdd.Count);
            } else
            {
                newPlat.isInLevel = true;
            }
        }

        // establishing links for the link blocks with the platforms
        if (levelLinkBlocksConnIds != null && levelLinkBlocksConnIds.Count > 0)
        {
            for (int i = 0; i < levelLinkBlocksConnIds.Count; i++)
            {
                if (levelLinkBlocksConnIds[i] != null && levelLinkBlocksConnIds[i].Length > 0) // if this link has a connection.
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
        }

        // update the win conditions for the objective blocks
        gameController.setLevelObjectiveBlocksList(levelObjectiveBlocks);
        gameController.setLevelPlatformEntitiesList(levelPlatformEntities);
        gameController.updateObjectiveHUDAndBlocks();
        gameController.updatePlatformEntities();
        
        string[] varNames = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "p", "q", "r", "z", "y" };
        int varIndex = 0;
        // verify that no link blocks are being displayed
        foreach (LinkBlockBehavior lb in levelLinkBlocks)
        {
            if (!lb.isHelicopterLink && !lb.isStartingLink) { 
                lb.variableName = varNames[varIndex++];
            }
            lb.setDisplayMarker(false);
        }
        // also do this for the robot
        if (gameController.helicopterRobotRef != null)
        {
            gameController.helicopterRobotRef.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<LinkBlockBehavior>().setDisplayMarker(false);
        }


        gameController.codePanelBehavior.clearCodeText();
        gameController.hudBehavior.setLevelOnText(levelFileIndex + 1); 
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
            CreateWorldFromLevelDescription();
            string actMsg = "level " + (levelFileIndex + 1) + " was created";
            gameController.loggingManager.send_To_Server(actMsg);
        } else
        { 
            string actMsg = "Game is won!";
            gameController.loggingManager.send_To_Server(actMsg);
            gameController.hudBehavior.gameObject.SetActive(false);
            gameController.winGameCanvas.gameObject.SetActive(true);
        }
    }


    public void setGameController(GameController gc)
    {
        gameController = gc;
    }
}
