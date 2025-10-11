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
        "Dandelion",
        "Sunflower",
        "Orange Blossom",
        "Daisy",
        "Thistle",
        "Blueberry Blossom",
        "Tupelo",

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
        "Insulation",
        "Emergency Kit",

        "HONEYCYCLE",

        "SEASONS",

        "HONEYMARKET",

        "BEESTATS",
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
        "Goldenrod",
        "Dandelion",
        "Sunflower",
        "Orange Blossom",
        "Daisy",
        "Thistle",
        "Blueberry Blossom",
        "Tupelo",
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
        "Emergency Kit"
    };

    List<string> honeyCycle = new List<string>()
    {
        "HONEYCYCLE"
    };
    List<string> seasons = new List<string>()
    {
        "SEASONS"
    };
    List<string> honeyMarket = new List<string>()
    {
        "HONEYMARKET"
    };
    List<string> beeStats = new List<string>()
    {
        "BEESTATS"
    };

    Dictionary<string, string> descriptions = new Dictionary<string, string>()
    {
        {"AFFLICTIONS", "<b><size=36px><color=white>Afflictions</size><br><br>    Hives can become afflicted with certain conditions over time.</color></b> Every turn, depending on the season, species of bee, and many other factors, a hive may develop an affliction. These afflictions disrupt the colony and their honey production in different ways. Listed below are all the afflictions, what causes them, and what can be done to remedy them."},
        {"Mites", "Mites are blood-sucking parasites that attach to the back of bees. A hive infested with mites is <color=red>50% less efficient</color>. <br><br><color=green>Mite Repellant</color> can be used as a short term remedy."},
        {"Mice", "Mice love to eat honey and comb. During the fall, and the winter especially, mice can find their way into the hive and build a nest, for the cold seasons. <color=red>Every turn a mouse is left in the hive, half a frame of comb is destroyed.</color><br><br>To prevent mice from getting in, give the hive an <color=green>enterance reducer.</color>"},
        {"Glued", "Over time, bees build up a substance called propolis in the hive. This substance acts as glued to keep the hive sealed and insulated. <color=red>Hives that are glued shut can not be accessed by the player.</color> <br><br>This can be remedied by using a <color=green>hive tool.</color>"},
        {"Aggrevated", "Bees can get quite defenseive of their hives to protect their honey and the queen. <color=red>An aggrevated colony can not be accessed by the player.</color> <br><br>This can be remedied by using a <color=green>smoker.</color>"},
        {"Starving", "During the winter, bees live off of their stored honey. <color=red>If the hive runs out of honey, the hive will die.</color> <br><br>During the winter turn, if a hive is starving, the player can relieve pressure by adding <color=green>sugar water.</color> <color=red>This is a last resort and does not gurantee the hive's survival.<br><br>NOTE: If a hive has freezing and <color=yellow>any other affliction</color> at the same time, it is guaranteed to die.</color>"},
        {"Freezing", "During the winter, bees huggle together around the queen to stay warm. <color=red>If the hive's population drops too low and there is no insulation on the hive, the hive will die.</color> <br><br><color=green>Insulation</color> is the best preventative, <color=red>but nothing can be done once the hive has started freezing.</color> A freezing hive still has a <color=green>chance to survive.</color> <br><br><color=red>NOTE: If a hive has starving and <color=yellow>any other affliction</color> at the same time, it is guaranteed to die.</color>"},

        {"SPECIES", "<b><size=36px><color=white>Species</size></color></b><br><br>    There are many different species of honey bees. Each species has evolved and adapted to survive and thrive in their respective environments. These traits can be leveraged in order to reach desired outcomes. <br><br>    In the beginning you will only have access to 3 species: Italian, Russian, and Japanese.<br><b><color=white>More species can be unlocked through the research tree.</color></b>"},
        {"Italian", "Due to the abundance of flora and mild temperatures of Italy, Italian honey bees are better at producing honey than other species. <br><br>Unique species effect: <color=green>25% increased honey production</color>"},
        {"Russian", "The cold winters and abundance of predators have led Russian honey bees to be more aggressive than other species, but also more efficient. <br><br>Unqiue species effect: <color=red> Increased chance of becoming aggrevated</color>, but <color=green>10% increased hive efficiency</color>"},
        {"Japanese", "Japanese honey bees have had to adapt to unique predators and circumstances. This has led Japanese honey bees to become adept at creating their own solutions to problems. <br><br>Unique species effect: <color=green>33% chance to resolve afflictions themselves.</color><br>NOTE: This does not include freezing or starving."},

        {"FLOWERS", "<b><size=36px><color=white>Flowers</size><br><br>    Bees need nectar to create honey, which is collected from flower nearby.</color></b> The type of flowers that the bees collect honey from determine the type of honey that is produced.<br><br>Ex. Clover flowers produce clover honey.<br><br><b><color=white>The closer a flower is to a hive and the more honey it produces, the more influence it has on that hive's produced honey type.</color></b> <br><br>Each type of flower produce <b><color=white>different amounts of nectar</color></b> that vary depending on their position relative to other flowers. Some flowers spread or take over other flowers too!"},
        {"'Wildflower'", "'Wildflower' isn't actually a flower, but just a term to describe honey that is produced from a wide variety of flowers combined."},
        {"Clover", "Clover flowers produce <color=green>0 nectar</color> by themselves. However, they gain <color=green>10 nectar</color> per clover flower <color=yellow>adjacent</color> or <color=yellow>diagonal</color> to themselves."},
        {"Alfalfa", "Alfalfa flowers produce <color=green>0 nectar</color> by themselves. However, they gain <color=green>20 nectar</color> per alfalfa flower <color=yellow>diagonal</color> to themselves."},
        {"Buckwheat", "Buckwheat flowers produce <color=green>15 nectar</color>.<br><br>Buckwheat flowers also posess a <color=green>33% chance to spread</color> to <color=yellow>empty</color> tiles <color=yellow>adjacent</color> to themselves."},
        {"Fireweed", "Fireweed flowers produce <color=green>30 nectar</color>.<br><br>Fireweed flowers also posess a <color=green>50% chance to convert</color> to <color=yellow>non-empty</color> tiles <color=yellow>adjacent</color> to themselves."},
        {"Goldenrod", "Goldenrod flowers produce <color=green>50 nectar</color>."},
        {"Dandelion", "Dadenlions produce <color=green>20 nectar</color>. At the end of the turn, dandelions die and respawn in a new empty tile, with a <color=yellow>10% chance</color> to create a second new dandelion"},
        {"Sunflower", "Sunflowers produce <color=green>30 nectar</color> for each <color=yellow> empty tile</color> adjacent or diagonal to it."},
        {"Orange Blossom", "Orange blossoms produce <color=green>50 nectar</color>. Since these are trees, they take up a 2x2, each tile counting as its own flower."},
        {"Daisy", "Daisies produce <color=green>50 nectar</color> for each <color=yellow>unique flower</color> adjacent or diagonal to it."},
        {"Thistle", "Thistles produce <color=green>0 nectar</color> on their own, however, each turn they will <color=red>kill</color> an adjacent or diagonal flower and produce nectar equal to 3x the amount that flower produced. This flower always scores last."},
        {"Blueberry Blossom", "Blueberry blossoms produce <color=green>180 nectar</color>, but only in <color=yellow>Summer</color>."},
        {"Tupelo", "Tupelo flowers produce <color=green>75 nectar</color>. Since these are trees, they take up a 2x2, each tile counting as its own flower."},

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
        {"Frame", "Each <color=yellow>super</color> has a default amount of 6 frames. Using this upgrade will <color=green>increase the frame count of every <color=yellow>super</color> in the hive by 1</color>, up to 4 additional frames."},
        {"Hive Stand", "The hive stand raises the hive off the ground, making it more accessible. <color=green>Increases hive efficieny by 10%</color>. It also just looks nice."},
        {"Enterance Reducer", "An enterenace reducer reduces the enterance to a smaller hole. This is used to <color=green>cure</color> the <color=yellow>mice affliction</color> and is a <color=yellow>permanent</color> fix."},
        {"Sugar Water", "Sugar water mimics nectar from flowers, which allows the bees to have better nectar collection. <color=green>Increases nectar collection by 250</color>."},
        {"Mite Repellant", "Mite repellant cures the <color=yellow>mites affliction</color>. This <color=red>does not prevent</color> the hive of getting mites again in the future. It will last for <color=green>4 turns</color>."},
        {"Insulation", "Helps insulate the hive in <color=yellow>winter</color>. This <color=green>lowers the chance</color> of a hive to start <color=yellow>freezing</color>."},
        {"Emergency Kit", "Can be used during winter to prevent a hive from <color=red>freezing</color> or <color=red>starving</color>."},

        {"HONEYCYCLE", "<b><size=36px><color=white>Honey Cycle</color></size></b><br><br>    The <color=yellow>Honey Cycle</color> describes the process of constructing comb, collecting nectar, and producing honey. In order to produce honey, the hive must first have comb constructed to store it, and nectar collected. <b><color=white>If a hive has no comb with nectar stored it cannot produce honey yet</color></b>. This means that hives will <color=red>not be able to produce honey</color> for their first turn." +
            "<br><br><b>To construct comb</b>, nothing is required other than the hive having a <color=yellow>queen</color> assigned to it. <br><br><b><color=white>Collecting nectar</color></b> requires there to be flowers on the plot of land. The more flowers, the more nectar is collected per turn. See the <color=yellow>Flowers tab</color> for more information on how to increase nectar gains."},

        {"SEASONS", "<b><size=36px><color=white>Seasons</color></size></b><br><br>    <b><color=white>Different seasons can increase or decrease the likeliness of something to happen, or how productive a hive is.</color></b> <color=green>At the end of each season, a reward is given.</color> <br><br><br>" +
            "<b><color=green>Spring</color></b>: <color=white><br><br>The <color=red>glued affliction</color> is more likely to occur. <br><br>The <color=red>mice affliction</color> is less likely to occur.<br><br> <color=green>Hives build comb at a higher rate</color>.</color>" +
            "<br><br><br><b><color=yellow>Summer</color></b>: <color=white><br><br>The <color=red>mites affliction</color> is more likely to occur. <br><br>The <color=red>aggrevated affliction</color> is less likely to occur.<br><br> <color=green>Hives collect nectar at a higher rate</color>.</color>" +
            "<br><br><br><b><color=orange>Fall</color></b>: <color=white><br><br>The <color=red>aggrevated affliction</color> is more likely to occur. <br><br>The <color=red>glued affliction</color> is less likely to occur.<br><br> <color=green>Hives produce honey at a higher rate</color>.</color>" +
            "<br><br><br><b><color=blue>Winter</color></b>: <color=white><br><br>The <color=red>mice affliction</color> is <b>much</b> more likely to occur. <br><br>The <color=red>starving</color> and <color=red>freezing afflictions</color> can occur. These afflictions are particularly deadly. Hives are not guranteed to survive the winter if they have <b><color=white>either</color></b> of the afflictions. If a hive has <b><color=white>both</color></b> it will die. Hives can be supplemented once per winter, to try and remedy these afflictions, but at a hefty cost.<br><br> <color=green>Completeing the winter gives you access to one of 3 bonus rewards and one of 3 run modifiers</color>.</color>"},

        {"HONEYMARKET", "<b><size=36px><color=white>Honey Market</color></size></b><br><br>    The honey market is where you go to sell your hard-earned honey. <color=white><b>Different types of honey have different price-points they sell for, which change over time as supply and demand changes.</color></b> <br><br>" +
            "You can tell if the price of a honey is trending upwards if it's banner is green, and downwards if it is red. The number at the top of the banner represents the price 1lb of that honey is selling for, and the number at the bottom of the banner represents how many lbs you havel." +
            "After a certain amount of time, the prices for each honey will reset back to their original price. <br><br>" +
            "<color=white><b>You can also choose to buy honey in the honey market</color></b>. This is useful if you need to supplement a hive with honey over the winter, or if you want to invest in a certain type of honey while it's price is low. <br><br>" +
            "Lastly, at the bottom of the honey market there is a multi-colored bar. This represents honey quality. The higher quality your honey is, the more it sells for." +
            "The red portion of the bar represents low quality honey, the yellow represents average quality, and the green represents high quality. A honey's quality is determined by the purity of the honey. <br><br><color=white>For example</color>, if you only have 1 type of flower on your plot, the purity will be 100%."},

        {"BEESTATS", "<b><size=36px><color=white>Bee Stats</color></size></b><br><br>    Each queen bee comes with an array of stats that are randomly generated, as well as some quirks. <color=white><b>To see what a quirk does, hover over the quirk on the queen bee info panel</b></color>. <br><br>" +
            "Each queen bee also comes with a species and a grade. The species effects can be viewed in the <color=yellow>Bee Species tab</color>. Grade is determined by taking an average of the stats of the queen. <color=white><b>This grade is what determines the price of a queen in the shop</b></color>. <br><br>" +
            "<color=white><b>There is no way to directly look at the stat values a queen provides, but you can use the grade as a good estimate.</b></color><br><br> " +
            "Each stat is listed below. <br><br>" +
            "<color=white><b>Construction</b></color>: Affects how much <color=green>comb</color> is constrcuted each turn. <br><br>" +
            "<color=white><b>Collection</b></color>: Affects how much <color=green>nectar</color> is gathered each turn. <br><br>" +
            "<color=white><b>Production</b></color>: Affects how much <color=green>honey</color> is produced each turn. <br><br>" +
            "<color=white><b>Aggressiveness</b></color>: Affects likeliness of a hive being <color=red>aggressive</color>. Reduces chance of <color=red>Mice</color>.<br><br>" +
            "<color=white><b>Resilience</b></color>: Affects how resistant a hive is to succumbing to <color=red>freezing</color> or <color=red>starving</color>. <br><br>"}
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
            activeGlossary.Q<VisualElement>("HoneyCycle").AddManipulator(new Clickable(e => OpenTab("HoneyCycle")));
            activeGlossary.Q<VisualElement>("Seasons").AddManipulator(new Clickable(e => OpenTab("Seasons")));
            activeGlossary.Q<VisualElement>("HoneyMarket").AddManipulator(new Clickable(e => OpenTab("HoneyMarket")));
            activeGlossary.Q<VisualElement>("BeeStats").AddManipulator(new Clickable(e => OpenTab("BeeStats")));
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
        document.GetComponent<AudioSource>().Play();
        //Change shading of tab and unshade previous tab
        if (currentTab != null)
            activeGlossary.Q<VisualElement>(currentTab).style.backgroundColor = new Color(0, 0, 0, 0f);
        currentTab = tabName;
        activeGlossary.Q<VisualElement>(tabName).style.backgroundColor = new Color(0, 0, 0, 0.35f);

        //clear tab items list
        VisualElement list = activeGlossary.Q<ScrollView>("List").Q<VisualElement>(content);
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
            case "HoneyCycle":
                itemsToAdd = honeyCycle;
                break;
            case "Seasons":
                itemsToAdd = seasons;
                break;
            case "HoneyMarket":
                itemsToAdd = honeyMarket;
                break;
            case "BeeStats":
                itemsToAdd = beeStats;
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
                description.style.color = new Color(0.87f, 0.87f, 0.87f);
                description.style.unityFontStyleAndWeight = FontStyle.Normal;
            }

            list.Add(item);
        }
    }

    public void GameLoaded()
    {
        document.rootVisualElement.Q<CustomVisualElement>("GlossaryButton").AddManipulator(new Clickable(() => OpenGlossary("Hive")));
    }
}
