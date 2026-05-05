using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    protected MapLoader map;
    protected PlayerController player;
    protected GameController game;
    protected Tile spawnTile;
    protected GameEventController eventController;
    protected int turnsActive = 0;
    public int index;

    protected virtual void Start()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        game = GameObject.Find("GameController").GetComponent<GameController>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        eventController = GameObject.Find("EventController").GetComponent<GameEventController>();
        spawnTile = map.tiles[(int)transform.position.x / 2, (int)transform.position.z / 2];
    }
}
