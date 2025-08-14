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
    private PlayerController player;

    [SerializeField]
    private MapLoader map;

    [SerializeField]
    private HoneyMarket honeyMarket;

    private int turn = 1;
    private int year = 1;
    private VisualElement root;
    private CustomVisualElement turnButton;
    private string season = "spring";

    private Button newGameButton;
    private Button continueButton;

    private int quota = 0;

    public bool nectarCollectingFinished;
    public bool flowerAdvanceFinished;

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
                List<int> choiceList = new List<int>() { 3, 2, 2, 2 };
                StartCoroutine(choices.GiveChoice(choiceList, true));
            }
            currentState = value;
        }
    }

    void Awake()
    {
        root = document.rootVisualElement;
        newGameButton = root.Q<Button>("NewGame");
        continueButton = root.Q<Button>("Continue");
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
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadContinue;
    }

    private void NewGame()
    {
        SceneManager.LoadScene("Game");
        SceneManager.sceneLoaded += OnSceneLoadNew;
    }

    private void OnSceneLoadNew(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        document.visualTreeAsset = gameUI;
        CurrentState = GameStates.Start;
        root = document.rootVisualElement;
        turnButton = root.Q<CustomVisualElement>("TurnButton");
        turnButton.AddManipulator(new Clickable(e => StartCoroutine(NextTurn())));
        Quota = 25;

        map.GameStart(false);
    }

    private void OnSceneLoadContinue(Scene scene, LoadSceneMode mode)
    {
        gameObject.GetComponent<QueenChooser>().OnSceneLoaded();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        honeyMarket = GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>();
        document.visualTreeAsset = gameUI;
        CurrentState = GameStates.Running;
        root = document.rootVisualElement;
        turnButton = root.Q<CustomVisualElement>("TurnButton");
        turnButton.AddManipulator(new Clickable(e => StartCoroutine(NextTurn())));

        map.GameStart(true);
    }

    private void UpdateLabels()
    {
        root.Q<Label>("TurnCount").text = "Year " + year + " Turn " + turn;
        root.Q<Label>("Quota").text = "Quota: $" + quota;
        root.Q<Label>("Turns").text = "Due in " + (4 - ((turn - 1) % 4)) + " turns";
    }

    private IEnumerator NextTurn()
    {
        //Don't let player go next turn if the last turn is still processing
        if (CurrentState == GameStates.TurnEnd || CurrentState == GameStates.Paused)
            yield break;

        player.CloseHiveUI(player.currentHive);
        player.CloseTab();
        honeyMarket.CloseMarket();

        CurrentState = GameStates.TurnEnd;
        turn++;
        if (turn == 14)
            turn = 1;

        UpdateLabels();
        StartCoroutine(map.GetNectarGains());

        yield return new WaitWhile(() => !nectarCollectingFinished);
        nectarCollectingFinished = false;

        player.OnTurnIncrement();

        if ((turn - 1) % 4 == 0)
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
                    break;
                case "winter":
                    season = "spring";
                    break;
            }
            player.Money -= Quota;
            if (player.Money < 0)
                EndGame();
            Quota = (int)(1.5f * Quota);
        }
        else
            Quota = quota;

        map.AdvanceFlowerStates(); //This should be done after all the animations for GetNectarGains is done.
        yield return new WaitWhile(() => !flowerAdvanceFinished);
        flowerAdvanceFinished = false;
        CurrentState = GameStates.Running;
    }

    public void EndGame()
    {
        currentState = GameStates.End;
        Debug.Log("Game Over");
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