using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class HoneyMarket : MonoBehaviour
{
    [SerializeField]
    UIDocument document;

    [SerializeField]
    VisualTreeAsset marketAsset;

    [SerializeField]
    PlayerController player;

    [SerializeField]
    GameController controller;

    [SerializeField]
    VisualTreeAsset marketCard;

    private CustomVisualElement marketButton;
    public TemplateContainer marketTemplate;

    Dictionary<FlowerType, List<float>> marketValues = new Dictionary<FlowerType, List<float>>();
    Dictionary<FlowerType, float> amountSold = new Dictionary<FlowerType, float>();
    int turn = 0;
    System.Array values = System.Enum.GetValues(typeof(FlowerType));

    public bool marketOpen;

    private VisualElement selectedElement;
    private FlowerType selectedType = FlowerType.Empty;
    private float inventory; //Get from player
    private float price;

    VisualElement Wildflower;
    VisualElement Clover;
    VisualElement Alfalfa;
    VisualElement Buckwheat;
    VisualElement Fireweed;
    VisualElement Goldenrod;

    Button sell1;
    Button sell10;
    Button sell50;
    Button buy1;
    Button buy10;
    Button buy50;

    Label amountLabel;

    private const int totalBarWidth = 900;
    private VisualElement lowBar;
    private VisualElement mediumBar;
    private VisualElement highBar;
    private float lowWidth;
    private float mediumWidth;
    private float highWidth;

    private VisualElement exit;

    public bool fromSave;

    private List<FlowerType> addedFlowers = new List<FlowerType>();

    public void GameLoaded()
    {
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        controller = GameObject.Find("GameController").GetComponent<GameController>();
        ReloadUI();

        if (!fromSave)
        {

            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                marketValues.Add(fType, new List<float>() { 0, 0, 0 }); //Current Value, Growth per Turn, Base Value
                amountSold.Add(fType, 0);
            }

            marketValues[FlowerType.Wildflower][2] = 3;
            marketValues[FlowerType.Clover][2] = 4;
            marketValues[FlowerType.Alfalfa][2] = 5;
            marketValues[FlowerType.Buckwheat][2] = 4;
            marketValues[FlowerType.Fireweed][2] = 5;
            marketValues[FlowerType.Dandelion][2] = 6;
            marketValues[FlowerType.Sunflower][2] = 6;
            marketValues[FlowerType.Daisy][2] = 6;
            marketValues[FlowerType.Thistle][2] = 6;
            marketValues[FlowerType.Blueberry][2] = 6;
            marketValues[FlowerType.Orange][2] = 6;
            marketValues[FlowerType.Tupelo][2] = 6;

            ResetToBaseValue();
            UpdateMarket();
        }

        OpenMarket();
        for (int i = 0; i < values.Length; i++)
        {
            if (i != 0)
            {
                if (i < 6)
                    marketTemplate.Q<VisualElement>("Row1").Add(marketCard.Instantiate());
                else if (i < 11)
                    marketTemplate.Q<VisualElement>("Row2").Add(marketCard.Instantiate());
                else
                    marketTemplate.Q<VisualElement>("Row3").Add(marketCard.Instantiate());
            }
        }

        //Add Wildflower card
        VisualElement elem = marketTemplate.Q<VisualElement>("EmptyCard");
        elem.name = FlowerType.Wildflower.ToString();
        elem.Q<Label>("Label").text = FlowerType.Wildflower.ToString();
        addedFlowers.Add(FlowerType.Wildflower);
        elem.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Wildflower);

        SetAllLabels();

        SelectHoney(elem, FlowerType.Wildflower);

        CloseMarket();
    }

    public void AddHoneyCards(List<int> flowers)
    {
        foreach (int f in flowers)
        {
            VisualElement elem = marketTemplate.Q<VisualElement>("EmptyCard");
            elem.name = ((FlowerType)(f + 2)).ToString();
            elem.Q<Label>("Label").text = ((FlowerType)(f + 2)).ToString();
            addedFlowers.Add((FlowerType)(f + 2));
            elem.RegisterCallback<PointerDownEvent, FlowerType>(Select, (FlowerType)(f + 2));
        }
    }

    public void ReloadUI()
    {
        marketButton = document.rootVisualElement.Q<CustomVisualElement>("MarketButton");
        marketButton.AddManipulator(new Clickable(e => OpenMarket()));
        marketButton.RegisterCallback<PointerDownEvent>(e => ReferenceGlossary(e));
    }

    public float GetPrice(FlowerType f)
    {
        return marketValues[f][0];
    }

    private void ReferenceGlossary(PointerDownEvent e)
    {
        if (e.button == 1)
            document.GetComponent<Glossary>().OpenGlossary("HoneyMarket");
    }

    private void OpenMarket()
    {
        if (controller.CurrentState == GameStates.TurnEnd || controller.CurrentState == GameStates.Paused)
            return;

        document.GetComponent<AudioSource>().Play();
        if (marketTemplate == null)
            marketTemplate = marketAsset.Instantiate();

        marketTemplate.style.position = Position.Absolute;
        document.rootVisualElement.Q<VisualElement>("Base").Add(marketTemplate);

        
        SetUpMarket();

        document.rootVisualElement.Q<Label>("Money").visible = false;

        marketOpen = true;
        SetAllLabels();
    }

    private void SetUpMarket()
    {
        //Get Buttons
        sell1 = marketTemplate.Q<Button>("Sell1");
        sell10 = marketTemplate.Q<Button>("Sell10");
        sell50 = marketTemplate.Q<Button>("Sell50");
        buy1 = marketTemplate.Q<Button>("Buy1");
        buy10 = marketTemplate.Q<Button>("Buy10");
        buy50 = marketTemplate.Q<Button>("Buy50");

        amountLabel = marketTemplate.Q<Label>("AmountLabel");

        lowBar = marketTemplate.Q<VisualElement>("Low");
        mediumBar = marketTemplate.Q<VisualElement>("Medium");
        highBar = marketTemplate.Q<VisualElement>("High");

        exit = marketTemplate.Q<VisualElement>("Close");
        exit.AddManipulator(new Clickable(() => CloseMarket()));


        //Set cost and change labels
        SetAllLabels();
        UpdateBarRatio();

        //SelectHoney(Wildflower, FlowerType.Wildflower);

        //Assign button callbacks
        sell1.clickable = new Clickable( e => Sell(1));
        sell10.clickable = new Clickable(e => Sell(10));
        sell50.clickable = new Clickable(e => Sell(50));
        buy1.clickable = new Clickable(e => Buy(1));
        buy10.clickable = new Clickable(e => Buy(10));
        buy50.clickable = new Clickable(e => Buy(50));

        marketTemplate.Q<Label>("MoneyLabel").text = "$" + player.Money;
    }

    private void SetAllLabels()
    {
        foreach (FlowerType f in addedFlowers)
        {
            SetLabel(marketTemplate.Q<VisualElement>(f.ToString()), f);
        }
        SetAmountLabel();
    }

    private void SetLabel(VisualElement element, FlowerType fType)
    {
        string cost = marketValues[fType][0].ToString();
        if (cost.IndexOf('.') + 3 < cost.Length)
            element.Q<Label>("Cost").text = "$" + cost.Substring(0, cost.IndexOf('.') + 3);
        else
            element.Q<Label>("Cost").text = "$" + cost;

        if (player.inventory.Count == 0)
            return;

        //element.Q<Label>("Cost").text = "$" + marketValues[fType][0];
        string amount = player.inventory[fType][0].ToString();
        if (amount.IndexOf('.') + 3 < amount.Length)
            element.Q<Label>("Change").text = amount.Substring(0, amount.IndexOf('.') + 3) + " lbs.";
        else
            element.Q<Label>("Change").text = amount + " lbs.";

        if (marketValues[fType][1] < 0)
            element.style.unityBackgroundImageTintColor = new Color(.86f, .47f, .47f, 1);
        else if (marketValues[fType][1] > 0)
            element.style.unityBackgroundImageTintColor = new Color(.47f, .86f, .47f, 1);
        else
            element.style.unityBackgroundImageTintColor = Color.white;
    }

    private void SetAmountLabel()
    {
        if (selectedElement != null && player.inventory.Count != 0)
        {
            amountLabel.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
            float amount = 0;
            amount += player.inventory[selectedType][0];
            string amountString = amount.ToString();
            if (amountString.IndexOf('.') + 3 < amountString.Length)
                amountLabel.text = "You have \n" + amountString.Substring(0, amountString.IndexOf('.') + 3) + " lbs. selected";
            else
                amountLabel.text = "You have \n" + amountString + " lbs. selected";
        }
    }

    private void UpdateBarRatio()
    {
        if (player.inventory.Count == 0)
            return;

        float total = 0;
        float lowRatio = 0;
        float mediumRatio = 0;
        float highRatio = 0;

        if (selectedType != FlowerType.Empty)
        {
            total = player.inventory[selectedType][1] + player.inventory[selectedType][2] + player.inventory[selectedType][3];
            lowRatio = player.inventory[selectedType][1] / total;
            mediumRatio = player.inventory[selectedType][2] / total;
            highRatio = player.inventory[selectedType][3] / total;
        }

        if (total == 0)
        {
            lowWidth = 300;
            mediumWidth = 300;
            highWidth = 300;
            lowBar.style.width = totalBarWidth / 3;
            mediumBar.style.width = totalBarWidth / 3;
            highBar.style.width = totalBarWidth / 3;
            return;
        }
        lowWidth = Mathf.Clamp(lowRatio * totalBarWidth, 40, 820);
        mediumWidth = Mathf.Clamp(mediumRatio * totalBarWidth, 40, 820);
        highWidth = Mathf.Clamp(highRatio * totalBarWidth, 40, 820);
        lowBar.style.width = lowWidth;
        mediumBar.style.width = mediumWidth;
        highBar.style.width = highWidth;

        SetAmountLabel();
    }

    private void Select(PointerDownEvent e, FlowerType fType)
    {
        document.GetComponent<AudioSource>().Play();
        SelectHoney(e.currentTarget as VisualElement, fType);
    }

    private void SelectHoney(VisualElement item, FlowerType fType)
    {
        DeselectHoney();

        selectedElement = item;
        selectedType = fType;
        price = float.Parse(item.Q<Label>("Cost").text.Substring(1));
        item.style.borderTopWidth = 4;
        item.style.borderBottomWidth = 4;
        item.style.borderRightWidth = 4;
        item.style.borderLeftWidth = 4;

        SetAmountLabel();
        UpdateBarRatio();

        item.UnregisterCallback<PointerDownEvent, FlowerType>(Select);
    }

    private void DeselectHoney()
    {
        if (selectedElement == null)
            return;

        selectedElement.style.borderTopWidth = 0;
        selectedElement.style.borderBottomWidth = 0;
        selectedElement.style.borderRightWidth = 0;
        selectedElement.style.borderLeftWidth = 0;

        selectedElement.RegisterCallback<PointerDownEvent, FlowerType>(Select, selectedType);

        selectedType = FlowerType.Empty;
        price = 0;
        selectedElement = null;
        SetAmountLabel();
    }

    private void Sell(float amount)
    {
        if (selectedElement == null)
            return;

        document.GetComponent<AudioSource>().Play();
        if (player.inventory[selectedType][0] < amount)
            amount = player.inventory[selectedType][0];
        player.Money = Mathf.RoundToInt(amount * price);
        float toBePaid = amount;

        toBePaid = SellLow(toBePaid);
        if (toBePaid > 0)
            toBePaid = SellMedium(toBePaid);
        if (toBePaid > 0)
            toBePaid = SellHigh(toBePaid);

        Debug.Log("Amount: " + amount + " toBePaid: " + toBePaid);
        Debug.Log("Subtracting " + (amount - toBePaid));
        player.inventory[selectedType][0] -= amount - toBePaid;


        //implement bar ratio calculation for player honey purities

        SetAllLabels();
        UpdateBarRatio();
    }

    private float SellLow(float amount)
    {
        if (player.inventory[selectedType][1] < amount)
        {
            amount -= player.inventory[selectedType][1];
            player.inventory[selectedType][1] = 0;
        }
        else
        {
            player.inventory[selectedType][1] -= amount;
            amount = 0;
        }

        return amount;
    }
    private float SellMedium(float amount)
    {
        if (player.inventory[selectedType][2] < amount)
        {
            amount -= player.inventory[selectedType][2];
            player.inventory[selectedType][2] = 0;
        }
        else
        {
            player.inventory[selectedType][2] -= amount;
            amount = 0;
        }

        return amount;
    }

    private float SellHigh(float amount)
    {
        if (player.inventory[selectedType][3] < amount)
        {
            amount -= player.inventory[selectedType][3];
            player.inventory[selectedType][3] = 0;
        }
        else
        {
            player.inventory[selectedType][3] -= amount;
            amount = 0;
        }

        return amount;
    }

    private void Buy(float amount)
    {
        if (selectedElement == null)
            return;

        document.GetComponent<AudioSource>().Play();
        if (player.Money < amount * price)
            amount = Mathf.RoundToInt(player.Money / price);
        player.Money = -Mathf.RoundToInt(amount * price);
        player.inventory[selectedType][0] += amount;
        player.inventory[selectedType][2] += amount;
        SetAllLabels();
        UpdateBarRatio();
    }

    public void CloseMarket()
    {
        if (!marketOpen)
            return;

        document.rootVisualElement.Q<VisualElement>("Base").Remove(marketTemplate);
        //marketTemplate = null;
        marketOpen = false;
        document.rootVisualElement.Q<Label>("Money").visible = true;
    }

    public void UpdateMarket()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            if (marketValues[fType][1] == 0 || turn % 4 == 0)
            {
                float randValue = Random.Range(0.25f, 1);
                if (Random.Range(0, 2) == 0)
                    randValue *= -1;
                marketValues[fType][1] = randValue;
            }
            else
            {
                float roundedRand;
                if (marketValues[fType][1] >= 0)
                    roundedRand = Random.Range(1, 100) / 100.0f;
                else
                    roundedRand = Random.Range(-99, 0) / 100.0f;
                marketValues[fType][1] = roundedRand;
            }


            if (turn % 14 == 0)
                ResetToBaseValue();
            else if (marketValues[fType][0] > marketValues[fType][2] / 2f)
                marketValues[fType][0] += marketValues[fType][1] - amountSold[fType] / 10f;

            if (marketValues[fType][0] < marketValues[fType][2] / 2f)
                marketValues[fType][0] = marketValues[fType][2] / 2f;
        }
        turn++;
        LogValues();
        if (marketTemplate != null)
            SetAllLabels();
    }

    private void ResetToBaseValue()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            marketValues[fType][1] = marketValues[fType][2] - marketValues[fType][0];
            marketValues[fType][0] = marketValues[fType][2];
        }
    }

    private void LogValues()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            Debug.Log(marketValues[fType][0] + " " + marketValues[fType][1]);
        }
    }

    public void Save(ref MarketSaveData data)
    {
        List<float> sold = new List<float>();
        List<float> values = new List<float>();

        foreach (KeyValuePair<FlowerType, List<float>> kvp in marketValues)
            foreach (float f in kvp.Value)
                values.Add(f);


        foreach (KeyValuePair<FlowerType, float> kvp in amountSold)
            sold.Add(kvp.Value);

        data.marketValues = values;
        data.amountSold = sold;
        data.turn = turn;
    }

    public void Load(MarketSaveData data)
    {
        fromSave = true;

        int count = 0;
        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            amountSold[fType] = data.amountSold[count];
            count++;
        }

        int index = 0;
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            List<float> temp = new List<float>() {data.marketValues[index], data.marketValues[index + 1], data.marketValues[index + 2]};
            marketValues[fType] = temp;
            index += 3;
        }

        turn = data.turn;
    }
}

[System.Serializable]
public struct MarketSaveData
{
    public List<float> marketValues;
    public List<float> amountSold;
    public int turn;
}
