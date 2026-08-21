using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class RunModifiers : MonoBehaviour
{
    public Dictionary<FlowerType, List<string>> flowerAttributes = new Dictionary<FlowerType, List<string>>()
    {
        {FlowerType.Empty, new List<string>() {} },
        {FlowerType.Clover, new List<string>() { "pink", "short"} },
        {FlowerType.Alfalfa, new List<string>() { "purple", "tall"} },
        {FlowerType.Buckwheat, new List<string>() { "white", "tall"} },
        {FlowerType.Dandelion, new List<string>() { "yellow", "short"} },
        {FlowerType.Sunflower, new List<string>() { "yellow", "tall"} },
        {FlowerType.Orange, new List<string>() { "white", "tall", "tree"} },
        {FlowerType.Fireweed, new List<string>() { "pink", "tall"} },
        {FlowerType.Goldenrod, new List<string>() { "yellow", "tall"} },
        {FlowerType.Daisy, new List<string>() { "white", "yellow", "short"} },
        {FlowerType.Thistle, new List<string>() { "purple", "short"} },
        {FlowerType.Blueberry, new List<string>() { "white", "short"} },
        {FlowerType.Tupelo, new List<string>() { "purple", "tall", "tree"} },
    };


    public Dictionary<int, Modifier> allMods = new Dictionary<int, Modifier>();
    public List<Modifier> accquiredMods = new List<Modifier>();

    [SerializeField]
    public List<Texture2D> modSprites = new List<Texture2D>();

    //EXPIRAMENTAL WIP
    //public Dictionary<int, (string, int)> modStats1 = new Dictionary<int, (string, int)>()
    //{
    //    {0, ("# Of Triggers: ", 0) },
    //    {1, ("# Of Triggers: ", 0) },
    //    {2, ("# Of Triggers: ", 0) },
    //    {3, ("# Of Triggers: ", 0) },
    //};

    void Start()
    {
        //Create modifiers
        //All flowers should get one baseBonus modifier and one multBonus modifier
        //Above modifiers work in 1 direction only, flower 1 being the scoring flower.
        //id, sprite, [flower1 flower2], amount of flower 2 required for bonus, direction flower 2 must be in, base, mult

        #region mods
        //flower base
        allMods.Add(0, new FlowerModifier("Clover Clover Bias", modSprites[0], 1, 0, new FlowerType[2] { FlowerType.Clover, FlowerType.Clover }, 6, "adjacent or diagonal", 16, 1));
        allMods.Add(1, new FlowerModifier("Alfalfa Clover Bias", modSprites[1], 1, 1, new FlowerType[2] { FlowerType.Alfalfa, FlowerType.Clover }, 1, "adjacent", 10, 1));
        allMods.Add(2, new FlowerModifier("Buckwheat Thistle Bias", modSprites[2], 1, 2, new FlowerType[2] { FlowerType.Buckwheat, FlowerType.Thistle }, 1, "adjacent", 5, 1));
        allMods.Add(3, new FlowerModifier("Dandelion Goldenrod Bias", modSprites[3], 1, 3, new FlowerType[2] { FlowerType.Dandelion, FlowerType.Goldenrod }, 1, "adjacent or diagonal", 15, 1));
        allMods.Add(4, new FlowerModifier("Sunflower Blueberry Bias", modSprites[4], 1, 4, new FlowerType[2] { FlowerType.Sunflower, FlowerType.Blueberry }, 2, "diagonal", 30, 1));
        allMods.Add(5, new FlowerModifier("Orange Tupelo Bias", modSprites[5], 1, 5, new FlowerType[2] { FlowerType.Orange, FlowerType.Tupelo }, 1, "adjacent", 10, 1));
        allMods.Add(6, new FlowerModifier("Fireweed Orange Bias", modSprites[6], 1, 6, new FlowerType[2] { FlowerType.Fireweed, FlowerType.Orange }, 1, "adjacent or diagonal", 10, 1));
        allMods.Add(7, new FlowerModifier("Goldenrod Alaflfa Bias", modSprites[7], 1, 7, new FlowerType[2] { FlowerType.Goldenrod, FlowerType.Alfalfa }, 2, "adjacent", 10, 1));
        allMods.Add(8, new FlowerModifier("Daisy Sunflower Bias", modSprites[8], 1, 8, new FlowerType[2] { FlowerType.Daisy, FlowerType.Sunflower }, 1, "diagonal", 10, 1));
        allMods.Add(9, new FlowerModifier("Thistle Dandelion Bias", modSprites[9], 1, 9, new FlowerType[2] { FlowerType.Thistle, FlowerType.Dandelion }, 1, "diagonal", 15, 1));
        allMods.Add(10, new FlowerModifier("Blueberry Buckwheat Bias", modSprites[10], 1, 10, new FlowerType[2] { FlowerType.Blueberry, FlowerType.Buckwheat }, 4, "adjacent or diagonal", 14, 1));
        allMods.Add(11, new FlowerModifier("Tupelo Orange Bias", modSprites[11], 1, 11, new FlowerType[2] { FlowerType.Tupelo, FlowerType.Orange }, 1, "adjacent", 10, 1));

        //flower mult
        allMods.Add(12, new FlowerModifier("Clover Orange Symbiosis", modSprites[12], 1, 12, new FlowerType[2] { FlowerType.Clover, FlowerType.Orange }, 2, "adjacent or diagonal", 0, 2));
        allMods.Add(13, new FlowerModifier("Alfalfa Goldenrod Symbiosis", modSprites[13], 1, 13, new FlowerType[2] { FlowerType.Alfalfa, FlowerType.Goldenrod }, 2, "diagonal", 0, 3));
        allMods.Add(14, new FlowerModifier("Buckwheat Fireweed Symbiosis", modSprites[14], 1, 14, new FlowerType[2] { FlowerType.Buckwheat, FlowerType.Fireweed }, 3, "adjacent or diagonal", 0, 3));
        allMods.Add(15, new FlowerModifier("Dandelion Sunflower Symbiosis", modSprites[15], 1, 15, new FlowerType[2] { FlowerType.Dandelion, FlowerType.Sunflower }, 2, "adjacent", 0, 3));
        allMods.Add(16, new FlowerModifier("Sunflower Buckwheat Symbiosis", modSprites[16], 1, 16, new FlowerType[2] { FlowerType.Sunflower, FlowerType.Buckwheat }, 3, "adjacent", 0, 2));
        allMods.Add(17, new FlowerModifier("Orange Alaflfa Symbiosis", modSprites[17], 1, 17, new FlowerType[2] { FlowerType.Orange, FlowerType.Alfalfa }, 2, "diagonal", 0, 2));
        allMods.Add(18, new FlowerModifier("Fireweed Isolation", modSprites[18], 1, 18, new FlowerType[2] { FlowerType.Fireweed, FlowerType.Empty }, 4, "adjacent or diagonal", 0, 4));
        allMods.Add(19, new FlowerModifier("Goldenrod Tupelo Symbiosis", modSprites[19], 1, 19, new FlowerType[2] { FlowerType.Goldenrod, FlowerType.Tupelo }, 6, "adjacent or diagonal", 0, 4));
        allMods.Add(20, new FlowerModifier("Daisy Clover Symbiosis", modSprites[20], 1, 20, new FlowerType[2] { FlowerType.Daisy, FlowerType.Clover }, 3, "adjacent or diagonal", 0, 3));
        allMods.Add(21, new FlowerModifier("Thistle Blueberry Symbiosis", modSprites[21], 1, 21, new FlowerType[2] { FlowerType.Thistle, FlowerType.Blueberry }, 4, "adjacent", 0, 10));
        allMods.Add(22, new FlowerModifier("Blueberry Sunflower Symbiosis", modSprites[22], 1, 22, new FlowerType[2] { FlowerType.Blueberry, FlowerType.Sunflower }, 1, "adjacent", 0, 2));
        allMods.Add(23, new FlowerModifier("Tupelo Daisy Symbiosis", modSprites[23], 1, 23, new FlowerType[2] { FlowerType.Tupelo, FlowerType.Daisy }, 1, "diagonal", 0, 3));

        //honey mults
        allMods.Add(24, new HoneyModifier("Wildflower Specialty", modSprites[24], 1, 24, FlowerType.Wildflower, 0, 2));
        allMods.Add(25, new HoneyModifier("Clover Specialty", modSprites[25], 1, 25, FlowerType.Clover, 0, 1.5f));
        allMods.Add(26, new HoneyModifier("Alfalfa Specialty", modSprites[26], 1, 26, FlowerType.Alfalfa, 0, 1.5f));
        allMods.Add(27, new HoneyModifier("Buckwheat Specialty", modSprites[27], 1, 27, FlowerType.Buckwheat, 0, 1.5f));
        allMods.Add(28, new HoneyModifier("Dandelion Specialty", modSprites[28], 1, 28, FlowerType.Dandelion, 0, 1.5f));
        allMods.Add(29, new HoneyModifier("Sunflower Specialty", modSprites[29], 1, 29, FlowerType.Sunflower, 0, 1.5f));
        allMods.Add(30, new HoneyModifier("Orange Specialty", modSprites[30], 1, 30, FlowerType.Orange, 0, 1.5f));
        allMods.Add(31, new HoneyModifier("Fireweed Specialty", modSprites[31], 1, 31, FlowerType.Fireweed, 0, 1.25f));
        allMods.Add(32, new HoneyModifier("Goldenrod Specialty", modSprites[32], 1, 32, FlowerType.Goldenrod, 0, 1.25f));
        allMods.Add(33, new HoneyModifier("Daisy Specialty", modSprites[33], 1, 33, FlowerType.Daisy, 0, 1.25f));
        allMods.Add(34, new HoneyModifier("Thistle Specialty", modSprites[34], 1, 34, FlowerType.Thistle, 0, 1.25f));
        allMods.Add(35, new HoneyModifier("Blueberry Specialty", modSprites[35], 1, 35, FlowerType.Blueberry, 0, 1.25f));
        allMods.Add(36, new HoneyModifier("Tupelo Specialty", modSprites[36], 1, 36, FlowerType.Tupelo, 0, 1.25f));

        //order mods
        //bool flower, Flowertype flower, string attribute, string myAttribute, int tiles, bool before, bool after, bool inf, mult, base, retrigger (future), 
        //bool tells whether we need to check for flower type, or flower attribute, then uses the respetive variable
        //int represents how many tiles before it need to match

        //scoring a short before a tall, adds 5 base
        //scoring a tall before a short, multiplies by 1.25x
        //scoring 3 of the same flower multiples next flower by 2x

        //for each yellow flower after this flower, add 1 base 
        //for each pink before this flower, multiply score by 1.25x
        //for each white flower in the hive radius, add 2 base
        //for each purple flower in hive radius, multiply score by 1.1x

        allMods.Add(37, new OrderModifier("Guidance", modSprites[37], 1, 37, false, "short", "tall", 1, false, true, false, 5, 1));
        allMods.Add(38, new OrderModifier("Admiration", modSprites[38], 1, 38, false, "tall", "short", 1, false, true, false, 0, 1.25f));
        allMods.Add(39, new OrderModifier("Block Planting", modSprites[39], 1, 39, true, "", "", 3, false, true, false, 0, 2));
        allMods.Add(40, new OrderModifier("Golden Future", modSprites[40], 1, 40, false, "yellow", "", -1, false, true, true, 2, 1));
        allMods.Add(41, new OrderModifier("Rosy Past", modSprites[41], 1, 41, false, "pink", "", -1, true, false, true, 0, 1.25f));
        allMods.Add(42, new OrderModifier("Bed of Clouds", modSprites[42], 1, 42, false, "white", "", -1, true, true, true, 1, 1));
        allMods.Add(43, new OrderModifier("Royal Carpet", modSprites[43], 1, 43, false, "purple", "", -1, true, true, true, 0, 1.1f));


        #endregion
        #region tools
        //allTools.Add();
        #endregion
        //test
        //Debug.Log(allMods[0].GetType().ToString());
        //AddMod(0);
        //AddMod(1);
        //Debug.Log(GetArchetype<HoneyModifier>()[0].Name);
        //Debug.Log(GetArchetype<HoneyModifier>()[0].Description);
    }

    public void AddMod(int id)
    {
        accquiredMods.Add(allMods[id]); //CHANGE THIS WHEN DONE TESTING
    }

    public List<T> GetArchetypeAccquired<T>() where T : struct
    {
        List<T> modsOfType = new List<T>();
        foreach (Modifier mod in accquiredMods)
        {
            if (mod.GetType() == typeof(T))
                modsOfType.Add((T)mod);
        }
        return modsOfType;
    }

    public List<T> GetArchetypeAll<T>() where T : struct
    {
        List<T> modsOfType = new List<T>();
        foreach (KeyValuePair<int, Modifier> kvp in allMods)
        {
            if (kvp.Value.GetType() == typeof(T))
                modsOfType.Add((T)kvp.Value);
        }
        return modsOfType;
    }
}

