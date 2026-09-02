using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum Tool
{
    HiveTool,
    Smoker,
    Shovel,
    Dolly,
    BeeSuit,
    Extractor
}

public class ToolManager : MonoBehaviour
{
    [SerializeField]
    public ShovelTool shovel;

    [SerializeField]
    public SmokerTool smoker;

    [SerializeField]
    public HiveTool hiveTool;

    [SerializeField]
    public DollyTool dolly;

    [SerializeField]
    public ExtractorTool extractor;

    [SerializeField]
    public SuitTool suit;

    public Dictionary<Tool, bool> toolsMaxed = new Dictionary<Tool, bool>()
    {
        {Tool.Smoker, false},
        {Tool.Shovel, false},
        {Tool.Dolly, false},
        {Tool.HiveTool, false},
        {Tool.BeeSuit, false},
        {Tool.Extractor, false}
    };

    //Object to Move
    [SerializeField]
    private GameObject holo;

    [SerializeField]
    private Material redHolo;

    [SerializeField]
    private Material greenHolo;

    [SerializeField]
    private MapLoader map;

    [SerializeField]
    private UnlockTracker unlocks;

    [SerializeField]
    private UIDocument document;

    private GameObject objectToMove;
    private Vector3 storedPos;
    private Tile storedTile;
    private FlowerType storedFType;
    private GameObject activeHolo;
    bool pickedUpThisFrame;

    private ToolScript activeTool;
    private PlayerController player;

    public Tile peppermintTile = null;
    public GameObject peppermint = null;
    public MiceScript mice = null;

    public GameObject ObjectToMove
    {
        get { return objectToMove; }
        set
        {
            if (value == null)
            {
                objectToMove.transform.position = storedPos;
                if (objectToMove.TryGetComponent<Hive>(out Hive h))
                    h.GetTileRadius(h.x, h.y);
                storedTile.FlowerNoAnimation(storedFType, true);
                storedTile = null;
                storedFType = FlowerType.Empty;
                Destroy(activeHolo);
            }
            else
            {
                Destroy(activeHolo);
                activeHolo = Instantiate(holo, value.transform, true); //holo hover for placeables
                value.TryGetComponent<Cost>(out Cost c);
                if (c != null && c.tree)
                {
                    activeHolo.transform.localScale = new Vector3(3, 3, 3);
                    activeHolo.transform.position = new Vector3(value.transform.position.x, 2f, value.transform.position.z);
                }
                storedPos = value.transform.position;

                pickedUpThisFrame = true;
                if (value.TryGetComponent<Hive>(out Hive h))
                {
                    h.hiveTile.HasHive = false;
                    h.hiveTile.hive = h;
                }
            }
            objectToMove = value;
        }
    }

