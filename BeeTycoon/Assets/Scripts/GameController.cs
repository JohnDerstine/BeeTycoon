using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum GameStates
{
    Menu,
    Start,
    Tutorial,
    Running,
    TurnEnd,
    Paused,
    End,
    Win
}

public class GameController : MonoBehaviour
{
    private GameStates currentState = GameStates.Menu;

    [SerializeField]
    private QueenChooser choices;

    [SerializeField]
    private UnlockTracker unlocks;

    [SerializeField]
    private UIDocument document;

    [SerializeField]
    private VisualTreeAsset mainMenu;

    [SerializeField]
    private VisualTreeAsset gameUI;

    [SerializeField]
    private VisualTreeAsset blankUI;

    [SerializeField]
    private VisualTreeAsset nectarUI;

    [SerializeField]
    private VisualTreeAsset deckUI;

    [SerializeField]
    public VisualTreeAsset tutorialGraphic;

    [SerializeField]
    public VisualTreeAsset tutorialPopup;

    private PlayerController player;
    private HexMenu hexMenu;

    [SerializeField]
    private MapLoader map;

    [SerializeField]
    private HoneyMarket honeyMarket;

    [SerializeField]
    private VisualTreeAsset newTurnUI;

    [SerializeField]
    private VisualTreeAsset quotaScreenUI;

    [SerializeField]
    private VisualTreeAsset techTokenScreenUI;

    [SerializeField]
    private NectarScoring nectar;

    private ToolManager toolManager;

    private GameEventController eventController;

    public delegate void OnTurnNextCalback();
    public event OnTurnNextCalback turnCallback;

    private int turn = 1;
    public int year = 1;
    private CustomVisualElement turnButton;
    private string season = "spring";

    private TemplateContainer quotaContainer;
    private TemplateContainer doubleQuotaContainer;

    private Button newGameButton;
    private Button continueButton;
    private VisualElement techTreeButton;

    private int quota = 25;
    //private float quotaScaling = 1.5f;
    private int previousQuota = 0;

    public bool nectarCollectingFinished;
    public bool flowerAdvanceFinished;
    private bool turnAnimationFinished;
    private bool quotaScreenFinished;
    private bool doubleQuotaScreenFinished;

    private int previousMoney;

    private int techPoints = 4;

    public bool tutorialEnabled = true;

    public bool checkpoint1Reached = false;
    public bool checkpoint2Reached = false;
    public bool checkpoint3Reached = false;
    public bool firstClose = true;

    private List<string> tutorialStrings = new List<string>()
    {
        "Welcome! This is the plot of land you recently inherited from your grandfather. With your debt growing and your previous entrepreneurial ventures failing, you have deemed picking up your grandfathers beekeeping buisness as your last chance.\n\n Your goal is to optimize and grow the plot for your hives, sell honey, and meet your quota deadlines until you reach your end goal.\n\nIn this instance, your end goal will be to reach year 5.\n\n(Click to Continue)",
        "These are queen bees. They dictate many things about a hive! Hovering over the queens will give you more details.\n\nChoose a queen to get started!",
        "Assign the queen to your hive",
        "The hive interface has lots of important information, be sure to check it regularly! The stress tab is especially important to manage. Clicking on the '!' hex will open it",
        "These tabs on the side are your shop. Everything costs vouchers. You earn 1 voucher each turn, per populated hive.",
        "Before you click next turn, use the shovel tool at the bottom to move flowers into your hive's radius. You can check how each flower produces nectar by going to the flower tab and hovering over each one.",
        "This shows you your quota, how many turns left to reach it, and your current progress.",
        "Go to your hive and click any of the harvest buttons to harvest honey.\n\n When approaching Winter, you may not want to choose 'Large Harvest', because bees eat honey to survive!",
        "When you are ready to sell, click the truck, select the type of honey you harvested, and click the amount you want to sell.",
        "Lastly, after reaching a quota, you will get a special shop where you can purchase run modifiers! You can view all modifiers you have by clicking on the Shed."
    };

    public List<string> TutorialStrings
    {
        get { return tutorialStrings; }
    }

    public bool firstPopulate = true;

    private Label description;
    private Label title;
    private int selectedDifficulty = 1;
    private int gameDifficulty = 0;
    private string deck = "Longevity";
    private WinCondition winCon;