public interface Modifier
{
    public string Name
    {
        get;
    }

    public string Description
    {
        get;
    }

    public int Rarity
    {
        get;
    }

    public Texture2D Sprite
    {
        get;
    }

    public int ID
    {
        get;
    }
}

public struct FlowerModifier : Modifier
{
    private string name;
    public string Name
    {
        get { return name; }
    }

    private string description;
    public string Description
    {
        get { return description; }
    }

    private int rarity;
    public int Rarity
    {
        get { return rarity; }
    }

    private Texture2D sprite;
    public Texture2D Sprite
    {
        get { return sprite; }
    }

    private int id;
    public int ID
    {
        get { return id; }
    }

    private FlowerType[] flowers;
    public FlowerType[] Flowers
    {
        get { return flowers; }
    }

    private int amount;
    public int Amount
    {
        get { return amount; }
    }

    private string direction;
    public string Direction
    {
        get { return direction; }
    }

    private int baseMod;
    public int BaseMod
    {
        get { return baseMod; }
    }

    private float multMod;
    public float MultMod
    {
        get {  return multMod; }
    }

    private float stat1;
    public float Stat1
    {
        get { return stat1; }
        set { stat1 = value; }
    }

    private float stat2;
    public float Stat2
    {
        get { return stat2; }
        set { stat2 = value; }
    }

