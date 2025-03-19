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

    private VisualElement root;
    EventCallback<PointerMoveEvent> queenMoveCallback;
    EventCallback<PointerLeaveEvent> queenExitCallback;

    EventCallback<PointerEnterEvent> quirkEnterCallback;
    EventCallback<PointerLeaveEvent> quirkExitCallback;
    Color darkTint = new Color(0.8f, 0.8f, 0.8f, 1f);
    Color lightTint = Color.white;

    void Start()
    {
        root = document.rootVisualElement;
        queenExitCallback = new EventCallback<PointerLeaveEvent>(OnQueenExit);
        queenMoveCallback = new EventCallback<PointerMoveEvent>(OnQueenMove);
        quirkExitCallback = new EventCallback<PointerLeaveEvent>(OnQuirkExit);
        quirkEnterCallback = new EventCallback<PointerEnterEvent>(OnQuirkEnter);
    }

    public IEnumerator GiveChoice(int choice, bool starter = false)
    {
        yield return new WaitUntil(() => !selectionActive);
    }

    public IEnumerator GiveChoice(List<int> choices, bool starter = true)
    {
        isChoosing = true;
        for (int i = 0; i < choices.Count; i++)
        {
            StartCoroutine(SpawnChoices(choices[i], starter));
            yield return new WaitUntil(() => !selectionActive);
        }

        isChoosing = false;
    }

    private IEnumerator SpawnChoices(int numChoices, bool starter)
    {
        List<QueenBee> queens = new List<QueenBee>();

        for (int i = 0; i < numChoices; i++)
        {
            GameObject temp = Instantiate(queenPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            queens.Add(temp.GetComponent<QueenBee>());
        }
        yield return new WaitForFixedUpdate();

        template = choicesContainer.Instantiate();
        VisualElement container = template.Q<VisualElement>("Container");
        template.style.position = Position.Absolute;
        template.style.flexDirection = FlexDirection.Row;
        template.style.justifyContent = Justify.FlexStart;

        List<string> possibilites = new List<string>();
        foreach (KeyValuePair<string, bool> kvp in tracker.species)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }

        for (int i = 0; i < numChoices; i++)
        {
            if (starter)
            {
                int rand = Random.Range(0, possibilites.Count);
                queens[i].species = possibilites[rand];
                possibilites.RemoveAt(rand);
            }

            TemplateContainer temp = queenUI.Instantiate();
            VisualElement popup = temp.Q<VisualElement>("Popup");
            popup.RegisterCallback(queenMoveCallback);
            popup.RegisterCallback(queenExitCallback);
            temp.Q<VisualElement>("Icon").style.backgroundImage = queenSprite;
            temp.Q<Label>("Species").text = "Species: " + queens[i].species;
            temp.Q<Label>("Age").text = "Age: " + queens[i].age.ToString() + " Months";
            temp.Q<Label>("Grade").text = "Grade: " + queens[i].grade.ToString() + "/10";
            foreach (string s in queens[i].quirks)
            {
                Label quirk = new Label();
                quirk.text = s;
                quirk.AddToClassList("Quirk");
                temp.Q<VisualElement>("QuirkContainer").Add(quirk);
                quirk.RegisterCallback(quirkEnterCallback);
                quirk.RegisterCallback(quirkExitCallback);
            }
            container.Add(temp);
        }
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);
        selectionActive = true;
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

    private void OnQuirkEnter(PointerEnterEvent e)
    {
        Debug.Log("Entered");
    }

    private void OnQuirkExit(PointerLeaveEvent e)
    {
        Debug.Log("Exited");
    }
}