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

    [SerializeField]
    GameController controller;

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
    private bool lowSelected = true;
    private bool mediumSelected = true;
    private bool highSelected = true;


    private VisualElement bar;
    private EventCallback<PointerDownEvent> dragBracketCallback;
    private bool draggingStart;
    private bool draggingeEnd;

    private VisualElement startBracket;
    private VisualElement endBracket;
    private float startBracketPos = -385;
    private float endBracketPos = 385;

    private VisualElement exit;

    public bool fromSave;

    // Start is called before the first frame update
    void Start()
    {
        document = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        controller = GameObject.Find("GameController").GetComponent<GameController>();
        marketButton = document.rootVisualElement.Q<CustomVisualElement>("MarketButton");
        marketButton.AddManipulator(new Clickable(e => OpenMarket()));
        marketButton.RegisterCallback<PointerDownEvent>(e => ReferenceGlossary(e));

        if (!fromSave)
        {
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                marketValues.Add(fType, new List<float>() { 0, 0, 0 }); //Current Value, Growth per Turn, Base Value
                amountSold.Add(fType, 0);
            }

            marketValues[FlowerType.Wildflower][2] = 5;
            marketValues[FlowerType.Clover][2] = 7;
            marketValues[FlowerType.Alfalfa][2] = 9;
            //marketValues[FlowerType.Blossom][2] = 15;
            marketValues[FlowerType.Buckwheat][2] = 12;
            marketValues[FlowerType.Fireweed][2] = 10;
            marketValues[FlowerType.Goldenrod][2] = 15;

            ResetToBaseValue();
            UpdateMarket();
        }
    }

    private void ReferenceGlossary(PointerDownEvent e)
    {
        if (e.button == 1)
            document.GetComponent<Glossary>().OpenGlossary("HoneyMarket");
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            draggingeEnd = false;
            draggingStart = false;
        }

        if (draggingStart)
        {
            Vector2 pos = bar.WorldToLocal(Input.mousePosition);
            if (pos.x <= lowBar.resolvedStyle.width / 2)
                startBracketPos = (lowBar.resolvedStyle.left - totalBarWidth / 2) + 40;
            else if (pos.x > lowBar.resolvedStyle.width / 2 && pos.x <= mediumBar.resolvedStyle.left + mediumBar.resolvedStyle.width / 2)
                startBracketPos = lowBar.resolvedStyle.left + lowBar.resolvedStyle.width - totalBarWidth / 2;
            else if (pos.x > mediumBar.resolvedStyle.left + mediumBar.resolvedStyle.width / 2 && pos.x <= highBar.resolvedStyle.left)
                startBracketPos = highBar.resolvedStyle.left - totalBarWidth / 2;

            if (endBracketPos > startBracketPos)
            {
                startBracket.style.left = startBracketPos + startBracket.resolvedStyle.width / 2;
                SelectPurities();
            }
        }
        else if (draggingeEnd)
        {
            Vector2 pos = bar.WorldToLocal(Input.mousePosition);
            if (pos.x >= highBar.resolvedStyle.left + highBar.resolvedStyle.width / 2)
                endBracketPos = totalBarWidth / 2 - 40;
            else if (pos.x >= mediumBar.resolvedStyle.left + mediumBar.resolvedStyle.width / 2 && pos.x < highBar.resolvedStyle.left + highBar.resolvedStyle.width / 2)
                endBracketPos = highBar.resolvedStyle.left - totalBarWidth / 2;
            else if (pos.x >= lowBar.resolvedStyle.width / 2 && pos.x < mediumBar.resolvedStyle.left + mediumBar.resolvedStyle.width / 2)
                endBracketPos = lowBar.resolvedStyle.left + lowBar.resolvedStyle.width - totalBarWidth / 2;

            if (startBracketPos < endBracketPos)
            {
                endBracket.style.left = endBracketPos - endBracket.resolvedStyle.width / 2;
                SelectPurities();
            }
        }
    }

    private IEnumerator AdjustBrackets()
    {
        yield return new WaitForFixedUpdate();
        if (!lowSelected || !highSelected)
        {
            if (!lowSelected && !highSelected)
            {
                startBracketPos = lowBar.resolvedStyle.width - (totalBarWidth / 2);
                endBracketPos = lowBar.resolvedStyle.width + mediumBar.resolvedStyle.width - (totalBarWidth / 2);
                startBracket.style.left = startBracketPos + (startBracket.resolvedStyle.width / 2);
                endBracket.style.left = endBracketPos - (endBracket.resolvedStyle.width / 2);
            }
            else if (!lowSelected)
            {
                startBracketPos = lowBar.resolvedStyle.width - (totalBarWidth / 2);
                startBracket.style.left = startBracketPos + (startBracket.resolvedStyle.width / 2);
            }
            else if (!highSelected)
            {
                endBracketPos = lowBar.resolvedStyle.width + mediumBar.resolvedStyle.width - (totalBarWidth / 2);
                endBracket.style.left = endBracketPos - (endBracket.resolvedStyle.width / 2);
            }
        }
    }

    private void SelectPurities()
    {
        if (startBracketPos < lowWidth - (totalBarWidth / 2) - startBracket.resolvedStyle.width / 2)
        {
            lowSelected = true;

            if (endBracketPos > lowWidth + mediumWidth + highWidth - (totalBarWidth / 2) - endBracket.resolvedStyle.width - 16) //16 is for buffer zone
            {
                mediumSelected = true;
                highSelected = true;
            }
            else if (endBracketPos > lowWidth + mediumWidth - (totalBarWidth / 2) - endBracket.resolvedStyle.width)
            {
                mediumSelected = true;
                highSelected = false;
            }
            else
            {
                mediumSelected = false;
                highSelected = false;
            }
        }
        else if (startBracketPos >= lowWidth - (totalBarWidth / 2) && startBracketPos <= lowWidth + mediumWidth - (totalBarWidth / 2) - startBracket.resolvedStyle.width / 2)
        {
            lowSelected = false;

            if (endBracketPos > lowWidth + mediumWidth + highWidth - (totalBarWidth / 2) - endBracket.resolvedStyle.width - 16)
            {
                mediumSelected = true;
                highSelected = true;
            }
            else
            {
                mediumSelected = true;
                highSelected = false;
            }
        }
        else if (startBracketPos > lowWidth + mediumWidth - (totalBarWidth / 2) - startBracket.resolvedStyle.width / 2)
        {
            highSelected = true;
            mediumSelected = false;
            lowSelected = false;
        }
    }

    private void OpenMarket()
    {
        if (controller.CurrentState == GameStates.TurnEnd || controller.CurrentState == GameStates.Paused)
            return;

        marketTemplate = marketAsset.Instantiate();
        marketTemplate.style.position = Position.Absolute;
        document.rootVisualElement.Q<VisualElement>("Base").Add(marketTemplate);

        SetUpMarket();

        document.rootVisualElement.Q<Label>("Money").visible = false;

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

        lowBar = marketTemplate.Q<VisualElement>("Low");
        mediumBar = marketTemplate.Q<VisualElement>("Medium");
        highBar = marketTemplate.Q<VisualElement>("High");

        startBracket = marketTemplate.Q<VisualElement>("StartBracket");
        endBracket = marketTemplate.Q<VisualElement>("EndBracket");
        bar = marketTemplate.Q<VisualElement>("Bar");
        startBracket.style.left = -385;
        endBracket.style.left = 385;
        startBracket.RegisterCallback<PointerDownEvent>(DragBracket);
        endBracket.RegisterCallback<PointerDownEvent>(DragBracket);

        exit = marketTemplate.Q<VisualElement>("Close");
        exit.AddManipulator(new Clickable(() => CloseMarket()));


        //Set cost and change labels
        SetAllLabels();
        UpdateBarRatio();

        Wildflower.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Wildflower);
        Clover.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Clover);
        Alfalfa.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Alfalfa);
        Buckwheat.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Buckwheat);
        Fireweed.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Fireweed);
        Goldenrod.RegisterCallback<PointerDownEvent, FlowerType>(Select, FlowerType.Goldenrod);

        SelectHoney(Wildflower, FlowerType.Wildflower);

        //Assign button callbacks
        sell1.clickable = new Clickable( e => Sell(1));
        sell10.clickable = new Clickable(e => Sell(10));
        sell50.clickable = new Clickable(e => Sell(50));
        buy1.clickable = new Clickable(e => Buy(1));
        buy10.clickable = new Clickable(e => Buy(10));
        buy50.clickable = new Clickable(e => Buy(50));

        SelectPurities();

        marketTemplate.Q<Label>("MoneyLabel").text = "$" + player.Money;
    }

    private void SetAllLabels()
    {
        Debug.Log("setting labels");
        SetLabel(Wildflower, FlowerType.Wildflower);
        SetLabel(Clover, FlowerType.Clover);
        SetLabel(Alfalfa, FlowerType.Alfalfa);
        SetLabel(Buckwheat, FlowerType.Buckwheat);
        SetLabel(Fireweed, FlowerType.Fireweed);
        SetLabel(Goldenrod, FlowerType.Goldenrod);
        SetAmountLabel();
    }

    private void SetLabel(VisualElement element, FlowerType fType)
    {
        string cost = marketValues[fType][0].ToString();
        if (cost.IndexOf('.') + 3 < cost.Length)
            element.Q<Label>("Cost").text = "$" + cost.Substring(0, cost.IndexOf('.') + 3);
        else
            element.Q<Label>("Cost").text = "$" + cost;

        //element.Q<Label>("Cost").text = "$" + marketValues[fType][0];
        string amount = player.inventory[fType][0].ToString();
        Debug.Log(amount);
        //Debug.Log(amount);
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
        if (selectedElement != null)
        {
            Debug.Log("setting amount label");
            amountLabel.style.backgroundColor = new Color(0.26f, 0.26f, 0.26f);
            float amount = 0;
            if (lowSelected)
                amount += player.inventory[selectedType][1];
            if (mediumSelected)
                amount += player.inventory[selectedType][2];
            if (highSelected)
                amount += player.inventory[selectedType][3];
            string amountString = amount.ToString();
            if (amountString.IndexOf('.') + 3 < amountString.Length)
                amountLabel.text = "You have \n" + amountString.Substring(0, amountString.IndexOf('.') + 3) + " lbs. selected";
            else
                amountLabel.text = "You have \n" + amountString + " lbs. selected";
            Debug.Log("Selected:" + amount);
        }
    }

    private void UpdateBarRatio()
    {
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

    private void DragBracket(PointerDownEvent e)
    {
        if (e.currentTarget as VisualElement == startBracket)
            draggingStart = true;
        else
            draggingeEnd = true;
    }

    private void Select(PointerDownEvent e, FlowerType fType)
    {
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

        if (player.inventory[selectedType][0] < amount)
            amount = player.inventory[selectedType][0];
        player.Money = Mathf.RoundToInt(amount * price);
        float toBePaid = amount;

        if (lowSelected)
            toBePaid = SellLow(toBePaid);
        if (mediumSelected && toBePaid > 0)
            toBePaid = SellMedium(toBePaid);
        if (highSelected && toBePaid > 0)
            toBePaid = SellHigh(toBePaid);

        Debug.Log("Amount: " + amount + " toBePaid: " + toBePaid);
        Debug.Log("Subtracting " + (amount - toBePaid));
        player.inventory[selectedType][0] -= amount - toBePaid;


        //implement bar ratio calculation for player honey purities

        SetAllLabels();
        UpdateBarRatio();
        StartCoroutine(AdjustBrackets());
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
            player.inventory[selectedType][1] -= amount;
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
            player.inventory[selectedType][1] -= amount;
            amount = 0;
        }

        return amount;
    }

    private void Buy(float amount)
    {
        if (selectedElement == null)
            return;
        if (player.Money < amount * price)
            amount = Mathf.RoundToInt(player.Money / price);
        player.Money = -Mathf.RoundToInt(amount * price);
        player.inventory[selectedType][0] += amount;
        player.inventory[selectedType][2] += amount;
        SetAllLabels();
        UpdateBarRatio();
        StartCoroutine(AdjustBrackets());
    }

    public void CloseMarket()
    {
        if (!marketOpen)
            return;

        document.rootVisualElement.Q<VisualElement>("Base").Remove(marketTemplate);
        marketTemplate = null;
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
