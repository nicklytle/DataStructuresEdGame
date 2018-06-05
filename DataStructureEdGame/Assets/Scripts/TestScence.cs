using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class TestScence : MonoBehaviour {
    public Transform ground;
    public Transform player;
    public TextAsset levelDescriptionJson;

	// Use this for initialization
	void Start () {
        if (levelDescriptionJson == null)
        {
            for (int j = 0; j < 16; j++)
            {
                //for (int i = 0; i < 16; i++)
                //{
                Instantiate(ground, new Vector2(j, 0), Quaternion.identity);

                //}

            }
            Instantiate(player, new Vector2(3, 3), Quaternion.identity);
        } else
        {
            BlockCollection world = JsonUtility.FromJson<BlockCollection>(levelDescriptionJson.text);
            for (int i = 0; i < world.blocks.Length; i++)
            {
                Debug.Log(world.blocks[i]);
                Vector2 blockPos = new Vector2((int)world.blocks[i].x, (int)world.blocks[i].y);
                if (world.blocks[i].type.Equals("NORMAL_BLOCK")) // ground
                {
                    Object obj = Instantiate(ground, blockPos, Quaternion.identity);
                }
                else if (world.blocks[i].type.Equals("PLAYER")) // player
                {
                    Object obj = Instantiate(player, blockPos, Quaternion.identity);
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
