using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GameEventController : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> eventObjects = new List<GameObject>();

    private List<string> eventTitles = new List<string>()
    {
        "Wild Cow",
        "Wax Moths",
        "Fungal Growths",
        "Mouse Nest",
        "Swarm"
    };

    private List<string> eventDescriptions = new List<string>()
    {
        "A cow that eats flowers surrounding it\r\nevery turn. Will disappear in 3 turns.",
        "A swarm of wax eating moths. When in\r\na hive's radius, the hive gains 1 stress for 3 turns.\r\nHives on the border of the plot are immune.",
        "Multiple fungus spawn. When both fungus\r\nand a water tile are in a hive's radius,\r\nthe hive gains 1 stress for 3 turns.",
        "A nest of mice. When in a hive's radius,\r\nthe hive gains 2 stress. Peppermint plants\r\nin the hive's radius repel mice.",
        "A swarm of bees. When in a hive's\r\nradius the hive gains 1 stress. If the hive\r\nis in another hive's radius, that hive will defend it.\r\nIf there is an empty hive adjacent to the swarm\r\nIt will enter the hive."
    };

    [SerializeField]
    private VisualTreeAsset eventText;

    public Dictionary<GameObject, bool> eventObjectDict = new Dictionary<GameObject, bool>();

    MapLoader map;

    UIDocument document;

    private GameObject activeObject;

    public int activeEvents = 0;

    private bool animationComplete = false;
    public bool allComplete = false;

    [SerializeField]
    public GameObject queenBee;

    void Start()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        foreach (GameObject g in eventObjects)
            eventObjectDict.Add(g, false);
    }

    public IEnumerator SpawnMapEvent()
    {
        if (activeEvents >= 3 || !CheckAvailability())
            yield break;

        //Chance for event to occur
        if (Random.Range(0, 10) <= 3)
        {
            GetEvent(false);
            yield return new WaitWhile(() => !animationComplete);
            animationComplete = false;
            activeEvents++;
            //chance for additional event to occur
            if (Random.Range(0, 10) <= 1)
            {
                GetEvent(false);
                yield return new WaitWhile(() => !animationComplete);
                animationComplete = false;
                activeEvents++;
            }
        }

        //Chance for swarm to spawn
        int chance = 2;
        foreach (Hive h in GameObject.Find("PlayerController").GetComponent<PlayerController>().hives)
        {
            if (!h.queen.nullQueen && h.population > h.PopCap * .90f)
                chance += 10;
            else if (h.queen.nullQueen)
                chance += 5;
        }

        if (Random.Range(0, 100) <= chance && !eventObjectDict[eventObjects[4]])
        {
            GetEvent(true);
            yield return new WaitWhile(() => !animationComplete);
            animationComplete = false;
            activeEvents++;
        }

        allComplete = true;
    }

    private void GetEvent(bool swarm)
    {
        int randObject;
        if (swarm)
        {
            randObject = 4;
            activeObject = eventObjects[randObject];
        }
        else
        {
            do
            {
                randObject = Random.Range(0, eventObjects.Count);
                activeObject = eventObjects[randObject];
            }
            while ((eventObjectDict[activeObject] && CheckAvailability()));
        }

        eventObjectDict[activeObject] = true;

        EventHelper(randObject);
    }

    private void EventHelper(int randObject)
    {
        int randX, randY;
        do
        {
            randX = Random.Range(0, map.mapWidth);
            randY = Random.Range(0, map.mapHeight);
        }
        while (!map.tiles[randX, randY].alive || map.tiles[randX, randY].water || map.tiles[randX, randY].special || map.tiles[randX, randY].HasHive);

        map.tiles[randX, randY].special = true;
        map.tiles[randX, randY].Flower = FlowerType.Empty;
        GameObject instantiatedObject;
        if (randObject == 1)
            instantiatedObject = Instantiate(activeObject, new Vector3((randX * 2) -0.5f, 1, (randY * 2) - 0.5f), Quaternion.identity); //Bad lazy code. Model won't set origin correctly for moths
        else
            instantiatedObject = Instantiate(activeObject, new Vector3(randX * 2, 0, randY * 2), Quaternion.identity);
        instantiatedObject.GetComponent<EventObject>().index = randObject;

        TemplateContainer container = eventText.Instantiate();
        container.style.flexGrow = 1;
        container.Q<Label>("Title").text = eventTitles[randObject];
        container.Q<Label>("Desc").text = eventDescriptions[randObject];
        document.rootVisualElement.Q<VisualElement>("Base").Add(container);
        

        StartCoroutine(AnimateEvent(instantiatedObject));
    }

    private bool CheckAvailability()
    {
        foreach (KeyValuePair<GameObject, bool> kvp in eventObjectDict)
            if (!kvp.Value)
                return true;
        return false;
    }

    private IEnumerator AnimateEvent(GameObject eventObject)
    {
        Camera.main.transform.position = new Vector3(eventObject.transform.position.x, 0, eventObject.transform.position.z); //Moths aren't centered, idk why
        Camera.main.transform.position += new Vector3(0, 3, -2);

        float timeLapsed = 0.0f;

        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
        {
            Camera.main.transform.LookAt(eventObject.transform);
            Camera.main.transform.Translate(Vector3.right * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
            timeLapsed += Time.deltaTime;
        }

        document.rootVisualElement.Q<VisualElement>("Base").Clear();

        animationComplete = true;
    }
}