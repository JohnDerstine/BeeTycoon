using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class QueenChooser : MonoBehaviour
{
    [SerializeField]
    private Texture2D testQueenSprite;

    [SerializeField]
    private VisualTreeAsset queenUI;

    [SerializeField]
    private VisualTreeAsset choicesContainer;

    [SerializeField]
    private UIDocument document;

    [SerializeField] 
    private PlayerController player;

    [SerializeField]
    private GameObject queenPrefab;

    [SerializeField]
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
    Color darkTint = new Color(0.8f, 0.8f, 0.8f, 1f);
    Color lightTint = Color.white;

    private List<QueenBee> queenOptions = new List<QueenBee>();

    public void OnSceneLoaded()
    {
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        tracker = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();

        root = document.rootVisualElement;
        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent>(OnQueenMove);
        quirkExitCallback = new EventCallback<PointerLeaveEvent>(OnQuirkExit);
        quirkEnterCallback = new EventCallback<PointerEnterEvent, string>(OnQuirkEnter);
    }

    public IEnumerator GiveChoice(int choice, bool starter = false)
    {
        isChoosing = true;
        template = choicesContainer.Instantiate();
        container = template.Q<VisualElement>("Container");
        template.style.position = Position.Absolute;
        template.style.flexDirection = FlexDirection.Row;
        template.style.justifyContent = Justify.FlexStart;
        container.style.justifyContent = Justify.SpaceAround;
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);

        StartCoroutine(SpawnChoices(choice, starter));
        yield return new WaitForFixedUpdate(); //Wait for selectionActive to be updated
        yield return new WaitUntil(() => !selectionActive); //Wait for selectionActive to be false until spawning more choices

        document.rootVisualElement.Q<VisualElement>("Base").Remove(template);
        template = null;
        container = null;
        isChoosing = false;
    }

    public IEnumerator GiveChoice(List<int> choices, bool starter = true)
    {
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
            StartCoroutine(SpawnChoices(choices[i], starter));
            yield return new WaitForFixedUpdate(); //Wait for selectionActive to be updated
            yield return new WaitUntil(() => !selectionActive); //Wait for selectionActive to be false until spawning more choices
        }

        document.rootVisualElement.Q<VisualElement>("Base").Remove(template);
        template = null;
        container = null;
        isChoosing = false;
    }

    //Creates Queen Bee choices for the user to select from
    //Takes an input for the number of choices and whether or not this will be the player's starter Queen
    private IEnumerator SpawnChoices(int numChoices, bool starter)
    {
        //Instatiate Queen Objects
        for (int i = 0; i < numChoices; i++)
        {
            GameObject temp = Instantiate(queenPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            queenOptions.Add(temp.GetComponent<QueenBee>());
        }
        yield return new WaitForFixedUpdate(); //Wait a frame for the Queens to be instatiated

        template.Q<Label>("ChooseLabel").text = "Choose 1 of " + numChoices; //Set up instruction text
        if (numChoices == 2)
            template.Q<Label>("Description").text = "This will be added to your shop";

        List<string> possibilites = new List<string>(); //Get a list of the species of bees the player has unlocked
        foreach (KeyValuePair<string, bool> kvp in tracker.species)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }

        //Set up each queen choice
        for (int i = 0; i < numChoices; i++)
        {
            //Decide the queen's species
            if (starter)
            {
                int rand = Random.Range(0, possibilites.Count);
                queenOptions[i].species = possibilites[rand];
                possibilites.RemoveAt(rand);
                if (numChoices == 3) //First queen of game is free.
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
        selectionActive = true;
    }

    private void SelectQueen(int num)
    {
        selectionActive = false;
        for (int i = 0; i < queenOptions.Count; i++)
        {
            if (i != num)
                Destroy(queenOptions[i].gameObject);
            else
                StartCoroutine(player.AddQueen(queenOptions[i]));
        }
        queenOptions.Clear();
        document.rootVisualElement.Q<VisualElement>("Container").Clear();
    }

    private void OnQueenMove(PointerMoveEvent e)
    {
        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        if (target.ContainsPoint(e.localPosition))
            target.style.unityBackgroundImageTintColor = lightTint;
        else
            target.style.unityBackgroundImageTintColor = darkTint;
    }

    private void OnQueenExit(PointerLeaveEvent e)
    {
        CustomVisualElement target = e.currentTarget as CustomVisualElement;
        target.style.unityBackgroundImageTintColor = darkTint;
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