    private List<string> Titles = new List<string>()
    {
        "Longevity"
    };
    private Dictionary<string, string> Descriptions = new Dictionary<string, string>()
    {
        {"Longevity", "Reach year *"}
    };
    private Dictionary<string, List<string>> Levels = new Dictionary<string, List<string>>()
    {
        { "Longevity", new List<string>(){ "5", "7", "9" } }
    };

    public int TechPoint
    {
        get { return techPoints; }
        set { techPoints = value; }
    }

    public string Season
    {
        get { return season; }
    }

    public int Quota
    {
        get { return quota; }
        set
        {
            quota = value;
            UpdateLabels();
        }
    }

    public GameStates CurrentState
    {
        get { return currentState; }
        set
        {
            GameStates prev = currentState;
            currentState = value;
            if (value == GameStates.Start && prev != GameStates.Tutorial)
            {
                //StartCoroutine(choices.CreateYearReward());
                StartCoroutine(choices.CreateBundles());
            }

        }
    }

    void Start()
    {
        newGameButton = document.rootVisualElement.Q<Button>("NewGame");
        continueButton = document.rootVisualElement.Q<Button>("Continue");
        techTreeButton = document.rootVisualElement.Q<VisualElement>("TechTree");
        techTreeButton.AddManipulator(new Clickable((e) => GoToTechTree()));
        newGameButton.clickable = new Clickable(e => ChooseDeck());
        if (!SaveSystem.CheckSaveFile())
            continueButton.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        else
            continueButton.clickable = new Clickable(e => ContinueGame());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            SaveSystem.Save();
        if (Input.GetKeyDown(KeyCode.P))
            SaveSystem.Load();
    }

    private void GoToTechTree()
    {
        document.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("TechTree");
        SceneManager.sceneLoaded += OnSceneTechTree;
    }

    public void BackToMain()
    {
        CurrentState = GameStates.Menu;
        SceneManager.LoadScene("MainMenu");
        SceneManager.sceneLoaded += OnSceneMain;
    }

    private void ContinueGame()
    {
        document.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadContinue;
    }

    private void ChooseDeck()
    {
        selectedDifficulty = 1;
        document.visualTreeAsset = deckUI;
        description = document.rootVisualElement.Q<Label>("Description");
        title = document.rootVisualElement.Q<Label>("Title");
        document.rootVisualElement.Q<Label>("Back").AddManipulator(new Clickable((e) => ResetMainMenu()));
        document.rootVisualElement.Q<Label>("Start").AddManipulator(new Clickable((e) => NewGame()));
        document.rootVisualElement.Q<VisualElement>("Hard").AddManipulator(new Clickable((e) => SelectHard()));
        document.rootVisualElement.Q<VisualElement>("Medium").AddManipulator(new Clickable((e) => SelectMedium()));
        document.rootVisualElement.Q<VisualElement>("Easy").AddManipulator(new Clickable((e) => SelectEasy()));
    }

    private void Deselect()
    {
        description.text = Descriptions[title.text];
        document.rootVisualElement.Q<VisualElement>("Hard").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
        document.rootVisualElement.Q<VisualElement>("Hard1").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
        document.rootVisualElement.Q<VisualElement>("Hard2").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
        document.rootVisualElement.Q<VisualElement>("Medium").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
        document.rootVisualElement.Q<VisualElement>("Medium1").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
        document.rootVisualElement.Q<VisualElement>("Easy").style.unityBackgroundImageTintColor = new Color(0.53f, 0.53f, 0.53f);
    }

    private void SelectHard()
    {
        Deselect();
        document.rootVisualElement.Q<VisualElement>("Hard").style.unityBackgroundImageTintColor = Color.white;
        document.rootVisualElement.Q<VisualElement>("Hard1").style.unityBackgroundImageTintColor = Color.white;
        document.rootVisualElement.Q<VisualElement>("Hard2").style.unityBackgroundImageTintColor = Color.white;
        selectedDifficulty = 3;
        description.text = description.text.Replace("*", Levels[title.text][selectedDifficulty - 1]);
    }

