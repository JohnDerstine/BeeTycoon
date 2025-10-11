using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HexMenu : MonoBehaviour
{
    [SerializeField]
    private UIDocument ui;

    [SerializeField]
    private GameController game;

    private UnlockTracker unlocks;

    private PlayerController player;

    [SerializeField]
    private Glossary glossary;

    [SerializeField]
    private VisualTreeAsset queenUI;

    private List<List<Texture2D>> spriteList = new List<List<Texture2D>>();

    private List<List<GameObject>> objectList = new List<List<GameObject>>();

    [SerializeField]
    public List<Texture2D> allFlowerSprites = new List<Texture2D>();

    [SerializeField]
    private List<GameObject> allFlowerObjects = new List<GameObject>();

    private List<Texture2D> flowerSprites = new List<Texture2D>(); //Random flower stages added to this list

    private List<GameObject> flowerObjectList = new List<GameObject>(); //Random flower stages added to this list

    [SerializeField]
    public List<Texture2D> beeSprites = new List<Texture2D>();

    [SerializeField]
    public List<GameObject> beeObjectList = new List<GameObject>();

    [SerializeField]
    private List<Texture2D> toolSprites = new List<Texture2D>();

    [SerializeField]
    private List<GameObject> toolObjectList = new List<GameObject>();

    [SerializeField]
    private List<Texture2D> hiveSprites = new List<Texture2D>();

    [SerializeField]
    private List<GameObject> hiveObjectList = new List<GameObject>();

    [SerializeField]
    private GameObject testQueen;

    [SerializeField]
    private Texture2D testQueenSprite;

    [SerializeField]
    private GameObject nextStage;

    [SerializeField]
    private Texture2D nextStageSprite;

    private TemplateContainer hoverTemplate;

    private Manipulator close;
    public Manipulator open1;
    public Manipulator open2;
    public Manipulator open3;
    public Manipulator open4;
    private EventCallback<PointerUpEvent> endSelectionCallback;
    private EventCallback<PointerMoveEvent, int> queenMoveCallback;
    private EventCallback<PointerLeaveEvent> queenExitCallback;

    private VisualElement left;
    private CustomVisualElement tab1;
    private CustomVisualElement tab2;
    private CustomVisualElement tab3;
    private CustomVisualElement tab4;

    public int tab1ItemCount = 0;
    private int tab2ItemCount = 6;
    private int tab3ItemCount = 9;
    private int tab4ItemCount = 1;

    [SerializeField]
    private Texture2D hex;

    [SerializeField]
    private StyleSheet tabStyle;

    [SerializeField]
    private StyleSheet itemStyle;

    [SerializeField]
    private StyleSheet costStyle;

    int tabItemsPerRow = 8;

    private List<CustomVisualElement> tabHexes = new List<CustomVisualElement>();
    private List<CustomVisualElement> tabs = new List<CustomVisualElement>();
    private List<int> tabItemCounts = new List<int>();
    private CustomVisualElement activeTab;
    public Dictionary<FlowerType, int> flowersOwned = new Dictionary<FlowerType, int>();

    private Hive selectedHive;

    private VisualElement selectedHex;

    private bool fromSave;

    public List<FlowerType> availableFTypes = new List<FlowerType>();

    public VisualElement SelectedHex
    {
        get { return selectedHex; }
        set
        {
            selectedHex = value;
            if (value != null)
                selectedHex.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void GameLoaded()
    {
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();

        toolObjectList.Add(GameObject.Find("HiveTool"));
        toolObjectList.Add(GameObject.Find("Smoker"));
        toolObjectList.Add(GameObject.Find("ShovelTool"));
        toolObjectList.Add(GameObject.Find("DollyTool"));
        toolObjectList.Add(GameObject.Find("Suit"));
        toolObjectList.Add(GameObject.Find("Extractor"));

        flowerObjectList.Add(nextStage);
        flowerSprites.Add(nextStageSprite);

        close = new Clickable(close => CloseTab());
        open1 = new Clickable(open => OpenTab(0, open1, false));
        open2 = new Clickable(open => OpenTab(1, open2, false));
        open3 = new Clickable(open => OpenTab(2, open3, false));
        open4 = new Clickable(open => OpenTab(3, open4, false));

        ReloadUI();

        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent, int>(OnQueenMove);

        if (!fromSave)
        {
            var values = System.Enum.GetValues(typeof(FlowerType));
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                flowersOwned.Add(fType, 0);
            }
        }

        List<int> flowers = unlocks.GetNextFlowers();
        foreach (int i in flowers)
        {
            availableFTypes.Add((FlowerType)(i + 2));
            flowerObjectList.Insert(0, allFlowerObjects[i]);
            flowerObjectList[0].GetComponent<Cost>().ftype = (FlowerType)(i + 2);
            flowerSprites.Insert(0, allFlowerSprites[i]);
            tab4ItemCount++;
        }

        RefreshMenuLists();
    }

    public void OpenTab(int num, Manipulator open, bool fromHive, Hive hive = null)
    {
        if (game.CurrentState == GameStates.TurnEnd || game.CurrentState == GameStates.Paused)
            return;

        ui.GetComponent<AudioSource>().Play();

        //Close any open tabs
        if (activeTab != null)
            CloseTab();

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
        Cost cost = objectList[num][index].GetComponent<Cost>();
        int price = cost.Price;
        if (num == 2 && objectList[num][index].GetComponent<Cost>().Purchased)
        {
            costLabel.text = "Owned";
            return 0;
        }
        else if (cost.ftype != FlowerType.Empty && flowersOwned[(FlowerType)(index + 2)] > 0)
        {
            costLabel.text = flowersOwned[(FlowerType)(index + 2)] + " free";
            return 0;
        }
        costLabel.text = (price == 0) ? "Owned" : "$" + price;
        return price;
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
    public void RefreshMenuLists()
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
        ui.GetComponent<AudioSource>().Play();
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
        if (player.Money < cost)
            return;

        ui.GetComponent<AudioSource>().Play();
        if (selectedHex != null)
        {
            selectedHex.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);
            selectedHex = null;

            if (player.SelectedItem == item)
            {
                player.SelectedItem = null;
                return;
            }
        }

        if (item.tag == "Stage")
        {
            if (player.Money < cost)
                return;

            player.Money = -cost;
            List<int> flowers = unlocks.GetNextFlowers();
            foreach (int i in flowers)
            {
                flowerObjectList.Insert(0, allFlowerObjects[i]);
                flowerSprites.Insert(0, allFlowerSprites[i]);
                tab4ItemCount++;
            }
            return;
        }
        
        if (item.tag != "Placeable")
        {
            UnityEngine.Cursor.SetCursor(sprite, new Vector2(sprite.width / 2, sprite.height / 2), CursorMode.Auto);
            player.SelectedItem = item;
            player.selectedItemSprite = sprite;
        }

        if (item.tag == "Placeable")
        {
            player.SelectedItem = item;
            player.selectedItemSprite = sprite;
        }
        else if (item.GetComponent<Cost>().OneTime)
        {
            item.GetComponent<Cost>().Purchased = true;
            Label costLabel = hex.Q<Label>();
            if (costLabel.text != "Owned")
                player.Money = -cost;
            costLabel.text = "Owned";
            item.GetComponent<Cost>().Price = 0;
        }

        hex.style.unityBackgroundImageTintColor = new Color(0.65f, 0.65f, 0.65f, 1f);
        selectedHex = hex;
    }

    public void UnhighlightHex()
    {
        if (selectedHex != null)
            selectedHex.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);
    }

    //Check to see if a Queen was selected from the shop, after clicking on the HiveUI queen button
    private void SelectHive(GameObject item, Texture2D sprite, int cost, Hive hive)
    {
        if (player.Money < cost)
            return;

        ui.GetComponent<AudioSource>().Play();
        player.Money = -cost;
        hive.Populate(item.GetComponent<QueenBee>());
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

    private void ReferToGlossary(PointerDownEvent e, string keyword)
    {
        if (e.button == 2)
            glossary.OpenGlossary(keyword);
    }

    public void ReloadUI()
    {
        tabs.Clear();

        left = ui.rootVisualElement.Q<VisualElement>("Left");
        endSelectionCallback = new EventCallback<PointerUpEvent>(EndQueenSelection);

        tab1 = ui.rootVisualElement.Q<CustomVisualElement>("tab1");
        tab1.AddManipulator(open1);
        tabs.Add(tab1);
        tab2 = ui.rootVisualElement.Q<CustomVisualElement>("tab2");
        tab2.AddManipulator(open2);
        tabs.Add(tab2);
        tab3 = ui.rootVisualElement.Q<CustomVisualElement>("tab3");
        tab3.AddManipulator(open3);
        tabs.Add(tab3);
        tab4 = ui.rootVisualElement.Q<CustomVisualElement>("tab4");
        tab4.AddManipulator(open4);
        tabs.Add(tab4);

        tab1.RegisterCallback<PointerDownEvent>(e => ReferToGlossary(e, "BeeStats"));
        tab2.RegisterCallback<PointerDownEvent>(e => ReferToGlossary(e, "Tools"));
        tab3.RegisterCallback<PointerDownEvent>(e => ReferToGlossary(e, "Hive"));
        tab4.RegisterCallback<PointerDownEvent>(e => ReferToGlossary(e, "Flowers"));

        RefreshMenuLists();
    }
    public void Save(ref HexMenuSaveData data)
    {
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

        //tools
        List<bool> toolsPurchased = new List<bool>();
        List<int> toolPrice = new List<int>();

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

        //Tools
        foreach (GameObject go in toolObjectList)
        {
            Cost cost = go.GetComponent<Cost>();
            toolsPurchased.Add(cost.purchased);
            toolPrice.Add(cost.Price);
        }

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

        data.toolsPurchased = toolsPurchased;
        data.toolPrice = toolPrice;
    }

    public void Load(HexMenuSaveData data)
    {
        fromSave = true;
        for (int i = 0; i < data.queenCount; i++)
        {
            QueenBee queen = Instantiate(testQueen, new Vector3(-100, -100, -100), Quaternion.identity).GetComponent<QueenBee>();
            queen.fromSave = true;
            SetQueens(queen, data, i);
            queen.gameObject.GetComponent<Cost>().purchased = data.queenPurchased[i];
            queen.gameObject.GetComponent<Cost>().Price = data.queenPrice[i];
            StartCoroutine(AddQueen(queen));
        }

        int l = 0;
        foreach (GameObject go in toolObjectList)
        {
            Cost cost = go.GetComponent<Cost>();
            cost.purchased = data.toolsPurchased[l];
            cost.Price = data.toolPrice[l];
            l++;
        }
    }

    private void SetQueens(QueenBee queen, HexMenuSaveData data, int i)
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
            if (data.quirks.Count == 0)
                break;
            queen.quirks.Add(data.quirks[j]);
            data.quirks.RemoveAt(j);
            count++;
        }
    }
}

[System.Serializable]
public struct HexMenuSaveData
{
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

    public List<bool> toolsPurchased;
    public List<int> toolPrice;
}
