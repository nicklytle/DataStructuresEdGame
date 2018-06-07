using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class TestScence : MonoBehaviour {
    public Transform ground;
    public Transform player;
    public TextAsset levelDescriptionJson;
    
    public Transform GetAssocInstanceFromType(string type)
    {
        if (type.Equals("NORMAL_BLOCK"))
        {
            return ground;
        } else if (type.Equals("PLAYER"))
        {
            return player;
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
                Instantiate(objToInstances, blockPos, Quaternion.identity);
            }
        }
    }


	// Use this for initialization
	void Start () {
        CreateWorldFromLevelDescription();
		//Cursor.visible = false;

    }

	// Update is called once per frame
	void Update () {		
	}
}
