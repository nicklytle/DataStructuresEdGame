using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;
using UnityEngine.UI;

/**
 * Handles all world and level generation from given level description files.
 */
public class WorldGenerationBehavior : MonoBehaviour {

    private GameController gameController;

    [Header("World generation files and options")]
    public int levelFileIndex;  // which level you are currently playing. This starts from 0 and 
                                // goes until the size of the level description files array minus one.
    public TextAsset[] levelDescriptionJsonFiles; // an array of level files. The order of this array determines the order of levels.
    public bool generateWorld; // whether to generate the world or not

    [Header("Internal references to game objects")]
    public List<Transform> levelEntities; // all of the entities that were generated for the level.
    public List<LinkBehavior> levelLinks; // reference to link blocks, including those which are inside other entities.
    public Sprite startLinkBlockSprite;  // references to Sprites
    public Sprite nullStartLinkBlockSprite;
    public Transform backgroundRef; // reference to the background game object

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

    private bool busy;

    public void ManualStartGenerator()
    {
        if (generateWorld)
        {
            busy = true;
            CreateWorldFromLevelDescription();
            busy = false;
        } else
        {
            Debug.Log("generateWorld is turned off. Please turn it on in the WorldGenerator");
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
        LevelEntitiesJSON level = JsonUtility.FromJson<LevelEntitiesJSON>(levelDescriptionJsonFiles[levelFileIndex].text);
        gameController.winConditon = GetWinConditionFromString(level.winCondition);
        // list of link blocks we are creating
        levelLinks = new List<LinkBehavior>();
        Debug.Log("Loading blocks");
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
                obj.GetComponent<LoggableBehavior>().setLogID(level.blocks[i].logId); // ground block 
                levelEntities.Add(obj);
            }

        }
        Debug.Log("Loading player");
        // create the player
        if (level.player != null)
        {
            Vector2 loc = new Vector2((float)(level.player.x + (1 / 2f)), (float)(level.player.y - (1 / 2f)));
            gameController.playerRef = Instantiate(playerPreFab, loc, Quaternion.identity);
            gameController.playerRef.GetComponent<PlayerBehavior>().gameController = gameController;
            gameController.playerRef.GetComponent<LoggableBehavior>().setLogID(level.player.logId);
            levelEntities.Add(gameController.playerRef);
            // move the backdrop right behind the player initially.
            backgroundRef.position = gameController.playerRef.position + new Vector3(0, 0, -10);
        }
        Debug.Log("Loading goal");
        if (level.goalPortal != null)
        {
            Vector2 loc = new Vector2((float)(level.goalPortal.x + (1 / 2f)), (float)(level.goalPortal.y - (1 / 2f)));
            Transform goal = Instantiate(goalPortalPreFab, loc, Quaternion.identity);
            goal.GetComponent<LoggableBehavior>().setLogID(level.goalPortal.logId);
            levelEntities.Add(goal);
        }
        Debug.Log("Loading helicopter robot");
        if (level.helicopterRobot != null)
        {
            Vector2 loc = new Vector2((float)(level.helicopterRobot.x + (1 / 2f)), (float)(level.helicopterRobot.y - (1 / 2f)));
            Transform robot = Instantiate(helicopterRobotPreFab, loc, Quaternion.identity);
            HelicopterRobotBehavior robotBehavior = robot.GetComponent<HelicopterRobotBehavior>();
            robotBehavior.gameController = gameController;
            robotBehavior.targetLocation = robot.position;
            robotBehavior.GetComponent<LoggableBehavior>().setLogID(level.helicopterRobot.logId);

            gameController.helicopterRobotRef = robot; 
            robotBehavior.GetComponent<ContainerEntityBehavior>().refreshChildList();
            robotBehavior.getChildLink().GetComponent<LoggableBehavior>().setLogID("helicopter");
            robotBehavior.getChildLink().type = LinkBehavior.Type.HELICOPTER;
            robotBehavior.getChildLink().setVariableName("temp");
            robotBehavior.getChildLink().setContainerEntity(robot.GetComponent<ContainerEntityBehavior>()); 
            levelEntities.Add(robot);
            levelLinks.Add(robotBehavior.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>());
        }
        
