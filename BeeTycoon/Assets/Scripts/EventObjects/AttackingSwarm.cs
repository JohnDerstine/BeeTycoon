using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingSwarm : EventObject
{
    private int turnMax = 3;

    protected override void Start()
    {
        base.Start();
        game.turnCallback += applyEffect;
        spawnTile.Flower = FlowerType.Empty;
    }

    protected virtual void applyEffect()
    {
        turnsActive++;

        foreach (Tile t in map.GetAdjacentTiles(spawnTile.x, spawnTile.y))
        {
            if (t.HasHive && !t.hive.queen.nullQueen)
            {
                QueenBee queen = Instantiate(eventController.queenBee, new Vector3(-100, -100, -100), Quaternion.identity).GetComponent<QueenBee>();
                t.hive.Populate(queen);

                RemoveSwarm();
            }
        }

        foreach (Hive h in player.hives)
        {
            if (h.tileRadius.Contains(spawnTile) && !CheckHiveOverlap(h))
            {
                h.AddCondition("Swarmed");
            }
        }

        if (turnsActive >= turnMax)
            RemoveSwarm();
    }

    private void RemoveSwarm()
    {
        foreach (Hive h in player.hives)
        {
            if (h.conditions.Contains("Swarmed"))
                h.CureCondition("Swarmed");
        }
        spawnTile.special = false;
        eventController.activeEvents--;
        eventController.eventObjectDict[eventController.eventObjects[index]] = false;
        game.turnCallback -= applyEffect;
        Destroy(gameObject);
    }

    private bool CheckHiveOverlap(Hive thisHive)
    {
        foreach (Hive h in player.hives)
        {
            if (h != thisHive && h.tileRadius.Contains(map.tiles[thisHive.x, thisHive.y]))
                return true;
        }
        return false;
    }
}
