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

    private int turn = 0;
    private VisualElement root;
    private CustomVisualElement turnButton;
    private string season = "spring";

    public string Season
    {
        get { return season; }
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

    private void NextTurn()
    {
        turn++;
        player.OnTurnIncrement();
    }
}