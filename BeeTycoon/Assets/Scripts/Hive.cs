using System.Collections;
using System.Collections.Generic;
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
}

public class Hive : MonoBehaviour
{
    private UIDocument document;
    private MapLoader map;
    private PlayerController player;
    private GameController game;

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
    private Texture2D currentIcon;

    public TemplateContainer template;
    private TemplateContainer hoverTemplate;
    private TemplateContainer tooltip;
    public bool empty = true;
    private bool placed;

    public int x;
    public int y;
    public Tile hiveTile;

    private int size = 1;
    private float population = 5000;
    private float popCap = 20000; //what population the hive can currently house
    private float popSizeCap = 20000; //how much each level of size changes the popCap
    private float comb = 0;
    private int combCap = 8; //how much honey the hive can currently store
    private int combSizeCap = 8; //how much each level of size changes the honeyCap
    private float nectar;
    private float honey;

    private float addedNectar = 0;

    private float storage = 0; //how much storage the hive has
    private float storagePerComb = 6; //how much each level of size changes the storage - lbs.

    private float birthRate = 2500;

    private float hiveEfficency; //Efficiency is a multiplier to all the hive's actions and is calculated by the population / total population * size of the hive

    //stats 0-1f
    private float production = 9.6f; // was 400
    private float construction = 1f; //was 0.5f
    //private float collection = 1f; //was 400 //Not currently in use. Nectar is now caclulated through flowers
    private float resilience = 1;
    private float aggressivness = 1;

    private QueenBee queen;

    private Dictionary<FlowerType, float> flowerValues = new Dictionary<FlowerType, float>();
    private Dictionary<FlowerType, float> nectarValues = new Dictionary<FlowerType, float>();
    private float totalFlowerWeight = 0;
    private FlowerType honeyType = FlowerType.Empty;
    private float honeyPurity = 0;

    //UI
    private VisualElement smallHarvest;
    private VisualElement mediumHarvest;
    private VisualElement largeHarvest;
    private ProgressBar combMeter;
    private ProgressBar nectarMeter;
    private ProgressBar honeyMeter;
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
    private CustomVisualElement currentHover;
    private StyleColor darkTint;
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
    private TemplateContainer activePopup;

    public int Size
    {
        get { return size; }
        set
        {
            size += value;
            popCap = popSizeCap * size;
            combCap = combSizeCap * size;
            UpdateMeters();
        }
    }

    public int Frames
    {
        get { return combSizeCap; }
        set
        {
            combSizeCap++;
            combCap += size;
        }
    }

    public bool Placed
    {
        get { return placed; }
        set
        {
            placed = value;
            if (value)
                player.OpenHiveUI(template, hiveUI, this);
        }
    }

