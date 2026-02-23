using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class RunModifiers : MonoBehaviour
{
    public Dictionary<int, Modifier> allMods = new Dictionary<int, Modifier>();
    public List<Modifier> accquiredMods = new List<Modifier>();

    //private List<ITool> allTools = new List<ITool>();

    [SerializeField]
    private List<Texture2D> modSprites = new List<Texture2D>();

    void Start()
    {
        //Create modifiers
        //All flowers should get one baseBonus modifier and one multBonus modifier
        //Above modifiers work in 1 direction only, flower 1 being the scoring flower.
        //id, sprite, [flower1 flower2], amount of flower 2 required for bonus, direction flower 2 must be in, base, mult
        #region mods
        //flower base
        allMods.Add(0, new FlowerModifier("Clover Clover Bias", modSprites[0], new FlowerType[2] { FlowerType.Clover, FlowerType.Clover }, 6, "adjacent or diagonal", 16, 1));
        allMods.Add(1, new FlowerModifier("Alfalfa Clover Bias", modSprites[1], new FlowerType[2] { FlowerType.Alfalfa, FlowerType.Clover }, 1, "adjacent", 10, 1));
        allMods.Add(2, new FlowerModifier("Buckwheat Thistle Bias", modSprites[2], new FlowerType[2] { FlowerType.Buckwheat, FlowerType.Thistle }, 1, "adjacent", 5, 1));
        allMods.Add(3, new FlowerModifier("Dandelion Goldenrod Bias", modSprites[3], new FlowerType[2] { FlowerType.Dandelion, FlowerType.Goldenrod }, 1, "adjacent or diagonal", 15, 1));
        allMods.Add(4, new FlowerModifier("Sunflower Blueberry Bias", modSprites[4], new FlowerType[2] { FlowerType.Sunflower, FlowerType.Blueberry }, 2, "diagonal", 30, 1));
        allMods.Add(5, new FlowerModifier("Orange Tupelo Bias", modSprites[5], new FlowerType[2] { FlowerType.Orange, FlowerType.Tupelo }, 1, "adjacent", 10, 1));
        allMods.Add(6, new FlowerModifier("Fireweed Orange Bias", modSprites[6], new FlowerType[2] { FlowerType.Fireweed, FlowerType.Orange }, 1, "adjacent or diagonal", 10, 1));
        allMods.Add(7, new FlowerModifier("Goldenrod Alaflfa Bias", modSprites[7], new FlowerType[2] { FlowerType.Goldenrod, FlowerType.Alfalfa }, 2, "adjacent", 10, 1));
        allMods.Add(8, new FlowerModifier("Daisy Sunflower Bias", modSprites[8], new FlowerType[2] { FlowerType.Daisy, FlowerType.Sunflower }, 1, "diagonal", 10, 1));
        allMods.Add(9, new FlowerModifier("Thistle Dandelion Bias", modSprites[9], new FlowerType[2] { FlowerType.Thistle, FlowerType.Dandelion }, 1, "diagonal", 15, 1));
        allMods.Add(10, new FlowerModifier("Blueberry Buckwheat Bias", modSprites[10], new FlowerType[2] { FlowerType.Blueberry, FlowerType.Buckwheat }, 4, "adjacent or diagonal", 14, 1));
        allMods.Add(11, new FlowerModifier("Tupelo Orange Bias", modSprites[11], new FlowerType[2] { FlowerType.Tupelo, FlowerType.Orange }, 1, "adjacent", 10, 1));

        //flower mult
        allMods.Add(12, new FlowerModifier("Clover Orange Symbiosis", modSprites[12], new FlowerType[2] { FlowerType.Clover, FlowerType.Orange }, 2, "adjacent or diagonal", 0, 2));
        allMods.Add(13, new FlowerModifier("Alfalfa Goldenrod Symbiosis", modSprites[13], new FlowerType[2] { FlowerType.Alfalfa, FlowerType.Goldenrod }, 2, "diagonal", 0, 3));
        allMods.Add(14, new FlowerModifier("Buckwheat Fireweed Symbiosis", modSprites[14], new FlowerType[2] { FlowerType.Buckwheat, FlowerType.Fireweed }, 3, "adjacent or diagonal", 0, 3));
        allMods.Add(15, new FlowerModifier("Dandelion Sunflower Symbiosis", modSprites[15], new FlowerType[2] { FlowerType.Dandelion, FlowerType.Sunflower }, 2, "adjacent", 0, 3));
        allMods.Add(16, new FlowerModifier("Sunflower Buckwheat Symbiosis", modSprites[16], new FlowerType[2] { FlowerType.Sunflower, FlowerType.Buckwheat }, 3, "adjacent", 0, 2));
        allMods.Add(17, new FlowerModifier("Orange Alaflfa Symbiosis", modSprites[17], new FlowerType[2] { FlowerType.Orange, FlowerType.Alfalfa }, 2, "diagonal", 0, 2));
        allMods.Add(18, new FlowerModifier("Fireweed Isolation", modSprites[18], new FlowerType[2] { FlowerType.Fireweed, FlowerType.Empty }, 4, "adjacent or diagonal", 0, 4));
        allMods.Add(19, new FlowerModifier("Goldenrod Tupelo Symbiosis", modSprites[19], new FlowerType[2] { FlowerType.Goldenrod, FlowerType.Tupelo }, 6, "adjacent or diagonal", 0, 4));
        allMods.Add(20, new FlowerModifier("Daisy Clover Symbiosis", modSprites[20], new FlowerType[2] { FlowerType.Daisy, FlowerType.Clover }, 3, "adjacent or diagonal", 0, 3));
        allMods.Add(21, new FlowerModifier("Thistle Blueberry Symbiosis", modSprites[21], new FlowerType[2] { FlowerType.Thistle, FlowerType.Blueberry }, 4, "adjacent", 0, 10));
        allMods.Add(22, new FlowerModifier("Blueberry Sunflower Symbiosis", modSprites[22], new FlowerType[2] { FlowerType.Blueberry, FlowerType.Sunflower }, 1, "adjacent", 0, 2));
        allMods.Add(23, new FlowerModifier("Tupelo Daisy Symbiosis", modSprites[23], new FlowerType[2] { FlowerType.Tupelo, FlowerType.Daisy }, 1, "diagonal", 0, 3));

        //honey mults
        allMods.Add(24, new HoneyModifier("Wildflower Specialty", modSprites[24], FlowerType.Wildflower, 0, 2));
        allMods.Add(25, new HoneyModifier("Clover Specialty", modSprites[25], FlowerType.Clover, 0, 1.5f));
        allMods.Add(26, new HoneyModifier("Alfalfa Specialty", modSprites[26], FlowerType.Alfalfa, 0, 1.5f));
        allMods.Add(27, new HoneyModifier("Buckwheat Specialty", modSprites[27], FlowerType.Buckwheat, 0, 1.5f));
        allMods.Add(28, new HoneyModifier("Dandelion Specialty", modSprites[28], FlowerType.Dandelion, 0, 1.5f));
        allMods.Add(29, new HoneyModifier("Sunflower Specialty", modSprites[29], FlowerType.Sunflower, 0, 1.5f));
        allMods.Add(30, new HoneyModifier("Orange Specialty", modSprites[30], FlowerType.Orange, 0, 1.5f));
        allMods.Add(31, new HoneyModifier("Fireweed Specialty", modSprites[31], FlowerType.Fireweed, 0, 1.25f));
        allMods.Add(32, new HoneyModifier("Goldenrod Specialty", modSprites[32], FlowerType.Goldenrod, 0, 1.25f));
        allMods.Add(33, new HoneyModifier("Daisy Specialty", modSprites[33], FlowerType.Daisy, 0, 1.25f));
        allMods.Add(34, new HoneyModifier("Thistle Specialty", modSprites[34], FlowerType.Thistle, 0, 1.25f));
        allMods.Add(35, new HoneyModifier("Blueberry Specialty", modSprites[35], FlowerType.Blueberry, 0, 1.25f));
        allMods.Add(36, new HoneyModifier("Tupelo Specialty", modSprites[36], FlowerType.Tupelo, 0, 1.25f));


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
        accquiredMods.Add(allMods[id]);
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

    public Texture2D Sprite
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

    private Texture2D sprite;
    public Texture2D Sprite
    {
        get { return sprite; }
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

    public FlowerModifier(string name, Texture2D sprite, FlowerType[] flowers, int amount, string direction, int baseMod, float multMod)
    {
        this.name = name;
        this.sprite = sprite;
        this.flowers = flowers;
        this.amount = amount;
        this.direction = direction;
        this.baseMod = baseMod;
        this.multMod = multMod;

        string mod = (multMod == 1) ? baseMod + "" : multMod + "x";
        string modType = (multMod == 1) ? "increase" : "multiply";

        description = "When " + amount + " " + flowers[0].ToString() + "s are " + direction + " to a " 
            + flowers[1].ToString() + ", " + modType + " the nectar gain by " + mod;
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

    private Texture2D sprite;
    public Texture2D Sprite
    {
        get { return sprite; }
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

    public HoneyModifier(string name, Texture2D sprite, FlowerType flower, float baseMod, float multMod)
    {
        this.name = name;
        this.sprite = sprite;
        this.flower = flower;
        this.baseMod = baseMod;
        this.multMod = multMod;

        string mod = (multMod == 1) ? baseMod + "" : multMod + "x";
        string modType = (multMod == 1) ? "Increase" : "Multiply";

        description = modType + " selling price of " + flower.ToString() + " honey by " + mod;
    }
}
