using System.Collections.Generic;
using UnityEngine;

public class FungusScript : EventObject
{
    [SerializeField]
    GameObject subFungus;

    private int turnMax = 1;


    protected override void Start()
    {
        base.Start();
        if (subFungus != null)
        {
            for (int i = 0; i < 2; i++)
            {
                int randX, randY;
                do
                {
                    randX = Random.Range(0, map.mapWidth);
                    randY = Random.Range(0, map.mapHeight);
                }
                while (!map.tiles[randX, randY].alive || map.tiles[randX, randY].water || map.tiles[randX, randY].special || map.tiles[randX, randY].HasHive);

                map.tiles[randX, randY].special = true;
                Instantiate(subFungus, new Vector3(randX * 2, 0, randY * 2), Quaternion.identity);
                subFungus.GetComponent<EventObject>().index = -1;
            }
        }
        spawnTile.Flower = FlowerType.Empty;
        game.turnCallback += applyEffect;
    }

    protected virtual void applyEffect()
    {
        turnsActive++;
        foreach (Hive h in player.hives)
        {
            if (h.tileRadius.Contains(spawnTile) && CheckWater(h))
            {
                h.AddCondition("Fungus");
            }
        }

        if (turnsActive >= turnMax)
        {
            spawnTile.special = false;
            eventController.activeEvents--;
            if (index != -1)
                eventController.eventObjectDict[eventController.eventObjects[index]] = false;
            game.turnCallback -= applyEffect;
            Destroy(gameObject);
        }
    }

    private bool CheckWater(Hive h)
    {
        foreach (Tile t in h.tileRadius)
            if (t.water)
                return true;
        return false;
    }
}