    public string Condition
    {
        get { return condition; }
        set
        {
            if (activePopup != null)
            {
                document.rootVisualElement.Q<VisualElement>("Base").Remove(activePopup);
                activePopup = null;
            }

            condition = value;
            activePopup = afflictionPopupUI.Instantiate();

            switch (value)
            {
                case "Mites":
                    hiveEfficency /= 2;
                    currentIcon = remedyIcons[0];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[0];
                    break;
                case "Mice":
                    construction /= 2;
                    currentIcon = remedyIcons[1];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[1];
                    break;
                case "Glued":
                    canBeOpened = false;
                    currentIcon = remedyIcons[2];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[2];
                    break;
                case "Freezing":
                    construction /= 2;
                    currentIcon = remedyIcons[3];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[2];
                    break;
                case "Starving":
                    construction /= 2;
                    currentIcon = remedyIcons[4];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[3];
                    break;
                case "Aggrevated":
                    canBeOpened = false;
                    currentIcon = remedyIcons[5];
                    activePopup.Q<VisualElement>("Icon").style.backgroundImage = afflictionIcons[1];
                    break;
                case "Healthy":
                    activePopup = null;
                    break;
            }
            Debug.Log(condition);

            if (activePopup != null)
            {
                AdjustPopupTransform();
                activePopup.style.position = Position.Absolute;
                activePopup.style.flexGrow = 0;
                document.rootVisualElement.Q<VisualElement>("Base").Add(activePopup);
                activePopup.RegisterCallback<PointerEnterEvent>(OnAfflictionHover);
            }
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

    void Start()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        game = GameObject.Find("GameController").GetComponent<GameController>();
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        queen = GetComponent<QueenBee>();

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            flowerValues.Add(fType, 0);
            nectarValues.Add(fType, 0);
        }

        Color darkTintColor = Color.black;
        darkTintColor.a = 0.6f;
        darkTint = new StyleColor(darkTintColor);
        Color lightTintColor = Color.black;
        lightTintColor.a = 0.0f;
        lightTint = new StyleColor(lightTintColor);

        assignQueen = new Clickable(OpenQueenTab);

        hiveEfficency = (population / popCap) * size;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectingQueen)
            {
                CloseQueenSelection();
            }
            else if (isOpen && player.SelectedItem == null)
                player.CloseHiveUI(this);
        }

        if (activePopup != null)
        {
            AdjustPopupTransform();
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
            player.CloseTab();
        }
    }

    public void UpdateHive()
    {
        if (empty)
            return;

        //Mice eat through comb every turn
        if (Condition == "Mice")
        {
            comb -= 0.5f;
            if (comb <= 4)
                comb = 4;
        }

        GetFlowerRatios();

        float possibleComb = construction * queen.constructionMult * hiveEfficency;
        if (possibleComb + comb > combCap)
            possibleComb = combCap - comb;
        comb += possibleComb;
        storage = storagePerComb * comb;

        float possibleHoney = production * queen.productionMult * hiveEfficency;
        if (possibleHoney > nectar)
            possibleHoney = nectar;
        honey += possibleHoney;
        nectar -= possibleHoney;

        float nectarGain = addedNectar + map.nectarGains.Values.Sum() *.006f; //scale it down to lbs
        //Debug.Log(queen.collectionMult);
        float possibleNectar = nectarGain * queen.collectionMult * hiveEfficency; // * Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f)
        if (possibleNectar + nectar + honey > storage)
            possibleNectar = storage - (nectar + honey);
        nectar += possibleNectar;

        if (possibleNectar > 0)
            SplitNectar(possibleNectar);

        float possiblePop = birthRate;
        if (possiblePop + population > popCap)
            possiblePop = popCap - population;
        population += possiblePop;

        hiveEfficency = (population / popCap) * size;

        //Debug.Log("Population: " + population);
        //Debug.Log("Comb: " + comb);
        //Debug.Log("Nectar: " + nectar);
        //Debug.Log("Honey: " + honey);
        //Debug.Log("Storage: " + storage);
        //Debug.Log("Efficiency: " + hiveEfficency);

        //reset flowerValues
        foreach (FlowerType key in flowerValues.Keys.ToList())
            flowerValues[key] = 0f;
        totalFlowerWeight = 0f;

        CalcHoneyStats();
        //Harvest();
        if (template != null)
            UpdateMeters();

        TryAddCondition();
    }

    private void Harvest(float percent)
    {
        //if (noHarvest.value)
        //    return;

        //if (harvestDict[smallHarvest])
        //    harvestPercentage = 0.33f;
        //else if (harvestDict[mediumHarvest])
        //    harvestPercentage = 0.66f;
        //else if (harvestDict[largeHarvest])
        //    harvestPercentage = 1;
        Debug.Log(honeyType + " " + honey);
        if (honeyType != FlowerType.Wildflower)
        {
            float amount = percent * honey;
            player.inventory[honeyType][0] += amount;
            honey -= amount;
            Debug.Log(amount);

            if (honeyPurity >= .9f)
                player.inventory[honeyType][3] += amount;
            else if (honeyPurity > .7f)
                player.inventory[honeyType][2] += amount;
            else
                player.inventory[honeyType][1] += amount;
        }
        else
        {
            float amount = percent * honey;
            player.inventory[honeyType][0] += amount;
            honey -= amount;
            Debug.Log(amount);
            player.inventory[honeyType][2] += amount;
        }

        UpdateMeters();
    }

    private void GetFlowerRatios()
    {
        for (int i = 0; i < map.mapWidth; i++)
        {
            for (int j = 0; j < map.mapHeight; j++)
            {
                int distance = Mathf.CeilToInt((Mathf.Abs(x - i) + Mathf.Abs(y - j)) / 2);
                if (distance == 0)
                    distance = 1;
                switch (map.tiles[i,j].Flower)
                {
                    case FlowerType.Clover:
                        flowerValues[FlowerType.Clover] += 1f / distance;
                        break;
                    case FlowerType.Alfalfa:
                        flowerValues[FlowerType.Alfalfa] += 1f / distance;
                        break;
                    //case FlowerType.Blossom:
                    //    flowerValues[FlowerType.Blossom] += 1f / distance;
                        //break;
                    case FlowerType.Buckwheat:
                        flowerValues[FlowerType.Buckwheat] += 1f / distance;
                        break;
                    case FlowerType.Empty:
                        break;
                    default:
                        break;
                }
            }
        }
        totalFlowerWeight = flowerValues.Values.Sum();

        //foreach (KeyValuePair<FlowerType, float> kvp in flowerValues)
        //    Debug.Log(kvp.Key + ": " + kvp.Value);
    }

    private void SplitNectar(float inputNectar)
    {
        //Apply the weights of each type of flower to the nectar being gained this turn
        foreach (FlowerType key in nectarValues.Keys.ToList())
            nectarValues[key] += inputNectar * (flowerValues[key] / totalFlowerWeight);
    }

    private void CalcHoneyStats()
    {
        //set honeyType and honeyPurity to the type of honey that is most appundant from the available flowers this turn
        if (honey > 0)
        {
            honeyType = nectarValues.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            honeyPurity = nectarValues[honeyType] / nectarValues.Values.Sum();
            //Debug.Log(honeyType);
            //Debug.Log(honeyPurity);
        }
    }

    private void UpdateMeters()
    {
        combMeter.value = (comb / combCap * 100) + 8;
        float nectarGain = addedNectar + map.nectarGains.Values.Sum() * .006f; //scale it down to lbs.
        if (production * queen.productionMult * hiveEfficency != 0)
            nectarMeter.value = (nectarGain * queen.collectionMult * hiveEfficency / (production * queen.productionMult * hiveEfficency) * 100) + 8;// * Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f) 
        else
            nectarMeter.value = 8;
        honeyMeter.value = (honey / (combCap * storagePerComb) * 100) + 8;
        UpdateMeterLabels();
    }

    private void UpdateMeterLabels()
    {
        float nectarGain = addedNectar + map.nectarGains.Values.Sum() * .006f;
        if (production * queen.productionMult * hiveEfficency != 0)
            nectarHover.Q<Label>("Percent").text = (Mathf.Round(nectarGain * queen.collectionMult * hiveEfficency / (production * queen.productionMult * hiveEfficency) * 100 * 10) / 10.0f).ToString() + "%"; //* Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f
        else
            nectarHover.Q<Label>("Percent").text = "0%";
        nectarHover.Q<Label>("Flat").text = (Mathf.Round(nectarGain * queen.collectionMult * hiveEfficency  * 10) / 10.0f) + " lbs."; //*Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f)

        honeyHover.Q<Label>("Percent").text = (Mathf.Round(honey / (combCap * storagePerComb) * 100 * 10) / 10.0f).ToString() + "%";
        honeyHover.Q<Label>("Flat").text = (Mathf.Round(production * queen.productionMult * hiveEfficency * 10) / 10.0f) + " lbs.";

        combHover.Q<Label>("Percent").text = (Mathf.Round(comb / combCap * 100) * 10 / 10.0f).ToString() + "%";
        combHover.Q<Label>("Flat").text = (Mathf.Round(construction * queen.constructionMult * hiveEfficency * 10) / 10.0f) + " lbs.";
    }

    public void Populate(QueenBee q, Texture2D sprite = null)
    {
        if (q == null)
            return;

        StartCoroutine(queen.TransferStats(q));
        Destroy(q.gameObject);
        empty = false;

        if (sprite != null)
            queenHex.style.backgroundImage = sprite;
        queenHex.style.unityBackgroundImageTintColor = new Color(1, 1, 1, 1);
    }

    private void TryAddCondition()
    {
        if (Condition == "Healthy")
        {
            int rand = Random.Range(0, 20);
            if (!hasRepellant)
            {
                if (rand <= 1)
                    Condition = "Mites";
            }
            if (!hasReducer)
            {
                if (rand <= 3)
                    Condition = "Mice";
            }
            else if (rand <= 5 + aggressivness * queen.aggressivnessMult)
            {
                Condition = "Aggrevated";
            }
            else if (rand <= 7 + production * queen.productionMult)
            {
                Condition = "Glued";
            }
            else if (game.Season == "Winter" && honey <= population / (16 - resilience * queen.resilienceMult))
            {
                Condition = "Starving";
            }
            else if (game.Season == "Winter" && !hasInsulation && population <= popCap / (Size * 2))
            {
                Condition = "Freezing";
            }
            else
            {
                Condition = "Healthy";
            }
        }
    }

    public void CureCondition()
    {
        switch (Condition)
        {
            case "Mites":
                hiveEfficency *= 2;
                break;
            case "Mice":
                break;
            case "Glued":
                canBeOpened = true;
                break;
            case "Aggrevated":
                canBeOpened = true;
                break;
        }

        Condition = "Healthy";
    }

    public void AddSugarWater()
    {
        //collection *= 1.5f;
        addedNectar += 250;
        hasSugar = true;
    }

    #region UI

    void OnMouseDown()
    {
        if (!placed)
            return;

        player.OpenHiveUI(template, hiveUI, this);
        SetUpTemplate();
    }

    public void childOnMouseDown()
    {
        player.OpenHiveUI(template, hiveUI, this);
        SetUpTemplate();
    }

    public void SetUpTemplate()
    {
        if (harvestDict.Keys.Count == 0)
        {
            //noHarvest = template.Q<Toggle>();
            //noHarvest.RegisterValueChangedCallback(OnHarvestToggled);

            smallHarvest = template.Q<VisualElement>("SmallClick");
            mediumHarvest = template.Q<VisualElement>("MediumClick");
            largeHarvest = template.Q<VisualElement>("LargeClick");

            //smallHarvest.AddManipulator(new Clickable(e => SelectHarvest(smallHarvest)));
            //mediumHarvest.AddManipulator(new Clickable(e => SelectHarvest(mediumHarvest)));
            //largeHarvest.AddManipulator(new Clickable(e => SelectHarvest(largeHarvest)));
            smallHarvest.AddManipulator(new Clickable(e => SelectHarvest(smallHarvest)));
            mediumHarvest.AddManipulator(new Clickable(e => SelectHarvest(mediumHarvest)));
            largeHarvest.AddManipulator(new Clickable(e => SelectHarvest(largeHarvest)));

            combMeter = template.Q<ProgressBar>("CombBar");
            nectarMeter = template.Q<ProgressBar>("NectarBar");
            honeyMeter = template.Q<ProgressBar>("HoneyBar");

            queenHex = template.Q<VisualElement>("QueenHex");
            queenClick = template.Q<CustomVisualElement>("QueenClick");
            queenClick.AddManipulator(assignQueen);
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

            honeyHover = template.Q<CustomVisualElement>("HoneyHover");
            honeyHover.RegisterCallback(moveCallback);
            honeyHover.RegisterCallback(exitCallback);

            combHover = template.Q<CustomVisualElement>("CombHover");
            combHover.RegisterCallback(moveCallback);
            combHover.RegisterCallback(exitCallback);

            exit = template.Q<VisualElement>("Close");
            exit.AddManipulator(new Clickable(() => player.CloseHiveUI(this)));

            UpdateMeterLabels();
        }
    }

    private void SelectHarvest(VisualElement clickedElement)
    {
        StartCoroutine(ClickResponse(clickedElement));
        float harvestPercentage = 0;
        if (clickedElement == smallHarvest)
            harvestPercentage = 0.25f;
        else if (clickedElement == mediumHarvest)
            harvestPercentage = 0.50f;
        else if (clickedElement == largeHarvest)
            harvestPercentage = 0.75f;
        Harvest(harvestPercentage);
    }

    private IEnumerator ClickResponse(VisualElement clickedElement)
    {
        clickedElement.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
        yield return new WaitForSeconds(0.1f);
        clickedElement.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
    }

    //private void SelectHarvest(VisualElement clickedElement)
    //{
    //    if (harvestDict[clickedElement] == false && !noHarvest.value)
    //    {
    //        Dictionary<VisualElement, bool> temp = new Dictionary<VisualElement, bool>();
    //        temp.Add(smallHarvest, harvestDict[smallHarvest]);
    //        temp.Add(mediumHarvest, harvestDict[mediumHarvest]);
    //        temp.Add(largeHarvest, harvestDict[largeHarvest]);

    //        foreach (KeyValuePair<VisualElement, bool> kvp in temp)
    //        {
    //            if (kvp.Key != clickedElement)
    //                harvestDict[kvp.Key] = false;
    //            else
    //                harvestDict[kvp.Key] = true;
    //        }
    //    }
    //    AdjustTints();
    //}

    //private void OnHarvestToggled(ChangeEvent<bool> evt)
    //{
    //    if (noHarvest.value)
    //    {
    //        harvestDict[smallHarvest] = false;
    //        harvestDict[mediumHarvest] = false;
    //        harvestDict[largeHarvest] = false;
    //    }
    //    else
    //        harvestDict[mediumHarvest] = true;
    //    AdjustTints();
    //}

    //private void AdjustTints()
    //{
    //    foreach (KeyValuePair<VisualElement, bool> kvp in harvestDict)
    //    {
    //        VisualElement tint = kvp.Key.Q<VisualElement>("Tint");
    //        if (kvp.Value == false)
    //            tint.style.unityBackgroundImageTintColor = darkTint;
    //        else
    //            tint.style.unityBackgroundImageTintColor = lightTint;

    //    }
    //}

    private void OpenQueenTab()
    {
        queenClick.RemoveManipulator(assignQueen);
        selectingQueen = true;
        queenClick.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = darkTint;
        player.OpenTab(0, player.open1, true, this);
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
        tooltip.Q<Label>("Affliction").text = Condition;
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
    #endregion
}