    void Awake()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
    }

    //Logic for tool functionality
    void Update()
    {
        pickedUpThisFrame = false;

        if (Input.GetMouseButtonDown(1))
        {
            if (ObjectToMove != null)
            {
                if (ObjectToMove.TryGetComponent<Hive>(out Hive h) && !h.isOpen)
                    h.HideHiveRadius();
                ObjectToMove = null;
            }
            activeTool = null;
        }

        if (activeTool == dolly || activeTool == shovel)
        {
            if (ObjectToMove == null)
                CheckForPickup();
            else
            {
                CheckForPlacement();
                if (activeTool == dolly)
                    CheckForRotation();
            }
        }
        else if (activeTool == smoker || activeTool == hiveTool)
        {
            CheckForUse();
        }

        if (objectToMove != null && !pickedUpThisFrame)
        {
            FollowCursor();
        }
        //Else if other tool active, CheckForUse()
    }

    public void TurnReset()
    {
        shovel.TurnReset();
        dolly.TurnReset();
        hiveTool.TurnReset();
        smoker.TurnReset();
    }

    public ToolScript GetToolFromTag(string tag)
    {
        switch (tag)
        {
            case "Dolly":
                return dolly;
            case "Shovel":
                return shovel;
            case "Smoker":
                return smoker;
            case "HiveTool":
                return hiveTool;
            case "Extractor":
                return extractor;
            case "BeeSuit":
                return suit;
            default:
                return null;
        }
    }

    public List<Tool> GetUnmaxedTools()
    {
        List<Tool> unmaxedTools = new List<Tool>();
        foreach (KeyValuePair<Tool, bool> kvp in toolsMaxed)
            if (!kvp.Value && GetToolLevelUnlocked(kvp.Key))
                unmaxedTools.Add(kvp.Key);
        return unmaxedTools;
    }

    private bool GetToolLevelUnlocked(Tool t)
    {
        ToolScript tool = GetToolFromTag(t.ToString());
        if (tool.Level > 0 && unlocks.toolUpgrades[t.ToString() + (tool.Level).ToString()])
            return true;
        else if (tool.Level == 0)
            return true;
        return false;
    }

    public void SetActiveTool(GameObject item)
    {   
        activeTool = GetToolFromTag(item.tag);
    }

    public void SetToolNull(){ 
        activeTool = null; 
    }

    private void CheckForPickup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (activeTool == shovel && shovel.usesLeft > 0 && t.Flower != FlowerType.Empty)
                    {
                        storedTile = t;
                        storedFType = t.Flower;
                        t.FlowerNoAnimation(FlowerType.Empty, true);
                        ObjectToMove = t.FlowerObject;
                    }
                    else if (activeTool == dolly && dolly.usesLeft > 0 && t.HasHive)
                    {
                        ObjectToMove = t.hive.gameObject;
                        storedTile = t;
                        t.hive.HideHiveRadius();
                        t.hive.DisplayHiveRadius();
                    }
                    else if (activeTool == shovel && shovel.usesLeft > 0 && t == peppermintTile)
                    {
                        Debug.Log("Picking up peppermint");
                        storedTile = t;
                        storedFType = t.Flower;
                        t.Flower = FlowerType.Empty;
                        ObjectToMove = peppermint;
                    }
                }
            }
        }
    }

    private void CheckForPlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If trash is clicked, delete flower
            if (Physics.Raycast(ray, out var trashHit, 1000, LayerMask.GetMask("Trash")) && activeTool == shovel)
            {
                player.CheckForFavoriteFlowerUpdates();
                Destroy(objectToMove);

                shovel.usesLeft--;
                VisualElement shovelElem = document.rootVisualElement.Q<VisualElement>("Shovel");
                shovelElem.Q<Label>("Uses").text = shovel.usesLeft.ToString();
                if (shovel.usesLeft == 0)
                    SetDepletedUI(shovelElem);

                CleanUpShovel();
                return;
            }

            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t) && t.alive && !t.water && !t.special)
                {
                    if (activeTool == shovel)
                    {
                        if (t.Flower == FlowerType.Empty && !t.HasHive && objectToMove.tag != "Peppermint")
                        {
                            t.FlowerNoAnimation(storedFType, true);
                            t.FlowerObject = storedTile.FlowerObject;
                            storedTile.FlowerObject = null;
                            player.CheckForFavoriteFlowerUpdates();

                            shovel.usesLeft--;
                            VisualElement shovelElem = document.rootVisualElement.Q<VisualElement>("Shovel");
                            shovelElem.Q<Label>("Uses").text = shovel.usesLeft.ToString();
                            if (shovel.usesLeft == 0)
                                SetDepletedUI(shovelElem);

                            CleanUpShovel(t);
                        }
                        else if (t.Flower == FlowerType.Empty && !t.HasHive && objectToMove.tag == "Peppermint")
                        {
                            ObjectToMove.transform.position = t.transform.position;
                            peppermintTile = t;
                            mice.pepperTile = t;
                            t.FlowerNoAnimation(storedFType, true);
                            t.FlowerObject = storedTile.FlowerObject;
                            storedTile.FlowerObject = null;
                            t.special = true;
                            storedTile.special = false;

                            shovel.usesLeft--;
                            VisualElement shovelElem = document.rootVisualElement.Q<VisualElement>("Shovel");
                            shovelElem.Q<Label>("Uses").text = shovel.usesLeft.ToString();
                            if (shovel.usesLeft == 0)
                                SetDepletedUI(shovelElem);

                            CleanUpShovel(t);
                        }
                    }
                    else if (activeTool == dolly && t.Flower == FlowerType.Empty && !t.HasHive)
                    {
                        Hive h = objectToMove.GetComponent<Hive>();
                        h.hiveTile = t;
                        t.HasHive = true;
                        t.hive = h;
                        h.x = t.x;
                        h.y = t.y;
                        h.transform.position = t.transform.position;
                        h.transform.position += new Vector3(0, 0.5f, 0);

                        dolly.usesLeft--;
                        VisualElement dollyElem = document.rootVisualElement.Q<VisualElement>("Dolly");
                        dollyElem.Q<Label>("Uses").text = dolly.usesLeft.ToString();
                        if (dolly.usesLeft == 0)
                            SetDepletedUI(dollyElem);

                        CleanUpDolly(t);
                        h.GetTileRadius(h.x, h.y);
                        if (!h.isOpen)
                            h.HideHiveRadius();
                        h.CheckForCarniolan();
                    }
                }
            }
        }
    }

    private void CheckForRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Hive h = objectToMove.GetComponent<Hive>();
            if (h.rotation == 270)
                h.rotation = 0;
            else
                h.rotation += 90;
            h.HideHiveRadius();
            h.GetTileRadius(h.x, h.y);
        }
    }

    private void CheckForUse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hiveHit, 1000, LayerMask.GetMask("Hive")))
            {
                //If a hive is clicked with an item, apply the item's effect
                if (hiveHit.collider.gameObject.TryGetComponent<Hive>(out Hive h))
                {
                    if (activeTool == smoker)
                    {
                        h.CureCondition("Aggrevated");
                        if (smoker.calming && !h.conditions.Contains("Relaxed"))
                            h.AddCondition("Relaxed");

                        smoker.usesLeft--;
                        VisualElement smokerlElem = document.rootVisualElement.Q<VisualElement>("Smoker");
                        smokerlElem.Q<Label>("Uses").text = smoker.usesLeft.ToString();
                        if (smoker.usesLeft == 0)
                            SetDepletedUI(smokerlElem);
                    }
                    else if (activeTool == hiveTool)
                    {
                        h.CureCondition("Glued");

                        hiveTool.usesLeft--;
                        VisualElement hiveToolElem = document.rootVisualElement.Q<VisualElement>("Hivetool");
                        hiveToolElem.Q<Label>("Uses").text = hiveTool.usesLeft.ToString();
                        if (hiveTool.usesLeft == 0)
                            SetDepletedUI(hiveToolElem);
                    }
                }
            }
        }
    }

    private void SetDepletedUI(VisualElement elem)
    {
        elem.Q<Label>("Uses").text = "";
        elem.style.unityBackgroundImageTintColor = new Color(0.57f, 0.57f, 0.57f);
        elem.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = new Color(0.57f, 0.57f, 0.57f);
    }

    private void CleanUpShovel(Tile t = null)
    {
        if (activeHolo != null)
            Destroy(activeHolo);
        objectToMove = null;
        if (storedTile != t)
            storedTile.Flower = FlowerType.Empty;
        storedTile = null;
        storedFType = FlowerType.Empty;
    }

    private void CleanUpDolly(Tile t)
    {
        if (activeHolo != null)
            Destroy(activeHolo);
        objectToMove = null;
        if (storedTile != t)
        {
            storedTile.HasHive = false;
            storedTile.hive = null;
            storedTile = null;
        }
        storedFType = FlowerType.Empty;
    }

    private void FollowCursor()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //If a tile is clicked while holding a placeable object, place the object
        if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
        {
            objectToMove.TryGetComponent<Cost>(out Cost c);
            if (c != null && objectToMove.GetComponent<Cost>().tree)
                activeHolo.transform.position = new Vector3(objectToMove.transform.position.x, 2f, objectToMove.transform.position.z);
            else
                activeHolo.transform.position = objectToMove.transform.position;

            if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
            {
                if (c != null && objectToMove.GetComponent<Cost>().tree)
                    objectToMove.transform.position = new Vector3(t.gameObject.transform.position.x + 1, t.gameObject.transform.position.y, t.gameObject.transform.position.z + 1);
                else
                    objectToMove.transform.position = t.gameObject.transform.position;

                if (c != null && objectToMove.GetComponent<Cost>().tree && (t.y == map.mapHeight - 1 || t.x == map.mapWidth - 1 || !t.Check234() || t.HasHive || t.Flower != FlowerType.Empty || t.special) || !t.alive)
                    activeHolo.GetComponent<MeshRenderer>().material = redHolo;
                else if (t.HasHive || t.Flower != FlowerType.Empty || t.water || !t.alive || t.special)
                    activeHolo.GetComponent<MeshRenderer>().material = redHolo;
                else
                    activeHolo.GetComponent<MeshRenderer>().material = greenHolo;

                if (objectToMove.TryGetComponent<Hive>(out Hive h))
                {
                    h.HideHiveRadius();
                    h.GetTileRadius(t.x, t.y);
                    h.DisplayHiveRadius();
                }
            }
        }
        else if (Physics.Raycast(ray, out var hit2, 1000, LayerMask.GetMask("OOB")))
        {
            activeHolo.GetComponent<MeshRenderer>().material = redHolo;
            objectToMove.transform.position = hit2.point;
            if (objectToMove.GetComponent<Cost>().tree)
                activeHolo.transform.position = new Vector3(objectToMove.transform.position.x, objectToMove.transform.position.y + 2f, objectToMove.transform.position.z);
            else
                activeHolo.transform.position = objectToMove.transform.position;
        }
    }
}