        // corresponding list of IDs telling the link blocks what they should point to when the level is generated
        List<string> levelLinksConnIds = new List<string>();
        List<LinkBehavior> levelLinkComps = new List<LinkBehavior>();
        List<ObjectiveBlockBehavior> levelObjectiveBlocks = new List<ObjectiveBlockBehavior>();
        List<PlatformBehavior> levelPlatformEntities = new List<PlatformBehavior>();
        gameController.platformsToAdd = new List<PlatformBehavior>();
        // create the start link
        Debug.Log("Loading start link");
        if (level.startLink != null)
        {
            Vector2 loc = new Vector2((float)(level.startLink.x + (1 / 2f)), (float)(level.startLink.y - (1 / 2f)));
            Transform startLinkTran = Instantiate(linkBlockPreFab, loc, Quaternion.identity);
            LinkBehavior startLinkBehavior = startLinkTran.GetComponent<LinkBehavior>();
            startLinkTran.position = startLinkTran.position + (new Vector3(0, 0, -5));
            startLinkBehavior.type = LinkBehavior.Type.START;
            startLinkBehavior.defaultSprite = startLinkBlockSprite;
            startLinkBehavior.nullSprite = nullStartLinkBlockSprite;
            startLinkBehavior.GetComponent<LoggableBehavior>().setLogID(level.startLink.logId); 

            startLinkBehavior.GetComponent<SpriteRenderer>().sprite = startLinkBlockSprite;
            gameController.startingLink = startLinkBehavior; // set start link reference
            levelLinks.Add(startLinkBehavior);
            levelEntities.Add(startLinkTran);

            levelLinksConnIds.Add(level.startLink.objIDConnectingTo);
            levelLinkComps.Add(startLinkBehavior); 
        }

