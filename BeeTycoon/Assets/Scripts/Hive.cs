using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum FlowerType
{
    Empty = 0,
    Wildflower = 1,
    Clover = 2,
    Alfalfa = 3,
    Buckwheat = 4,
    Goldenrod = 5,
    Fireweed = 6,
    Dandelion = 7,
    Sunflower = 8,
    Daisy = 9,
    Thistle = 10,
    Blueberry = 11,
    Orange = 12,
    Tupelo = 13
}

public class Hive : MonoBehaviour
{
    private UIDocument document;
    private MapLoader map;
    private PlayerController player;
    private GameController game;
    private UnlockTracker tracker;
    private HexMenu hexMenu;

    [SerializeField]
    private VisualTreeAsset hiveUI;

    [SerializeField]
    private VisualTreeAsset queenUI;

    [SerializeField]
    private VisualTreeAsset GlossaryUI;

    [SerializeField]
    private VisualTreeAsset afflictionPopupUI;

    [SerializeField]
    private VisualTreeAsset afflictionToolTipUI;

    [SerializeField]
    private List<Texture2D> afflictionIcons = new List<Texture2D>();

    [SerializeField]
    private List<Texture2D> remedyIcons = new List<Texture2D>();

    [SerializeField]
    private VisualTreeAsset honeyGlobIcon;

    [SerializeField]
    private Texture2D queenSprite;

    [SerializeField]
    private Texture2D deadSprite;

    [SerializeField]
    private AudioClip audio;

    [SerializeField]
    private VisualTreeAsset StressPanel;

    [SerializeField]
    private Material Highlight;

    private Texture2D currentIcon;

    private ToolManager toolManager;

    public TemplateContainer template;
    private TemplateContainer hoverTemplate;
    private TemplateContainer tooltip;
    public bool empty = true;
    public bool placed;

    public int x;
    public int y;
    public Tile hiveTile;

    public const float conversionRate = 0.01f; //0.006f;

    private int size = 1;
    private float basePopulation = 5000;
    public float population = 5000;
    private float popCap = 20000; //what population the hive can currently house
    private float popSizeCap = 20000; //how much each level of size changes the popCap
    public float comb = 6;
    //private int combCap = 6; //how much honey the hive can currently store
    public int combSizeCap = 6; //how much each level of size changes the combCap
    public float honey;
    public float maxHoneyProduction;
    private float maxHoneyBase = 10;

    float greedy;
    float industrious;
    float agile;
    float rugged;
    float docile;
    float motherly;
    float picky;

    private float addedNectar = 0;

    private float baseStoragePerComb = 5; //how much storage you start with
    private float storagePerComb = 5; //how much each level of size changes the storage - lbs.

    private float birthRate = 2500;
    private float hiveEfficency = 0; //Efficiency is a multiplier to all the hive's actions and is calculated by the population / total population * size of the hive

    private int stressLevel = 0;
    public List<string> conditions = new List<string>();
    private List<string> randConditions = new List<string>() { "Mice", "Mites", "Moths", "Fungus", "Aggrevated", "Glued"};
    private List<string> baseRandConditions = new List<string>() { "Mice", "Mites", "Moths", "Fungus", "Aggrevated", "Glued" };
    private Dictionary<string, int> conditionValues = new Dictionary<string, int>()
    {
        { "Mice", 2},
        { "Mites", 3},
        { "Moths", 1},
        { "Fungus", 1},
        { "Aggrevated", 1},
        { "Glued", 1},
        { "Freezing", 4},
        { "Starving", 4},
        { "Defending", 1},
        { "Swarmed", 1},
        { "Killer", 1},
        { "Undisturbed", -1},
        { "Relaxed", -1},
        { "Calm", -1},
        { "Indulged", -1},
        { "Carniolan", - 1}
    };
    private bool attacking;


    public QueenBee queen;

    public Dictionary<FlowerType, float> nectarValues = new Dictionary<FlowerType, float>();
    public Dictionary<FlowerType, float> buckfastGains = new Dictionary<FlowerType, float>();
    public Dictionary<FlowerType, float> buckfastValues = new Dictionary<FlowerType, float>();
    public FlowerType honeyType = FlowerType.Empty;
    public float honeyPurity = 0;

    public float nectarGain;
    public Dictionary<FlowerType, float> personalNectarGains = new Dictionary<FlowerType, float>();

    //UI
    private CustomVisualElement infoHover;
    private VisualElement infoHoverTint;
    private VisualElement infoTint;
    private VisualElement infoIcon;
    private Label honeyPurityLabel;
    private Label honeyTypeLabel;
    private VisualElement smallHarvest;
    private VisualElement mediumHarvest;
    private VisualElement largeHarvest;
    private VisualElement combMeter;
    private VisualElement nectarMeter;
    private VisualElement honeyMeter;
    private CustomVisualElement stressClick;
    private TemplateContainer stressContainer;
    private Clickable openWindow;
    private Clickable closeWindow;
    private Dictionary<VisualElement, bool> harvestDict = new Dictionary<VisualElement, bool>();
    private Toggle noHarvest;
    //private float harvestPercentage;
    private CustomVisualElement nectarHover;
    private CustomVisualElement honeyHover;
    private CustomVisualElement combHover;
    EventCallback<PointerMoveEvent> moveCallback;
    EventCallback<PointerLeaveEvent> exitCallback;
    EventCallback<PointerMoveEvent> queenMoveCallback;
    EventCallback<PointerLeaveEvent> queenExitCallback;
    EventCallback<PointerEnterEvent> harvestEnterCallback;
    EventCallback<PointerLeaveEvent> harvestLeaveCallback;
    EventCallback<PointerEnterEvent> infoEnterCallback;
    EventCallback<PointerLeaveEvent> infoLeaveCallback;
    Clickable smallHarvestClick;
    Clickable mediumHarvestClick;
    Clickable largeHarvestClick;
    private CustomVisualElement currentHover;
    private StyleColor darkTint;
    private StyleColor darkerTint;
    public StyleColor lightTint;
    private VisualElement queenHex;
    public CustomVisualElement queenClick;
    public Clickable assignQueen;
    private VisualElement exit;
    public bool selectingQueen;
    public bool isOpen;
    public bool hasSugar;
    public bool hasReducer;
    public bool hasStand;
    public bool hasRepellant;
    public bool hasInsulation;
    public bool canBeOpened = true;
    private string condition = "Healthy";
    private TemplateContainer activePopup = null;
    private Coroutine activePulse;
    public bool fromSave;
    public int repellantTurns;
    List<TemplateContainer> globs = new List<TemplateContainer>();
    private AudioSource source;
    private bool animsRunning;