    public FlowerModifier(string name, Texture2D sprite, int rarity, int id, FlowerType[] flowers, int amount, string direction, int baseMod, float multMod, int stat1 = 0, int stat2 = 0)
    {
        this.name = name;
        this.sprite = sprite;
        this.rarity = rarity;
        this.id = id;
        this.flowers = flowers;
        this.amount = amount;
        this.direction = direction;
        this.baseMod = baseMod;
        this.multMod = multMod;
        this.stat1 = stat1;
        this.stat2 = stat2;

        string mod = (multMod == 1) ? baseMod + "" : multMod + "x";
        string modType = (multMod == 1) ? "increase" : "multiply";

        description = "When " + amount + " " + flowers[1].ToString() + "s are " + direction + " to a " 
            + flowers[0].ToString() + ", " + modType + " the nectar gain of the " + flowers[0].ToString() + " by " + mod;
    }
}

public struct HoneyModifier : Modifier
{
    private string name;
    public string Name
    {
        get{ return name; }
    }

    private string description;
    public string Description
    {
        get { return description; }
    }

    private int rarity;
    public int Rarity
    {
        get { return rarity; }
    }

    private Texture2D sprite;
    public Texture2D Sprite
    {
        get { return sprite; }
    }

    private int id;
    public int ID
    {
        get { return id; }
    }

