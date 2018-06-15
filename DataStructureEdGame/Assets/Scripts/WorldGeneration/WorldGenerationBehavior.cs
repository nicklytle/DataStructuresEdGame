using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class WorldGenerationBehavior : MonoBehaviour {

    public GameController gameController;

    // world generation properties.
    public TextAsset levelDescriptionJson;
    public bool generateWorld;

    // references to PreFabs
    public Transform groundPreFab;
    public Transform playerPreFab;
    public Transform linkBlockPreFab;
    public Transform singleLinkedListPreFab;

    public Sprite startLinkBlockSprite;

    // Use this for initialization
    void Start () { 
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
        if (type.Equals("NORMAL_BLOCK"))
        {
            return groundPreFab;
        }
        else if (type.Equals("PLAYER"))
        {
            return playerPreFab;
        }
        else if (type.Equals("START_LINK"))
        {
            return linkBlockPreFab;
        }
        return null;
    }

    /**
     * Generate a world using the given .JSON file in 'levelDescriptionJson'
     */
    public void CreateWorldFromLevelDescription()
    {
        LevelEntities level = JsonUtility.FromJson<LevelEntities>(levelDescriptionJson.text);
        for (int i = 0; i < level.blocks.Length; i++)
        {
            Vector2 blockPos = new Vector2((int)level.blocks[i].x, (int)level.blocks[i].y);
            Transform objToInstances = GetAssocInstanceFromType(level.blocks[i].type);
            if (objToInstances != null)
            {
                Transform obj = Instantiate(objToInstances, blockPos, Quaternion.identity);
            }
        }
        // create the player
        if (level.player != null)
        {
            gameController.playerRef = Instantiate(playerPreFab, new Vector2((int)level.player.x, (int)level.player.y), Quaternion.identity);
        }
        List<LinkBlockBehavior> levelLinkBlocks = new List<LinkBlockBehavior>();
        List<string> levelLinkBlocksConnIds = new List<string>();
        // create the start link
        if (level.startLink != null)
        {
            Transform startLinkTran = Instantiate(linkBlockPreFab, new Vector2((int)level.startLink.x, (int)level.startLink.y), Quaternion.identity);
            LinkBlockBehavior startLinkBehavior = startLinkTran.GetComponent<LinkBlockBehavior>();
            startLinkBehavior.gameController = gameController;
            startLinkBehavior.isStartingLink = true;  // mark link as start.
            startLinkBehavior.defaultSprite = startLinkBlockSprite;
            startLinkBehavior.GetComponent<SpriteRenderer>().sprite = startLinkBlockSprite;
            gameController.startingLink = startLinkBehavior; // set start link reference
            levelLinkBlocks.Add(startLinkBehavior);
            levelLinkBlocksConnIds.Add(level.startLink.objIDConnectingTo);
        }

        // create the link blocks
        for (int i = 0; i < level.linkBlocks.Length; i++)
        {
            Transform newLink = Instantiate(linkBlockPreFab, new Vector2((int)level.linkBlocks[i].x, (int)level.linkBlocks[i].y), Quaternion.identity);
            LinkBlockBehavior lb = newLink.GetComponent<LinkBlockBehavior>();
            lb.gameController = gameController;
            levelLinkBlocks.Add(lb);
            levelLinkBlocksConnIds.Add(level.linkBlocks[i].objIDConnectingTo);
        }

        // create the platforms.
        Dictionary<string, PlatformBehavior> listPlatformMap = new Dictionary<string, PlatformBehavior>();
        for (int i = 0; i < level.singleLinkedListPlatforms.Length; i++)
        {
            Transform newLLPlatform = Instantiate(singleLinkedListPreFab, new Vector2((int)level.singleLinkedListPlatforms[i].x, (int)level.singleLinkedListPlatforms[i].y), Quaternion.identity);
            PlatformBehavior newPlat = newLLPlatform.GetComponent<PlatformBehavior>();
            LinkBlockBehavior innerLink = newLLPlatform.Find("LinkBlock").GetComponent<LinkBlockBehavior>();
            innerLink.gameController = gameController;
            innerLink.parentPlatform = newPlat;
            newPlat.gameController = gameController;
            newPlat.childLink = innerLink.gameObject;
            newPlat.childValueBlock = newLLPlatform.Find("ValueBlock").gameObject;
            listPlatformMap.Add(level.singleLinkedListPlatforms[i].objId, newPlat);
            levelLinkBlocks.Add(innerLink); // add it to the list of blocks for references
            levelLinkBlocksConnIds.Add(level.singleLinkedListPlatforms[i].childLinkBlockConnectId);
        }

        // establishing links for the link blocks with the platforms
        for (int i = 0; i < levelLinkBlocksConnIds.Count; i++) {
            if (levelLinkBlocksConnIds[i].Length > 0) // if this link has a connection.
            {
                string platformId = levelLinkBlocksConnIds[i];
                // establish the connection
                levelLinkBlocks[i].connectingPlatform = listPlatformMap[platformId];
                levelLinkBlocks[i].connectingPlatform.addIncomingConnectingLink(levelLinkBlocks[i]);
            }
        }
    }


    // Update is called once per frame
    void Update () {
		
	}
}