    public List<Tile> tileRadius = new List<Tile>();
    public int rotation = 0;

    private int turnsSinceLastHarvest = 0;
    private int mothTurns = 0;

    private bool harvestActive = true;

    [SerializeField]
    Material selectedMaterial;

    public int Size
    {
        get { return size; }
        set
        {
            size += value;
            popCap = popSizeCap * size;
            comb = combSizeCap * size;
            //combCap = combSizeCap * size;
            UpdateMeters();
        }
    }

    public bool Placed
    {
        get { return placed; }
        set
        {
            placed = value;
            if (value)
            {
                //if (queen.nullQueen)
                //    Condition = "Dead";
                player.OpenHiveUI(template, hiveUI, this);

                if (game.CurrentState != GameStates.Running)
                    player.CloseHiveUI(this);
            }
        }
    }

    public float PopCap
    { get { return popCap; } }    

    //EFFECTS:
    //-1 +10% efficiency
    //0 Nothing
    //1 -33% efficiency 
    //2 You can't harvest without a suit
    //3 Attacks other hives
    //4 Halt all production
    //5 Death

    public int StressLevel
    {
        get { return stressLevel; }
        set
        {
            if (stressLevel != value)//spawn popup bubble
            {
                if (activePopup != null)
                {
                    activePopup.RemoveFromHierarchy();
                    activePopup = null;
                }

                activePopup = afflictionPopupUI.Instantiate();
                activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[1];
                AdjustPopupTransform();
                activePopup.style.position = Position.Absolute;
                activePopup.style.flexGrow = 0;
                document.rootVisualElement.Q<VisualElement>("Base").Add(activePopup);
                activePopup.RegisterCallback<PointerDownEvent>(GlossaryOpen);
            }

            stressLevel = value;

            if ((stressLevel >= 2 && toolManager.suit.Level == 0) && harvestActive)
                DisableHarvestButtons();
            else if ((stressLevel < 2 || toolManager.suit.Level > 0) && !harvestActive)
                EnableHarvestButtons();

            if (stressLevel >= 3 && !attacking)
            {
                attacking = true;
                foreach (Hive h in player.hives)
                {
                    if (h != this)
                        h.AddCondition("Defending");
                }
            }
            else if (stressLevel < 3 && attacking)
            {
                attacking = false;
                foreach (Hive h in player.hives)
                {
                    if (h != this)
                        h.CureCondition("Defending");
                }
            }

            if (stressLevel >= 5) //Kill hive if stress >= 5
            {
                empty = true;

                if (activePopup != null)
                {
                    activePopup.RemoveFromHierarchy();
                    activePopup = null;
                }

                activePopup = afflictionPopupUI.Instantiate();
                activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[5];
                AdjustPopupTransform();
                activePopup.style.position = Position.Absolute;
                activePopup.style.flexGrow = 0;
                document.rootVisualElement.Q<VisualElement>("Base").Add(activePopup);
                activePopup.RegisterCallback<PointerDownEvent>(GlossaryOpen);

                queenHex.style.backgroundImage = deadSprite;
                queenClick.UnregisterCallback<PointerMoveEvent>(OnQueenMove);

            }
        }
    }

    private void GlossaryOpen(PointerDownEvent e)
    {
        if (e.button == 1)
        {
            document.GetComponent<Glossary>().OpenGlossary("Afflictions");
        }
    }

    public CustomVisualElement CurrentHover
    {
        get { return currentHover; }
        set
        {
            if (value == null && currentHover != null)
            {
                currentHover.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
                currentHover.Q<Label>("Percent").style.visibility = Visibility.Hidden;
                currentHover.Q<Label>("PercentOf").style.visibility = Visibility.Hidden;
                currentHover.Q<Label>("Flat").style.visibility = Visibility.Hidden;
                currentHover.Q<Label>("FlatOf").style.visibility = Visibility.Hidden;
                currentHover = value;
            }
            else if (value != null)
            {
                currentHover = value;
                currentHover.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
                currentHover.Q<Label>("Percent").style.visibility = Visibility.Visible;
                currentHover.Q<Label>("PercentOf").style.visibility = Visibility.Visible;
                currentHover.Q<Label>("Flat").style.visibility = Visibility.Visible;
                currentHover.Q<Label>("FlatOf").style.visibility = Visibility.Visible;
            }
        }
    }

