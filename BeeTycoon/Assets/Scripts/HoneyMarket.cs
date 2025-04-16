using System.Collections;
using System.Collections.Generic;
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

    private CustomVisualElement marketButton;
    private TemplateContainer marketTemplate;

    Dictionary<FlowerType, List<float>> marketValues = new Dictionary<FlowerType, List<float>>();
    Dictionary<FlowerType, float> amountSold = new Dictionary<FlowerType, float>();
    int turn = 0;
    System.Array values = System.Enum.GetValues(typeof(FlowerType));

    public bool marketOpen;

    private VisualElement selectedElement;
    private FlowerType selectedType;
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

    // Start is called before the first frame update
    void Start()
    {
        marketButton = document.rootVisualElement.Q<CustomVisualElement>("MarketButton");
        marketButton.AddManipulator(new Clickable(e => OpenMarket()));

        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            marketValues.Add(fType, new List<float>() { 0, 0, 0}); //Current Value, Growth per Turn, Base Value
            amountSold.Add(fType, 0);
        }

        marketValues[FlowerType.Wildflower][2] = 5;
        marketValues[FlowerType.Clover][2] = 7;
        marketValues[FlowerType.Alfalfa][2] = 9;
        marketValues[FlowerType.Blossom][2] = 15;
        marketValues[FlowerType.Buckwheat][2] = 12;
        marketValues[FlowerType.Fireweed][2] = 10;
        marketValues[FlowerType.Goldenrod][2] = 15;

        ResetToBaseValue();
        UpdateMarket();
    }

    private void OpenMarket()
    {
        marketTemplate = marketAsset.Instantiate();
        marketTemplate.style.position = Position.Absolute;
        document.rootVisualElement.Q<VisualElement>("Base").Add(marketTemplate);

        SetUpMarket();

        marketOpen = true;
    }

    private void SetUpMarket()
    {
        //Get Items
        Wildflower = marketTemplate.Q<VisualElement>("Wildflower");
        Clover = marketTemplate.Q<VisualElement>("Clover");
        Alfalfa = marketTemplate.Q<VisualElement>("Alfalfa");
        Buckwheat = marketTemplate.Q<VisualElement>("Buckwheat");
        Fireweed = marketTemplate.Q<VisualElement>("Fireweed");
        Goldenrod = marketTemplate.Q<VisualElement>("Goldenrod");

        //Get Buttons
        sell1 = marketTemplate.Q<Button>("Sell1");
        sell10 = marketTemplate.Q<Button>("Sell10");
        sell50 = marketTemplate.Q<Button>("Sell50");
        buy1 = marketTemplate.Q<Button>("Buy1");
        buy10 = marketTemplate.Q<Button>("Buy10");
        buy50 = marketTemplate.Q<Button>("Buy50");

        amountLabel = marketTemplate.Q<Label>("AmountLabel");

        //Set cost and change labels
        SetAllLabels();

        Wildflower.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Wildflower);
        Clover.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Clover);
        Alfalfa.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Alfalfa);
        Buckwheat.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Buckwheat);
        Fireweed.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Fireweed);
        Goldenrod.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, FlowerType.Goldenrod);


        //Assign button callbacks
        sell1.clickable = new Clickable( e => Sell(1));
        sell10.clickable = new Clickable(e => Sell(10));
        sell50.clickable = new Clickable(e => Sell(50));
        buy1.clickable = new Clickable(e => Buy(1));
        buy10.clickable = new Clickable(e => Buy(10));
        buy50.clickable = new Clickable(e => Buy(50));
    }

    private void SetAllLabels()
    {
        SetLabel(Wildflower, FlowerType.Wildflower);
        SetLabel(Clover, FlowerType.Clover);
        SetLabel(Alfalfa, FlowerType.Alfalfa);
        SetLabel(Buckwheat, FlowerType.Buckwheat);
        SetLabel(Fireweed, FlowerType.Fireweed);
        SetLabel(Goldenrod, FlowerType.Goldenrod);
    }

    private void SetLabel(VisualElement element, FlowerType fType)
    {
        string symbol = "";
        element.Q<Label>("Cost").text = "$" + marketValues[fType][0];
        symbol = (marketValues[fType][1] < 0) ? "-" : "+";
        element.Q<Label>("Change").text = symbol + "$" + Mathf.Abs(marketValues[fType][1]);
        element.Q<Label>("Change").style.color = (symbol == "+") ? Color.green : Color.red;
    }

    private void SelectHoney(PointerDownEvent e, FlowerType fType)
    {
        VisualElement item = e.currentTarget as VisualElement;
        DeselectHoney();

        selectedElement = item;
        selectedType = fType;
        price = float.Parse(item.Q<Label>("Cost").text.Substring(1));
        item.style.borderTopWidth = 4;
        item.style.borderBottomWidth = 4;
        item.style.borderRightWidth = 4;
        item.style.borderLeftWidth = 4;

        sell1.style.backgroundColor = new Color(0.26f , 0.26f, 0.26f);
        sell10.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
        sell50.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
        buy1.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
        buy10.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
        buy50.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);

        amountLabel.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
        amountLabel.text = "You have \n" + player.inventory[fType] + " lbs.";

        item.UnregisterCallback<PointerDownEvent, FlowerType>(SelectHoney);
        item.RegisterCallback<PointerDownEvent>(DeselectEvent);
    }

    private void DeselectEvent(PointerDownEvent e)
    {
        DeselectHoney();
    }

    private void DeselectHoney()
    {
        if (selectedElement == null)
            return;

        selectedElement.style.borderTopWidth = 0;
        selectedElement.style.borderBottomWidth = 0;
        selectedElement.style.borderRightWidth = 0;
        selectedElement.style.borderLeftWidth = 0;

        sell1.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        sell10.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        sell50.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        buy1.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        buy10.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        buy50.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);

        amountLabel.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
        amountLabel.text = "---";

        selectedElement.UnregisterCallback<PointerDownEvent>(DeselectEvent);
        selectedElement.RegisterCallback<PointerDownEvent, FlowerType>(SelectHoney, selectedType);

        selectedType = FlowerType.Empty;
        price = 0;
        selectedElement = null;
    }

    private void Sell(float amount)
    {
        if (selectedElement == null)
            return;
        if (player.inventory[selectedType] < amount)
            amount = player.inventory[selectedType];
        player.Money += Mathf.RoundToInt(amount * price);
        player.inventory[selectedType] -= amount;
    }

    private void Buy(float amount)
    {
        if (selectedElement == null)
            return;
        if (player.Money < amount * price)
            amount = Mathf.RoundToInt(player.Money / price);
        player.Money -= Mathf.RoundToInt(amount * price);
        player.inventory[selectedType] += amount;
    }

    public void CloseMarket()
    {
        document.rootVisualElement.Q<VisualElement>("Base").Remove(marketTemplate);
        marketTemplate = null;
        marketOpen = false;
    }

    public void UpdateMarket()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            if (marketValues[fType][1] == 0 || turn % 4 == 0)
            {
                float randValue = Random.Range(1, 5);
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


            if (turn % 8 == 0)
                ResetToBaseValue();
            else if (marketValues[fType][0] > marketValues[fType][2] / 2f)
                marketValues[fType][0] += marketValues[fType][1] - amountSold[fType] / 10f;

            if (marketValues[fType][0] < marketValues[fType][2] / 2f)
                marketValues[fType][0] = marketValues[fType][2] / 2f;
        }
        turn++;
        //LogValues();
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
}