    private void SelectMedium()
    {
        Deselect();
        document.rootVisualElement.Q<VisualElement>("Medium").style.unityBackgroundImageTintColor = Color.white;
        document.rootVisualElement.Q<VisualElement>("Medium1").style.unityBackgroundImageTintColor = Color.white;
        selectedDifficulty = 2;
        description.text = description.text.Replace("*", Levels[title.text][selectedDifficulty - 1]);
    }

    private void SelectEasy()
    {
        Deselect();
        document.rootVisualElement.Q<VisualElement>("Easy").style.unityBackgroundImageTintColor = Color.white;
        selectedDifficulty = 1;
        description.text = description.text.Replace("*", Levels[title.text][selectedDifficulty - 1]);
    }

    private void ResetMainMenu()
    {
        document.visualTreeAsset = mainMenu;
        newGameButton = document.rootVisualElement.Q<Button>("NewGame");
        continueButton = document.rootVisualElement.Q<Button>("Continue");
        techTreeButton = document.rootVisualElement.Q<VisualElement>("TechTree");
        techTreeButton.AddManipulator(new Clickable((e) => GoToTechTree()));
        newGameButton.clickable = new Clickable(e => ChooseDeck());
        if (!SaveSystem.CheckSaveFile())
            continueButton.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        else
            continueButton.clickable = new Clickable(e => ContinueGame());
        GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>().ResetToStart();
        year = 1;
        turn = 1;
        quota = 25;
        previousQuota = 0;
        season = "spring";
    }

    private void NewGame()
    {
        gameDifficulty = selectedDifficulty;
        deck = title.text;
        SetWinCondition();
        document.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadNew;
    }

    private void SetWinCondition()
    {
        switch (deck)
        {
            case "Longevity":
                winCon = new WinCondition(() => year == int.Parse(Levels[deck][selectedDifficulty]));
                break;
        }
    }

    private void OnSceneMain(Scene scene, LoadSceneMode mode)
    {
        ResetMainMenu();
    }

    private void OnSceneTechTree(Scene scene, LoadSceneMode mode)
    {
        document.visualTreeAsset = null;
    }

    private void OnSceneLoadNew(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        hexMenu = GameObject.Find("HexMenu").GetComponent<HexMenu>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        eventController = GameObject.Find("EventController").GetComponent<GameEventController>();

        document.visualTreeAsset = gameUI;
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.GameLoaded();
        hexMenu.GameLoaded();
        CurrentState = GameStates.Start;
        ReloadUI();
        SetToolUsesLabels();
        toolManager.TurnReset();

        document.rootVisualElement.Q<Label>("GoalLabel").text = Descriptions[title.text];
        document.rootVisualElement.Q<Label>("GoalLabel").text = document.rootVisualElement.Q<Label>("GoalLabel").text.Replace("*", Levels[title.text][selectedDifficulty - 1]);

        Quota = 25;

        if (unlocks.majorTechs["Bonus"])
            player.RoyalJelly += 2;

        map.GameStart(false);
        nectar.GameStart();
        SceneManager.sceneLoaded -= OnSceneLoadNew;
    }

    private void OnSceneLoadContinue(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        hexMenu = GameObject.Find("HexMenu").GetComponent<HexMenu>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        toolManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        eventController = GameObject.Find("EventController").GetComponent<GameEventController>();
        document.visualTreeAsset = gameUI;
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.GameLoaded();
        hexMenu.GameLoaded();
        CurrentState = GameStates.Running;
        ReloadUI();

        map.GameStart(true);
        nectar.GameStart();
        SceneManager.sceneLoaded -= OnSceneLoadContinue;
    }

    private void ReloadUI()
    {
        turnButton = document.rootVisualElement.Q<CustomVisualElement>("TurnButton");
        turnButton.AddManipulator(new Clickable(e => StartCoroutine(NextTurn())));
        document.rootVisualElement.Q<VisualElement>("Fill").style.height = Mathf.Clamp(((float)player.Money / Quota) * 250, 0 ,250);
        document.rootVisualElement.Q<Label>("TimeLabel").text = (4 - turn).ToString();
    }

