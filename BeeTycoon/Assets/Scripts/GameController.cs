using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public enum GameStates
{
    Menu,
    Start,
    Running,
    TurnEnd,
    Paused,
    End
}

public class GameController : MonoBehaviour
{
    private GameStates currentState = GameStates.Menu;

    [SerializeField]
    private QueenChooser choices;

    [SerializeField]
    private UIDocument document;

    [SerializeField]
    private VisualTreeAsset mainMenu;

    [SerializeField]
    private VisualTreeAsset gameUI;

    [SerializeField]
    private VisualTreeAsset nectarUI;

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

    private int turn = 1;
    public int year = 1;
    private CustomVisualElement turnButton;
    private string season = "spring";

    private TemplateContainer quotaContainer;

    private Button newGameButton;
    private Button continueButton;

    private int quota = 0;
    private float quotaScaling = 1.5f;
    private int previousQuota = 0;

    public bool nectarCollectingFinished;
    public bool flowerAdvanceFinished;
    private bool turnAnimationFinished;
    private bool quotaScreenFinished;

    private int previousMoney;

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
            if (value == GameStates.Start)
            {
                List<int> choiceList = new List<int>() { 3, 3, 3};
                StartCoroutine(choices.GiveChoice(choiceList, true));
            }
            currentState = value;
        }
    }

    void Awake()
    {
        newGameButton = document.rootVisualElement.Q<Button>("NewGame");
        continueButton = document.rootVisualElement.Q<Button>("Continue");
        newGameButton.clickable = new Clickable(e => NewGame());
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

    private void ContinueGame()
    {
        document.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadContinue;
    }

    private void NewGame()
    {
        document.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadNew;
    }

    private void OnSceneLoadNew(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        hexMenu = document.gameObject.GetComponent<HexMenu>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        document.visualTreeAsset = gameUI;
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.GameLoaded();
        hexMenu.GameLoaded();
        CurrentState = GameStates.Start;
        ReloadUI();
        Quota = 25;

        map.GameStart(false);
        SceneManager.sceneLoaded -= OnSceneLoadNew;
    }

    private void OnSceneLoadContinue(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        hexMenu = document.gameObject.GetComponent<HexMenu>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        document.visualTreeAsset = gameUI;
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.GameLoaded();
        hexMenu.GameLoaded();
        CurrentState = GameStates.Running;
        ReloadUI();

        map.GameStart(true);
        SceneManager.sceneLoaded -= OnSceneLoadContinue;
    }

    private void ReloadUI()
    {
        turnButton = document.rootVisualElement.Q<CustomVisualElement>("TurnButton");
        turnButton.AddManipulator(new Clickable(e => StartCoroutine(NextTurn())));
    }

    private void UpdateLabels()
    {
        Debug.Log("Updating");
        string adjustedSeason = Season.ToString();
        adjustedSeason = adjustedSeason.Substring(0, 1).ToUpper() + adjustedSeason.Substring(1);
        document.rootVisualElement.Q<Label>("TurnCount").text = adjustedSeason + " " + year + " Turn " + turn;
        document.rootVisualElement.Q<Label>("Quota").text = "Quota: $" + quota;
        int turns = (season == "winter") ? (2 - ((turn - 1) % 4)) : (4 - ((turn - 1) % 4));
        document.rootVisualElement.Q<Label>("Turns").text = "Due in " + turns + " turns";
    }

    private IEnumerator NextTurn()
    {
        //Don't let player go next turn if the last turn is still processing
        if (CurrentState == GameStates.TurnEnd || CurrentState == GameStates.Paused)
            yield break;
        document.GetComponent<AudioSource>().Play();

        player.CenterCamera();

        player.CloseHiveUI(player.currentHive);
        hexMenu.CloseTab();
        honeyMarket.CloseMarket();

        CurrentState = GameStates.TurnEnd;
        turn++;
        if (turn == 5)
            turn = 1;

        document.visualTreeAsset = nectarUI;

        StartCoroutine(map.GetNectarGains());

        yield return new WaitWhile(() => !nectarCollectingFinished);
        nectarCollectingFinished = false;
        player.OnTurnIncrement();

        document.visualTreeAsset = gameUI;
        ReloadUI();
        player.ReloadUI();
        hexMenu.ReloadUI();
        document.GetComponent<Glossary>().GameLoaded();
        honeyMarket.ReloadUI();
        UpdateLabels();

        previousQuota = quota;
        map.AdvanceFlowerStates(); //This should be done after all the animations for GetNectarGains is done.
        yield return new WaitWhile(() => !flowerAdvanceFinished);
        flowerAdvanceFinished = false;

        if ((turn - 1) % 4 == 0 || (season == "winter" && (turn - 1) % 4 == 2))
        {
            bool newYear = false;
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
                    quotaScaling += 0.5f;
                    break;
            }

            foreach (Hive h in player.hives)
            {
                h.queen.age += 3;
                if (h.queen.age >= 56)
                {
                    h.Populate(null);
                }
            }

            map.SeasonRecolor();
            previousMoney = player.Money;
            player.Money = -Quota;
            if (player.Money < 0)
            {
                CurrentState = GameStates.End;
                StartCoroutine(QuotaScreen());
                yield break;
            }

            StartCoroutine(QuotaScreen());
            yield return new WaitWhile(() => !quotaScreenFinished);
            quotaScreenFinished = false;

            if (newYear)
            {
                StartCoroutine(choices.GiveChoice(3, false));
                yield return new WaitWhile(() => choices.isChoosing);
            }
            else
            {
                StartCoroutine(choices.GiveChoice(2, false));
                yield return new WaitWhile(() => choices.isChoosing);
            }
        }
        else
            Quota = quota;

        StartCoroutine(NewTurnAnimation());
        yield return new WaitWhile(() => !turnAnimationFinished);
        turnAnimationFinished = false;

        if ((turn - 1) % 4 == 0)
        {

            Quota = (int)(quotaScaling * Quota);
        }

        CurrentState = GameStates.Running;
    }

    private IEnumerator QuotaScreen()
    {
        quotaContainer = quotaScreenUI.Instantiate();
        quotaContainer.style.position = Position.Absolute;
        quotaContainer.style.width = Screen.width;
        quotaContainer.style.height = Screen.height;
        quotaContainer.Q<Button>().clicked += NextButton;
        string outcome = (CurrentState == GameStates.End) ? "<color=white><gradient=\"Failure\">Failed</gradient></color>" : "<color=white><gradient=\"TurnText\">Reached!</gradient></color>";
        quotaContainer.Q<Label>("Outcome").text = outcome;
        quotaContainer.Q<Label>("QuotaResult").text = "<color=green>$" + previousMoney + "</color> / <color=yellow>$" + previousQuota;
        quotaContainer.Q<Label>("MoneyEarned").text = "Money Earned: <indent=80%>$" + player.moneyEarned;
        quotaContainer.Q<Label>("MoneySpent").text = "Money Spent: <indent=80%>$" + Mathf.Abs(player.moneySpent);
        quotaContainer.Q<Label>("HoneySold").text = "Honey Sold: <indent=80%>" + player.Money + " lbs.";
        quotaContainer.Q<Label>("Hives").text = "Hives: <indent=80%>" + player.HivesCount;
        string nextText = (CurrentState == GameStates.End) ? "End Run" : "Choose Reward";
        quotaContainer.Q<Button>().text = nextText;
        Color color = (CurrentState == GameStates.End) ? new Color(0.68f, 0.31f, 0.13f) : new Color(0.37f, 0.68f, 0.13f);
        quotaContainer.Q<Button>().style.backgroundColor = color;
        document.rootVisualElement.Q<VisualElement>("Base").Add(quotaContainer);
        player.moneyEarned = 0;
        player.moneySpent = 0;
        yield return null;
    }

    private void NextButton()
    {
        document.GetComponent<AudioSource>().Play();
        if (currentState == GameStates.End)
        {
            SaveSystem.DeleteSave();
            ReturnToMainMenu();
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

        yield return new WaitForSeconds(2f);

        while (label.resolvedStyle.fontSize > 24)
        {
            label.style.fontSize = label.resolvedStyle.fontSize - 12;
            yield return new WaitForSeconds(0.01f);
        }
        document.rootVisualElement.Q<VisualElement>("Base").Remove(temp);
        UpdateLabels();

        turnAnimationFinished = true;
    }

    private void ReturnToMainMenu()
    {
        currentState = GameStates.Menu;
        Destroy(map.gameObject);
        Destroy(document.gameObject);
        SceneManager.LoadScene("MainMenu");
        Destroy(gameObject);
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