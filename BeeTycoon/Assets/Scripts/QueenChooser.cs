using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.GridBrushBase;

public class QueenChooser : MonoBehaviour
{
    [SerializeField]
    private RunModifiers mods;

    [SerializeField]
    private GameController game;

    [SerializeField]
    private Texture2D testQueenSprite;

    [SerializeField]
    private VisualTreeAsset queenUI;

    [SerializeField]
    private VisualTreeAsset flowerUI;

    [SerializeField]
    private VisualTreeAsset honeyUI;

    [SerializeField]
    private VisualTreeAsset sizeUI;

    [SerializeField]
    private VisualTreeAsset toolUI;

    [SerializeField]
    private VisualTreeAsset modifierUI;

    [SerializeField]
    private Texture2D queenHex;

    [SerializeField]
    private Texture2D flowerHex;

    [SerializeField]
    private Texture2D honeyHex;

    [SerializeField]
    private Texture2D sizeHex;

    [SerializeField]
    private Texture2D toolHex;

    [SerializeField]
    private VisualTreeAsset modifierHex;

    [SerializeField]
    private VisualTreeAsset bundleHex;

    [SerializeField]
    private VisualTreeAsset choicesContainer;

    [SerializeField]
    private VisualTreeAsset shopContainer;

    [SerializeField]
    private UIDocument document;

    [SerializeField]
    private Texture2D honeySprite;

    private Shed modifierList;

    private PlayerController player;
    private ToolManager toolManager;
    private HexMenu hexMenu;

    [SerializeField]
    private GameObject queenPrefab;

    private UnlockTracker tracker;

    [SerializeField]
    private Texture2D queenSprite;

    [SerializeField]
    private StyleSheet descriptionStyle;

    private TemplateContainer template;

    private TemplateContainer shop;

    public bool isChoosing;

    private bool selectionActive;

    private Label activeLabel;

    private VisualElement root;
    private VisualElement container;
    EventCallback<PointerMoveEvent> queenMoveCallback;
    EventCallback<PointerLeaveEvent> queenExitCallback;

    EventCallback<PointerEnterEvent, string> quirkEnterCallback;
    EventCallback<PointerLeaveEvent> quirkExitCallback;
    Color dark = new Color(0.65f, 0.65f, 0.65f);
    Color light = new Color(0.9f, 0.9f, 0.9f);

    private List<QueenBee> queenOptions = new List<QueenBee>();
    private List<VisualTreeAsset> rngOptions;
    private List<string> sizeDirections = new List<string>() { "right", "left", "down"};
    private List<Texture2D> shopOptions = new List<Texture2D>();

    private List<int> usedIds = new List<int>();

    TemplateContainer hoverTemp;

    private bool skippable = true;

    private EventCallback<WheelEvent, QueenBee> cycleDetails;
    private VisualElement activeHover;
    private VisualElement hoveredQueen;
    private int detailsIndex = 0;
    private List<string> details = new List<string>();

    public void OnSceneLoaded()
    {
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        tracker = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        hexMenu = GameObject.Find("HexMenu").GetComponent<HexMenu>();

        root = document.rootVisualElement;
        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent>(OnQueenMove);
        quirkExitCallback = new EventCallback<PointerLeaveEvent>(OnQuirkExit);
        quirkEnterCallback = new EventCallback<PointerEnterEvent, string>(OnQuirkEnter);
        cycleDetails = new EventCallback<WheelEvent, QueenBee>(OnDetailTab);
        ResetRNGOptions();
    }

    private void ResetShopOptions()
    {
        shopOptions.Clear();
        shopOptions.Add(queenHex);
        shopOptions.Add(honeyHex);
        shopOptions.Add(flowerHex);
        shopOptions.Add(toolHex);
    }

