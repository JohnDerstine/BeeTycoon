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

    private TemplateContainer template;

    public bool isChoosing;

    private bool selectionActive;

    private VisualElement root;

    void Start()
    {
        root = document.rootVisualElement;
    }

    void Update()
    {
        
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
            Debug.Log("Before wait");
            SpawnChoices(choices[i]);
            yield return new WaitUntil(() => !selectionActive);
        }

        isChoosing = false;
    }

    private void SpawnChoices(int numChoices)
    {
        Debug.Log("Reached");
        template = choicesContainer.Instantiate();
        VisualElement container = template.Q<VisualElement>("Container");
        template.style.left = -726;
        for (int i = 0; i < numChoices; i++)
        {
            TemplateContainer temp = queenUI.Instantiate();
            container.Add(temp);
        }
        document.rootVisualElement.Q<VisualElement>("Base").Add(template);
        selectionActive = true;
    }
}