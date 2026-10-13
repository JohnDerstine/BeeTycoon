using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class TechTree : MonoBehaviour
{
    [SerializeField]
    UIDocument document;

    UnlockTracker unlocks;
    GameController gameController;

    [SerializeField]
    VisualTreeAsset tree1;
    [SerializeField]
    VisualTreeAsset tree2;
    [SerializeField]
    VisualTreeAsset tree3;

    [SerializeField]
    VisualTreeAsset tooltip;

    TemplateContainer tooltipContainer;

    [SerializeField]
    Texture2D darkImage;

    [SerializeField]
    Texture2D lightImage;


    [SerializeField]
    List<GameObject> frames = new List<GameObject>();

    List<VisualElement> nodes = new List<VisualElement>();

    Dictionary<string, VisualElement> nodesByName = new Dictionary<string, VisualElement>();

    Dictionary<string, string[]> nodeReqs = new Dictionary<string, string[]>() {
        {"HoneySelect", new string[0]},
        {"Carniolan", new string[] {"HoneySelect"} },
        {"Caucasian", new string[] {"HoneySelect"} },
        {"Bonus", new string[] {"Caucasian", "Carniolan"} },
        {"Himalayan", new string[] { "Bonus" } },
        {"Cordovan", new string[] { "Bonus" } },
        {"Buckfast", new string[] { "Bonus" } },
        {"Killer", new string[] { "Bonus" } },
        {"ToolSelect", new string[0]},
        {"Shovel1", new string[] { "ToolSelect" } },
        {"Smoker1", new string[] { "ToolSelect" } },
        {"Dolly1", new string[] { "ToolSelect" } },
        {"Shovel2", new string[] { "Shovel1" } },
        {"Smoker2", new string[] { "Smoker1" } },
        {"Dolly2", new string[] { "Dolly1" } },
        {"HiveTool1", new string[] { "Shovel2" } },
        {"BeeSuit1", new string[] { "Smoker2" } },
        {"Extractor1", new string[] { "Dolly2" } },
        {"HiveTool2", new string[] { "HiveTool1" } },
        {"BeeSuit2", new string[] { "BeeSuit1" } },
        {"Extractor2", new string[] { "Extractor1" } },
        {"FlowerSelect", new string[0]},
        {"Tulip", new string[] { "FlowerSelect" } },
        {"Sundew", new string[] { "FlowerSelect" } },
        {"Composte", new string[] { "FlowerSelect" } },
        {"Hydrangea", new string[] { "FlowerSelect" } },
        {"Lavendar", new string[] { "FlowerSelect" } },
        {"TulipPoplar", new string[] { "Tulip" } },
        {"PitcherPlant", new string[] { "Sundew" } },
        {"WaterLily", new string[] { "Hydrangea" } },
        {"Hibiscus", new string[] { "Lavendar" } },
        {"HiddenFlower", new string[] {"Hibiscus", "WaterLily", "PitcherPlant", "TulipPoplar" } }
    };

    Dictionary<string, string> nodeDescs = new Dictionary<string, string>() {
        {"HoneySelect", "Adds chance for 5 lbs of honey to appear as an option in the Reward Shop"},
        {"Carniolan", "Carniolan honey bees provide a calming effect to other hives in their radius, reducing the other hives' stress by 1"},
        {"Caucasian", "Caucasian honey bees prefer open space. When over half the tiles in their radius are empty, all flowers in their radius produce double the nectar. This effect persists for other hives who score those flowers"},
        {"Bonus", "Start the game with an extra 2 Royal Jellies."},
        {"Himalayan", "Himalayan honey bees' maximum honey production is increased by 25%, and all honey is treated as pure when there are no other hives in their radiu"},
        {"Cordovan", "Cordovan honey bees recieve -50% maximum honey production. For each other populated hive you own, increase this hive's maixmum honey product by 25%, up to +150%"},
        {"Buckfast", "Buckfast honey bees' honey type and purity are dictated only by the flowers diagonal and adjacent to the hive. All other flowers are scored normally"},
        {"Killer", "Killer honey bees do not halt production at stress level 4. Additionally, increase nectar gained from flowers by 50% for each stress level"},
        {"ToolSelect", "Adds chance for plot size increase to appear as an option in the Reward Shop. Plots can only be expanded 3 times, once in each direction"},
        {"Shovel1", "Adds chance for an upgrade to the shovel tool to appear as an option in the Reward Shop. Increases shovel uses to 5 per turn"},
        {"Smoker1", "Adds chance for an upgrade to the smoker tool to appear as an option in the Reward Shop. Increases smoker uses to 2 per turn"},
        {"Dolly1", "Adds chance for an upgrade to the dolly tool to appear as an option in the Reward Shop. Increases dolly super carry capacity to 5"},
        {"Shovel2", "Adds chance for an upgrade to the shovel tool to appear as an option in the Reward Shop. Increases shovel uses to 7 per turn"},
        {"Smoker2", "Adds chance for an upgrade to the smoker tool to appear as an option in the Reward Shop. Smoker calms hive, reducing stress by 1"},
        {"Dolly2", "Adds chance for an upgrade to the dolly tool to appear as an option in the Reward Shop. Increases dolly uses to 3 per turn"},
        {"HiveTool1", "Adds chance for an upgrade to the hive tool to appear as an option in the Reward Shop. Increases all tool uses by 1 per turn"},
        {"BeeSuit1", "Adds chance for an upgrade to the bee suit to appear as an option in the Reward Shop. Bee suit grants 25% chance to cure a random affliction when harvesting an almost full hive"},
        {"Extractor1", "Adds chance for an upgrade to the extractor tool to appear as an option in the Reward Shop. Increases extractor honey harvest multiplier to 25%"},
        {"HiveTool2", "Adds chance for an upgrade to the hive tool to appear as an option in the Reward Shop. Increases all tool uses by an additional 1 per turn"},
        {"BeeSuit2", "Adds chance for an upgrade to the bee suit to appear as an option in the Reward Shop. Bee suit grants 50% chance to cure a random affliction when harvesting an almost full hive"},
        {"Extractor2", "Adds chance for an upgrade to the extractor tool to appear as an option in the Reward Shop. Increases extractor honey harvest multiplier to 50%"},
        {"FlowerSelect", "Adds chance for 5 free flowers to appear as an option in the Reward Shop"},
        {"Tulip", "Produces double a random honey type. This type changes each season"},
        {"Sundew", "Sundews produce 0.20 lbs of nectar. Any tiles with a sundew counts as 'water'"},
        {"Composte", "A permament upgrade to your plot. When moving flowers with a shovel, you can remove them permanently by moving them to the composte"},
        {"Hydrangea", "Hydrangeas produce 0.30 lbs of nectar, and produce 3x that when adjacent to water."},
        {"Lavendar", "Lavendars produce 0.15 lbs of nectar. If the hive is above 0 stress, apply the soothing condition. Otherwise, produce an additional 0.10 lbs of nectar."},
        {"TulipPoplar", "Tulip Poplar trees take up 4 tiles, but score on all of those tiles. They produce 0.05 lbs of nectar per tile. Extremely high & stable sell price."},
        {"PitcherPlant", "Pitcher Plants produce more nectar the close they are to the hive, capping at 1 lb. This value is then multiplied by the number of adjacent hives + 1. The hive also loses population based on distance, capped at 300 per flower."},
        {"WaterLily", "Water Lillies produce 0.50 lbs of nectar and can be place on water."},
        {"Hibiscus", "Produces 0.20 lbs of nectar for each condition the hive has."},
        {"HiddenFlower", "Add an additional end-of-year reward option that adds a random unused flower from your current stage to your shop."},
    };

    //Position and rotation for enhanced frames
    Vector3 frameStart;
    Vector3 cameraPoint = new Vector3(1.95f, 1.1f, 1);
    Quaternion frameRotationStart;
    Quaternion cameraRotation = Quaternion.Euler(60, -90 , 0);

    GameObject selectedFrame;
    bool inAnimation;
    bool frameOpen;
    bool frameOpenLoop;
    bool fadeStarted;

    //animation speed
    float speed = 1.5f;
    float startTime;
    float duration;

    Painter2D painter;
    EventCallback<PointerEnterEvent> enterCallback;
    EventCallback<PointerMoveEvent> moveCallback;
    EventCallback<PointerLeaveEvent> exitCallback;

    void Awake()
    {
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        enterCallback = new EventCallback<PointerEnterEvent>(OnEnter);
        moveCallback = new EventCallback<PointerMoveEvent>(OnMove);
        exitCallback = new EventCallback<PointerLeaveEvent>(OnExit);
    }

    void Update()
    {
        if (!frameOpen && !inAnimation && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            gameController.BackToMain();

        RunAnimation();

        if (frameOpenLoop && !inAnimation && !fadeStarted)
            StartCoroutine(FadeInUI());

        if (inAnimation)
            return;

        CheckForClick();

        if (frameOpen)
            return;



        GameObject hoveredFrame = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var frame, 1000) && frame.collider.transform.tag == "frame")
        {
            Transform t = frame.collider.transform;
            frame.collider.transform.position = new Vector3(t.position.x, 0.37f, t.position.z);
            hoveredFrame = frame.collider.gameObject;
        }
        else
            hoveredFrame = null;
        
        foreach (GameObject f in frames)
                if (f != hoveredFrame)
                    f.transform.position = new Vector3(f.transform.position.x, 0.27f, f.transform.position.z);
    }

    private void RunAnimation()
    {
        if (inAnimation && !frameOpenLoop)
        {
            EnhanceFrame(selectedFrame, frameStart, cameraPoint, frameRotationStart, cameraRotation);
        }
        else if (inAnimation && frameOpenLoop)
        {
            document.visualTreeAsset = null;
            fadeStarted = false;
            EnhanceFrame(selectedFrame, cameraPoint, frameStart, cameraRotation, frameRotationStart);
        }
    }

    private void GenerateLines(MeshGenerationContext m)
    {
        painter = m.painter2D;
        painter.strokeColor = Color.black;
        painter.lineWidth = 10f;
        painter.lineCap = LineCap.Round;

        foreach (VisualElement node in nodes)
        {
            for (int i = 0; i < nodeReqs[node.name].Length; i++)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(node.resolvedStyle.left + 45, node.resolvedStyle.top + 45));
                painter.LineTo(new Vector2(nodesByName[nodeReqs[node.name][i]].resolvedStyle.left + 45, nodesByName[nodeReqs[node.name][i]].resolvedStyle.top + 45));
                painter.Stroke();
            }
        }
    }

    private IEnumerator FadeInUI()
    {
        nodes.Clear();
        nodesByName.Clear();

        VisualTreeAsset asset = tree1;
        if (selectedFrame == frames[1])
            asset = tree2;
        else if (selectedFrame == frames[2])
            asset = tree3;
        document.visualTreeAsset = asset;

        document.rootVisualElement.Q<Label>("TokenCount").text = gameController.TechPoint.ToString();

        document.rootVisualElement.Q<VisualElement>("root").generateVisualContent += GenerateLines;
        foreach (VisualElement ve in document.rootVisualElement.Q<VisualElement>("root").Children())
        {
            if (ve.name != "lines")
            {
                nodes.Add(ve);
                nodesByName.Add(ve.name, ve);
                if (CheckLocked(ve.name))
                    ve.style.backgroundImage = darkImage;
            }
        }

        fadeStarted = true;
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += 0.01f;
            foreach (VisualElement ve in nodes)
            {
                ve.style.unityBackgroundImageTintColor = new Color(alpha, alpha, alpha, alpha);
                foreach (VisualElement child in ve.Children())
                    child.style.unityBackgroundImageTintColor = new Color(alpha, alpha, alpha, alpha);
            }
            yield return new WaitForSeconds(0.005f);
        }

        foreach (VisualElement ve in nodes)
        {
            ve.AddManipulator(new Clickable(() => UnlockTech(ve)));
            ve.RegisterCallback(enterCallback);
            ve.RegisterCallback(moveCallback);
            ve.RegisterCallback(exitCallback);
        }
    }

    private void OnEnter(PointerEnterEvent e)
    {
        VisualElement target = e.target as VisualElement;
        tooltipContainer = tooltip.Instantiate();
        tooltipContainer.pickingMode = PickingMode.Ignore;
        tooltipContainer.style.position = Position.Absolute;
        tooltipContainer.style.left = Input.mousePosition.x + 20;
        tooltipContainer.style.top = UnityEngine.Screen.height - Input.mousePosition.y + 20;
        tooltipContainer.Q<Label>("Desc").text = nodeDescs[target.name];
        document.rootVisualElement.Add(tooltipContainer);
    }

    private void OnMove(PointerMoveEvent e)
    {
        VisualElement target = e.currentTarget as VisualElement;
        target.style.scale = new StyleScale(new Vector2(1.5f, 1.5f));
    }

    private void OnExit(PointerLeaveEvent e)
    {
        VisualElement target = e.currentTarget as VisualElement;
        target.style.scale = new StyleScale(new Vector2(1, 1));
        if (tooltipContainer != null)
            document.rootVisualElement.Remove(tooltipContainer);
        tooltipContainer = null;
    }

    private bool CheckLocked(string tech)
    {
        if (unlocks.species.ContainsKey(tech) && !unlocks.species[tech])
            return true;
        else if (unlocks.majorTechs.ContainsKey(tech) && !unlocks.majorTechs[tech])
            return true;
        else if (unlocks.toolUpgrades.ContainsKey(tech) && !unlocks.toolUpgrades[tech])
            return true;
        else if (unlocks.Stage12FlowersUnlocked.ContainsKey(StringToFType(tech)) && !unlocks.Stage12FlowersUnlocked[StringToFType(tech)])
            return true;
        else if (unlocks.Stage34FlowersUnlocked.ContainsKey(StringToFType(tech)) && !unlocks.Stage34FlowersUnlocked[StringToFType(tech)])
            return true;
        return false;
    }

    private void UnlockTech(VisualElement tech)
    {
        name = tech.name;
        if (nodeReqs[name].Length > 0)
        {
            foreach (string s in nodeReqs[name])
            {
                if (CheckLocked(s))
                    return;
            }
        }

        if (gameController.TechPoint <= 0 || !CheckLocked(name))
            return;

        //Check each unlock tracker dictionary for correct tech
        if (unlocks.species.ContainsKey(name))
            unlocks.species[name] = true;
        else if (unlocks.majorTechs.ContainsKey(name))
            unlocks.majorTechs[name] = true;
        else if (unlocks.toolUpgrades.ContainsKey(name))
            unlocks.toolUpgrades[name] = true;
        else if (unlocks.Stage12FlowersUnlocked.ContainsKey(StringToFType(name)))
            unlocks.Stage12FlowersUnlocked[StringToFType(name)] = true;
        else if (unlocks.Stage34FlowersUnlocked.ContainsKey(StringToFType(name)))
            unlocks.Stage34FlowersUnlocked[StringToFType(name)] = true;

        tech.style.backgroundImage = lightImage;

        gameController.TechPoint--;
        document.rootVisualElement.Q<Label>("TokenCount").text = gameController.TechPoint.ToString();
    }

    private FlowerType StringToFType(string tech)
    {
        switch (tech)
        {
            case "Tulip":
                return FlowerType.Tulip;
            case "TulipPoplar":
                return FlowerType.TulipPoplar;
            case "Sundew":
                return FlowerType.Sundew;
            case "PitcherPlant":
                return FlowerType.PitcherPlant;
            case "Hydrangea":
                return FlowerType.Hydrangea;
            case "WaterLily":
                return FlowerType.WaterLily;
            case "Lavendar":
                return FlowerType.Lavendar;
            case "Hibiscus":
                return FlowerType.Hibiscus;
            default:
                return FlowerType.Empty;
        }
    }

    private void CheckForClick()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var frame, 1000) && frame.collider.transform.tag == "frame")
        {
            if (Input.GetMouseButtonDown(0) && !frameOpenLoop)
            {
                frameStart = frame.collider.gameObject.transform.position;
                frameRotationStart = frame.collider.gameObject.transform.rotation;
                selectedFrame = frame.collider.gameObject;
                inAnimation = true;
                frameOpen = true;
                startTime = Time.time;
                duration = Vector3.Distance(frameStart, cameraPoint);
            }
            else if (Input.GetMouseButtonDown(1) && frameOpenLoop)
            {
                startTime = Time.time;
                duration = Vector3.Distance(frameStart, cameraPoint);
                inAnimation = true;
            }
        }
        else if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && frameOpenLoop)
        {
            startTime = Time.time;
            duration = Vector3.Distance(frameStart, cameraPoint);
            inAnimation = true;
        }
    }

    private void EnhanceFrame(GameObject frame, Vector3 start, Vector3 end, Quaternion startQ, Quaternion endQ)
    {
        float delta = (Time.time - startTime) * speed;
        float distanceToMove = delta / duration;
        frame.transform.position = Vector3.Lerp(start, end, distanceToMove);
        frame.transform.rotation = Quaternion.Lerp(startQ, endQ, distanceToMove);

        if (frame.transform.position == end)
        {
            if (frameOpenLoop)
                frameOpen = false;
            frameOpenLoop = !frameOpenLoop;
            inAnimation = false;
        }
    }
}
