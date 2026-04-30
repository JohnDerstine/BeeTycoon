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
        {"Orders", new string[] {"Caucasian", "Carniolan"} },
        {"Himalayan", new string[] {"Orders"} },
        {"Cordovan", new string[] { "Orders" } },
        {"Buckfast", new string[] { "Orders" } },
        {"Killer", new string[] { "Orders" } },
        {"SizeSelect", new string[0]},
        {"Shovel1", new string[] { "SizeSelect" } },
        {"Smoker1", new string[] { "SizeSelect" } },
        {"Dolly1", new string[] { "SizeSelect" } },
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
        {"Rose", new string[] { "FlowerSelect" } },
        {"Lavendar", new string[] { "FlowerSelect" } },
        {"TulipPoplar", new string[] { "Tulip" } },
        {"PitcherPlant", new string[] { "Sundew" } },
        {"Quince", new string[] { "Rose" } },
        {"Hibiscus", new string[] { "Lavendar" } }
    };

    Dictionary<string, string> nodeDescs = new Dictionary<string, string>() {
        {"HoneySelect", "Adds chance for 5 lbs of honey to appear as an option in choice selection screens"},
        {"Carniolan", "A species of bee known for their exception sprint-time build up. 100% increased comb construction in spring"},
        {"Caucasian", "A species of bee known for having the longest tongue and being docile. 10% increased nectar gain, and immune to aggrevated affliction"},
        {"Orders", "WIP: Regular customers will now have specialized orders for you. Completing them will earn growing rewards over time"},
        {"Himalayan", "A rare species of bee that excels at gathering one type of honey. All honey produced is considered high quality"},
        {"Cordovan", "A fully yellow bee that is known for its passiveness. Other hives will not attack these bees"},
        {"Buckfast", "A new species of bee bred to be immune to the varroa mite. Immune to the mites affliction"},
        {"Killer", "A highly aggressive hybrid bee. 200% honey production, but all other hives in their radius recieve the Defending affliction"},
        {"SizeSelect", "Adds chance for plot size increase to appear as an option in choice selection screens. Plots can only be expanded 3 times, once in each direction"},
        {"Shovel1", "Adds chance for an upgrade to the shovel tool to appear as an option in choice selection screens. Increases shovel uses to 5 per turn"},
        {"Smoker1", "Adds chance for an upgrade to the smoker tool to appear as an option in choice selection screens. Increases smoker uses to 2 per turn"},
        {"Dolly1", "Adds chance for an upgrade to the dolly tool to appear as an option in choice selection screens. Increases dolly super carry capacity to 5"},
        {"Shovel2", "Adds chance for an upgrade to the shovel tool to appear as an option in choice selection screens. Increases shovel uses to 7 per turn"},
        {"Smoker2", "Adds chance for an upgrade to the smoker tool to appear as an option in choice selection screens. Smoker calms hive, reducing stress by 1"},
        {"Dolly2", "Adds chance for an upgrade to the dolly tool to appear as an option in choice selection screens. Increases dolly uses to 3 per turn"},
        {"HiveTool1", "Adds chance for an upgrade to the hive tool to appear as an option in choice selection screens. Increases hive tool uses to 2 per turn"},
        {"BeeSuit1", "Adds chance for an upgrade to the bee suit to appear as an option in choice selection screens. Bee suit grants 25% chance to cure a random affliction when harvesting an almost full hive"},
        {"Extractor1", "Adds chance for an upgrade to the extractor tool to appear as an option in choice selection screens. Increases extractor honey harvest multiplier to 25%"},
        {"HiveTool2", "Adds chance for an upgrade to the hive tool to appear as an option in choice selection screens. Allows for removal of a super using hive tool. This increase hive swarm chance"},
        {"BeeSuit2", "Adds chance for an upgrade to the bee suit to appear as an option in choice selection screens. Bee suit grants 50% chance to cure a random affliction when harvesting an almost full hive"},
        {"Extractor2", "Adds chance for an upgrade to the extractor tool to appear as an option in choice selection screens. Removes all comb loss when harvesting"},
        {"FlowerSelect", "Adds chance for 5 free flowers to appear as an option in choice selection screens"},
        {"Tulip", "WIP"},
        {"Sundew", "WIP"},
        {"Composte", "A permament upgrade to your plot. When moving flowers with a shovel, you can remove them permanently by moving them to the composte"},
        {"Rose", "WIP"},
        {"Lavendar", "WIP"},
        {"TulipPoplar", "WIP"},
        {"PitcherPlant", "WIP"},
        {"Quince", "WIP"},
        {"Hibiscus", "WIP"}
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
                return FlowerType.Empty;
            case "TulipPoplar":
                return FlowerType.Empty;
            case "Sundew":
                return FlowerType.Empty;
            case "PitcherPlant":
                return FlowerType.Empty;
            case "Rose":
                return FlowerType.Empty;
            case "Quince":
                return FlowerType.Empty;
            case "Lavendar":
                return FlowerType.Empty;
            case "Hibiscus":
                return FlowerType.Empty;
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