    public void LoadShop()
    {
        ResetShopOptions();

        shop = shopContainer.Instantiate();
        container = shop.Q<VisualElement>("Container");
        VisualElement baseElem = document.rootVisualElement.Q<VisualElement>("Base");
        shop.style.position = Position.Absolute;
        shop.style.top = 0;
        shop.style.left = 0;
        shop.style.flexGrow = 0;
        shop.style.width = 1920;
        shop.style.height = 1080;
        baseElem.Add(shop);
        VisualElement backgroundTint = shop.Q<VisualElement>("BackgroundTint");
        backgroundTint.Q<Label>("RJCount").text = player.RoyalJelly.ToString();

        //Only give mods that make sense for the player
        //i.e. If they haven't unlocked the flowers they pertain to
        List<Modifier> applicableMods = new List<Modifier>();
        foreach (FlowerModifier mod in mods.GetArchetypeAll<FlowerModifier>())
        {
            foreach (FlowerType f in tracker.ownedFlowers)
            {
                if (mod.Flowers.Contains(f) && !applicableMods.Contains(mod))
                    applicableMods.Add(mod);
            }
        }
        foreach (HoneyModifier mod in mods.GetArchetypeAll<HoneyModifier>())
        {
            foreach (FlowerType f in tracker.ownedFlowers)
            {
                if (mod.Flower == f && !applicableMods.Contains(mod))
                    applicableMods.Add(mod);
            }
        }
        foreach (OrderModifier mod in mods.GetArchetypeAll<OrderModifier>())
        {
            applicableMods.Add(mod);
        }

        for (int i = 0; i < 3; i++)
        {
            Modifier randMod = applicableMods[Random.Range(0, applicableMods.Count)];
            applicableMods.Remove(randMod);

            modifierHex.CloneTree(backgroundTint);
            VisualElement item = backgroundTint.Q<VisualElement>("ItemContainer");
            item.Q<VisualElement>("Icon").style.backgroundImage = randMod.Sprite;
            Label cost = item.Q<Label>("Cost");
            cost.text = (randMod.Rarity * 2).ToString();
            item.name = "ShopHex";
            item.AddManipulator(new Clickable(e => SelectModifierShop(randMod.ID, item)));
            item.RegisterCallback<PointerMoveEvent>(e => OnModMove(e, randMod.ID));
        }

        for (int i = 0; i < 3; i++)
        {
            Texture2D randHex = shopOptions[Random.Range(0, shopOptions.Count)];

            modifierHex.CloneTree(backgroundTint);
            VisualElement item = backgroundTint.Q<VisualElement>("ItemContainer");
            item.Q<VisualElement>("Item").style.backgroundImage = randHex;
            Label cost = item.Q<Label>("Cost");

            if (randHex == queenHex)
            {
                item.Q<VisualElement>("Icon").style.backgroundImage = queenSprite;
                cost.text = "1";

                GameObject q = Instantiate(queenPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
                QueenBee queen = q.GetComponent<QueenBee>();
                List<string> possibilites = new List<string>(); //Get a list of the species of bees the player has unlocked
                foreach (KeyValuePair<string, bool> kvp in tracker.species)
                {
                    if (kvp.Value == true)
                        possibilites.Add(kvp.Key);
                }
                queen.species = possibilites[Random.Range(0, possibilites.Count)];
                item.AddManipulator(new Clickable(e => SelectQueenShop(queen, item)));
                item.RegisterCallback<PointerMoveEvent>(e => OnQueenMove(e, queen));
            }

            if (randHex == honeyHex)
            {
                FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                item.Q<VisualElement>("Icon").style.backgroundImage = honeySprite;
                cost.text = "1";
                item.AddManipulator(new Clickable(e => SelectHoneyShop(rand, item)));
                item.RegisterCallback<PointerMoveEvent>(e => OnHoneyMove(e, rand));
            }

            if (randHex == flowerHex)
            {
                FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)rand - 2];
                cost.text = "1";
                item.AddManipulator(new Clickable(e => SelectFlowerShop(rand, item)));
                item.RegisterCallback<PointerMoveEvent>(e => OnFlowerMove(e, rand));
            }

            if (randHex == toolHex)
            {
                cost.text = "2";
                Tool rand = toolManager.GetUnmaxedTools()[Random.Range(0, toolManager.GetUnmaxedTools().Count)];
                item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.toolSprites[(int)rand];
                item.AddManipulator(new Clickable(e => SelectToolShop(rand, item)));
                item.RegisterCallback<PointerMoveEvent>(e => OnToolMove(e, rand));
            }

            item.name = "ShopHex";
            shopOptions.Remove(randHex);
        }

