using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    private VisualTreeAsset choicesContainer;

    [SerializeField]
    private UIDocument document;

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

    public void OnSceneLoaded()
    {
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        tracker = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        hexMenu = document.gameObject.GetComponent<HexMenu>();

        root = document.rootVisualElement;
        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent>(OnQueenMove);
        quirkExitCallback = new EventCallback<PointerLeaveEvent>(OnQuirkExit);
        quirkEnterCallback = new EventCallback<PointerEnterEvent, string>(OnQuirkEnter);
        rngOptions = new List<VisualTreeAsset>() { queenUI, flowerUI, honeyUI, sizeUI, toolUI};
    }

    public IEnumerator GiveChoice(int choice, bool starter = false, bool modifier = false)
    {
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
        GameObject.Find("GameController").GetComponent<GameController>().CurrentState = GameStates.Running;
    }

    public IEnumerator GiveChoice(List<int> choices, bool starter = false, bool modifier = false)
    {
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
            if (wasTrue && i == 2)
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

        //Get the options that user will have to choose from
        List<VisualTreeAsset> rngChoices = new List<VisualTreeAsset>();
        if (starter)
        {
            for (int i = 0; rngChoices.Count < numChoices; i++)
                rngChoices.Add(toolUI);
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
                    popup.AddManipulator(new Clickable(e => SelectSize()));
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
                            if (mod.Flowers.Contains(f))
                                applicableMods.Add(mod);
                        }
                    }
                    foreach (HoneyModifier mod in mods.GetArchetypeAll<HoneyModifier>())
                    {
                        foreach (FlowerType f in tracker.ownedFlowers)
                        {
                            if (mod.Flower == f)
                                applicableMods.Add(mod);
                        }
                    }

                    int randID = Random.Range(0, applicableMods.Count());
                    popup.Q<VisualElement>("Icon").style.backgroundImage = applicableMods[randID].Sprite;
                    popup.Q<Label>("Title").text = applicableMods[randID].Name;
                    popup.Q<Label>("Description").text = applicableMods[randID].Description;

                    popup.AddManipulator(new Clickable(e => SelectModifier(randID))); //Looking back on it, this doesn't make sense. I don't want randID, I want the ID of the mod in applicableMods[randID]
                    //Double check to make sure that the game refelcts this being incorrect
                }
                    container.Add(temp);
            }
        }

        yield return new WaitForFixedUpdate();

        template.Q<Label>("ChooseLabel").text = "Choose 1 of " + numChoices; //Set up instruction text
        if (!starter)
            template.Q<Label>("Description").text = "This will be added to your shop";

        rngOptions = new List<VisualTreeAsset>() { queenUI, flowerUI, honeyUI, sizeUI, toolUI };
        selectionActive = true;
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
            temp.Q<Label>("Age").text = "Age: " + queenOptions[i].age.ToString() + " Months";
            temp.Q<Label>("Grade").text = "Grade: " + queenOptions[i].grade.ToString() + "/10";

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

    private void SelectSize()
    {
        selectionActive = false;
        GameObject.Find("MapLoader").GetComponent<MapLoader>().IncreaseMapSize();
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

    private void OnQuirkExit(PointerLeaveEvent e)
    {
        document.rootVisualElement.Remove(activeLabel);
        activeLabel = null;
    }
}