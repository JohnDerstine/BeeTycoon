using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Glossary : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset glossary;

    [SerializeField]
    VisualTreeAsset glossaryItem;

    [SerializeField]
    UIDocument document;

    [SerializeField]
    List<Texture2D> serializedIcons = new List<Texture2D>();

    TemplateContainer activeGlossary = null;
    VisualElement root;
    string currentTab = "Afflictions";
    const string content = "unity-content-container";

    public bool open;

    #region Descriptions
    List<string> terms = new List<string>()
    {
        "AFFLICTIONS",
        "Mites",
        "Mice",
        "Glued",
        "Aggrevated",
        "Starving",
        "Freezing",

        "SPECIES",
        "Italian",
        "Russian",
        "Japanese",

        "FLOWERS",
        "'Wildflower'",
        "Clover",
        "Alfalfa",
        "Buckwheat",
        "Fireweed",
        "Goldenrod",

        "TOOLS",
        "Hive Tool",
        "Smoker",
        "Dolly",
        "Shovel",
        "Suit",
        "Extractor",

        "HIVE",
        "Beehive",
        "Super",
        "Frame",
        "Hive Stand",
        "Enterance Reducer",
        "Sugar Water",
        "Mite Repellant",
        "Insulation"
    };

    List<string> afflictions = new List<string>()
    {
        "AFFLICTIONS",
        "Mites",
        "Mice",
        "Glued",
        "Aggrevated",
        "Starving",
        "Freezing"
    };

    List<string> species = new List<string>()
    {
        "SPECIES",
        "Italian",
        "Russian",
        "Japanese"
    };

    List<string> flowers = new List<string>()
    {
        "FLOWERS",
        "'Wildflower'",
        "Clover",
        "Alfalfa",
        "Buckwheat",
        "Fireweed",
        "Goldenrod"
    };

    List<string> tools = new List<string>()
    {
        "TOOLS",
        "Hive Tool",
        "Smoker",
        "Dolly",
        "Shovel",
        "Suit",
        "Extractor"
    };

    List<string> hive = new List<string>()
    {
        "HIVE",
        "Beehive",
        "Super",
        "Frame",
        "Hive Stand",
        "Enterance Reducer",
        "Sugar Water",
        "Mite Repellant",
        "Insulation",
    };

    Dictionary<string, string> descriptions = new Dictionary<string, string>()
    {
        {"AFFLICTIONS", "<b><size=36px><color=white>Afflictions</size><br><br>    Hives can become afflicted with certain conditions over time.</color></b> Every turn, depending on the season, species of bee, and many other factors, a hive may develop an affliction. These afflictions disrupt the colony and their honey production in different ways. Listed below are all the afflictions, what causes them, and what can be done to remedy them."},
        {"Mites", "Mites are blood-sucking parasites that attach to the back of bees. A hive infested with mites is <color=red>50% less efficient</color>. <br><br><color=green>Mite Repellant</color> can be used as a short term remedy."},
        {"Mice", "Mice love to eat honey and comb. During the fall, and the winter especially, mice can find their way into the hive and build a nest, for the cold seasons. <color=red>Every turn a mouse is left in the hive, half a frame of comb is destroyed.</color><br><br>To prevent mice from getting in, give the hive an <color=green>enterance reducer.</color>"},
        {"Glued", "Over time, bees build up a substance called propolis in the hive. This substance acts as glued to keep the hive sealed and insulated. <color=red>Hives that are glued shut can not be accessed by the player.</color> <br><br>This can be remedied by using a <color=green>hive tool.</color>"},
        {"Aggrevated", "Bees can get quite defenseive of their hives to protect their honey and the queen. <color=red>An aggrevated colony can not be accessed by the player.</color> <br><br>This can be remedied by using a <color=green>smoker.</color>"},
        {"Starving", "During the winter, bees live off of their stored honey. <color=red>If the hive runs out of honey, the hive will die.</color> <br><br>During the winter turn, if a hive is starving, the player can relieve pressure by adding <color=green>sugar water.</color> <color=red>This is a last resort and does not gurantee the hive's survival.<br><br>NOTE: If a hive is starving and freezing at the same time, it is guaranteed to die next turn.</color>"},
        {"Freezing", "During the winter, bees huggle together around the queen to stay warm. <color=red>If the hive's population drops too low and there is no insulation on the hive, the hive will die.</color> <br><br><color=green>Insulation</color> is the best preventative, <color=red>but nothing can be done once the hive has started freezing.</color> A freezing hive still has a <color=green>chance to survive.</color> <br><br><color=red>NOTE: If a hive is starving and freezing at the same time, it is guaranteed to die next turn.</color>"},

        {"SPECIES", "<b><size=36px><color=white>Species</size></color></b><br><br>    There are many different species of honey bees. Each species has evolved and adapted to survive and thrive in their respective environments. These traits can be leveraged in order to reach desired outcomes. <br><br>    In the beginning you will only have access to 3 species: Italian, Russian, and Japanese.<br><b><color=white>More species can be unlocked through the research tree.</color></b>"},
        {"Italian", "Due to the abundance of flora and mild temperatures of Italy, Italian honey bees are better at producing honey than other species. <br><br>Unique species effect: <color=green>25% increased honey production</color>"},
        {"Russian", "The cold winters and abundance of predators have led Russian honey bees to be more aggressive than other species, but also more efficient. <br><br>Unqiue species effect: <color=red> Increased chance of becoming aggrevated</color>, but <color=green>10% increased hive efficiency</color>"},
        {"Japanese", "Japanese honey bees have had to adapt to unique predators and circumstances. This has led Japanese honey bees to become adept at creating their own solutions to problems. <br><br>Unique species effect: <color=green>33% chance to resolve afflictions themselves.</color><br>NOTE: This does not include freezing or starving."},

        {"FLOWERS", "<b><size=36px><color=white>Flowers</size><br><br>    Bees need nectar to create honey, which is collected from flower nearby.</color></b> The type of flowers that the bees collect honey from determine the type of honey that is produced.<br><br>Ex. Clover flowers produce clover honey.<br><br><b><color=white>The closer a flower is to a hive, the more influence it has on that hive's produced honey type.</color></b> <br><br>Each type of flower produce <b><color=white>different amounts of nectar</color></b> that vary depending on their position relative to other flowers. Some flowers spread or take over other flowers too!"},
        {"'Wildflower'", "'Wildflower' isn't actually a flower, but just a term to describe honey that is produced from a wide variety of flowers combined."},
        {"Clover", "Clover flowers produce <color=green>0 nectar</color> by themselves. However, they gain <color=green>5 nectar</color> per clover flower <color=yellow>adjacent</color> or <color=yellow>diagonal</color> to themselves."},
        {"Alfalfa", "Alfalfa flowers produce <color=green>0 nectar</color> by themselves. However, they gain <color=green>7 nectar</color> per alfalfa flower <color=yellow>diagonal</color> to themselves."},
        {"Buckwheat", "Buckwheat flowers produce <color=green>10 nectar</color>.<br><br>Buckwheat flowers also posess a <color=green>33% chance to spread</color> to <color=yellow>empty</color> tiles <color=yellow>adjacent</color> to themselves."},
        {"Fireweed", "Fireweed flowers produce <color=green>10 nectar</color>.<br><br>Fireweed flowers also posess a <color=green>50% chance to convert</color> to <color=yellow>non-empty</color> tiles <color=yellow>adjacent</color> to themselves."},
        {"Goldenrod", "Goldenrod flowers produce <color=green>15 nectar</color>."},

        {"TOOLS", "<b><size=36px><color=white>Tools</color></size></b><br><br>    As a beekeeper, a variety of tools are needed to be successful. <b><color=white>Once a tool is bought from the shop, you will never have to buy it again in that run.</color></b> Not all tools are necessary from the start, but you will most likely need to use each one at somepoint in order to be succesful. <br><br><b><color=white>To use or purchase a tool, go to the tools tab on the right and click on the tool. You can then click on the target to use it.</color></b>"},
        {"Hive Tool", "The hivetool is a simple prybar-like tool that is used to seperate and open a <color=yellow>hive</color> that has the <color=yellow>glued affliction</color>. It can also be used to remove a <color=yellow>super</color> from a hive.<br><br>Tip: Removing a super will decrease the amount of space in a hive, encouraging a the hive to <color=yellow>swarm</color>."},
        {"Smoker", "A smoker is used to generate smoke and blow it inside of the <color=yellow>hive</color>. This has a calming effect on the bees removing the <color=yellow>aggrevated affliction</color>."},
        {"Dolly", "A dolly is used to move the <color=yellow>hive</color> to another <color=yellow>empty tile</color>."},
        {"Shovel", "A shovel is used to either move <color=yellow>flowers</color> to a different <color=yellow>empty tile</color>, or to <color=red>remove</color> them all together."},
        {"Suit", "A suit is required when working with <color=yellow>bee species</color> that are naturally super aggressive."},
        {"Extractor", "With the extrator purchased, <color=yellow>honey harvesting</color> does not destroy the comb in the process."},

        {"HIVE", "<b><size=36px><color=white>Hive</color></size></b><br><br>    Knowing how to prepare, manage, and care for your hives is your biggest task. <b><color=white>Buying new hives and upgrading old hives will be your primary tool for progressing through the game</color></b>. Below is a list of hive upgrades and remedies for afflictions. <br><br><b><color=white>Note: For information regarding hive mechanics, refer to the mechanics tab.</color></b>"},
        {"Beehive", "This is the <color=yellow>hive</color>. Clicking on a placed hive brings up the hive menu, for that specific hive. This is where you assign your queen bee, track honey production, and harvest."},
        {"Super", "Each hive 'box' is called a <color=yellow>super</color>. Each super holds <color=yellow>frames</color> that the bees will fill out with comb and store honey in. Using this upgrade will add another super, <color=green>increasing the maximum honey storage and population</color> of the hive. A hive can have a max of <color=green>5</color> supers."},
        {"Frame", "Each <color=yellow>super</color> has a default amount of 6 frames. Using this upgrade will <color=green>increase the frame count of every <color=yellow>super</color> in the hive by 1</color>."},
        {"Hive Stand", "The hive stand raises the hive off the ground, making it more accessible. <color=green>Increases hive efficieny by 10%</color>. It also just looks nice."},
        {"Enterance Reducer", "An enterenace reducer reduces the enterance to a smaller hole. This is used to <color=green>cure</color> the <color=yellow>mice affliction</color> and is a <color=yellow>permanent</color> fix."},
        {"Sugar Water", "Sugar water mimics nectar from flowers, which allows the bees to have better nectar collection. <color=green>Increases nectar collection by 500</color>. <color=yellow>Only lasts 1 turn</color>."},
        {"Mite Repellant", "Mite repellant cures the <color=yellow>mites affliction</color>. This <color=red>does not prevent</color> the hive of getting mites again in the future."},
        {"Insulation", "Helps insulate the hive in <color=yellow>winter</color>. This <color=green>lowers the chance</color> of a hive to start <color=yellow>freezing</color>."}
    };

    Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
    #endregion

    void Start()
    {
        for (int i = 0; i < serializedIcons.Count; i++)
            icons.Add(terms[i], serializedIcons[i]);
    }

    public void OpenGlossary(string tabName)
    {
        if (activeGlossary == null)
        {
            root = document.rootVisualElement;
            activeGlossary = glossary.Instantiate();
            activeGlossary.style.position = Position.Absolute;
            activeGlossary.style.width = Screen.width;
            activeGlossary.style.height = Screen.height;
            activeGlossary.Q<VisualElement>("Afflictions").AddManipulator(new Clickable(e => OpenTab("Afflictions")));
            activeGlossary.Q<VisualElement>("Species").AddManipulator(new Clickable(e => OpenTab("Species")));
            activeGlossary.Q<VisualElement>("Flowers").AddManipulator(new Clickable(e => OpenTab("Flowers")));
            activeGlossary.Q<VisualElement>("Tools").AddManipulator(new Clickable(e => OpenTab("Tools")));
            activeGlossary.Q<VisualElement>("Hive").AddManipulator(new Clickable(e => OpenTab("Hive")));
        }
        root.Q<VisualElement>("Base").Add(activeGlossary);
        OpenTab(tabName);
        open = true;
    }

    public void CloseGlossary()
    {
        root.Q<VisualElement>("Base").Remove(activeGlossary);
        open = false;
    }

    private void OpenTab(string tabName)
    {
        //Change shading of tab and unshade previous tab
        if (currentTab != null)
            activeGlossary.Q<VisualElement>(currentTab).style.backgroundColor = new Color(0, 0, 0, 0f);
        currentTab = tabName;
        activeGlossary.Q<VisualElement>(tabName).style.backgroundColor = new Color(0, 0, 0, 0.35f);

        //clear tab items list
        VisualElement list = activeGlossary.Q<VisualElement>(content);
        list.Clear();

        //Add tab items to the list
        List<string> itemsToAdd;
        switch (tabName)
        {
            case "Afflictions":
                itemsToAdd = afflictions;
                break;
            case "Species":
                itemsToAdd = species;
                break;
            case "Flowers":
                itemsToAdd = flowers;
                break;
            case "Tools":
                itemsToAdd = tools;
                break;
            case "Hive":
                itemsToAdd = hive;
                break;
            default:
                itemsToAdd = hive;
                break;
        }


        for (int i = 0; i < itemsToAdd.Count; i++)
        {
            TemplateContainer item = glossaryItem.Instantiate();
            item.Q<Label>("ItemLabel").text = itemsToAdd[i];
            item.Q<Label>("Description").text = descriptions[itemsToAdd[i]];
            item.Q<VisualElement>("ItemIcon").style.backgroundImage = icons[itemsToAdd[i]];

            if (i == 0) //GENERAL information for each list
            {
                VisualElement header = item.Q<VisualElement>("ItemHeader");
                VisualElement container = item.Q<VisualElement>("ItemContainer");
                VisualElement description = item.Q<Label>("Description");
                container.Remove(header);
                container.style.flexGrow = 1;
                container.style.height = StyleKeyword.Auto;
                container.style.paddingTop = 10;
                container.style.marginBottom = 45;
                container.style.backgroundColor = new Color(0.53f, 0.33f, 0.1f, 0.6f);
                description.style.paddingLeft = 25;
                description.style.paddingRight = 25;
                description.style.unityTextAlign = TextAnchor.MiddleLeft;
                description.style.flexBasis = Length.Percent(100);
                description.style.fontSize = 24;
                description.style.color = new Color(0.83f, 0.83f, 0.83f);
                description.style.unityFontStyleAndWeight = FontStyle.Normal;
            }

            list.Add(item);
        }
    }
}
