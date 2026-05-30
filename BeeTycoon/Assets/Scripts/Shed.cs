using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Shed : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset ModifierList;

    [SerializeField]
    VisualTreeAsset Modifier;

    [SerializeField]
    VisualTreeAsset ModifierUI;

    RunModifiers runModifiers;

    TemplateContainer modTemplate;

    UIDocument ui;

    Dictionary<int, VisualElement> modifiers = new Dictionary<int, VisualElement>();
    //Dictionary<int, VisualElement> modifiers = new Dictionary<int, VisualElement>();

    TemplateContainer modHover;

    void Start()
    {
        ui = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        runModifiers = GameObject.Find("GameController").GetComponent<RunModifiers>();
        SetUpTemplate();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            ui.rootVisualElement.Q("Base").Remove(modTemplate);
    }

    private void OnMouseDown()
    {
        ui.rootVisualElement.Q("Base").Add(modTemplate);
        if (modHover == null)
        {
            modHover = ModifierUI.Instantiate();

            int modID = -999;
            foreach (KeyValuePair<int, VisualElement> kvp in modifiers)
                modID = kvp.Key;

            modHover.Q("Icon").style.backgroundImage = runModifiers.modSprites[modID];
            modHover.Q<Label>("Title").text = runModifiers.allMods[modID].Name;
            modHover.Q<Label>("Description").text = runModifiers.allMods[modID].Description;

            modTemplate.Q("HexUI").Add(modHover);
        }
    }

    private void SetUpTemplate()
    {
        modTemplate = ModifierList.Instantiate();

        //Resolved style is NaN until updated
        modTemplate.RegisterCallback((GeometryChangedEvent evt) => {
            modTemplate.style.position = Position.Absolute;
            modTemplate.style.left = (Screen.width - modTemplate.resolvedStyle.width) / 2;
            modTemplate.style.top = (Screen.height - modTemplate.resolvedStyle.height) / 2;
        });
    }

    public void AddModifier(int mod)
    {
        TemplateContainer modContainer = Modifier.Instantiate();
        modContainer.Q("Icon").style.backgroundImage = runModifiers.modSprites[mod];
        modTemplate.Q<VisualElement>("List").Add(modContainer);
        modContainer.Q("Hex").RegisterCallback<PointerEnterEvent>(HoverModifier);
        modifiers.Add(mod, modContainer);
    }

    private void HoverModifier(PointerEnterEvent e)
    {
        if (modHover != null)
            modTemplate.Q("HexUI").Remove(modHover);

        modHover = ModifierUI.Instantiate();

        int modID = -999;
        foreach (KeyValuePair<int, VisualElement> kvp in modifiers)
            if (kvp.Value.Q("Hex") == e.target)
                modID = kvp.Key;

        modHover.Q("Icon").style.backgroundImage = runModifiers.modSprites[modID];
        modHover.Q<Label>("Title").text = runModifiers.allMods[modID].Name;
        modHover.Q<Label>("Description").text = runModifiers.allMods[modID].Description;

        modTemplate.Q("HexUI").Add(modHover);
    }
}