        backgroundTint.Q<Button>("Done").clickable = new Clickable(CloseShop);
        backgroundTint.Q<Button>("Skip").clickable = new Clickable(Skip);
    }

    private void Skip()
    {
        if (!skippable)
            return;

        player.RoyalJelly += 3;
        CloseShop();
    }

    private void CloseShop()
    {
        document.rootVisualElement.Q<VisualElement>("Base").Remove(shop);
        shop = null;
        isChoosing = false;
    }

    public IEnumerator CreateBundles()
    {
        template = choicesContainer.Instantiate();
        container = template.Q<VisualElement>("Container");
        template.style.position = Position.Absolute;
        template.style.flexDirection = FlexDirection.Row;
        template.style.justifyContent = Justify.FlexStart;
        container.style.justifyContent = Justify.SpaceAround;
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);

        document.rootVisualElement.Q<VisualElement>("Left").visible = false;
        document.rootVisualElement.Q<VisualElement>("Right").visible = false;
        document.rootVisualElement.Q<VisualElement>("Center").visible = false;
        document.rootVisualElement.Q<VisualElement>("Toolbar").visible = false;

        VisualElement banner1 = container.Q<VisualElement>("Banner1");
        VisualElement banner2 = container.Q<VisualElement>("Banner2");
        VisualElement banner3 = container.Q<VisualElement>("Banner3");

        List<string> possibilites = new List<string>(); //Get a list of the species of bees the player has unlocked
        foreach (KeyValuePair<string, bool> kvp in tracker.species)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }

        List<VisualElement> banners = new List<VisualElement>() { banner1, banner2, banner3 };
        foreach (VisualElement banner in banners)
        {
            TemplateContainer queenElem = bundleHex.Instantiate();
            banner.Add(queenElem);
            GameObject q = Instantiate(queenPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            QueenBee queen = q.GetComponent<QueenBee>();
            queenOptions.Add(queen);
            Debug.Log(possibilites.Count);
            queen.species = possibilites[Random.Range(0, possibilites.Count)];
            Debug.Log(queen.species);
            possibilites.Remove(queen.species);
            int savedI = queenOptions.Count - 1;

            banner.RegisterCallback((PointerEnterEvent e) =>{
                banner.style.unityBackgroundImageTintColor = Color.white;
                foreach (VisualElement child in banner.Children())
                {
                    child.Q<CustomVisualElement>("Item").style.unityBackgroundImageTintColor = Color.white;
                    child.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = Color.white;
                }
            });

            banner.RegisterCallback((PointerLeaveEvent e) => {
                banner.style.unityBackgroundImageTintColor = Color.gray;
                foreach (VisualElement child in banner.Children())
                {
                    child.Q<CustomVisualElement>("Item").style.unityBackgroundImageTintColor = Color.gray;
                    child.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = Color.gray;
                }
            });

            banner.RegisterCallback<ClickEvent>(e => SelectQueen(savedI));
            queenElem.RegisterCallback<PointerMoveEvent>(e => OnQueenMove(e, queen));

            if (true)//tracker.majorTechs["FlowerSelect"])
            {
                FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];

                TemplateContainer elem = bundleHex.Instantiate();
                elem.Q<VisualElement>("Item").style.backgroundImage = sizeHex;
                elem.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)rand - 2];
                banner.RegisterCallback<ClickEvent>(e => SelectFlower(rand));
                elem.RegisterCallback<PointerMoveEvent>(e => OnFlowerMove(e, rand));
                banner.Add(elem);
            }

            if (true)//tracker.majorTechs["ToolSelect"])
            {
                Tool rand = toolManager.GetUnmaxedTools()[Random.Range(0, toolManager.GetUnmaxedTools().Count)];

                TemplateContainer elem = bundleHex.Instantiate();
                elem.Q<VisualElement>("Item").style.backgroundImage = toolHex;
                elem.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.toolSprites[(int)rand];
                banner.RegisterCallback<ClickEvent>(e => SelectTool(rand));
                elem.RegisterCallback<PointerMoveEvent>(e => OnToolMove(e, rand));
                banner.Add(elem);
            }
        }
        selectionActive = true;
        yield return new WaitWhile(() => selectionActive);

        document.rootVisualElement.Q<VisualElement>("Left").visible = true;
        document.rootVisualElement.Q<VisualElement>("Right").visible = true;
        document.rootVisualElement.Q<VisualElement>("Center").visible = true;
        document.rootVisualElement.Q<VisualElement>("Toolbar").visible = true;

        if (game.CurrentState == GameStates.Start)
            game.CurrentState = GameStates.Running;

        document.rootVisualElement.Q<VisualElement>("Base").Remove(template);
    }

    private void ResetRNGOptions()
    {
        rngOptions = new List<VisualTreeAsset>() { };
        rngOptions.Add(queenUI);

        if (tracker.majorTechs["HoneySelect"])
            rngOptions.Add(honeyUI);
        if (tracker.majorTechs["FlowerSelect"])
            rngOptions.Add(flowerUI);
        if (tracker.majorTechs["SizeSelect"])
            rngOptions.Add(sizeUI);

        if (toolManager.GetUnmaxedTools().Count > 0)
            rngOptions.Add(toolUI);
    }
    #region Deprecated Selects

    private void SelectFlower(FlowerType f)
    {
        hexMenu.flowersOwned[f] += 5;
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectSize(string dir)
    {
        GameObject.Find("MapLoader").GetComponent<MapLoader>().IncreaseMapSize(dir);
        sizeDirections.Remove(dir);
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectTool(Tool tool)
    {
        ToolScript toolScript = toolManager.GetToolFromTag(tool.ToString());
        if (toolScript.Level == 0)
            toolScript.gameObject.GetComponent<Cost>().Purchased = true;
        toolManager.GetToolFromTag(tool.ToString()).Upgrade();

        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectQueen(int num)
    {
        document.GetComponent<AudioSource>().Play();
        selectionActive = false;
        for (int i = 0; i < queenOptions.Count; i++)
        {
            if (i != num)
                Destroy(queenOptions[i].gameObject);
            else
                StartCoroutine(hexMenu.AddQueen(queenOptions[i]));
        }
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }
    #endregion

    #region Shop Selects
    private void SelectQueenShop(QueenBee queen, VisualElement container)
    {
        int cost = int.Parse(container.Q<Label>("Cost").text);
        if (player.RoyalJelly < cost && container.name != "Owned")
            return;

        player.RoyalJelly -= cost;
        document.GetComponent<AudioSource>().Play();
        StartCoroutine(hexMenu.AddQueen(queen));
        CleanShop(container);
    }

    private void SelectModifierShop(int id, VisualElement container)
    {
        int cost = int.Parse(container.Q<Label>("Cost").text);
        if (player.RoyalJelly < cost && container.name != "Owned")
            return;

        player.RoyalJelly -= cost;
        document.GetComponent<AudioSource>().Play();
        mods.AddMod(id);
        if (modifierList == null)
            modifierList = GameObject.Find("Shed(Clone)").GetComponent<Shed>();
        modifierList.AddModifier(id);
        CleanShop(container);
    }

    private void SelectToolShop(Tool tool, VisualElement container)
    {
        int cost = int.Parse(container.Q<Label>("Cost").text);
        if (player.RoyalJelly < cost && container.name != "Owned")
            return;

        player.RoyalJelly -= cost;
        document.GetComponent<AudioSource>().Play();
        ToolScript toolScript = toolManager.GetToolFromTag(tool.ToString());
        if (toolScript.Level == 0)
            toolScript.gameObject.GetComponent<Cost>().Purchased = true;
        toolManager.GetToolFromTag(tool.ToString()).Upgrade();
        CleanShop(container);
    }

    private void SelectHoneyShop(FlowerType f, VisualElement container)
    {
        int cost = int.Parse(container.Q<Label>("Cost").text);
        if (player.RoyalJelly < cost && container.name != "Owned")
            return;

        player.RoyalJelly -= cost;
        document.GetComponent<AudioSource>().Play();
        player.inventory[f][0] += 5; //add to total honey
        player.inventory[f][2] += 5; //add to medium quality honey
        CleanShop(container);
    }

    private void SelectFlowerShop(FlowerType f, VisualElement container)
    {
        int cost = int.Parse(container.Q<Label>("Cost").text);
        if (player.RoyalJelly < cost && container.name != "Owned")
            return;

        player.RoyalJelly -= cost;
        document.GetComponent<AudioSource>().Play();
        hexMenu.flowersOwned[f] += 5;
        CleanShop(container);
    }

    private void CleanShop(VisualElement container)
    {
        container.Q<Label>("Cost").visible = false;
        container.Q<VisualElement>("Currency").visible = false;
        container.Q<VisualElement>("Item").style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
        container.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
        container.name = "Owned";

        if (skippable)
        {
            skippable = false;
            shop.Q<Button>("Skip").style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            shop.Q<Label>("SkipLabel").visible = false;
            shop.Q<VisualElement>("SkipIcon").visible = false;
        }

        shop.Q<Label>("RJCount").text = player.RoyalJelly.ToString();
    }
    #endregion

    private void OnQueenMove(PointerMoveEvent e)
    {
        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        if (target.ContainsPoint(e.localPosition))
            target.style.unityBackgroundImageTintColor = light;
        else
            target.style.unityBackgroundImageTintColor = dark;
    }

    private void OnQueenExit(PointerLeaveEvent e)
    {
        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        target.style.unityBackgroundImageTintColor = dark;
    }

    private void OnQuirkEnter(PointerEnterEvent e, string quirk)
    {
        if (activeLabel != null)
        {
            document.rootVisualElement.Remove(activeLabel);
            activeLabel = null;
        }

        activeLabel = new Label();
        activeLabel.styleSheets.Add(descriptionStyle);
        activeLabel.text = tracker.quirkDescriptions[quirk];
        document.rootVisualElement.Add(activeLabel);
        activeLabel.style.left = e.position.x;
        activeLabel.style.top = e.position.y;
        activeLabel.pickingMode = PickingMode.Ignore;
    }

    private void OnDetailTab(WheelEvent e, QueenBee queen)
    {
        if (hoverTemp != null)
        {
            detailsIndex++;
            if (detailsIndex > details.Count - 1)
                detailsIndex = 0;
            Label tip = hoverTemp.Q<Label>("Tip");
            tip.text = details[detailsIndex];
        }
    }

    private void SetDetailsList(QueenBee queen)
    {
        detailsIndex = 0;
        details.Clear();
        details.Add(tracker.speciesDetails[queen.species]);
        details.Add(tracker.flowerDetails[queen.favorite]);
        foreach (string s in queen.quirks)
            details.Add(tracker.quirkDescriptions[s]);
    }

    #region enters
    private void OnModMove(PointerMoveEvent e, int id)
    {
        VisualElement targetTemplate = e.currentTarget as VisualElement;
        CustomVisualElement target = targetTemplate.Q<CustomVisualElement>("Item");
        if (target.ContainsPoint(e.localPosition) && hoverTemp == null)
        {

            hoverTemp = modifierUI.Instantiate();
            VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

            popup.Q<VisualElement>("Icon").style.backgroundImage = mods.allMods[id].Sprite;
            popup.Q<Label>("Title").text = mods.allMods[id].Name;
            popup.Q<Label>("Description").text = mods.allMods[id].Description;

            hoverTemp.style.position = Position.Absolute;
            hoverTemp.pickingMode = PickingMode.Ignore;
            popup.pickingMode = PickingMode.Ignore;

            document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);

            VisualElement hex = e.target as VisualElement;
            hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
            {
                hoverTemp.style.top = (Screen.height / 2) - (hoverTemp.resolvedStyle.height / 2);
                if (hex.worldBound.x < Screen.width - (Screen.width / 2.5f))
                    hoverTemp.style.left = hex.worldBound.x + hex.resolvedStyle.width;
                else
                    hoverTemp.style.left = hex.worldBound.x - (hoverTemp.resolvedStyle.width);
            });
        }
        else if (!target.ContainsPoint(e.localPosition) && hoverTemp != null)
            Leave();
    }

    private void OnQueenMove(PointerMoveEvent e, QueenBee queen)
    {
        VisualElement targetTemplate = e.currentTarget as VisualElement;
        CustomVisualElement target = targetTemplate.Q<CustomVisualElement>("Item");
        if (target.ContainsPoint(e.localPosition) && hoverTemp == null)
        {
            hoveredQueen = target;
            hoverTemp = queenUI.Instantiate();
            VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

            //Display Info about queen
            hoverTemp.Q<VisualElement>("Icon").style.backgroundImage = queenSprite;
            hoverTemp.Q<Label>("Species").text = "Species: " + queen.species;
            hoverTemp.Q<Label>("Age").text = "Radius Type: " + queen.radiusType;
            hoverTemp.Q<Label>("Favorite").text = "Favorite Flower: " + queen.favorite.ToString();
            hoverTemp.Q<Label>("Tip").text = tracker.speciesDetails[queen.species];

            //Add quirk labels to the queen
            foreach (string s in queen.quirks)
            {
                Label quirk = new Label();
                quirk.text = s;
                quirk.AddToClassList("Quirk");
                hoverTemp.Q<VisualElement>("QuirkContainer").Add(quirk);
                quirk.RegisterCallback(quirkEnterCallback, quirk.text);
                quirk.RegisterCallback(quirkExitCallback);
            }

            hoverTemp.style.position = Position.Absolute;
            hoverTemp.pickingMode = PickingMode.Ignore;
            popup.pickingMode = PickingMode.Ignore;

            document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);
            activeHover = hoverTemp;

            VisualElement hex = e.target as VisualElement;
            hex.RegisterCallback(cycleDetails, queen);
            SetDetailsList(queen);
            hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
            {
                hoverTemp.style.top = (Screen.height / 2) - (hoverTemp.resolvedStyle.height / 2);
                if (hex.worldBound.x < Screen.width - (Screen.width / 2.5f))
                {
                    hoverTemp.style.left = hex.worldBound.x + (hex.resolvedStyle.width / 1.5f);
                    activeHover.Q<VisualElement>("Details").BringToFront();
                }
                else
                    hoverTemp.style.left = hex.worldBound.x - (hoverTemp.resolvedStyle.width);
            });
        }
        else if (!target.ContainsPoint(e.localPosition) && hoverTemp != null)
            Leave();
    }

    private void OnHoneyMove(PointerMoveEvent e, FlowerType f)
    {
        VisualElement targetTemplate = e.currentTarget as VisualElement;
        CustomVisualElement target = targetTemplate.Q<CustomVisualElement>("Item");
        if (target.ContainsPoint(e.localPosition) && hoverTemp == null)
        {
            hoverTemp = honeyUI.Instantiate();
            VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

            popup.Q<Label>("Type").text = f.ToString();
            popup.Q<Label>("Price").text = "$" + GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().GetPrice(f) + " / lb.";

            hoverTemp.style.position = Position.Absolute;
            hoverTemp.pickingMode = PickingMode.Ignore;
            popup.pickingMode = PickingMode.Ignore;

            document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);

            VisualElement hex = e.target as VisualElement;
            hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
            {
                hoverTemp.style.top = (Screen.height / 2) - (hoverTemp.resolvedStyle.height / 2);
                if (hex.worldBound.x < Screen.width - (Screen.width / 2.5f))
                    hoverTemp.style.left = hex.worldBound.x + hex.resolvedStyle.width;
                else
                    hoverTemp.style.left = hex.worldBound.x - (hoverTemp.resolvedStyle.width);
            });
        }
        else if (!target.ContainsPoint(e.localPosition) && hoverTemp != null)
            Leave();
    }

    private void OnFlowerMove(PointerMoveEvent e, FlowerType f)
    {
        VisualElement targetTemplate = e.currentTarget as VisualElement;
        CustomVisualElement target = targetTemplate.Q<CustomVisualElement>("Item");
        if (target.ContainsPoint(e.localPosition) && hoverTemp == null)
        {
            hoverTemp = flowerUI.Instantiate();
            VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

            popup.Q<Label>("Type").text = 5 + " " + f.ToString();
            popup.Q<Label>("Amount").text = tracker.flowerDetails[f];
            popup.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)f - 2];

            hoverTemp.style.position = Position.Absolute;
            hoverTemp.pickingMode = PickingMode.Ignore;
            popup.pickingMode = PickingMode.Ignore;

            document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);

            VisualElement hex = e.target as VisualElement;
            hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
            {
                hoverTemp.style.top = (Screen.height / 2) - (hoverTemp.resolvedStyle.height / 2);
                if (hex.worldBound.x < Screen.width - (Screen.width / 2.5f))
                    hoverTemp.style.left = hex.worldBound.x + hex.resolvedStyle.width;
                else
                    hoverTemp.style.left = hex.worldBound.x - (hoverTemp.resolvedStyle.width);
            });
        }
        else if (!target.ContainsPoint(e.localPosition) && hoverTemp != null)
            Leave();
    }

    private void OnToolMove(PointerMoveEvent e, Tool rand)
    {
        VisualElement targetTemplate = e.currentTarget as VisualElement;
        CustomVisualElement target = targetTemplate.Q<CustomVisualElement>("Item");
        if (target.ContainsPoint(e.localPosition) && hoverTemp == null)
        {
            hoverTemp = toolUI.Instantiate();
            VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

            string title = rand.ToString();
            int level = toolManager.GetToolFromTag(rand.ToString()).Level;
            if (level == 1)
                title += " Upgrade I";
            else if (level == 2)
                title += " Upgrade II";

            popup.Q<Label>("Type").text = title;
            popup.Q<Label>("Description").text = toolManager.GetToolFromTag(rand.ToString()).GetDescription();
            popup.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.toolSprites[(int)rand];

            hoverTemp.style.position = Position.Absolute;
            hoverTemp.pickingMode = PickingMode.Ignore;
            popup.pickingMode = PickingMode.Ignore;

            document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);

            VisualElement hex = e.target as VisualElement;
            hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
            {
                hoverTemp.style.top = (Screen.height / 2) - (hoverTemp.resolvedStyle.height / 2);
                if (hex.worldBound.x < Screen.width - (Screen.width / 2.5f))
                    hoverTemp.style.left = hex.worldBound.x + hex.resolvedStyle.width;
                else
                    hoverTemp.style.left = hex.worldBound.x - (hoverTemp.resolvedStyle.width);
            });
        }
        else if (!target.ContainsPoint(e.localPosition) && hoverTemp != null)
            Leave();
    }

    private void Leave()
    {
        if (hoveredQueen != null)
        {
            hoveredQueen.UnregisterCallback(cycleDetails);
            hoveredQueen = null;
        }

        document.rootVisualElement.Q<VisualElement>("BackgroundTint").Remove(hoverTemp);
        hoverTemp = null;
        if (activeHover != null)
            activeHover = null;

    }
    #endregion

    private void OnQuirkExit(PointerLeaveEvent e)
    {
        document.rootVisualElement.Remove(activeLabel);
        activeLabel = null;
    }
}