    private void UpdateLabels()
    {
        if (document.visualTreeAsset != gameUI)
            return;

        string adjustedSeason = Season.ToString();
        adjustedSeason = adjustedSeason.Substring(0, 1).ToUpper() + adjustedSeason.Substring(1);
        //document.rootVisualElement.Q<Label>("TurnCount").text = adjustedSeason + " " + year + " Turn " + turn;
        document.rootVisualElement.Q<Label>("Quota").text = "Quota: $" + quota;
        int turns = (season == "winter") ? (1 - ((turn - 1) % 4)) : (4 - ((turn - 1) % 4));
        document.rootVisualElement.Q<Label>("Turns").text = "Due in " + turns + " turns";

        document.rootVisualElement.Q<Label>("MeterQuota").text = "$" + quota;
    }

    private bool CheckPopulated()
    {
        foreach (Hive h in player.hives)
            if (h.queen != null && !h.queen.nullQueen)
                return true;
        return false;
    }

    private IEnumerator NextTurn()
    {
        //Don't let player go next turn if the last turn is still processing
        if (CurrentState == GameStates.TurnEnd || CurrentState == GameStates.Paused || CurrentState == GameStates.Tutorial || !CheckPopulated() || !checkpoint2Reached)
            yield break;
        document.GetComponent<AudioSource>().Play();

        player.CenterCamera();

        player.CloseHiveUI(player.currentHive, true);
        hexMenu.CloseTab();
        honeyMarket.CloseMarket();

        CurrentState = GameStates.TurnEnd;
        turn++;
        if (turn == 5)
            turn = 1;

        document.visualTreeAsset = nectarUI;

        StartCoroutine(nectar.GetNectarGains());

        yield return new WaitWhile(() => !nectarCollectingFinished);
        nectarCollectingFinished = false;

        document.visualTreeAsset = blankUI;

        bool newYear = false;
        if ((turn - 1) % 4 == 0 || (season == "winter" && (turn - 1) % 4 == 1))
        {
            switch (season)
            {
                case "spring":
                    season = "summer";
                    break;
                case "summer":
                    season = "fall";
                    break;
                case "fall":
                    season = "winter";
                    map.ClearFlowers();
                    break;
                case "winter":
                    year++;
                    season = "spring";
                    newYear = true;
                    StartCoroutine(map.GenerateFlowers());
                    //quotaScaling += 0.5f;
                    break;
            }

            if (player.Money >= Quota * 2 && year != 1 && Quota != 0)
            {
                StartCoroutine(DoubleQuotaScreen());
                yield return new WaitWhile(() => !doubleQuotaScreenFinished);
                doubleQuotaScreenFinished = false;
            }

            map.SeasonRecolor();
            previousMoney = player.Money;
            player.Money = -Quota / 2;
            if (player.Money < 0)
            {
                CurrentState = GameStates.End;
                StartCoroutine(QuotaScreen());
                yield break;
            }
            else if (winCon.Condition())
            {
                CurrentState = GameStates.Win;
                StartCoroutine(QuotaScreen());
                yield break;
            }

                StartCoroutine(QuotaScreen());
            yield return new WaitWhile(() => !quotaScreenFinished);
            quotaScreenFinished = false;

            if (newYear)
            {
                if (year == 2)
                    map.IncreaseMapSize("left");
                else if (year == 3)
                    map.IncreaseMapSize("right");
                else if (year == 4)
                    map.IncreaseMapSize("down");

                foreach (Hive h in player.hives)
                    h.hasInsulation = false;
            }
            else
            {
                choices.isChoosing = true;
                choices.LoadShop();
                yield return new WaitWhile(() => choices.isChoosing);


                //choices.isChoosing = true;
                //StartCoroutine(choices.GiveChoice(3, false, false));
                //yield return new WaitWhile(() => choices.isChoosing);
            }
        }
        else
            Quota = quota;

        StartCoroutine(NewTurnAnimation());
        yield return new WaitWhile(() => !turnAnimationFinished);
        turnAnimationFinished = false;
        int temp = quota;
        if ((turn - 1) % 4 == 0 || newYear)
        {
            if (newYear)
                quota = previousQuota; //Avoid looping on 0 quota
            else
                previousQuota = quota;

                Quota = Quota * 2;
            //Quota = (int)(quotaScaling * Quota);
        }
        if (season == "winter")
            Quota = 0;
        //if (year == 1 && season == "summer")
        //    Quota = 25;

        if (turnCallback != null)
            turnCallback();

        StartCoroutine(eventController.SpawnMapEvent());
        yield return new WaitWhile(() => !eventController.allComplete);
        eventController.allComplete = false;

        player.CenterCamera();

        //Here is where I should do the animations on flower advance saved tiles
        //first do flowers dying
        //then do flower growing
        map.AdvanceFlowerStates();
        yield return new WaitWhile(() => !flowerAdvanceFinished);
        flowerAdvanceFinished = false;

        document.visualTreeAsset = gameUI;
        ReloadUI();
        player.ReloadUI();
        hexMenu.ReloadUI();
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.ReloadUI();
        UpdateLabels();
        player.OnTurnIncrement();
        SetToolUsesLabels();

        toolManager.TurnReset();

        CurrentState = GameStates.Running;
        checkpoint3Reached = true;
    }