    private FlowerType flower;
    public FlowerType Flower
    {
        get { return flower; }
    }

    private float baseMod;
    public float BaseMod
    {
        get { return baseMod; }
    }

    private float multMod;
    public float MultMod
    {
        get { return multMod; }
    }

    private float stat1;
    public float Stat1
    {
        get { return stat1; }
        set { stat1 = value; }
    }

    public HoneyModifier(string name, Texture2D sprite, int rarity, int id, FlowerType flower, float baseMod, float multMod, float stat1 = 0)
    {
        this.name = name;
        this.sprite = sprite;
        this.rarity = rarity;
        this.id = id;
        this.flower = flower;
        this.baseMod = baseMod;
        this.multMod = multMod;
        this.stat1 = stat1;

        string mod = (multMod == 1) ? baseMod + "" : multMod + "x";
        string modType = (multMod == 1) ? "Increase" : "Multiply";

        description = modType + " selling price of high quality " + flower.ToString() + " honey by " + mod;
    }
}

public struct OrderModifier : Modifier
{

    private string name;
    public string Name
    {
        get { return name; }
    }

    private string description;
    public string Description
    {
        get { return description; }
    }

    private int rarity;
    public int Rarity
    {
        get { return rarity; }
    }

    private Texture2D sprite;
    public Texture2D Sprite
    {
        get { return sprite; }
    }