    void Awake()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        game = GameObject.Find("GameController").GetComponent<GameController>();
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        tracker = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        hexMenu = GameObject.Find("HexMenu").GetComponent<HexMenu>();
        source = GetComponent<AudioSource>();
        queen = GetComponent<QueenBee>();

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            personalNectarGains.Add(fType, 0);
            buckfastGains.Add(fType, 0);
            if (!fromSave)
            {
                nectarValues.Add(fType, 0);
                buckfastValues.Add(fType, 0);
            }
        }

        Color darkerTintColor = Color.black;
        darkerTintColor.a = 0.8f;
        darkerTint = new StyleColor(darkerTintColor);

        Color darkTintColor = Color.black;
        darkTintColor.a = 0.6f;
        darkTint = new StyleColor(darkTintColor);

        Color lightTintColor = Color.black;
        lightTintColor.a = 0.0f;
        lightTint = new StyleColor(lightTintColor);

        assignQueen = new Clickable(OpenQueenTab);

        CalcEfficiency();

        //If there is no queen, dislpay empty queen popup
        activePopup = afflictionPopupUI.Instantiate();
        activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[5];
        AdjustPopupTransform();
        activePopup.style.position = Position.Absolute;
        activePopup.style.flexGrow = 0;
        document.rootVisualElement.Q<VisualElement>("Base").Add(activePopup);
        activePopup.RegisterCallback<PointerDownEvent>(GlossaryOpen);
    }

    public void SetStatsModifiers()
    {
        greedy = (queen.quirks.Contains("Greedy")) ? tracker.quirkValues["Greedy"] : 1;
        industrious = (queen.quirks.Contains("Industrious")) ? tracker.quirkValues["Industrious"] : 0;
        agile = (queen.quirks.Contains("Agile")) ? tracker.quirkValues["Agile"] : 0;
        docile = (queen.quirks.Contains("Territorial")) ? tracker.quirkValues["Territorial"] : 0;
        rugged = (queen.quirks.Contains("Rugged")) ? tracker.quirkValues["Rugged"] : 1;
        motherly = (queen.quirks.Contains("Motherly")) ? tracker.quirkValues["Motherly"] : 0;
        picky = (queen.quirks.Contains("Picky")) ? tracker.quirkValues["Picky"] : 0;
    }

    public void ResetNectarGains()
    {
        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            personalNectarGains[fType] = 0;
            buckfastGains[fType] = 0;
        }
    }

    void Update()
    {
        //if (Condition != "Healthy" && !document.rootVisualElement.Q<VisualElement>("Base").Contains(activePopup))
        //    document.rootVisualElement.Q<VisualElement>("Base").Add(activePopup); //I don't know how the active popup is being added and removed from Base on the same frame. band-aid fix

        //Debug.Log(activePopup);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectingQueen)
            {
                CloseQueenSelection();
            }
            else if (isOpen && player.SelectedItem == null && !GameObject.Find("UIDocument").GetComponent<Glossary>().open)
            {
                player.CloseHiveUI(this);
            }
        }

        if (Input.GetMouseButtonDown(1) && isOpen)
        {
            if (stressContainer != null)
                CloseStressWindow();
            else
                player.CloseHiveUI(this);
        }

        if (Input.GetKeyDown(KeyCode.L))
            StressLevel = 3;

        if (activePopup != null)
        {
            AdjustPopupTransform();
        }

        if (activePopup != null)
        {
            if (game.CurrentState != GameStates.Running && activePopup.visible)
                activePopup.visible = false;
            else if (game.CurrentState == GameStates.Running && !activePopup.visible)
                activePopup.visible = true;
        }
    }

    private void AdjustPopupTransform()
    {
        Vector3 worldPos = gameObject.transform.position;
        worldPos = Camera.main.WorldToScreenPoint(worldPos);
        worldPos.x -= activePopup.resolvedStyle.width * 0.5f;
        worldPos.y += activePopup.resolvedStyle.width;
        activePopup.style.top = Screen.height - worldPos.y;
        activePopup.style.left = worldPos.x;
        activePopup.Q<VisualElement>("Background").style.width = 128 / (Camera.main.transform.position.y / 17);
        activePopup.Q<VisualElement>("Background").style.height = 128 / (Camera.main.transform.position.y / 17);
        activePopup.Q<VisualElement>("Icon").style.width = 56 / (Camera.main.transform.position.y / 17);
        activePopup.Q<VisualElement>("Icon").style.height = 56 / (Camera.main.transform.position.y / 17);
    }

    public void CloseQueenSelection()
    {
        if (selectingQueen)
        {
            selectingQueen = false;
            queenClick.AddManipulator(assignQueen);
            queenClick.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
            hexMenu.CloseTab();
            player.SelectedItem = null;
        }
    }

    public void CheckForCarniolan()
    {
        if (queen.species == "Carniolan")
        {
            foreach (Tile t in tileRadius)
            {
                if (t.HasHive && t.hive != this && !t.hive.conditions.Contains("Carniolan"))
                    t.hive.AddCondition("Carniolan");
            }
        }

        if (conditions.Contains("Carniolan"))
        {
            bool found = false;
            foreach (Hive h in player.hives)
            {
                if (h.queen.species == "Carniolan" && h != this)
                {
                    foreach (Tile t in tileRadius)
                    {
                        if (t.HasHive && t.hive == this)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                    return;
            }
            CureCondition("Carniolan");
        }
    }

    private void CalcEfficiency()
    {
        float popEff = population / basePopulation * size;
        float hivestand = (hasStand) ? 10 : 0;
        float russian = (queen.species == "Russian") ? 10 : 0;
        float stressMod = (stressLevel <= -1) ? 10 : 0;
        if (stressMod >= 1)
            stressMod = -10;

        hiveEfficency = Mathf.Clamp((100 + popEff + hivestand + russian + stressMod) / 100, 1, 1.5f + agile);
    }

    private float GetItalianMult()
    {
        float mult = 1;
        foreach (Hive h in player.hives)
            if (h.queen.species == "Italian")
                mult += 0.1f;
        return mult;
    }

    private float GetCordovanMult()
    {
        if (queen.species == "Cordovan")
        {
            float mult = 0.5f;
            mult += 0.25f * player.hives.Count - 1;
            return mult;
        }
        return 1;
    }

    private float GetHimalayanMult()
    {
        if (queen.species == "Himalayan" && CheckHimalayanCondition())
            return 1.25f;
        return 1;
    }

    private bool CheckHimalayanCondition()
    {
        foreach (Tile t in tileRadius)
            if (t.HasHive && t.hive != this)
                return false;
        return true;
    }

    public void UpdateHive()
    {
        if (empty)
            return;

        if (hasRepellant)
        {
            repellantTurns--;
            if (repellantTurns == 0)
                hasRepellant = false;
        }

        if (conditions.Contains("Moths"))
        {
            if (mothTurns == 3)
                CureCondition("Moths");
            mothTurns++;
        }

        if (game.Season == "winter")
        {
            //CONSUME HONEY FOR FOOD
            honey -= (population / 2000) / rugged;

            //POP DECLINE
            if (honey < 0)
            {
                population /= 2f;
                honey = 0;
            }
            return;
        }
        else if (conditions.Contains("Freezing"))
            CureCondition("Freezing");
            
        if (stressLevel < 4 || queen.species == "Killer")
        {
            nectarGain = (addedNectar + personalNectarGains.Values.Sum()) * conversionRate; //scale it down to lbs
            float buckGain = buckfastGains.Values.Sum() * conversionRate;

            float italian = (queen.species == "Italian") ? 1.25f : 1;
            maxHoneyProduction = maxHoneyBase * hiveEfficency * GetItalianMult() * greedy * GetCordovanMult() * GetHimalayanMult();
            storagePerComb = (baseStoragePerComb + industrious) * hiveEfficency;

            float possibleHoney = nectarGain;
            if (possibleHoney > maxHoneyProduction)
                possibleHoney = maxHoneyProduction;
            if (possibleHoney + honey > comb * storagePerComb)
                honey = comb * storagePerComb;
            else
                honey += possibleHoney;

            if (nectarGain > 0)
                SplitNectar(nectarGain);
            if (buckGain > 0)
                SplitBuckfast(buckGain);

            float possiblePop = birthRate + motherly;
            if (possiblePop + population > popCap)
                possiblePop = popCap - population;
            population += possiblePop;
        }

        CalcEfficiency();

        CalcHoneyStats();
        if (template != null)
            UpdateMeters();
        if (conditions.Count > 0 && (queen.species == "Japanese" || queen.japaneseInherited) && Random.Range(0, 3) == 0)
            CureRandomNegativeCondition();

        TryAddCondition();
        hasSugar = false;

        turnsSinceLastHarvest++;
        if (turnsSinceLastHarvest == 3)
            AddCondition("Undisturbed");

        if (!conditions.Contains("Indulged") && map.GetAdjacentFlowers(queen.favorite, x, y).Count > 0)
            AddCondition("Indulged");
        else if (conditions.Contains("Indulged") && map.GetAdjacentFlowers(queen.favorite, x, y).Count <= 0)
            CureCondition("Indulged");

        if (conditions.Contains("Relaxed"))
            CureCondition("Relaxed");

        CheckForCarniolan();
    }

    private void Harvest(float percent)
    {
        float amount = percent * honey;
        float extractorBonus = toolManager.extractor.extractorBonus;
        foreach (FlowerType key in nectarValues.Keys.ToList())
            nectarValues[key] -= nectarValues[key] * percent;

        if (honeyType != FlowerType.Wildflower)
        {
            player.inventory[honeyType][0] += amount * extractorBonus;
            honey -= amount;
            Debug.Log(amount);

            if (honeyPurity >= .9f)
                player.inventory[honeyType][3] += amount * extractorBonus;
            else if (honeyPurity > .7f)
                player.inventory[honeyType][2] += amount * extractorBonus;
            else
                player.inventory[honeyType][1] += amount * extractorBonus;
        }
        else
        {
            player.inventory[honeyType][0] += amount * extractorBonus;
            honey -= amount;
            Debug.Log(amount);
            player.inventory[honeyType][2] += amount * extractorBonus;
        }

        if (turnsSinceLastHarvest >= 3)
            CureCondition("Undisturbed");
        turnsSinceLastHarvest = 0;

        if (percent == 1f && amount / (storagePerComb * comb) > .75f)
            if (Random.Range(0, toolManager.suit.cureChance) == 0)
                CureCondition(conditions[Random.Range(0, conditions.Count)]);

        UpdateMeters();

        source.clip = audio;
        StartCoroutine(AnimateHarvest(amount));
    }

    private IEnumerator AnimateHarvest(float amount)
    {
        animsRunning = true;
        source.pitch = 0.5f;
        int rounded = Mathf.RoundToInt(amount);

        for (int i = 0; i < rounded; i++)
        {
            TemplateContainer glob = honeyGlobIcon.Instantiate();
            glob.style.position = Position.Absolute;
            glob.style.visibility = Visibility.Hidden;
            if (isOpen)
                document.rootVisualElement.Q<VisualElement>("HiveBase").Add(glob);
            globs.Add(glob);

            source.Play();

            yield return new WaitForEndOfFrame(); //let resolved style update

            float dir = Random.Range(0, 359);
            float radius = Random.Range(125, 175);
            float xOffset = 24;

            float top = honeyHover.resolvedStyle.top + (honeyHover.resolvedStyle.height / 2) - (glob.Q<VisualElement>("Glob").resolvedStyle.height / 2) + Mathf.Sin(dir) * radius;
            float left = document.rootVisualElement.Q<VisualElement>("HiveBase").Q<VisualElement>("Center").resolvedStyle.left + (honeyHover.resolvedStyle.width / 2) - (glob.Q<VisualElement>("Glob").resolvedStyle.width / 2) - xOffset + Mathf.Cos(dir) * radius;
            glob.style.visibility = Visibility.Visible;

            glob.style.top = honeyHover.resolvedStyle.top + (honeyHover.resolvedStyle.height / 2) - (glob.Q<VisualElement>("Glob").resolvedStyle.height / 2);
            glob.style.left = document.rootVisualElement.Q<VisualElement>("HiveBase").Q<VisualElement>("Center").resolvedStyle.left + (honeyHover.resolvedStyle.width / 2) - (glob.Q<VisualElement>("Glob").resolvedStyle.width / 2) - xOffset;

            yield return new WaitForEndOfFrame(); //let resolved style update

            StartCoroutine(ToPoint(glob, top, left, 0.5f, false));

            yield return new WaitForSeconds(0.1f); //delay per glob 0.1
        }

        yield return new WaitForSeconds(0.5f);

        source.pitch = 0.25f;
        foreach (TemplateContainer glob in globs)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(GameObject.Find("Truck").transform.position);
            float top = Screen.height - pos.y - 150;
            float left = pos.x - 1348;
            Debug.Log(top + " " + left);
            StartCoroutine(ToPoint(glob, top, left, 0.5f, true));
            yield return new WaitForSeconds(0.15f);
            source.Play();
            document.rootVisualElement.Q<VisualElement>("HiveBase").Remove(glob);

            //if (activePulse != null)
            //    StopCoroutine(activePulse);
            //activePulse = StartCoroutine(Pulse());
        }

        globs.Clear();
        animsRunning = false;
    }

    public void CheckCancelAnim()
    {
        if (!animsRunning)
            return;

        StopAllCoroutines();
        foreach (TemplateContainer glob in globs)
            if (document.rootVisualElement.Q<VisualElement>("HiveBase").Contains(glob))
            document.rootVisualElement.Q<VisualElement>("HiveBase").Remove(glob);
        globs.Clear();
    }

    private IEnumerator ToPoint(TemplateContainer glob, float top, float left, float t, bool destroyOnEnd)
    {
        while (Mathf.Abs(glob.resolvedStyle.left - left) >= 10 || Mathf.Abs(glob.resolvedStyle.top - top) >= 10)
        {
            glob.style.left = Mathf.Lerp(glob.resolvedStyle.left, left, t);
            glob.style.top = Mathf.Lerp(glob.resolvedStyle.top, top, t);
            yield return new WaitForSeconds(0.05f);
        }
    }

    //private IEnumerator Pulse()
    //{
    //    CustomVisualElement button = document.rootVisualElement.Q<CustomVisualElement>("MarketButton");
    //    while (152 - button.resolvedStyle.width >= 5)
    //    {
    //        button.style.width = Mathf.Lerp(button.resolvedStyle.width, 152, 0.5f);
    //        button.style.height = Mathf.Lerp(button.resolvedStyle.height, 152, 0.5f);
    //        yield return new WaitForSeconds(0.01f);
    //    }
    //    while (button.resolvedStyle.width >= 133)
    //    {
    //        button.style.width = Mathf.Lerp(button.resolvedStyle.width, 128, 0.5f);
    //        button.style.height = Mathf.Lerp(button.resolvedStyle.height, 128, 0.5f);
    //        yield return new WaitForSeconds(0.01f);
    //    }
    //    button.style.width = 128;
    //    button.style.height = 128;
    //}

    public void GetTileRadius(int x, int y)
    {
        tileRadius.Clear();
        switch (queen.radiusType)
        {
            case "Square":
                RadiusLoopHelper(2, 2, 4, 4, x, y);
                break;

            case "Long":
                int startMod = (rotation == 0 || rotation == 270)? 4 : 1;

                if (rotation == 0 || rotation == 180)
                    RadiusLoopHelper(startMod, 1, 5, 2, x, y);
                else
                    RadiusLoopHelper(1, startMod, 2, 5, x, y);

                break;

            case "L-Shaped":
                if (rotation == 0 || rotation == 90)
                    RadiusLoopHelper(1, 1, 2, 4, x, y);

                if (rotation == 90 || rotation == 180)
                    RadiusLoopHelper(1, 1, 4, 2, x, y);

                if (rotation == 180 || rotation == 270)
                    RadiusLoopHelper(1, 3, 2, 4, x, y);

                if (rotation == 270 || rotation == 0)
                    RadiusLoopHelper(3, 1, 4, 2, x, y);
                break;
        }
    }

    private void RadiusLoopHelper(int xStart, int yStart, int xMod, int yMod, int x, int y)
    {
        for (int i = x - xStart; i <= x - xStart + xMod; i++)
            for (int j = y - yStart; j <= y - yStart + yMod; j++)
                if (i >= 0 && j >= 0 && i <= 11 && j <= 15 && !tileRadius.Contains(map.tiles[i, j]))
                    tileRadius.Add(map.tiles[i, j]);
    }

    private void SplitNectar(float inputNectar)
    {
        //Apply the weights of each type of flower to the nectar being gained this turn
        foreach (FlowerType key in nectarValues.Keys.ToList())
            nectarValues[key] += personalNectarGains[key] / inputNectar;
    }

    private void SplitBuckfast(float inputNectar)
    {
        foreach (FlowerType key in buckfastValues.Keys.ToList())
            buckfastValues[key] += buckfastGains[key] / inputNectar;
    }

    public void CalcHoneyStats()
    {
        //set honeyType and honeyPurity to the type of honey that is most appundant from the available flowers this turn
        if (queen.species == "Buckfast")
        {
            honeyType = buckfastValues.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            honeyPurity = (buckfastValues[honeyType] / buckfastValues.Values.Sum()) + picky;
        }
        else
        {
            honeyType = nectarValues.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            if (queen.species == "Himalayan" && CheckHimalayanCondition())
                honeyPurity = 1;
            else
                honeyPurity = (nectarValues[honeyType] / nectarValues.Values.Sum()) + picky;
        }

        float roundedPurity = Mathf.Round(honeyPurity * 1000) / 10.0f;
        if (roundedPurity <= 60)
        {
            roundedPurity = 100 - roundedPurity;
            honeyType = FlowerType.Wildflower;
        }
        honeyTypeLabel.text = "Type:\n" + honeyType.ToString();
        if (honeyType != FlowerType.Wildflower)
            infoIcon.style.backgroundImage = hexMenu.allFlowerSprites[(int)honeyType - 2];
        else
            infoIcon.style.backgroundImage = hexMenu.allFlowerSprites[(int)honeyType - 1]; //TEMPORARY FIX FOR NO WILDFLOWER SPRITE
        honeyPurityLabel.text = "Purity:\n" + roundedPurity + "%";

        if (honeyType != FlowerType.Wildflower && roundedPurity >= 90)
            infoTint.style.unityBackgroundImageTintColor = new Color(0.29f, 0.83f, 0.15f);
        else if ((honeyType != FlowerType.Wildflower && roundedPurity >= 90) || honeyType == FlowerType.Wildflower)
            infoTint.style.unityBackgroundImageTintColor = new Color(0.75f, 0.83f, 0.15f);
        else
            infoTint.style.unityBackgroundImageTintColor = new Color(0.55f, 0.29f, 0.23f);
    }

    public void UpdateMeters()
    {
        if (combMeter == null)
            return;

        combMeter.style.top = 210 - (comb / (combSizeCap * 5) * 210);
        if (hiveEfficency != 0)
            nectarMeter.style.top = 210 - (nectarGain / maxHoneyProduction * 210);
        else
            nectarMeter.style.top = 210;

        float part1 = (honey / (comb * storagePerComb)) * 210; //210 is the style.top of the visual element at 0%
        honeyMeter.style.top = 210 - part1; //So to calc meter height, I get the percent of honey to storage, multiply 210, then subtract the result from 210 to invert it.
        UpdateMeterLabels();
    }

    private void UpdateMeterLabels()
    {
        if (hiveEfficency != 0)
            nectarHover.Q<Label>("Percent").text = (Mathf.Round(nectarGain / maxHoneyProduction * 100 * 10) / 10.0f).ToString() + "%";
        else
            nectarHover.Q<Label>("Percent").text = "0%";
        nectarHover.Q<Label>("Flat").text = (Mathf.Round(nectarGain * 10) / 10.0f) + " lbs.";

        if (comb * storagePerComb != 0)
            honeyHover.Q<Label>("Percent").text = (Mathf.Round((honey / (comb * storagePerComb)) * 100 * 10) / 10.0f).ToString() + "%";
        else
            honeyHover.Q<Label>("Percent").text = "0%";
        honeyHover.Q<Label>("Flat").text = (Mathf.Round(honey * 10) / 10.0f) + " lbs.";

        combHover.Q<Label>("Percent").text = (Mathf.Round(comb / (combSizeCap * 5) * 100) * 10 / 10.0f).ToString() + "%";
        combHover.Q<Label>("Flat").text = (Mathf.Round(comb * storagePerComb * 10) / 10.0f) + " lbs.";
    }

    public IEnumerator Populate(QueenBee q)
    {
        if (q == null)
        {
            if (queenHex != null)
            {
                queenHex.style.backgroundImage = deadSprite;
                queenClick.UnregisterCallback<PointerMoveEvent>(OnQueenMove);
            }
            yield break;
        }

        queen.transferComplete = false;
        StartCoroutine(queen.TransferStats(q));
        Destroy(q.gameObject);
        empty = false;
        queenHex.style.backgroundImage = queenSprite;
        queenHex.style.unityBackgroundImageTintColor = new Color(1, 1, 1, 1);

        yield return new WaitWhile(() => !queen.transferComplete);

        while (conditions.Count > 0)
            CureCondition(conditions[0]);

        HideHiveRadius();
        GetTileRadius(x, y);
        DisplayHiveRadius();
        EnableHarvestButtons();
        SetStatsModifiers();
        if (queen.quirks.Contains("Calm"))
            AddCondition("Calm");
        CheckForCarniolan();

        if (queen.species == "Killer")
            foreach (Hive h in player.hives)
                if (h != this)
                    h.AddCondition("Killer");

        foreach (Hive h in player.hives)
            if (h != this && h.queen.species == "Killer")
                AddCondition("Killer");

        CalcEfficiency();

        if (activePopup != null)
        {
            activePopup.RemoveFromHierarchy();
            activePopup = null;
        }

        if (!conditions.Contains("Indulged") && map.GetAdjacentFlowers(queen.favorite, x, y).Count > 0)
            AddCondition("Indulged");
        else if (conditions.Contains("Indulged") && map.GetAdjacentFlowers(queen.favorite, x, y).Count <= 0)
            CureCondition("Indulged");

        UpdateMeters();
    }

    //Load queen from save doesn't require transfering stats
    public void LoadPopulate()
    {
        if (queen.nullQueen == false)
        {
            empty = false;
            //Condition = "Healthy";
        }
        //else
        //    Condition = "Dead";
        queenHex.style.backgroundImage = queenSprite;
        queenHex.style.unityBackgroundImageTintColor = new Color(1, 1, 1, 1);
        game = GameObject.Find("GameController").GetComponent<GameController>();
        tracker = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();

        SetStatsModifiers();
        if (queen.quirks.Contains("Calm"))
            AddCondition("Calm");
        CalcEfficiency();

        CalcHoneyStats();
        UpdateMeters();
        EnableHarvestButtons();

        if (activePopup != null)
        {
            activePopup.RemoveFromHierarchy();
            activePopup = null;
        }
    }

    public void AddCondition(string con)
    {
        conditions.Add(con);
        StressLevel += conditionValues[con];

        if (randConditions.Contains(con))
            randConditions.Remove(con);
    }

    private void TryAddCondition()
    {
        if (game.Season == "winter")
        {
            if (honey <= (population / 2000) / rugged)
                AddCondition("Starving");

            if ((!hasInsulation || population <= popCap / (Size * 2)))
                AddCondition("Freezing");
        }
        else if (Random.Range(0, 20) <= 4 && game.year != 1)
        {
            AddCondition("Mites");
        }

        int aggrevatedChance = Random.Range(0, 5 + (int)docile);
        if (!conditions.Contains("Aggrevated") && aggrevatedChance == 0)
            AddCondition("Aggrevated");
    }

    public void CureCondition(string con)
    {
        conditions.Remove(con);
        StressLevel -= conditionValues[con];

        if (baseRandConditions.Contains(con))
            randConditions.Add(con);

        foreach (Tile t in tileRadius)
        {
            if (t.HasHive && t.hive.queen.species == "Japanese" && Random.Range(0, 10) == 0)
                queen.japaneseInherited = true;
        }
    }

    private void CureRandomNegativeCondition()
    {
        List<string> possible = new List<string>();
        foreach (string con in conditions)
            if (conditionValues[con] < 0)
                possible.Add(con);
        CureCondition(possible[Random.Range(0, possible.Count)]);
    }

    public void AddSugarWater()
    {
        //collection *= 1.5f;
        addedNectar += 250;
        hasSugar = true;
    }

    private void OnDestroy()
    {
        if (activePopup != null)
        {
            activePopup.RemoveFromHierarchy();
            activePopup = null;
        }
    }

    #region UI

    void OnMouseDown()
    {
        if (!placed)
            return;

        document.GetComponent<AudioSource>().Play();

        player.OpenHiveUI(template, hiveUI, this);
        SetUpTemplate();
    }

    public void childOnMouseDown()
    {
        document.GetComponent<AudioSource>().Play();
        player.OpenHiveUI(template, hiveUI, this);
        SetUpTemplate();
    }

    private void DisableHarvestButtons()
    {
        smallHarvest.RemoveManipulator(smallHarvestClick);
        mediumHarvest.RemoveManipulator(mediumHarvestClick);
        largeHarvest.RemoveManipulator(largeHarvestClick);

        smallHarvest.UnregisterCallback<PointerEnterEvent>(harvestEnterCallback);
        mediumHarvest.UnregisterCallback<PointerEnterEvent>(harvestEnterCallback);
        largeHarvest.UnregisterCallback<PointerEnterEvent>(harvestEnterCallback);

        smallHarvest.UnregisterCallback<PointerLeaveEvent>(harvestLeaveCallback);
        mediumHarvest.UnregisterCallback<PointerLeaveEvent>(harvestLeaveCallback);
        largeHarvest.UnregisterCallback<PointerLeaveEvent>(harvestLeaveCallback);

        smallHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
        mediumHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
        largeHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;

        harvestActive = false;
    }

    public void EnableHarvestButtons()
    {
        smallHarvest.AddManipulator(smallHarvestClick);
        mediumHarvest.AddManipulator(mediumHarvestClick);
        largeHarvest.AddManipulator(largeHarvestClick);

        smallHarvest.RegisterCallback<PointerEnterEvent>(harvestEnterCallback);
        mediumHarvest.RegisterCallback<PointerEnterEvent>(harvestEnterCallback);
        largeHarvest.RegisterCallback<PointerEnterEvent>(harvestEnterCallback);

        smallHarvest.RegisterCallback<PointerLeaveEvent>(harvestLeaveCallback);
        mediumHarvest.RegisterCallback<PointerLeaveEvent>(harvestLeaveCallback);
        largeHarvest.RegisterCallback<PointerLeaveEvent>(harvestLeaveCallback);

        smallHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
        mediumHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
        largeHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;

        harvestActive = true;
    }

    public void SetUpTemplate()
    {
        if (harvestDict.Keys.Count == 0)
        {
            infoHoverTint = template.Q<VisualElement>("InfoHoverTint");
            infoHover = template.Q<CustomVisualElement>("FlowerInfo");
            infoTint = template.Q<VisualElement>("InfoTint");
            infoIcon = template.Q<VisualElement>("InfoIcon");
            honeyPurityLabel = template.Q<Label>("HoneyPurity");
            honeyTypeLabel = template.Q<Label>("HoneyType");

            infoEnterCallback = new EventCallback<PointerEnterEvent>(InfoHoverEnter);
            infoLeaveCallback = new EventCallback<PointerLeaveEvent>(InfoHoverLeave);

            infoHover.RegisterCallback<PointerEnterEvent>(InfoHoverEnter);
            infoHover.RegisterCallback<PointerLeaveEvent>(InfoHoverLeave);


            smallHarvest = template.Q<VisualElement>("SmallClick");
            mediumHarvest = template.Q<VisualElement>("MediumClick");
            largeHarvest = template.Q<VisualElement>("LargeClick");

            harvestEnterCallback = new EventCallback<PointerEnterEvent>(HoverHarvestEnter);
            harvestLeaveCallback = new EventCallback<PointerLeaveEvent>(HoverHarvestLeave);
            smallHarvestClick = new Clickable(e => SelectHarvest(smallHarvest));
            mediumHarvestClick = new Clickable(e => SelectHarvest(mediumHarvest));
            largeHarvestClick = new Clickable(e => SelectHarvest(largeHarvest));

            smallHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
            mediumHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
            largeHarvest.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;

            smallHarvest.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));
            mediumHarvest.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));
            largeHarvest.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));

            openWindow = new Clickable(e => OpenStressWindow());
            closeWindow = new Clickable(e => CloseStressWindow());
            stressClick = template.Q<CustomVisualElement>("StressClick");
            stressClick.AddManipulator(openWindow);

            combMeter = template.Q<VisualElement>("CombMeterElement");
            nectarMeter = template.Q<VisualElement>("NectarMeterElement");
            honeyMeter = template.Q<VisualElement>("HoneyMeterElement");
            queenHex = template.Q<VisualElement>("QueenHex");
            queenClick = template.Q<CustomVisualElement>("QueenClick");
            queenClick.AddManipulator(assignQueen);
            queenClick.RegisterCallback<PointerDownEvent>(e => BeeStatsReference(e));
            queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
            queenMoveCallback = new EventCallback<PointerMoveEvent>(OnQueenMove);
            queenClick.RegisterCallback(queenMoveCallback);
            queenClick.RegisterCallback(queenExitCallback);

            harvestDict.Add(smallHarvest, false);
            harvestDict.Add(mediumHarvest, false);
            harvestDict.Add(largeHarvest, false);

            exitCallback = new EventCallback<PointerLeaveEvent>(OnExit);
            moveCallback = new EventCallback<PointerMoveEvent>(OnMove);

            nectarHover = template.Q<CustomVisualElement>("NectarHover");
            nectarHover.RegisterCallback(moveCallback);
            nectarHover.RegisterCallback(exitCallback);
            nectarHover.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));

            honeyHover = template.Q<CustomVisualElement>("HoneyHover");
            honeyHover.RegisterCallback(moveCallback);
            honeyHover.RegisterCallback(exitCallback);
            honeyHover.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));

            combHover = template.Q<CustomVisualElement>("CombHover");
            combHover.RegisterCallback(moveCallback);
            combHover.RegisterCallback(exitCallback);
            combHover.RegisterCallback<PointerDownEvent>(e => HoneyCycleReference(e));

            template.Q<VisualElement>("FlowerInfo").RegisterCallback<PointerDownEvent>(e => FlowersReference(e));

            exit = template.Q<VisualElement>("Close");
            exit.AddManipulator(new Clickable(() => player.CloseHiveUI(this)));

            if (queen == null)
                queen = GetComponent<QueenBee>();
            UpdateMeters();
        }
    }

    public void DisplayHiveRadius()
    {
        foreach (Tile t in tileRadius)
        {
            t.lastMaterial = t.GetComponent<MeshRenderer>().material;
            t.GetComponent<MeshRenderer>().material = selectedMaterial;
        }
    }

    public void HideHiveRadius()
    {
        foreach (Tile t in tileRadius)
            if (t.lastMaterial != null)
                t.GetComponent<MeshRenderer>().material = t.lastMaterial;
    }

    private void InfoHoverEnter(PointerEnterEvent e)
    {
        honeyPurityLabel.style.visibility = Visibility.Visible;
        honeyTypeLabel.style.visibility = Visibility.Visible;
        infoHoverTint.style.visibility = Visibility.Visible;
    }

    private void InfoHoverLeave(PointerLeaveEvent e)
    {
        honeyPurityLabel.style.visibility = Visibility.Hidden;
        honeyTypeLabel.style.visibility = Visibility.Hidden;
        infoHoverTint.style.visibility = Visibility.Hidden;
    }

    private void HoverHarvestEnter(PointerEnterEvent e)
    {
        VisualElement target = e.target as VisualElement;
        target.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
    }

    private void HoverHarvestLeave(PointerLeaveEvent e)
    {
        VisualElement target = e.target as VisualElement;
        target.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
    }

    private void SelectHarvest(VisualElement clickedElement)
    {
        document.GetComponent<AudioSource>().Play();
        StartCoroutine(ClickResponse(clickedElement));
        float harvestPercentage = 0;
        if (clickedElement == smallHarvest)
            harvestPercentage = 0.33f;
        else if (clickedElement == mediumHarvest)
            harvestPercentage = 0.66f;
        else if (clickedElement == largeHarvest)
            harvestPercentage = 1f;
        Harvest(harvestPercentage);
    }

    private IEnumerator ClickResponse(VisualElement clickedElement)
    {
        clickedElement.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkerTint;
        yield return new WaitForSeconds(0.1f);
        clickedElement.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
    }

    private void OpenStressWindow()
    {
        if (activePopup != null)
        {
            activePopup.RemoveFromHierarchy();
            activePopup = null;
        }

        stressContainer = StressPanel.Instantiate();
        stressContainer.style.width = 423;
        stressContainer.style.height = 550;
        stressContainer.style.position = Position.Absolute;
        stressContainer.style.left = 200;

        //Highlight current stress level
        stressContainer.Q<Label>("StressLabel").text = "Stress: " + stressLevel;
        if (stressLevel > 0)
        {
            for (int i = stressLevel; i > 0; i--)
            {
                VisualElement numVE = stressContainer.Q<VisualElement>(i.ToString());
                numVE.Q<Label>("num").style.color = Color.white;
                numVE.Q<Label>("desc").style.color = Color.white;
            }
        }
        else if (stressLevel <= -1)
        {
            VisualElement numVE = stressContainer.Q<VisualElement>("-1");
            numVE.Q<Label>("num").style.color = Color.white;
            numVE.Q<Label>("desc").style.color = Color.white;
        }

        //List afflictions
        for (int i = 0; i < conditions.Count; i++)
        {
            Label label = new Label();
            if (conditionValues[conditions[i]] < 0)
                label.text = conditions[i].ToString() + " " + conditionValues[conditions[i]];
            else
                label.text = conditions[i].ToString() + " +" + conditionValues[conditions[i]];

            if (i < 3)
                stressContainer.Q<VisualElement>("Afflictions1").Add(label);
            else
                stressContainer.Q<VisualElement>("Afflictions2").Add(label);
        }

        document.rootVisualElement.Q<VisualElement>("Right").Add(stressContainer);
        stressClick.RemoveManipulator(openWindow);
        stressClick.AddManipulator(closeWindow);
    }

    public void CloseStressWindow()
    {
        if (stressContainer == null)
            return;
        stressClick.RemoveManipulator(closeWindow);
        stressClick.AddManipulator(openWindow);
        document.rootVisualElement.Q<VisualElement>("Right").Remove(stressContainer);
        stressContainer = null;
    }    

    private void OpenQueenTab()
    {
        document.GetComponent<AudioSource>().Play();
        queenClick.RemoveManipulator(assignQueen);
        selectingQueen = true;
        queenClick.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
        if (player.SelectedItem != null && player.SelectedItem.tag == "Bee")
            hexMenu.SelectHive(player.SelectedItem, queenSprite, 0, this);
        else
            hexMenu.OpenTab(0, hexMenu.open1, true, this);
    }

    private void OnMove(PointerMoveEvent e)
    {
        CurrentHover = null;
        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        if (target.ContainsPoint(e.localPosition))
            CurrentHover = target;
        else
            CurrentHover = null;
    }

    private void OnExit(PointerLeaveEvent e)
    {
        CurrentHover = null;
    }

    private void OnQueenMove(PointerMoveEvent e)
    {
        if (queen.nullQueen)
            return;

        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        if (target.ContainsPoint(e.localPosition))
        {
            if (hoverTemplate == null)
            {
                hoverTemplate = queenUI.Instantiate();
                document.rootVisualElement.Q("Base").Add(hoverTemplate);
                VisualElement popup = hoverTemplate.Q<VisualElement>("Popup");

                //Resolved style is NaN until updated
                popup.RegisterCallback((GeometryChangedEvent evt) => {
                    hoverTemplate.style.position = Position.Absolute;
                    hoverTemplate.style.left = e.position.x - popup.resolvedStyle.width;
                    hoverTemplate.style.top = Screen.height - e.position.y - popup.resolvedStyle.height / 1.5f;
                });

                popup.Q<VisualElement>("Icon").style.backgroundImage = queenHex.style.backgroundImage;
                popup.Q<Label>("Species").text = "Species: " + queen.species;
                popup.Q<Label>("Age").text = "Favorite Flower: " + queen.favorite.ToString();
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
                document.rootVisualElement.Q("Base").Remove(hoverTemplate);
                hoverTemplate = null;
            }
        }
    }

    private void OnQueenExit(PointerLeaveEvent e)
    {
        if (hoverTemplate != null)
        {
            document.rootVisualElement.Q("Base").Remove(hoverTemplate);
            hoverTemplate = null;
        }
    }

    private void OnAfflictionHover(PointerEnterEvent e)
    {
        activePopup.RegisterCallback<PointerLeaveEvent>(OnAfflictionExit);
        tooltip = afflictionToolTipUI.Instantiate();
        tooltip.style.position = Position.Absolute;
        tooltip.style.left = e.position.x;// - tooltip.resolvedStyle.width;
        tooltip.style.top = e.position.y;//Screen.height - e.position.y;// - tooltip.resolvedStyle.height / 1.5f;
        tooltip.pickingMode = PickingMode.Ignore;
        //tooltip.Q<Label>("Affliction").text = Condition;
        tooltip.Q<VisualElement>("Icon").style.backgroundImage = currentIcon;
        document.rootVisualElement.Q<VisualElement>("Base").Add(tooltip);
    }

    private void OnAfflictionExit(PointerLeaveEvent e)
    {
        if (tooltip != null)
        {
            document.rootVisualElement.Q("Base").Remove(tooltip);
            tooltip = null;
            activePopup.RegisterCallback<PointerEnterEvent>(OnAfflictionHover);
        }

    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(1))
            document.GetComponent<Glossary>().OpenGlossary("Hive");
    }

    private void HoneyCycleReference(PointerDownEvent e)
    {
        if (e.button == 1)
            document.GetComponent<Glossary>().OpenGlossary("HoneyCycle");
    }

    private void FlowersReference(PointerDownEvent e)
    {
        if (e.button == 1)
            document.GetComponent<Glossary>().OpenGlossary("Flowers");
    }

    private void BeeStatsReference(PointerDownEvent e)
    {
        if (e.button == 1)
            document.GetComponent<Glossary>().OpenGlossary("BeeStats");
    }
    #endregion
}