    private void SetToolUsesLabels()
    {
        VisualElement shovelElem = document.rootVisualElement.Q<VisualElement>("Shovel");
        VisualElement dollyElem = document.rootVisualElement.Q<VisualElement>("Dolly");
        VisualElement smokerElem = document.rootVisualElement.Q<VisualElement>("Smoker");
        VisualElement hiveToolElem = document.rootVisualElement.Q<VisualElement>("Hivetool");

        VisualElement[] toolElems = new VisualElement[4] { shovelElem, dollyElem, smokerElem, hiveToolElem };
        ToolScript[] tools = new ToolScript[4] { toolManager.shovel, toolManager.dolly, toolManager.smoker, toolManager.hiveTool};

        for (int i = 0; i < toolElems.Length; i++)
        {
            if (tools[i].Level > 0)
            {
                toolElems[i].style.unityBackgroundImageTintColor = Color.white;
                toolElems[i].Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = Color.white;
            }
            else
            {
                toolElems[i].style.unityBackgroundImageTintColor = new Color(0.57f, 0.57f, 0.57f);
                toolElems[i].Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = new Color(0.57f, 0.57f, 0.57f);
            }
        }
    }

    private IEnumerator DoubleQuotaScreen()
    {
        doubleQuotaContainer = techTokenScreenUI.Instantiate();
        doubleQuotaContainer.style.position = Position.Absolute;
        doubleQuotaContainer.style.width = Screen.width;
        doubleQuotaContainer.style.height = Screen.height;
        doubleQuotaContainer.Q<Button>("Accept").clicked += AcceptDouble;
        doubleQuotaContainer.Q<Button>("Decline").clicked += DeclineDouble;
        doubleQuotaContainer.Q<Label>("QuotaLabel").text = "Quota: $" + Quota + " > $" + Quota * 2;
        document.rootVisualElement.Q<VisualElement>("Base").Add(doubleQuotaContainer);

        yield return null;
    }

    private void DeclineDouble()
    {
        document.rootVisualElement.Q<VisualElement>("Base").Remove(doubleQuotaContainer);
        doubleQuotaScreenFinished = true;
    }

    private void AcceptDouble()
    {
        Quota *= 2;
        TechPoint++;
        document.rootVisualElement.Q<VisualElement>("Base").Remove(doubleQuotaContainer);
        doubleQuotaScreenFinished = true;
    }

