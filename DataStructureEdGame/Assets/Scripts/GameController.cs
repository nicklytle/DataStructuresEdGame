using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform ground;
    public Transform player;
    public Transform interactTest;
    public TextAsset levelDescriptionJson;
    public Transform playerRef;
    public bool generateWorld;

    public Transform GetAssocInstanceFromType(string type)
    {
        if (type.Equals("NORMAL_BLOCK"))
        {
            return ground;
        }
        else if (type.Equals("PLAYER"))
        {
            return player;
        }
        else if (type.Equals("INTERACT_TEST"))
        {
            return interactTest;
        }
        return null;
    }

    public void CreateWorldFromLevelDescription()
    {
        BlockCollection world = JsonUtility.FromJson<BlockCollection>(levelDescriptionJson.text);
        for (int i = 0; i < world.blocks.Length; i++)
        {
            Vector2 blockPos = new Vector2((int)world.blocks[i].x, (int)world.blocks[i].y);
            Transform objToInstances = GetAssocInstanceFromType(world.blocks[i].type);
            if (objToInstances != null)
            {
                Transform obj = Instantiate(objToInstances, blockPos, Quaternion.identity);
                if (objToInstances == player)
                {
                    playerRef = obj;
                }
            }
        }
    }


    // Use this for initialization
    void Start()
    {
        if (generateWorld) { 
            CreateWorldFromLevelDescription();
            //Cursor.visible = fa}lse;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRef != null)
        {
            transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z);
        }
    }
}
