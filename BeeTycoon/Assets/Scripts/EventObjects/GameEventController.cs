using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventController : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> eventObjects = new List<GameObject>();

    public Dictionary<GameObject, bool> eventObjectDict= new Dictionary<GameObject, bool>();

    MapLoader map;

    private GameObject activeObject;

    public int activeEvents = 0;

    void Start()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        foreach (GameObject g in eventObjects)
            eventObjectDict.Add(g, false);
    }

    public void SpawnMapEvent()
    {
        if (activeEvents >= 3 || !CheckAvailability())
            return;

        //Chance for event to occur
        if (Random.Range(0, 10) <= 3)
        {
            EventHelper();
            activeEvents++;
            //chance for additional event to occur
            if (Random.Range(0, 10) <= 1)
            {
                EventHelper();
                activeEvents++;
            }
        }
    }

    private void EventHelper()
    {
        int randObject = Random.Range(0, eventObjects.Count);
        activeObject = eventObjects[randObject];
        while (eventObjectDict[activeObject] && CheckAvailability())
        {
            randObject = Random.Range(0, eventObjects.Count);
            activeObject = eventObjects[randObject];
        }
        eventObjectDict[activeObject] = true;

        int randX, randY;
        do
        {
            randX = Random.Range(0, map.mapWidth);
            randY = Random.Range(0, map.mapHeight);
        }
        while (!map.tiles[randX, randY].alive || map.tiles[randX, randY].water || map.tiles[randX, randY].special || map.tiles[randX, randY].HasHive);

        map.tiles[randX, randY].special = true;
        map.tiles[randX, randY].Flower = FlowerType.Empty;
        if (randObject == 1)
            Instantiate(activeObject, new Vector3((randX * 2) -0.5f, 1, (randY * 2) - 0.5f), Quaternion.identity); //Bad lazy code. Model won't set origin correctly for moths
        else
            Instantiate(activeObject, new Vector3(randX * 2, 0, randY * 2), Quaternion.identity);
        activeObject.GetComponent<EventObject>().index = randObject;
    }

    private bool CheckAvailability()
    {
        foreach (KeyValuePair<GameObject, bool> kvp in eventObjectDict)
            if (!kvp.Value)
                return true;
        return false;
    }
}