    private IEnumerator QuotaScreen()
    {

        quotaContainer = quotaScreenUI.Instantiate();
        quotaContainer.style.position = Position.Absolute;
        quotaContainer.style.width = Screen.width;
        quotaContainer.style.height = Screen.height;
        quotaContainer.Q<Button>().clicked += NextButton;

        if (currentState == GameStates.Win)
        {
            quotaContainer.Q<Label>("Outcome").text = "<color=white><gradient=\"TurnText\">You Win!</gradient></color>";
            quotaContainer.Q<Label>("QuotaResult").text = "<color=green>$" + previousMoney + "</color> / <color=yellow>$" + previousQuota;
            quotaContainer.Q<Label>("MoneyEarned").text = "Money Earned: <indent=80%>$" + player.moneyEarned;
            quotaContainer.Q<Label>("MoneySpent").text = "Money Spent: <indent=80%>$" + Mathf.Abs(player.moneySpent);
            quotaContainer.Q<Label>("HoneySold").text = "Honey Sold: <indent=80%>" + honeyMarket.GetHoneySold() + " lbs.";
            quotaContainer.Q<Label>("Hives").text = "Hives: <indent=80%>" + player.PopulatedHives;
            quotaContainer.Q<Button>().text = "End Run";
            quotaContainer.Q<Button>().style.backgroundColor = new Color(0.37f, 0.68f, 0.13f);
        }
        else
        {
            string outcome = (CurrentState == GameStates.End) ? "<color=white><gradient=\"Failure\">Failed</gradient></color>" : "<color=white><gradient=\"TurnText\">Reached!</gradient></color>";
            quotaContainer.Q<Label>("Outcome").text = outcome;
            quotaContainer.Q<Label>("QuotaResult").text = "<color=green>$" + previousMoney + "</color> / <color=yellow>$" + previousQuota;
            quotaContainer.Q<Label>("MoneyEarned").text = "Money Earned: <indent=80%>$" + player.moneyEarned;
            quotaContainer.Q<Label>("MoneySpent").text = "Money Spent: <indent=80%>$" + Mathf.Abs(player.moneySpent);
            quotaContainer.Q<Label>("HoneySold").text = "Honey Sold: <indent=80%>" + honeyMarket.GetHoneySold() + " lbs.";
            quotaContainer.Q<Label>("Hives").text = "Hives: <indent=80%>" + player.PopulatedHives;
            string nextText = (CurrentState == GameStates.End) ? "End Run" : "Choose Reward";
            quotaContainer.Q<Button>().text = nextText;
            Color color = (CurrentState == GameStates.End) ? new Color(0.68f, 0.31f, 0.13f) : new Color(0.37f, 0.68f, 0.13f);
            quotaContainer.Q<Button>().style.backgroundColor = color;
        }

        document.rootVisualElement.Q<VisualElement>("Base").Add(quotaContainer);
        player.moneyEarned = 0;
        player.moneySpent = 0;
        yield return null;
    }

    private void NextButton()
    {
        document.GetComponent<AudioSource>().Play();
        if (currentState == GameStates.End || currentState == GameStates.Win)
        {
            SaveSystem.DeleteSave();
            BackToMain();
        }

        quotaScreenFinished = true;
        document.rootVisualElement.Q<VisualElement>("Base").Remove(quotaContainer);
    }

    private IEnumerator NewTurnAnimation()
    {
        TemplateContainer temp = newTurnUI.Instantiate();
        Label label = temp.Q<Label>();
        temp.style.position = Position.Absolute;
        temp.style.width = Screen.width;
        temp.style.height = Screen.height;
        label.style.fontSize = 24;
        string adjustedSeason = Season.ToString();
        adjustedSeason = adjustedSeason.Substring(0, 1).ToUpper() + adjustedSeason.Substring(1);
        label.text = "<color=white><gradient=TurnText>" + adjustedSeason + " " + year + " Turn " + turn + "</gradient></color>";
        document.rootVisualElement.Q<VisualElement>("Base").Add(temp);
        yield return new WaitForEndOfFrame();
        while (label.resolvedStyle.fontSize < 172)
        {
            label.style.fontSize = label.resolvedStyle.fontSize + 7f;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.8f);

        while (label.resolvedStyle.fontSize > 24)
        { 
            label.style.fontSize = label.resolvedStyle.fontSize - 12;
            yield return new WaitForSeconds(0.01f);
        }
        document.rootVisualElement.Q<VisualElement>("Base").Remove(temp);
        UpdateLabels();

        turnAnimationFinished = true;
    }

    public void Save(ref GameSaveData data)
    {
        data.quota = quota;
        data.turn = turn;
        data.year = year;
        data.season = season;
    }

    public void Load(GameSaveData data)
    {
        Quota = data.quota;
        turn = data.turn;
        year = data.year;
        season = data.season;
        UpdateLabels();
    }
}

[System.Serializable]
public struct GameSaveData
{
    public int quota;
    public int turn;
    public int year;
    public string season;
}

//Stores the condition to re-evaluate every turn, and determine if the user has won.
public class WinCondition
{
    public WinCondition(Func<bool> condition)
    {
        this.Condition = condition;
    }

    public Func<bool> Condition { get; }
}