using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum FlowerType
{
    Empty = 0,
    Clover = 1,
    Alfalfa = 2,
    Blossom = 3,
    Buckwheat = 4
}

public class Hive : MonoBehaviour
{
    private UIDocument document;
    private MapLoader map;
    private PlayerController player;

    [SerializeField]
    private VisualTreeAsset hiveUI;

    public TemplateContainer template;

    public bool empty = true;

    public int x;
    public int y;

    private int size = 1;
    private float population = 5000;
    private float popCap = 20000; //what population the hive can currently house
    private float popSizeCap = 20000; //how much each level of size changes the popCap
    private float comb = 0;
    private float combCap = 8; //how much honey the hive can currently store
    private float combSizeCap = 8; //how much each level of size changes the honeyCap
    private float nectar;
    private float honey;

    private float storage = 0; //how much storage the hive has
    private float storagePerComb = 1000; //how much each level of size changes the storage

    private float birthRate = 2500;

    private float hiveEfficency; //Efficiency is a multiplier to all the hive's actions and is calculated by the population / total population * size of the hive

    //stats 0-1f
    private float production = 400;
    private float construction = 0.5f;
    private float collection = 400;
    private float resilience = 1;
    private float aggressivness = 1;

    private QueenBee queen;

    private Dictionary<FlowerType, float> flowerValues = new Dictionary<FlowerType, float>();
    private Dictionary<FlowerType, float> nectarValues = new Dictionary<FlowerType, float>();
    private float totalFlowerWeight = 0;
    private FlowerType honeyType = FlowerType.Empty;
    private float honeyPurity = 0;

    //UI
    private VisualElement root;
    private VisualElement smallHarvest;
    private VisualElement mediumHarvest;
    private VisualElement largeHarvest;
    private ProgressBar combMeter;
    private ProgressBar nectarMeter;
    private ProgressBar honeyMeter;
    private Dictionary<VisualElement, bool> harvestDict = new Dictionary<VisualElement, bool>();
    private Toggle noHarvest;

    public int Size
    {
        get { return size; }
        set
        {
            size += value;
            popCap = popSizeCap * size;
            combCap = combSizeCap * size;
        }
    }

    void Start()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();

        root = document.rootVisualElement;

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            flowerValues.Add(fType, 0);
            nectarValues.Add(fType, 0);
        }

        Populate();
    }

    void Update()
    {

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

        float possibleNectar = collection * queen.collectionMult * hiveEfficency;
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
        Debug.Log("Comb: " + comb);
        Debug.Log("Nectar: " + nectar);
        Debug.Log("Honey: " + honey);
        Debug.Log("Storage: " + storage);
        //Debug.Log("Efficiency: " + hiveEfficency);

        //reset flowerValues
        foreach (FlowerType key in flowerValues.Keys.ToList())
            flowerValues[key] = 0f;
        totalFlowerWeight = 0f;

        CalcHoneyStats();
        if (template != null)
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
        combMeter.value = (comb / combCap) * 100;
        //nectarMeter.value = (nectar / storage) * 100;
        nectarMeter.value = (nectar / production * queen.productionMult * hiveEfficency) * 100;
        honeyMeter.value = (honey / (combCap * storagePerComb)) * 100;
    }

    public void Populate(QueenBee q = null)
    {
        if (q == null)
            queen = new QueenBee(false);
        else 
            queen = q;
        empty = false;
    }

    #region UI

    void OnMouseDown()
    {
        player.OpenHiveUI(template, hiveUI, this);

        if (harvestDict.Keys.Count == 0)
        {
            noHarvest = template.Q<Toggle>();
            smallHarvest = template.Q<VisualElement>("SmallTint");
            mediumHarvest = template.Q<VisualElement>("MediumTint");
            largeHarvest = template.Q<VisualElement>("LargeTint");
            combMeter = template.Q<ProgressBar>("CombBar");
            nectarMeter = template.Q<ProgressBar>("NectarBar");
            honeyMeter = template.Q<ProgressBar>("HoneyBar");
            noHarvest.RegisterValueChangedCallback(OnHarvestToggled);
            smallHarvest.AddManipulator(new Clickable(e => SelectHarvest(smallHarvest)));
            mediumHarvest.AddManipulator(new Clickable(e => SelectHarvest(mediumHarvest)));
            largeHarvest.AddManipulator(new Clickable(e => SelectHarvest(largeHarvest)));

            harvestDict.Add(smallHarvest, false);
            harvestDict.Add(mediumHarvest, true);
            harvestDict.Add(largeHarvest, false);
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

        foreach (KeyValuePair<VisualElement, bool> kvp in harvestDict)
        {
            Color current = kvp.Key.resolvedStyle.unityBackgroundImageTintColor;
            if (kvp.Value == false)
                current.a = 0.5f;
            else
                current.a = 0.0f;
            kvp.Key.style.unityBackgroundImageTintColor = current;
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
            Color current = kvp.Key.resolvedStyle.unityBackgroundImageTintColor;
            if (kvp.Value == false)
                current.a = 0.5f;
            else
                current.a = 0.0f;
            kvp.Key.style.unityBackgroundImageTintColor = current;
        }
    }
    #endregion
}