    private int id;
    public int ID
    {
        get { return id; }
    }

    private bool isFlower;
    public bool IsFlower
    {
        get { return isFlower; }
    }

    private string attribute;
    public string Attribute
    {
        get { return attribute; }
    }

    private string myAttribute;
    public string MyAttribute
    {
        get { return myAttribute; }
    }

    private int tiles;
    public int Tiles
    {
        get { return tiles; }
    }

    private bool before;
    public bool Before
    {
        get { return before; }
    }

    private bool after;
    public bool After
    {
        get { return after; }
    }

    private bool inf;
    public bool Inf
    {
        get { return inf; }
    }

    private int baseMod;
    public int BaseMod
    {
        get { return baseMod; }
    }

    private float multMod;
    public float MultMod
    {
        get { return multMod; }
    }

    public OrderModifier(string name, Texture2D sprite, int rarity, int id, bool isFlower, string attribute, string myAttribute, int tiles, bool before, bool after, bool inf, int baseMod, float multMod)
    {
        this.name = name;
        this.sprite = sprite;
        this.rarity = rarity;
        this.id = id;
        this.isFlower = isFlower;
        this.attribute = attribute;
        this.myAttribute = myAttribute;
        this.tiles = tiles;
        this.before = before;
        this.after = after;
        this.inf = inf;
        this.baseMod = baseMod;
        this.multMod = multMod;

        string mod = (multMod == 1) ? baseMod + "" : multMod + "x";
        string modType = (multMod == 1) ? "increase" : "multiply";
        string amount = (tiles == 1) ? "a" : tiles.ToString();
        string plural = (tiles == 1) ? "" : "s";
        string direction = (after) ? "after" : "before";

        if (inf)
        {
            if (after && before)
                direction = "in hive radius";
            description = "For every " + attribute + " flower " + direction + " the scoring flower, " + modType + " the flower's nectar gain by " + mod;
        }
        else
        {
            if (attribute != "")
                description = "When a " + myAttribute + " flower scores " + direction + " " + amount + " " + attribute + " flower" + plural + ", " + modType + " it's nectar gain by " + mod;
            else
                description = "When " + amount + " of the same flower score consecutivly, " + modType + " the next flowers nectar gain by " + mod;
        }

    }
}