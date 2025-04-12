using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    private PlayerController player;

    [SerializeField]
    private MapLoader map;

    private int turn = 0;
    private VisualElement root;
    private CustomVisualElement turnButton;
    private string season = "spring";

    private int quota = 50;

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
            //update UI
        }
    }

    public GameStates CurrentState
    {
        get { return currentState; }
        set
        {
            if (value == GameStates.Start)
            {
                List<int> choiceList = new List<int>() { 3, 2, 2, 2};
                StartCoroutine(choices.GiveChoice(choiceList, true));
            }
        }
    }

    void Awake()
    {
        CurrentState = GameStates.Start;
        root = document.rootVisualElement;
        turnButton = root.Q<CustomVisualElement>("TurnButton");
        turnButton.AddManipulator(new Clickable(e => NextTurn()));
    }

    void Update()
    {
        
    }

    private IEnumerator NextTurn()
    {
        turn++;
        map.GetNectarGains();

        yield return new WaitWhile(() => !nectarCollectingFinished);
        nectarCollectingFinished = false;

        player.OnTurnIncrement();

        if (turn % 4 == 0)
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

        map.AdvanceFlowerStates(); //This should be done after all the animations for GetNectarGains is done.
        yield return new WaitWhile(() => !flowerAdvanceFinished);
        flowerAdvanceFinished = false;
        Debug.Log(season);
    }

    public void EndGame()
    {
        currentState = GameStates.End;
        Debug.Log("Game Over");
    }
}