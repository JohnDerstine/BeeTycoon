using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MothScript : EventObject
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
        foreach (Hive h in player.hives)
        {
            if (h.tileRadius.Contains(spawnTile) && !map.IsOnBorder(map.tiles[h.x, h.y]))
            {
                h.AddCondition("Moths");
            }
        }
        if (turnsActive >= turnMax)
        {
            spawnTile.special = false;
            eventController.activeEvents--;
            eventController.eventObjectDict[eventController.eventObjects[index]] = false;
            game.turnCallback -= applyEffect;
            Destroy(gameObject);
        }
    }
}
