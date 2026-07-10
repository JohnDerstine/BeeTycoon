using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiceScript : EventObject
{
    [SerializeField]
    GameObject peppermint;

    public Tile pepperTile;

    private int turnMax = 3;

    private GameObject activePepper;

    private ToolManager toolManager;

    protected override void Start()
    {
        base.Start();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        int randX, randY;
        do
        {
            randX = Random.Range(0, map.mapWidth);
            randY = Random.Range(0, map.mapHeight);
        }
        while (!map.tiles[randX, randY].alive || map.tiles[randX, randY].water || map.tiles[randX, randY].special || map.tiles[randX, randY].HasHive);
        map.tiles[randX, randY].special = true;
        activePepper = Instantiate(peppermint, new Vector3(randX * 2, 0, randY * 2), Quaternion.identity);
        pepperTile = map.tiles[randX, randY];
        pepperTile.Flower = FlowerType.Empty;
        toolManager.peppermintTile = pepperTile;
        toolManager.peppermint = activePepper;
        toolManager.mice = this;
        game.turnCallback += applyEffect;
    }

    private void applyEffect()
    {
        turnsActive++;
        foreach (Hive h in player.hives)
        {
            if (h.tileRadius.Contains(spawnTile) && !h.tileRadius.Contains(pepperTile))
            {
                h.AddCondition("Mice");
            }
        }

        if (turnsActive >= turnMax)
        {
            toolManager.peppermint = null;
            toolManager.peppermintTile = null;
            toolManager.mice = null;
            game.turnCallback -= applyEffect;
            pepperTile.special = false;
            Destroy(activePepper);
            spawnTile.special = false;
            Destroy(gameObject);
            eventController.activeEvents--;
            eventController.eventObjectDict[eventController.eventObjects[index]] = false;
        }
    }

    //TODO: Let shovel move peppermint, assign tiles state to sepcial, remove previous tiles special status
    //
}