        Debug.Log("Loading external link blocks");
        // create the indiv link blocks.
        for (int i = 0; i < level.linkBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.linkBlocks[i].x + (1 / 2f)), (float)(level.linkBlocks[i].y - (1 / 2f)));
            Transform newLink = Instantiate(linkBlockPreFab, loc, Quaternion.identity);
            newLink.position = newLink.position + (new Vector3(0,0,-5));
            LinkBehavior lb = newLink.GetComponent<LinkBehavior>();
            lb.GetComponent<LoggableBehavior>().setLogID(level.linkBlocks[i].logId); 
            levelLinks.Add(lb);
            levelEntities.Add(newLink);

            levelLinksConnIds.Add(level.linkBlocks[i].objIDConnectingTo);
            levelLinkComps.Add(lb);
        }

        Debug.Log("Loading objective blocks");
        // create the objective blocks (fire)
        for (int i = 0; i < level.objectiveBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.objectiveBlocks[i].x + (1 / 2f)), (float)(level.objectiveBlocks[i].y - (1 / 2f)));
            Transform newOBlock = Instantiate(objectiveBlockPreFab, loc, Quaternion.identity);
            newOBlock.GetComponent<LoggableBehavior>().setLogID(level.objectiveBlocks[i].logId);
            levelObjectiveBlocks.Add(newOBlock.GetComponent<ObjectiveBlockBehavior>());
            levelEntities.Add(newOBlock);
        }

        Debug.Log("Loading instruction blocks");
        // create the instruction blocks (question marks)
        for (int i = 0; i < level.instructionBlocks.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.instructionBlocks[i].x + (1 / 2f)), (float)(level.instructionBlocks[i].y - 1 ));
            Transform newInstructBlock = Instantiate(instructionViewBlockPreFab, loc, Quaternion.identity);
            newInstructBlock.GetComponent<InstructionViewBlockBehavior>().screenId = level.instructionBlocks[i].screenId;
            levelEntities.Add(newInstructBlock);
        }

        Debug.Log("Loading the platforms");
        // create the platforms.
        Dictionary<string, PlatformBehavior> listPlatformMap = new Dictionary<string, PlatformBehavior>();
        for (int i = 0; i < level.singleLinkedListPlatforms.Length; i++)
        {
            Vector2 loc = new Vector2((float)(level.singleLinkedListPlatforms[i].x + (3 / 2f)), (float)(level.singleLinkedListPlatforms[i].y - (1 / 2f)));
            Transform newLLPlatform = Instantiate(singleLinkedListPreFab, loc, Quaternion.identity);
            PlatformBehavior newPlat = newLLPlatform.GetComponent<PlatformBehavior>();
            newPlat.GetComponent<ConnectableEntityBehavior>().incomingConnectionLinks = new List<LinkBehavior>();
            newPlat.gameController = gameController;

            newPlat.GetComponent<ContainerEntityBehavior>().refreshChildList();
            newPlat.setValue(level.singleLinkedListPlatforms[i].value);
            newPlat.GetComponent<LoggableBehavior>().setLogID(level.singleLinkedListPlatforms[i].logId);
            newPlat.GetComponent<ConnectableEntityBehavior>().incomingConnectionLinks = new List<LinkBehavior>();
            listPlatformMap.Add(level.singleLinkedListPlatforms[i].objId, newPlat);
            newPlat.getChildLink().state = LinkBehavior.State.NORMAL;
            newPlat.getChildLink().GetComponent<LoggableBehavior>().setLogID("child" + level.singleLinkedListPlatforms[i].logId);
            newPlat.getChildLink().setContainerEntity(newPlat.GetComponent<ContainerEntityBehavior>());
            levelLinks.Add(newPlat.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>()); // add it to the list of blocks for references
            levelEntities.Add(newLLPlatform);

            levelLinksConnIds.Add(level.singleLinkedListPlatforms[i].childLinkBlockConnectId);
            levelLinkComps.Add(newPlat.getChildLink());
            levelPlatformEntities.Add(newPlat);

            newPlat.isInLevel = !level.singleLinkedListPlatforms[i].toAdd;
            if (level.singleLinkedListPlatforms[i].toAdd == true)
            {
                newLLPlatform.gameObject.SetActive(false); 
                gameController.platformsToAdd.Add(newPlat);
            }
        }
        gameController.hudBehavior.setPlatformsToAddText(gameController.platformsToAdd.Count);
        Debug.Log("Completed loading " + listPlatformMap.Count + " platforms");

        Debug.Log("Establishing links ");
        // establishing links for the link blocks with the platforms 
        for (int i = 0; i < levelLinksConnIds.Count; i++)
        {
            if (levelLinksConnIds[i] != null && levelLinksConnIds[i].Length > 0) // if this link has a connection.
            {
                string platformId = levelLinksConnIds[i];
                if (listPlatformMap[platformId].isInLevel == true)
                {
                    // establish the connection
                    levelLinkComps[i].ensureReferences();
                    levelLinkComps[i].setConnectionTo(listPlatformMap[platformId].GetComponent<ConnectableEntityBehavior>()); 
                }
            }
        }

        Debug.Log("Assigning variable names");
        string[] varNames = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "p", "q", "r", "z", "y" };
        int varIndex = 0;
        // verify that no link blocks are being displayed
        foreach (LinkBehavior lb in levelLinks)
        {
            if (lb.type == LinkBehavior.Type.HELICOPTER)
            {
                lb.setVariableName("temp");
            } else if (lb.type == LinkBehavior.Type.START)
            {
                lb.setVariableName("list.head");
            }
            else 
            {
                lb.setVariableName(varNames[varIndex++]);
            }
            lb.ensureReferences();
            lb.UpdateRendering();
        }
        
        Debug.Log("Updating everything");
        // update the win conditions for the objective blocks
        gameController.setLevelObjectiveBlocksList(levelObjectiveBlocks);
        gameController.setLevelPlatformEntitiesList(levelPlatformEntities);
        gameController.updateObjectiveHUDAndBlocks();
        gameController.updatePlatformEntities();
        gameController.codePanelBehavior.clearCodeText();
        gameController.hudBehavior.setLevelOnText(levelFileIndex + 1);
        gameController.hudBehavior.setPlatformsToAddText(gameController.platformsToAdd.Count);
        Debug.Log("Done loading world");
    }


    /**
     * Reset the level, rebuilding the level that is in the levelDescriptionFile value.
     */
    public void resetLevel()
    {
        foreach (LinkBehavior lb in levelLinks)
        {
            lb.clearArrowInstances();
        }

        foreach (Transform t in levelEntities)
        { 
            Destroy(t.gameObject);
        }
        gameController.clearReferenceLists(); // clear the references here
        levelEntities.Clear();

        // make sure there is a level file for this
        if (levelFileIndex < levelDescriptionJsonFiles.Length)
        {
            CreateWorldFromLevelDescription();
            string actMsg = "level " + (levelFileIndex + 1) + " was created";
            gameController.loggingManager.sendLogToServer(actMsg);
        } else
        { 
            string actMsg = "Game is won!";
            gameController.loggingManager.sendLogToServer(actMsg);
            gameController.hudBehavior.gameObject.SetActive(false);
            gameController.winGameCanvas.gameObject.SetActive(true);
        }
    }


    public void setGameController(GameController gc)
    {
        gameController = gc;
    }

    public bool isBusy()
    {
        return busy;
    }
}
