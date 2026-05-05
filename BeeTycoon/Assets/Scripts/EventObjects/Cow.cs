using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : EventObject
{
    private int turnMax = 5;

    Tile currentTile = null;

    protected override void Start()
    {
        base.Start();
        game.turnCallback += applyEffect;
        spawnTile.Flower = FlowerType.Empty;
    }

    private void applyEffect()
    {
        if (currentTile == null)
            currentTile = spawnTile;

        turnsActive++;
        if (turnsActive >= turnMax)
        {
            spawnTile.special = false;
            eventController.activeEvents--;
            eventController.eventObjectDict[eventController.eventObjects[index]] = false;
            game.turnCallback -= applyEffect;
            Destroy(gameObject);
        }

        List<Tile> tiles = map.GetAdjacentTiles(currentTile.x, currentTile.y);
        foreach (Tile t in map.GetDiagonalTiles(currentTile.x, currentTile.y))
            tiles.Add(t);

        List<Tile> flowerTiles = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.Flower != FlowerType.Empty)
                flowerTiles.Add(t);
        }

        Tile rand;
        if (flowerTiles.Count > 0)
            rand = flowerTiles[Random.Range(0, flowerTiles.Count)];
        else
            rand = tiles[Random.Range(0, tiles.Count)];

        rand.special = true;
        currentTile.special = false;

        gameObject.transform.position = new Vector3(rand.x * 2, 0, rand.y * 2);
        gameObject.transform.rotation = Quaternion.Euler((currentTile.transform.position - rand.transform.position).normalized);

        currentTile = rand;
        currentTile.Flower = FlowerType.Empty;
    }
}
