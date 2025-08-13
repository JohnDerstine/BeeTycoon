using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    [SerializeField]
    GameController game;

    List<List<Texture2D>> spriteList = new List<List<Texture2D>>();

    List<List<GameObject>> objectList = new List<List<GameObject>>();

    [SerializeField]
    List<Texture2D> flowerSprites = new List<Texture2D>();

    [SerializeField]
    List<GameObject> flowerObjectList = new List<GameObject>();

    [SerializeField]
    List<Texture2D> beeSprites = new List<Texture2D>();

    [SerializeField]
    List<GameObject> beeObjectList = new List<GameObject>();

    [SerializeField]
    List<Texture2D> toolSprites = new List<Texture2D>();

    [SerializeField]
    List<GameObject> toolObjectList = new List<GameObject>();

    [SerializeField]
    List<Texture2D> hiveSprites = new List<Texture2D>();

    [SerializeField]
    List<GameObject> hiveObjectList = new List<GameObject>();

    [SerializeField]
    MapLoader map;

    [SerializeField]
    UIDocument ui;

    [SerializeField]
    HoneyMarket honeyMarket;

    [SerializeField]
    public Sprite honey;

    [SerializeField]
    private GameObject testQueen;

    [SerializeField]
    private Texture2D testQueenSprite;

    [SerializeField]
    private VisualTreeAsset queenUI;

    [SerializeField]
    private GameObject standObject;

    private TemplateContainer hoverTemplate;

    private Glossary glossary;

    //[SerializeField]
    //private VisualTreeAsset hiveUI;

    private TemplateContainer activeUI;
    public Hive currentHive;

    private List<Hive> hives = new List<Hive>();

    private VisualElement root;
    private VisualElement left;
    private CustomVisualElement tab1;
    private CustomVisualElement tab2;
    private CustomVisualElement tab3;
    private CustomVisualElement tab4;

    private int tab1ItemCount = 0;
    private int tab2ItemCount = 6;
    private int tab3ItemCount = 8;
    private int tab4ItemCount = 5;

    [SerializeField]
    Texture2D hex;

    [SerializeField]
    StyleSheet tabStyle;

    [SerializeField]
    StyleSheet itemStyle;

    [SerializeField]
    StyleSheet costStyle;

    int tabItemsPerRow = 8;

    private List<CustomVisualElement> tabHexes = new List<CustomVisualElement>();
    private List<CustomVisualElement> tabs = new List<CustomVisualElement>();
    private List<int> tabItemCounts = new List<int>();
    private CustomVisualElement activeTab;
    private Label moneyLabel;

    private Manipulator close;
    public Manipulator open1;
    private Manipulator open2;
    private Manipulator open3;
    private Manipulator open4;
    private EventCallback<PointerUpEvent> endSelectionCallback;

    private Hive selectedHive;

    private VisualElement selectedHex = null;
    private GameObject selectedItem = null;
    private Texture2D selectedItemSprite;
    private GameObject hoverObject = null;
    private bool hovering;

    private GameObject objectToMove;
    private Vector3 storedPos;
    private bool pickedUpThisFrame = false;
    private Tile storedTile;

    EventCallback<PointerMoveEvent, int> queenMoveCallback;
    EventCallback<PointerLeaveEvent> queenExitCallback;

    private int money = 50;
    public Dictionary<FlowerType, List<float>> inventory = new Dictionary<FlowerType, List<float>>();

    public GameObject SelectedItem
    {
        get { return selectedItem; }
        set
        {
            selectedItem = value;
            if (value == null)// && selectedItem != null)
            {
                hovering = false;
                hoverObject = null;
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                
                if (selectedHex != null)
                    selectedHex.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);

                if (objectToMove != null)
                {
                    if (objectToMove.TryGetComponent<Hive>(out Hive h))
                        h.hiveTile.hasHive = true;

                    objectToMove.transform.position = storedPos;
                    objectToMove = null;
                }
            }
            else if (value != null)
            {
                if (selectedItem.tag == "Placeable" || selectedItem.tag == "Super")
                    hoverObject = Instantiate(selectedItem, new Vector3(-100, -100, -100), Quaternion.identity);
                else
                {

                    if (selectedItem.tag == "Bee")
                    {
                        hoverObject = Instantiate(selectedItem);
                        StartCoroutine(hoverObject.GetComponent<QueenBee>().TransferStats(selectedItem.GetComponent<QueenBee>()));
                    }
                }
                hovering = true;
            }
        }
    }

    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            moneyLabel.text = "$" + money;

            if (honeyMarket.marketOpen)
                honeyMarket.marketTemplate.Q<Label>("MoneyLabel").text = "$" + money;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.Find("GameController").GetComponent<GameController>();
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        ui = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        glossary = ui.gameObject.GetComponent<Glossary>();
        root = ui.rootVisualElement;
        left = root.Q<VisualElement>("Left");
        moneyLabel = root.Q<Label>("Money");
        moneyLabel.text = "$" + money;

        close = new Clickable(close => CloseTab());
        open1 = new Clickable(open => OpenTab(0, open1, false));
        open2 = new Clickable(open => OpenTab(1, open2, false));
        open3 = new Clickable(open => OpenTab(2, open3, false));
        open4 = new Clickable(open => OpenTab(3, open4, false));
        endSelectionCallback = new EventCallback<PointerUpEvent>(EndQueenSelection);

        tab1 = root.Q<CustomVisualElement>("tab1");
        tab1.AddManipulator(open1);
        tabs.Add(tab1);
        tab2 = root.Q<CustomVisualElement>("tab2");
        tab2.AddManipulator(open2);
        tabs.Add(tab2);
        tab3 = root.Q<CustomVisualElement>("tab3");
        tab3.AddManipulator(open3);
        tabs.Add(tab3);
        tab4 = root.Q<CustomVisualElement>("tab4");
        tab4.AddManipulator(open4);
        tabs.Add(tab4);

        RefreshMenuLists();

        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent, int>(OnQueenMove);

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            inventory.Add(fType, new List<float> {0, 0, 0, 0});
        }

        for (int i = 0; i < flowerObjectList.Count; i++)
            flowerObjectList[i].GetComponent<Cost>().ftype = (FlowerType)(i + 2);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            SaveSystem.Save();
        if (Input.GetKeyDown(KeyCode.P))
            SaveSystem.Load();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            map.GenerateFlowers();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(AddQueen());
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectedItem = hivePrefab;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (glossary.open)
                glossary.CloseGlossary();
            else if (honeyMarket.marketOpen)
                honeyMarket.CloseMarket();
            else if (SelectedItem != null)
            {
                Destroy(hoverObject);
                SelectedItem = null;
            }
            else
                CloseTab();
        }

        if (Input.GetKeyDown(KeyCode.G))
            glossary.OpenGlossary("Species");

        pickedUpThisFrame = false;
        //Checks to see if selected item is placed
        if (SelectedItem != null)
            checkForClick();

        if (objectToMove != null && !pickedUpThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
                objectToMove.transform.position = tileHit.point;
            CheckForPlacement();
        }



        //Displays selected item under mouse
        if (hovering && hoverObject != null && selectedItem != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (hoverObject.tag == "Placeable")
            {
                if (Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Tile")))
                {
                    if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                        hoverObject.transform.position = t.gameObject.transform.position;                        
                }
            }
            else
            {
                if (Physics.Raycast(ray, out var hit, 1000))
                    hoverObject.transform.position = hit.point + new Vector3(0, 1.5f, 0);
            }
        }
        //Map Controls
        CheckZoom();
        PanCamera();
    }

    //Check for user clicks on various GameObjects or when an object from the hex menu is selected
    private void checkForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (hoverObject != null && hoverObject.tag == "Placeable")
                    {
                        hoverObject.transform.position = t.gameObject.transform.position;
                        if (hoverObject.TryGetComponent(out Hive h))
                        {
                            hives.Add(h);
                            Money -= hoverObject.GetComponent<Cost>().Price;
                            h.x = (int)t.transform.position.x;
                            h.y = (int)t.transform.position.z;
                            SelectedItem = null;
                            h.Placed = true;
                            h.SetUpTemplate();
                            t.hasHive = true;
                            h.hiveTile = t;
                        }
                        else if (hoverObject.TryGetComponent<Cost>(out Cost c))
                        {
                            Destroy(hoverObject);
                            hoverObject = null;
                            if (c.ftype != FlowerType.Empty)
                                t.Flower = c.ftype;
                            Money -= c.Price;
                            SelectedItem = null;
                        }
                        return;
                    }
                    else if (selectedItem.tag == "Shovel" && t.Flower != FlowerType.Empty)
                    {
                        objectToMove = t.FlowerObject;
                        storedPos = t.FlowerObject.transform.position;
                        storedTile = t;
                        pickedUpThisFrame = true;
                        Debug.Log("Picked up object");
                    }
                }
            }
            if (Physics.Raycast(ray, out var hiveHit, 1000, LayerMask.GetMask("Hive")))
            {
                //If a hive is clicked with an item, apply the item's effect
                if (hiveHit.collider.gameObject.TryGetComponent<Hive>(out Hive h))
                {

                    if (selectedItem.tag != "Placeable" && selectedItem.tag != "Dolly" && selectedItem.tag != "Shovel" && selectedItem.tag != "HiveTool" && selectedItem.tag != "Smoker")
                    {
                        int cost = selectedItem.GetComponent<Cost>().Price;
                        Money -= cost;
                        if (selectedItem.TryGetComponent(out QueenBee queen))
                        {
                            h.Populate(queen, selectedItemSprite);
                            Money -= hoverObject.GetComponent<Cost>().Price;
                            beeObjectList.Remove(SelectedItem);
                            beeSprites.Remove(selectedItemSprite);
                            tab1ItemCount--;
                            RefreshMenuLists();
                            OpenTab(0, open1, false);
                        }
                        else if (selectedItem.tag == "Super")
                        {
                            if (h.Size < 5)
                            {
                                h.Size = 1;
                                GameObject newLevel = Instantiate(selectedItem, h.gameObject.transform);
                                newLevel.transform.localPosition = new Vector3(0, h.Size - 1, 0);
                            }
                        }
                        else if (selectedItem.tag == "Frame")
                        {
                            if (h.Frames < 10)
                            {
                                h.Frames++;
                            }
                        }
                        else if (selectedItem.tag == "Sugar")
                        {
                            if (!h.hasSugar)
                            {
                                h.AddSugarWater();
                            }
                        }
                        else if (selectedItem.tag == "Reducer")
                        {
                            if (!h.hasReducer)
                            {
                                h.hasReducer = true;
                                if (h.Condition == "Mice")
                                    h.CureCondition();
                            }
                        }
                        else if (selectedItem.tag == "Stand")
                        {
                            if (!h.hasStand)
                            {
                                h.hasStand = true;
                                Instantiate(standObject, h.transform.position, Quaternion.identity);
                                h.transform.position += Vector3.up;
                            }
                        }
                        else if (selectedItem.tag == "Repellant")
                        {
                            if (h.Condition == "Mites")
                                h.CureCondition();
                        }
                        else if (selectedItem.tag == "Insulation")
                        {
                            if (!h.hasReducer)
                            {
                                h.hasInsulation = true;
                            }
                        }

                        Destroy(hoverObject);
                        SelectedItem = null;
                        selectedItemSprite = null;
                    }

                    if (SelectedItem != null && selectedItem.tag == "Dolly" && h.hiveTile.hasHive == true)
                    {
                        objectToMove = h.gameObject;
                        storedPos = h.gameObject.transform.position;
                        h.hiveTile.hasHive = false;
                        pickedUpThisFrame = true;
                        Debug.Log("Picked up object");
                    }
                    else if (SelectedItem != null && selectedItem.tag == "HiveTool" && h.Condition == "Glued")
                    {
                        h.CureCondition();
                    }
                    else if (SelectedItem != null && selectedItem.tag == "Smoker" && h.Condition == "Aggrevated")
                    {
                        h.CureCondition();
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

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (t.Flower == FlowerType.Empty && !t.hasHive)
                    {
                        if (objectToMove.TryGetComponent<Hive>(out Hive h))
                        {
                            h.hiveTile = t;
                            t.hasHive = true;
                            Debug.Log("Put down object");
                        }
                        else
                        {
                            t.Flower = objectToMove.GetComponent<Cost>().ftype;
                            storedTile.Flower = FlowerType.Empty;
                            Debug.Log("Put down object");
                        }
                        objectToMove.transform.position = t.transform.position;
                        objectToMove = null;
                    }
                }
            }
        }
    }

    public void OnTurnIncrement()
    {
        Debug.Log("turn++");
        foreach (Hive h in hives)
            h.UpdateHive();
        honeyMarket.UpdateMarket();
    }

    #region Hex Tab Menus
    public void OpenTab(int num, Manipulator open, bool fromHive, Hive hive = null)
    {
        if (game.CurrentState == GameStates.TurnEnd || game.CurrentState == GameStates.Paused)
            return;

        //Close any open tabs
        if (activeTab != null)
        {
            CloseTab();
        }

        if (fromHive)
        {
            selectedHive = hive;
            foreach (CustomVisualElement t in tabs)
                t.RegisterCallback(endSelectionCallback);
        }

        CustomVisualElement tab = tabs[num];

        int items = tabItemCounts[tabs.IndexOf(tab)];
        int itemsInRow = -1;
        int rows = 1;

        activeTab = tab;
        tab.AddManipulator(close);
        tab.RemoveManipulator(open);

        //Grey out other tabs, highlight clicked tab
        foreach (CustomVisualElement t in tabs)
        {
            if (t != tab)
                t.style.unityBackgroundImageTintColor = new Color(0.36f, 0.21f, 0.17f, 1);
            else
                t.style.unityBackgroundImageTintColor = new Color(1f, 1f, 0f, 1f);
        }

        //Separate calculations for first item of each row
        if (items != 0)
        {
            SpawnTopHex(num, fromHive);
            itemsInRow++;
        }

        if (items == 1)
            return;

        //Create hex items that belong to that tab
        for (int i = 1; i < items; i++)
        {
            //hex to be added
            CustomVisualElement hex = new CustomVisualElement();
            //Get the last hex created
            CustomVisualElement lastHex = tabHexes[tabHexes.Count - 1];
            hex.styleSheets.Add(tabStyle);

            //Calculate and set position of new hex to be flush with the previous hexes
            //NOTE: tab1.resolvedStyle is used as the basis calculations since it is placed before runtime
            //and the resolvedStyle of it will give back dimensions that take into account scaling with screen size.
            float hexTop = (tab1.resolvedStyle.height * itemsInRow * .625f) + (tab1.resolvedStyle.height * .5f) + (tab1.resolvedStyle.marginBottom * .5f);
            hex.style.top = hexTop;
            StyleLength hexLeft = (itemsInRow % 2) == 0 ? lastHex.resolvedStyle.left - (tab1.resolvedStyle.width * .125f) + (tab1.resolvedStyle.width * .75f * (rows - 1))
                : lastHex.resolvedStyle.left + (tab1.resolvedStyle.width * .24f) + (tab1.resolvedStyle.width * .75f * (rows - 1));
            hex.style.left = hexLeft;

            //Set the width and height
            hex.style.height = tab1.resolvedStyle.height;
            hex.style.width = tab1.resolvedStyle.width;

            //Add to UI container and the list of active hexes
            left.Add(hex);
            tabHexes.Add(hex);

            //Add Icon and Cost Label to each hex item
            VisualElement icon = new VisualElement();
            Label costLabel = new Label();

            int list = num; //For some reason, when the clickable event is triggered, it goes back to find
            int item = i; //what num and i are equal to retroactivly. This causes index out of bounds. Store values as ints to avoid.
            int cost = SetHexImageObject(icon, costLabel, num, item);

            AddHexManipulators(hex, fromHive, list, item, cost);

            hex.Add(icon);
            hex.Add(costLabel);
            itemsInRow++;

            //Start a new row when end of the row is reached.
            if (itemsInRow == tabItemsPerRow - 1)
            {
                itemsInRow = -1;
                rows++;
            }
        }
    }

    //Spawn the top hex of the Hex tab menu
    //This is different from all the other hexes because each hex bases their position
    //off the previous hex. This isn't possible for the first hex
    private void SpawnTopHex(int num, bool fromHive)
    {
        //styling
        CustomVisualElement starterHex = new CustomVisualElement();
        starterHex.styleSheets.Add(tabStyle);
        float itemTop = tab1.resolvedStyle.top;
        starterHex.style.top = itemTop;
        StyleLength itemLeft = tab1.resolvedStyle.width * .25f;
        starterHex.style.left = itemLeft;

        starterHex.style.height = tab1.resolvedStyle.height;
        starterHex.style.width = tab1.resolvedStyle.width;
        left.Add(starterHex);
        tabHexes.Add(starterHex);

        VisualElement icon = new VisualElement();
        Label costLabel = new Label();
        int cost = SetHexImageObject(icon, costLabel, num, 0);

        //Add manipulators and callbacks
        AddHexManipulators(starterHex, fromHive, num, 0, cost);

        starterHex.Add(icon);
        starterHex.Add(costLabel);
    }

    private int SetHexImageObject(VisualElement icon, Label costLabel, int num, int index)
    {
        icon.styleSheets.Add(itemStyle);
        icon.style.backgroundImage = spriteList[num][index];
        costLabel.styleSheets.Add(costStyle);
        int cost = objectList[num][index].GetComponent<Cost>().Price;
        if (num == 2 && objectList[num][index].GetComponent<Cost>().Purchased)
        {
            costLabel.text = "Purchased";
            return 0;
        }
        costLabel.text = (cost == 0) ? "Purchased" : "$" + cost; 
        return cost;
    }

    private void AddHexManipulators(CustomVisualElement hex, bool fromHive, int num, int index, int cost)
    {
        if (!fromHive)
            hex.AddManipulator(new Clickable(e => SelectItem(objectList[num][index], spriteList[num][index], cost, hex)));
        else
            hex.AddManipulator(new Clickable(e => SelectHive(objectList[num][index], spriteList[num][index], cost, selectedHive)));

        if (num == 0)
        {
            QueenBee queen = objectList[num][index].GetComponent<QueenBee>();
            hex.RegisterCallback(queenMoveCallback, index);
            hex.RegisterCallback(queenExitCallback);
        }
    }

    //This shouldn't be needed, but I've used it in case this is ever a problem. (Testing purposes)
    private void RefreshMenuLists()
    {
        CloseTab();
        tabItemCounts.Clear();
        spriteList.Clear();
        objectList.Clear();

        tabItemCounts.Add(tab1ItemCount);
        tabItemCounts.Add(tab2ItemCount);
        tabItemCounts.Add(tab3ItemCount);
        tabItemCounts.Add(tab4ItemCount);

        spriteList.Add(beeSprites);
        spriteList.Add(toolSprites);
        spriteList.Add(hiveSprites);
        spriteList.Add(flowerSprites);

        objectList.Add(beeObjectList);
        objectList.Add(toolObjectList);
        objectList.Add(hiveObjectList);
        objectList.Add(flowerObjectList);
    }

    //Close Tabs
    public void CloseTab()
    {
        if (activeTab == null)
            return;
        else
        {
            foreach (CustomVisualElement t in tabs)
                t.style.unityBackgroundImageTintColor = Color.white;
            foreach (CustomVisualElement hex in tabHexes)
                left.Remove(hex);

            activeTab.RemoveManipulator(close);

            //Must check each case because the corresponding manipulator to open the tab must be readded
            if (activeTab == tab1)
                activeTab.AddManipulator(open1);
            else if (activeTab == tab2)
                activeTab.AddManipulator(open2);
            else if (activeTab == tab3)
                activeTab.AddManipulator(open3);
            else if (activeTab == tab4)
                activeTab.AddManipulator(open4);

            activeTab = null;
            tabHexes.Clear();
        }
    }

    //Check to see if a queen was selected from the shop normally
    private void SelectItem(GameObject item, Texture2D sprite, int cost, VisualElement hex)
    {
        if (money < cost)
            return;

        if (selectedHex != null)
        {
            selectedHex.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);
            selectedHex = null;

            if (selectedItem == item)
            {
                SelectedItem = null;
                return;
            }
        }


        if (item.tag != "Placeable")
        {
            UnityEngine.Cursor.SetCursor(sprite, new Vector2(sprite.width / 2, sprite.height / 2), CursorMode.Auto);
            SelectedItem = item;
            selectedItemSprite = sprite;
        }

        if (item.tag == "Placeable")
        {
            SelectedItem = item;
            selectedItemSprite = sprite;
        }
        else if (item.GetComponent<Cost>().OneTime)
        {
            item.GetComponent<Cost>().Purchased = true;
            Label costLabel = hex.Q<Label>();
            costLabel.text = "Purchased";
            Money -= cost;
        }

        hex.style.unityBackgroundImageTintColor = new Color(0.65f, 0.65f, 0.65f, 1f);
        selectedHex = hex;
    }

    //Check to see if a Queen was selected from the shop, after clicking on the HiveUI queen button
    private void SelectHive(GameObject item, Texture2D sprite, int cost, Hive hive)
    {
        if (money < cost)
            return;

        Money -= cost;
        hive.Populate(item.GetComponent<QueenBee>(), sprite);
        beeObjectList.Remove(item);
        beeSprites.Remove(sprite);
        tab1ItemCount--;
        if (hoverTemplate != null)
        {
            ui.rootVisualElement.Q("Base").Remove(hoverTemplate);
            hoverTemplate = null;
        }
        hive.CloseQueenSelection();
        RefreshMenuLists();
    }

    //Cancel queen selection from HiveUI queen button
    private void EndQueenSelection(PointerUpEvent e)
    {
        foreach (CustomVisualElement tab in tabs)
            tab.UnregisterCallback(endSelectionCallback);
        selectedHive.queenClick.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = selectedHive.lightTint;
        selectedHive.selectingQueen = false;
        selectedHive.queenClick.AddManipulator(selectedHive.assignQueen);
    }

    //Add hover template
    private void OnQueenMove(PointerMoveEvent e, int num)
    {
        QueenBee queen = objectList[0][num].GetComponent<QueenBee>();
        Texture2D sprite = spriteList[0][num];

        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        if (target.ContainsPoint(e.localPosition))
        {
            if (hoverTemplate == null)
            {
                hoverTemplate = queenUI.Instantiate();
                ui.rootVisualElement.Q("Base").Add(hoverTemplate);
                VisualElement popup = hoverTemplate.Q<VisualElement>("Popup");

                //Resolved style is NaN until updated
                popup.RegisterCallback((GeometryChangedEvent evt) => {
                    hoverTemplate.style.position = Position.Absolute;
                    hoverTemplate.style.left = e.position.x;
                    hoverTemplate.style.top = e.position.y - popup.resolvedStyle.height / 2f;
                    //Make sure the popup isn't off-screen
                    if (e.position.y - popup.resolvedStyle.height / 2f < 0)
                    {
                        hoverTemplate.style.top = 0;
                    }
                    else if (e.position.y + popup.resolvedStyle.height - popup.resolvedStyle.height / 2f > Screen.height)
                    {
                        hoverTemplate.style.bottom = Screen.height;
                        hoverTemplate.style.top = Screen.height - popup.resolvedStyle.height;
                    }
                });

                //Update tooltip text to reflect queen stats
                popup.Q<VisualElement>("Icon").style.backgroundImage = sprite;
                popup.Q<Label>("Species").text = "Species: " + queen.species;
                popup.Q<Label>("Age").text = "Age: " + queen.age.ToString() + " Months";
                popup.Q<Label>("Grade").text = "Grade: " + queen.grade.ToString() + "/10";
                VisualElement quirkContainer = popup.Q<VisualElement>("QuirkContainer");
                foreach (string s in queen.quirks)
                {
                    Label quirk = new Label();
                    quirk.text = s;
                    quirk.AddToClassList("Quirk2");
                    quirkContainer.Add(quirk);
                }
            }
        }
        else
        {
            if (hoverTemplate != null)
            {
                ui.rootVisualElement.Q("Base").Remove(hoverTemplate);
                hoverTemplate = null;
            }
        }
    }

    //Remove hover tooltip
    private void OnQueenExit(PointerLeaveEvent e)
    {
        if (hoverTemplate != null)
        {
            ui.rootVisualElement.Q("Base").Remove(hoverTemplate);
            hoverTemplate = null;
        }
    }
    #endregion

    public void OpenHiveUI(TemplateContainer template, VisualTreeAsset hiveUI, Hive hive)
    {
        Debug.Log(hive.canBeOpened + " " + game.CurrentState + " " + selectedItem);
        if (selectedItem != null || game.CurrentState == GameStates.Paused || game.CurrentState == GameStates.TurnEnd || hive.canBeOpened == false)
            return;

        Debug.Log("opening hive");
        //Check to see if the same hive is being clicked to close the hiveUI
        bool reclicked = false;
        if (activeUI == template && activeUI != null)
            reclicked = true;

        //Close any other open hive UI
        if (activeUI != null)
            CloseHiveUI(hive);

        if (reclicked)
            return;

        hive.isOpen = true;

        //Setup template
        if (template == null)
        {
            template = hiveUI.Instantiate();
            template.style.top = 50f;
            template.style.left = -100f;
            template.style.scale = new StyleScale(new Scale(new Vector3(0.8f, 0.8f, 1)));
            template.style.position = Position.Absolute;
        }

        //Add template to the UI document and save the template through the hive
        ui.rootVisualElement.Q("Right").Add(template);

        hive.template = template;
        activeUI = template;
        currentHive = hive;
    }

    public void CloseHiveUI(Hive hive)
    {
        if (activeUI == null)
            return;

        ui.rootVisualElement.Q("Right").Remove(activeUI);
        activeUI = null;
        currentHive = null;
        hive.isOpen = false;
    }

    //Coroutine, otherwise price label is $0 when added to the hex item list
    public IEnumerator AddQueen(QueenBee q = null)
    {
        if (q == null)
            beeObjectList.Add(Instantiate(testQueen, new Vector3(-100, -100, -100), Quaternion.identity));
        else
            beeObjectList.Add(q.gameObject);
        beeSprites.Add(testQueenSprite);
        yield return new WaitForFixedUpdate(); //Wait for QueenBee fields to be filled
        tab1ItemCount++;
        tabItemCounts[0] = tab1ItemCount;
        if (activeTab == tab1)
        {
            RefreshMenuLists();
            OpenTab(0, open1, false);
        }
    }

    #region Camera Control
    private void CheckZoom()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 scrollY = new Vector3(0, Input.mouseScrollDelta.y * Time.deltaTime * 75f, 0);
        if (cameraPos.y - scrollY.y > 22.5)
            Camera.main.transform.position = new Vector3(cameraPos.x, 22.5f, cameraPos.z);
        else if (cameraPos.y - scrollY.y < 10)
            Camera.main.transform.position = new Vector3(cameraPos.x, 10f, cameraPos.z);
        else
            Camera.main.transform.position -= scrollY;
    }

    private void PanCamera()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float sens = Input.GetKey(KeyCode.A) ? -10f : 10f;
            Vector3 panZ = new Vector3(0, 0, sens * Time.deltaTime);
            if (cameraPos.z + panZ.z > map.mapHeight * .75f * 2f)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, map.mapHeight * .75f * 2f);
            else if (cameraPos.z + panZ.z < 0)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0f);
            else
                Camera.main.transform.position += panZ;
        }

        cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            float sens = Input.GetKey(KeyCode.W) ? -10f : 10f;
            Vector3 panX = new Vector3(sens * Time.deltaTime, 0, 0);
            if (cameraPos.x + panX.x > map.mapWidth * 2f)
                Camera.main.transform.position = new Vector3(map.mapWidth * 2f, cameraPos.y, cameraPos.z);
            else if (cameraPos.x + panX.x < 0)
                Camera.main.transform.position = new Vector3(0f, cameraPos.y, cameraPos.z);
            else
                Camera.main.transform.position += panX;
        }
    }
    #endregion

    #region Save Load
    public void Save(ref PlayerSaveData data)
    {
        data.savedMoney = money;

        List<int> xs = new List<int>();
        List<int> ys = new List<int>();
        List<bool> hasSugar = new List<bool>();
        List<bool> hasReducer = new List<bool>();
        List<bool> hasStand = new List<bool>();
        List<bool> hasRepellant = new List<bool>();
        List<bool> hasInsulation = new List<bool>();
        List<float> honey = new List<float>();
        List<float> honeyPurity = new List<float>();
        List<FlowerType> honeyType = new List<FlowerType>();
        List<float> comb = new List<float>();
        List<int> combSizeCap = new List<int>();
        List<float> nectar = new List<float>();
        List<float> population = new List<float>();
        List<int> size = new List<int>();
        List<string> condition = new List<string>();


        //Queen
        List<bool> nullQueen = new List<bool>();
        List<bool> finishedGenerating = new List<bool>();
        List<float> constructionMult = new List<float>();
        List<float> productionMult = new List<float>();
        List<float> collectionMult = new List<float>();
        List<float> resilienceMult = new List<float>();
        List<float> aggressivnessMult = new List<float>();
        List<string> species = new List<string>();
        List<int> age = new List<int>();
        List<float> grade = new List<float>();
        List<string> quirks = new List<string>();
        List<int> quirksCount = new List<int>();
        List<bool> queenPurchased = new List<bool>();
        List<int> queenPrice = new List<int>();

        //honey inventory
        List<float> honeyInventory = new List<float>();

        foreach (Hive h in hives)
        {
            xs.Add(h.x);
            ys.Add(h.y);
            hasSugar.Add(h.hasSugar);
            hasReducer.Add(h.hasReducer);
            hasStand.Add(h.hasStand);
            hasRepellant.Add(h.hasRepellant);
            hasInsulation.Add(h.hasInsulation);
            honey.Add(h.honey);
            honeyPurity.Add(h.honeyPurity);
            honeyType.Add(h.honeyType);
            comb.Add(h.comb);
            combSizeCap.Add(h.combSizeCap);
            nectar.Add(h.nectar);
            population.Add(h.population);
            size.Add(h.Size);
            condition.Add(h.Condition);

            //Queen
            nullQueen.Add(h.queen.nullQueen);
            finishedGenerating.Add(h.queen.finishedGenerating);
            constructionMult.Add(h.queen.constructionMult);
            productionMult.Add(h.queen.productionMult);
            collectionMult.Add(h.queen.collectionMult);
            resilienceMult.Add(h.queen.resilienceMult);
            aggressivnessMult.Add(h.queen.aggressivnessMult);
            species.Add(h.queen.species);
            age.Add(h.queen.age);
            grade.Add(h.queen.grade);
            foreach (string s in h.queen.quirks)
                quirks.Add(s);
            quirksCount.Add(h.queen.quirks.Count);
            queenPurchased.Add(true);
            queenPrice.Add(0);

            var values = System.Enum.GetValues(typeof(FlowerType));
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                for (int i = 0; i < 4; i++)
                    honeyInventory.Add(inventory[fType][i]);
            }
        }

        //Shop queens
        foreach (GameObject go in beeObjectList)
        {
            QueenBee queen = go.GetComponent<QueenBee>();
            nullQueen.Add(false);
            finishedGenerating.Add(true);
            constructionMult.Add(queen.constructionMult);
            productionMult.Add(queen.productionMult);
            collectionMult.Add(queen.collectionMult);
            resilienceMult.Add(queen.resilienceMult);
            aggressivnessMult.Add(queen.aggressivnessMult);
            species.Add(queen.species);
            age.Add(queen.age);
            grade.Add(queen.grade);
            foreach (string s in queen.quirks)
                quirks.Add(s);
            quirksCount.Add(queen.quirks.Count);
            queenPurchased.Add(queen.gameObject.GetComponent<Cost>().purchased);
            queenPrice.Add(queen.gameObject.GetComponent<Cost>().Price);
        }

        data.hiveCount = hives.Count;
        data.xs = xs;
        data.ys = ys;
        data.hasSugar = hasSugar;
        data.hasReducer = hasReducer;
        data.hasStand = hasStand;
        data.hasRepellant = hasRepellant;
        data.hasInsulation = hasInsulation;
        data.honey = honey;
        data.honeyPurity = honeyPurity;
        data.honeyType = honeyType;
        data.comb = comb;
        data.combSizeCap = combSizeCap;
        data.nectar = nectar;
        data.population = population;
        data.size = size;
        data.condition = condition;

        //Queen
        data.nullQueen = nullQueen;
        data.finishedGenerating = finishedGenerating;
        data.constructionMult = constructionMult;
        data.productionMult = productionMult;
        data.collectionMult = collectionMult;
        data.resilienceMult = resilienceMult;
        data.aggressivnessMult = aggressivnessMult;
        data.species = species;
        data.age = age;
        data.grade = grade;
        data.quirks = quirks;
        data.quirksCount = quirksCount;
        data.queenPurchased = queenPurchased;
        data.queenPrice = queenPrice;

        data.queenCount = beeObjectList.Count;

        data.honeyInventory = honeyInventory;
    }

    public void Load(PlayerSaveData data)
    {
        money = data.savedMoney;

        for (int i = 0; i < data.hiveCount + data.queenCount; i++)
        {
            Hive hive = null;
            if (i < data.hiveCount - 1)
            {
                Vector3 pos = new Vector3(data.xs[i], 0, data.ys[i]);
                GameObject hiveObject = Instantiate(hivePrefab, pos, Quaternion.identity);
                hive = hiveObject.GetComponent<Hive>();
                hive.placed = true;
                hive.x = data.xs[i];
                hive.y = data.ys[i];
                hive.hiveTile = map.tiles[data.xs[i] / 2, data.ys[i] / 2];
                hive.hasSugar = data.hasSugar[i];
                hive.hasReducer = data.hasReducer[i];
                hive.hasStand = data.hasStand[i];
                hive.hasRepellant = data.hasRepellant[i];
                hive.hasInsulation = data.hasInsulation[i];
                hive.honey = data.honey[i];
                hive.honeyPurity = data.honeyPurity[i];
                hive.honeyType = data.honeyType[i];
                hive.comb = data.comb[i];
                hive.combSizeCap = data.combSizeCap[i];
                hive.nectar = data.nectar[i];
                hive.population = data.population[i];
                hive.Size = data.size[i];
                hive.Condition = data.condition[i];
            }

            //hive queens
            QueenBee queen = (hive != null) ? hive.gameObject.GetComponent<QueenBee>() : Instantiate(testQueen, new Vector3(-100, -100, -100), Quaternion.identity).GetComponent<QueenBee>();
            queen.fromSave = true;
            SetQueens(queen, data, i);
            if (hive == null)
            {
                queen.gameObject.GetComponent<Cost>().purchased = data.queenPurchased[i];
                queen.gameObject.GetComponent<Cost>().Price = data.queenPrice[i];
                StartCoroutine(AddQueen(queen));
            }

            //honey inventory
            int count = 0;
            var values = System.Enum.GetValues(typeof(FlowerType));
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                List<float> inv = new List<float>(){data.honeyInventory[count], data.honeyInventory[count + 1], data.honeyInventory[count + 2], data.honeyInventory[count + 3]};
                inventory[fType] = inv;
                count += 4;
            }

        }
    }

    private void SetQueens(QueenBee queen, PlayerSaveData data, int i)
    {
        if (data.nullQueen[i] == false)
        {
            queen.nullQueen = false;
            queen.finishedGenerating = true;
            queen.constructionMult = data.constructionMult[i];
            queen.productionMult = data.productionMult[i];
            queen.collectionMult = data.collectionMult[i];
            queen.resilienceMult = data.resilienceMult[i];
            queen.aggressivnessMult = data.aggressivnessMult[i];
            queen.species = data.species[i];
            queen.age = data.age[i];
            queen.grade = data.grade[i];

            int count = 0;
            for (int j = 0; count < data.quirksCount[i]; j = 0)
            {
                Debug.Log(j + " " + data.quirks.Count);
                if (data.quirks.Count == 0)
                    break;
                queen.quirks.Add(data.quirks[j]);
                data.quirks.RemoveAt(j);
                count++;
            }
        }
        else
        {
            queen.nullQueen = true;
            queen.finishedGenerating = true;
        }
    }
    #endregion
}

[System.Serializable]
public struct PlayerSaveData
{
    public int savedMoney;

    public int hiveCount;
    public List<int> xs;
    public List<int> ys;
    public List<bool> hasSugar;
    public List<bool> hasReducer;
    public List<bool> hasStand;
    public List<bool> hasRepellant;
    public List<bool> hasInsulation;
    public List<float> honey;
    public List<float> honeyPurity;
    public List<FlowerType> honeyType;
    public List<float> comb;
    public List<int> combSizeCap;
    public List<float> nectar;
    public List<float> population;
    public List<int> size;
    public List<string> condition;

    //queen
    public List<bool> nullQueen;
    public List<bool> finishedGenerating;
    public List<float> constructionMult;
    public List<float> productionMult;
    public List<float> collectionMult;
    public List<float> resilienceMult;
    public List<float> aggressivnessMult;
    public List<string> species;
    public List<int> age;
    public List<float> grade;
    public List<string> quirks;
    public List<int> quirksCount;

    //Shop queens
    public int queenCount;
    public List<bool> queenPurchased;
    public List<int> queenPrice;

    //honey inventory
    public List<float> honeyInventory;
}
