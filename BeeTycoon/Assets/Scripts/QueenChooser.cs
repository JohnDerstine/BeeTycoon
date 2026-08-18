using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class QueenChooser : MonoBehaviour
{
    [SerializeField]
    private RunModifiers mods;

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

    EventCallback<PointerLeaveEvent> callback;

    private bool skippable = true;

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
        callback = new EventCallback<PointerLeaveEvent>(OnAnyLeave);
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
            item.RegisterCallback<PointerEnterEvent>(e => OnModEnter(e, randMod.ID));
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
                item.RegisterCallback<PointerEnterEvent>(e => OnQueenEnter(e, queen));
            }

            if (randHex == honeyHex)
            {
                FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                item.Q<VisualElement>("Icon").style.backgroundImage = honeySprite;
                cost.text = "1";
                item.AddManipulator(new Clickable(e => SelectHoneyShop(rand, item)));
                item.RegisterCallback<PointerEnterEvent>(e => OnHoneyEnter(e, rand));
            }

            if (randHex == flowerHex)
            {
                FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)rand - 2];
                cost.text = "1";
                item.AddManipulator(new Clickable(e => SelectFlowerShop(rand, item)));
                item.RegisterCallback<PointerEnterEvent>(e => OnFlowerEnter(e, rand));
            }

            if (randHex == toolHex)
            {
                cost.text = "2";
                Tool rand = toolManager.GetUnmaxedTools()[Random.Range(0, toolManager.GetUnmaxedTools().Count)];
                item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.toolSprites[(int)rand];
                item.AddManipulator(new Clickable(e => SelectToolShop(rand, item)));
                item.RegisterCallback<PointerEnterEvent>(e => OnToolEnter(e, rand));
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

    public IEnumerator GiveChoice(int choice, bool starter = false, bool modifier = false)
    {
        ResetRNGOptions();
        isChoosing = true;
        template = choicesContainer.Instantiate();
        container = template.Q<VisualElement>("Container");
        template.style.position = Position.Absolute;
        template.style.flexDirection = FlexDirection.Row;
        template.style.justifyContent = Justify.FlexStart;
        container.style.justifyContent = Justify.SpaceAround;
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);

        StartCoroutine(SpawnChoices(choice, starter, modifier));
        yield return new WaitForFixedUpdate(); //Wait for selectionActive to be updated
        yield return new WaitUntil(() => !selectionActive); //Wait for selectionActive to be false until spawning more choices

        document.rootVisualElement.Q<VisualElement>("Base").Remove(template);
        template = null;
        container = null;
        isChoosing = false;
        //GameObject.Find("GameController").GetComponent<GameController>().CurrentState = GameStates.Running;
    }

    public IEnumerator GiveChoice(List<int> choices, bool starter = false, bool modifier = false)
    {
        ResetRNGOptions();
        bool wasTrue = starter;
        isChoosing = true;
        template = choicesContainer.Instantiate();
        container = template.Q<VisualElement>("Container");
        template.style.position = Position.Absolute;
        template.style.flexDirection = FlexDirection.Row;
        template.style.justifyContent = Justify.FlexStart;
        container.style.justifyContent = Justify.SpaceAround;
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);
        for (int i = 0; i < choices.Count; i++)
        {
            if (wasTrue && i == 1)
                modifier = true;
            StartCoroutine(SpawnChoices(choices[i], starter, modifier));
            yield return new WaitForFixedUpdate(); //Wait for selectionActive to be updated
            yield return new WaitUntil(() => !selectionActive); //Wait for selectionActive to be false until spawning more choices
            starter = false;
        }

        document.rootVisualElement.Q<VisualElement>("Base").Remove(template);
        template = null;
        container = null;
        isChoosing = false;
        GameObject.Find("GameController").GetComponent<GameController>().CurrentState = GameStates.Running;
    }

    //Creates Queen Bee choices for the user to select from
    //Takes an input for the number of choices and whether or not this will be the player's starter Queen
    private IEnumerator SpawnChoices(int numChoices, bool starter, bool modifier)
    {
        if (sizeDirections.Count == 0 && rngOptions.Contains(sizeUI))
            rngOptions.Remove(sizeUI);

        //Get the options that user will have to choose from
        List<VisualTreeAsset> rngChoices = new List<VisualTreeAsset>();
        if (starter)
        {
            for (int i = 0; rngChoices.Count < numChoices; i++)
                rngChoices.Add(queenUI);
        }
        else if (modifier)
        {
            for (int i = 0; rngChoices.Count < numChoices; i++)
                rngChoices.Add(modifierUI);
        }
        else
        {
            //rngChoices.Add(queenUI);
            for (int i = 0; rngChoices.Count < numChoices; i++)
            {
                if (rngOptions.Count == 0)
                    rngOptions.Add(queenUI);

                int rand = Random.Range(0, rngOptions.Count);
                rngChoices.Add(rngOptions[rand]);
                rngOptions.RemoveAt(rand);
            }
        }

        StartCoroutine(SetUpQueens(rngChoices, starter));
        
        for (int i = 0; i < rngChoices.Count; i++)
        {
            if (rngChoices[i] != queenUI)
            {
                //Set up UI template for choice
                TemplateContainer temp = rngChoices[i].Instantiate();
                VisualElement popup = temp.Q<VisualElement>("Popup");
                popup.RegisterCallback(queenMoveCallback); //register callbacks for hovering over the choices
                popup.RegisterCallback(queenExitCallback);

                if (rngChoices[i] == honeyUI)
                {
                    FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                    popup.Q<Label>("Type").text = rand.ToString();
                    popup.Q<Label>("Price").text = "$" + GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().GetPrice(rand) + " / lb.";
                    popup.AddManipulator(new Clickable(e => SelectHoney(rand)));
                }
                else if (rngChoices[i] == flowerUI)
                {
                    FlowerType rand = tracker.ownedFlowers[Random.Range(0, tracker.ownedFlowers.Count())];
                    popup.Q<Label>("Type").text = rand.ToString();
                    popup.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)rand - 2];

                    popup.AddManipulator(new Clickable(e => SelectFlower(rand)));
                }
                else if (rngChoices[i] == sizeUI)
                {
                    string dir = sizeDirections[Random.Range(0, sizeDirections.Count)];
                    popup.Q<Label>("Amount").text = "Expand plot<br> " + dir + "wards";
                    popup.AddManipulator(new Clickable(e => SelectSize(dir)));
                }
                else if (rngChoices[i] == toolUI)
                {
                    Tool rand = toolManager.GetUnmaxedTools()[Random.Range(0, toolManager.GetUnmaxedTools().Count)];

                    string title = rand.ToString();
                    int level = toolManager.GetToolFromTag(rand.ToString()).Level;
                    if (level == 1)
                        title += " Upgrade I";
                    else if (level == 2)
                        title += " Upgrade II";

                    popup.Q<Label>("Type").text = title;
                    popup.Q<Label>("Description").text = toolManager.GetToolFromTag(rand.ToString()).GetDescription();
                    popup.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.toolSprites[(int)rand];

                    popup.AddManipulator(new Clickable(e => SelectTool(rand)));
                }
                else if (rngChoices[i] == modifierUI)
                {
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
                    int randID = -999;
                    while (usedIds.Contains(randID) || randID == -999)
                        randID = Random.Range(0, applicableMods.Count());

                    usedIds.Add(randID);
                    popup.Q<VisualElement>("Icon").style.backgroundImage = applicableMods[randID].Sprite;
                    popup.Q<Label>("Title").text = applicableMods[randID].Name;
                    popup.Q<Label>("Description").text = applicableMods[randID].Description;
                    int actualID = applicableMods[randID].ID;
                    popup.AddManipulator(new Clickable(e => SelectModifier(actualID))); //Looking back on it, this doesn't make sense. I don't want randID, I want the ID of the mod in applicableMods[randID]
                    //Double check to make sure that the game refelcts this being incorrect
                }
                    container.Add(temp);
            }
        }
        usedIds.Clear();

        yield return new WaitForFixedUpdate();

        template.Q<Label>("ChooseLabel").text = "Choose 1 of " + numChoices; //Set up instruction text
        if (!starter)
            template.Q<Label>("Description").text = "This will be added to your shop";

        ResetRNGOptions();
        selectionActive = true;
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

    private IEnumerator SetUpQueens(List<VisualTreeAsset> rngChoices, bool starter)
    {
        List<string> possibilites = new List<string>(); //Get a list of the species of bees the player has unlocked
        foreach (KeyValuePair<string, bool> kvp in tracker.species)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }

        //Instatiate Queen Objects
        for (int i = 0; i < rngChoices.Count; i++)
        {
            if (rngChoices[i] == queenUI)
            {
                GameObject q = Instantiate(queenPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
                queenOptions.Add(q.GetComponent<QueenBee>());
            }
        }

        yield return new WaitForFixedUpdate(); //Wait a frame for the Queens to be instatiated

        ////Set up each queen choice
        for (int i = 0; i < queenOptions.Count; i++)
        {
            //Decide the queen's species
            if (starter)
            {
                int rand = Random.Range(0, possibilites.Count);
                queenOptions[i].species = possibilites[rand];
                possibilites.RemoveAt(rand);
                if (starter) //First queen of game is free.
                    queenOptions[i].GetComponent<Cost>().Price = 0;
            }

            //Set up UI template for queen choice
            TemplateContainer temp = queenUI.Instantiate();
            VisualElement popup = temp.Q<VisualElement>("Popup");
            popup.RegisterCallback(queenMoveCallback); //register callbacks for hovering over the choices
            popup.RegisterCallback(queenExitCallback);

            int savedI = i; //I needs to be saved to a variable for callbacks to reference it correctly
            popup.AddManipulator(new Clickable(e => SelectQueen(savedI))); //Add Click event

            //Display Info about queen
            temp.Q<VisualElement>("Icon").style.backgroundImage = queenSprite;
            temp.Q<Label>("Species").text = "Species: " + queenOptions[i].species;
            temp.Q<Label>("Age").text = "Radius Type: " + queenOptions[i].radiusType;
            temp.Q<Label>("Grade").text = "Grade: " + queenOptions[i].grade.ToString() + "/10";
            temp.Q<Label>("Favorite").text = "Favorite Flower: " + queenOptions[i].favorite.ToString();

            //Add quirk labels to the queen
            foreach (string s in queenOptions[i].quirks)
            {
                Label quirk = new Label();
                quirk.text = s;
                quirk.AddToClassList("Quirk");
                temp.Q<VisualElement>("QuirkContainer").Add(quirk);
                quirk.RegisterCallback(quirkEnterCallback, quirk.text);
                quirk.RegisterCallback(quirkExitCallback);
            }

            //Add choice to the UI Document
            container.Add(temp);
        }
    }

    #region Deprecated Selects

    private void SelectFlower(FlowerType f)
    {
        selectionActive = false;
        hexMenu.flowersOwned[f] += 5;
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectHoney(FlowerType f)
    {
        selectionActive = false;
        player.inventory[f][0] += 5; //add to total honey
        player.inventory[f][2] += 5; //add to medium quality honey
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectSize(string dir)
    {
        selectionActive = false;
        GameObject.Find("MapLoader").GetComponent<MapLoader>().IncreaseMapSize(dir);
        sizeDirections.Remove(dir);
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectTool(Tool tool)
    {
        selectionActive = false;
        ToolScript toolScript = toolManager.GetToolFromTag(tool.ToString());
        if (toolScript.Level == 0)
            toolScript.gameObject.GetComponent<Cost>().Purchased = true;
        toolManager.GetToolFromTag(tool.ToString()).Upgrade();

        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void SelectModifier(int id)
    {
        selectionActive = false;
        mods.AddMod(id);
        if (modifierList == null)
            modifierList = GameObject.Find("Shed(Clone)").GetComponent<Shed>();
        modifierList.AddModifier(id);
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

    #region enters
    private void OnModEnter(PointerEnterEvent e, int id)
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
            hoverTemp.style.top = hex.resolvedStyle.top - (hoverTemp.resolvedStyle.height / 4);
            if (hex.resolvedStyle.left > Screen.width / 4)
                hoverTemp.style.left = hex.resolvedStyle.left - (hoverTemp.resolvedStyle.width - hoverTemp.resolvedStyle.width / 4);
            else
                hoverTemp.style.left = hex.resolvedStyle.left + hex.resolvedStyle.width;
        });
        hex.RegisterCallback(callback);
    }

    private void OnQueenEnter(PointerEnterEvent e, QueenBee queen)
    {
        hoverTemp = queenUI.Instantiate();
        VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

        //Display Info about queen
        hoverTemp.Q<VisualElement>("Icon").style.backgroundImage = queenSprite;
        hoverTemp.Q<Label>("Species").text = "Species: " + queen.species;
        hoverTemp.Q<Label>("Age").text = "Radius Type: " + queen.radiusType;
        hoverTemp.Q<Label>("Grade").text = "Grade: " + queen.grade.ToString() + "/10";
        hoverTemp.Q<Label>("Favorite").text = "Favorite Flower: " + queen.favorite.ToString();

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

        VisualElement hex = e.target as VisualElement;
        hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
        {
            hoverTemp.style.top = hex.resolvedStyle.top - (hoverTemp.resolvedStyle.height / 4);
            if (hex.resolvedStyle.left > Screen.width / 4)
                hoverTemp.style.left = hex.resolvedStyle.left - (hoverTemp.resolvedStyle.width - hoverTemp.resolvedStyle.width / 4);
            else
                hoverTemp.style.left = hex.resolvedStyle.left + hex.resolvedStyle.width;
        });
        hex.RegisterCallback(callback);
    }

    private void OnHoneyEnter(PointerEnterEvent e, FlowerType f)
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
            hoverTemp.style.top = hex.resolvedStyle.top - (hoverTemp.resolvedStyle.height / 4);
            if (hex.resolvedStyle.left > Screen.width / 4)
                hoverTemp.style.left = hex.resolvedStyle.left - (hoverTemp.resolvedStyle.width - hoverTemp.resolvedStyle.width / 4);
            else
                hoverTemp.style.left = hex.resolvedStyle.left + hex.resolvedStyle.width;
        });
        hex.RegisterCallback(callback);
    }

    private void OnFlowerEnter(PointerEnterEvent e, FlowerType f)
    {
        hoverTemp = flowerUI.Instantiate();
        VisualElement popup = hoverTemp.Q<VisualElement>("Popup");

        popup.Q<Label>("Type").text = f.ToString();
        popup.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[(int)f - 2];

        hoverTemp.style.position = Position.Absolute;
        hoverTemp.pickingMode = PickingMode.Ignore;
        popup.pickingMode = PickingMode.Ignore;

        document.rootVisualElement.Q<VisualElement>("BackgroundTint").Add(hoverTemp);

        VisualElement hex = e.target as VisualElement;
        hoverTemp.RegisterCallback((GeometryChangedEvent evt) =>
        {
            hoverTemp.style.top = hex.resolvedStyle.top - (hoverTemp.resolvedStyle.height / 4);
            if (hex.resolvedStyle.left > Screen.width / 4)
                hoverTemp.style.left = hex.resolvedStyle.left - (hoverTemp.resolvedStyle.width - hoverTemp.resolvedStyle.width / 4);
            else
                hoverTemp.style.left = hex.resolvedStyle.left + hex.resolvedStyle.width;
        });
        hex.RegisterCallback(callback);
    }

    private void OnToolEnter(PointerEnterEvent e, Tool rand)
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
            hoverTemp.style.top = hex.resolvedStyle.top - (hoverTemp.resolvedStyle.height / 4);
            if (hex.resolvedStyle.left > Screen.width / 4)
                hoverTemp.style.left = hex.resolvedStyle.left - (hoverTemp.resolvedStyle.width - hoverTemp.resolvedStyle.width / 4);
            else
                hoverTemp.style.left = hex.resolvedStyle.left + hex.resolvedStyle.width;
        });
        hex.RegisterCallback(callback);
    }

    private void OnAnyLeave(PointerLeaveEvent e)
    {
        VisualElement hex = e.target as VisualElement;
        hex.UnregisterCallback(callback);
        document.rootVisualElement.Q<VisualElement>("BackgroundTint").Remove(hoverTemp);
        hoverTemp = null;
    }

    #endregion

    private void OnQuirkExit(PointerLeaveEvent e)
    {
        document.rootVisualElement.Remove(activeLabel);
        activeLabel = null;
    }
}