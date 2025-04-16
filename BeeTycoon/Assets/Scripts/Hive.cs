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
    Goldenrod = 4,
    Buckwheat = 5,
    Fireweed = 6,
    Blossom = 7
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

    public TemplateContainer template;
    private TemplateContainer hoverTemplate;
    public bool empty = true;
    private bool placed;

    public int x;
    public int y;

    private int size = 1;
    private float population = 5000;
    private float popCap = 20000; //what population the hive can currently house
    private float popSizeCap = 20000; //how much each level of size changes the popCap
    private float comb = 0;
    private int combCap = 8; //how much honey the hive can currently store
    private int combSizeCap = 8; //how much each level of size changes the honeyCap
    private float nectar;
    private float honey;

    private float storage = 0; //how much storage the hive has
    private float storagePerComb = 1000; //how much each level of size changes the storage

    private float birthRate = 2500;

    private float hiveEfficency; //Efficiency is a multiplier to all the hive's actions and is calculated by the population / total population * size of the hive

    //stats 0-1f
    private float production = 1600; // was 400
    private float construction = 1f; //was 0.5f
    private float collection = 800; //was 400
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
    public bool selectingQueen;
    public bool isOpen;
    public bool hasSugar;
    public bool hasReducer;
    public bool hasRepellant;
    public bool hasInsulation;
    private string condition = "Healthy";

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
            condition = value;

            switch (value)
            {
                case "Mites":
                    hiveEfficency /= 2;
                    break;
                case "Mice":
                    construction /= 2;
                    break;
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
                selectingQueen = false;
                queenClick.AddManipulator(assignQueen);
                queenClick.Q<VisualElement>("Tint").style.unityBackgroundImageTintColor = lightTint;
                player.CloseTab();
            }
            else if (isOpen && player.SelectedItem == null)
                player.CloseHiveUI(this);
        }
    }

    public void UpdateHive()
    {
        if (empty)
            return;

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

        float nectarGain = map.nectarGains.Values.Sum();
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
        if (template != null)
            UpdateMeters();

        TryAddCondition();
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
                    case FlowerType.Blossom:
                        flowerValues[FlowerType.Blossom] += 1f / distance;
                        break;
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
        float nectarGain = map.nectarGains.Values.Sum();
        if (production * queen.productionMult * hiveEfficency != 0)
            nectarMeter.value = (nectarGain * queen.collectionMult * hiveEfficency / (production * queen.productionMult * hiveEfficency) * 100) + 8;// * Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f) 
        else
            nectarMeter.value = 8;
        honeyMeter.value = (honey / (combCap * storagePerComb) * 100) + 8;
        UpdateMeterLabels();
    }

    private void UpdateMeterLabels()
    {
        float nectarGain = map.nectarGains.Values.Sum();
        if (production * queen.productionMult * hiveEfficency != 0)
            nectarHover.Q<Label>("Percent").text = (Mathf.Round(nectarGain * queen.collectionMult * hiveEfficency / (production * queen.productionMult * hiveEfficency) * 100 * 10) / 10.0f).ToString() + "%"; //* Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f
        else
            nectarHover.Q<Label>("Percent").text = "0%";
        nectarHover.Q<Label>("Flat").text = (Mathf.Round(nectarGain * queen.collectionMult * hiveEfficency  * 10) / 10.0f).ToString(); //*Mathf.Clamp(map.GetFlowerCount() / (map.mapWidth * map.mapHeight), 0.5f, 0.8f)

        honeyHover.Q<Label>("Percent").text = (Mathf.Round(honey / (combCap * storagePerComb) * 100 * 10) / 10.0f).ToString() + "%";
        honeyHover.Q<Label>("Flat").text = (Mathf.Round(production * queen.productionMult * hiveEfficency * 10) / 10.0f).ToString();

        combHover.Q<Label>("Percent").text = (Mathf.Round(comb / combCap * 100) * 10 / 10.0f).ToString() + "%";
        combHover.Q<Label>("Flat").text = (Mathf.Round(construction * queen.constructionMult * hiveEfficency * 10) / 10.0f).ToString();
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
    }

    private void TryAddCondition()
    {
        if (Condition == "Healthy")
        {
            int rand = Random.Range(0, 20);
            if (!hasRepellant)
            {
                if (rand <= 4)
                    Condition = "Mites";
            }
            else if (!hasReducer)
            {
                if (rand <= 2)
                    Condition = "Mice";
            }
            else if (game.Season == "Winter" && honey <= population / 16)
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
                construction *= 2;
                break;
        }

        Condition = "Healthy";
    }

    public void AddSugarWater()
    {
        collection *= 1.5f;
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
            noHarvest = template.Q<Toggle>();
            noHarvest.RegisterValueChangedCallback(OnHarvestToggled);

            smallHarvest = template.Q<VisualElement>("SmallClick");
            mediumHarvest = template.Q<VisualElement>("MediumClick");
            largeHarvest = template.Q<VisualElement>("LargeClick");

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
            harvestDict.Add(mediumHarvest, true);
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

            UpdateMeterLabels();
        }
    }

    private void SelectHarvest(VisualElement clickedElement)
    {
        if (harvestDict[clickedElement] == false && !noHarvest.value)
        {
            Dictionary<VisualElement, bool> temp = new Dictionary<VisualElement, bool>();
            temp.Add(smallHarvest, harvestDict[smallHarvest]);
            temp.Add(mediumHarvest, harvestDict[mediumHarvest]);
            temp.Add(largeHarvest, harvestDict[largeHarvest]);

            foreach (KeyValuePair<VisualElement, bool> kvp in temp)
            {
                if (kvp.Key != clickedElement)
                    harvestDict[kvp.Key] = false;
                else
                    harvestDict[kvp.Key] = true;
            }
        }
        AdjustTints();
    }

    private void OnHarvestToggled(ChangeEvent<bool> evt)
    {
        if (noHarvest.value)
        {
            harvestDict[smallHarvest] = false;
            harvestDict[mediumHarvest] = false;
            harvestDict[largeHarvest] = false;
        }
        else
            harvestDict[mediumHarvest] = true;
        AdjustTints();
    }

    private void AdjustTints()
    {
        foreach (KeyValuePair<VisualElement, bool> kvp in harvestDict)
        {
            VisualElement tint = kvp.Key.Q<VisualElement>("Tint");
            if (kvp.Value == false)
                tint.style.unityBackgroundImageTintColor = darkTint;
            else
                tint.style.unityBackgroundImageTintColor = lightTint;

        }
    }

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
    #endregion
}