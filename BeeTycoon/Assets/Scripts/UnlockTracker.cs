using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class UnlockTracker : MonoBehaviour
{
    public Dictionary<string, bool> species = new Dictionary<string, bool>()
    {
        {"Italian", true},
        {"Russian", true},
        {"Japanese", true},
        {"Carniolan", false}
    };

    public Dictionary<Tool, bool> toolUpgrades = new Dictionary<Tool, bool>()
    {
        {Tool.Smoker, false},
        {Tool.Shovel, false},
        {Tool.Dolly, false},
        {Tool.HiveTool, false},
        {Tool.BeeSuit, false},
        {Tool.Extractor, false}
    };

    //public Dictionary<Tool, bool> toolUpgrades2 = new Dictionary<Tool, bool>()
    //{
    //    {Tool.Smoker, false},
    //    {Tool.Shovel, false},
    //    {Tool.Dolly, false},
    //    {Tool.HiveTool, false},
    //    {Tool.BeeSuit, false},
    //    {Tool.Extractor, false}
    //};

    public Dictionary<FlowerType, bool> Stage12Flowers = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Clover, false},
        {FlowerType.Buckwheat, false},
        {FlowerType.Alfalfa, false},
        {FlowerType.Dandelion, false},
        {FlowerType.Sunflower, false},
        {FlowerType.Orange, false},
    };

    public Dictionary<FlowerType, bool> Stage34Flowers = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Fireweed, false},
        {FlowerType.Goldenrod, false},
        {FlowerType.Daisy, false},
        {FlowerType.Thistle, false},
        {FlowerType.Blueberry, false},
        {FlowerType.Tupelo, false},
    };

    //Add negative quirks that are opposite of 5 base
    public Dictionary<string, bool> quirks = new Dictionary<string, bool>()
    {
        {"Industrious", true},
        {"Greedy", true},
        {"Territorial", true},
        {"Rugged", true},
        {"Agile", true}
    };

    public Dictionary<string, string> quirkDescriptions = new Dictionary<string, string>()
    {
        {"Industrious", "This coloney is 50% more efficient at building comb"},
        {"Greedy", "This coloney is 50% more efficient at producing honey"},
        {"Territorial", "This coloney is 50% better at fighting pests and invaders"},
        {"Rugged", "This coloney is 50% more resistant disease"},
        {"Agile", "This coloney is 50% more efficient at collecting nectar"}
    };

    public Dictionary<string, float> quirkValues = new Dictionary<string, float>()
    {
        {"Industrious", 1.5f},
        {"Greedy", 1.5f},
        {"Territorial", 1.5f},
        {"Rugged", 1.5f},
        {"Agile", 1.5f}
    };

    public List<FlowerType> ownedFlowers = new List<FlowerType>();
    public List<Tool> ownedTools = new List<Tool>();

    private int stage = 0;

    public List<int> GetNextFlowers()
    {
        if (stage == 4)
            return null;

        stage++;

        List<int> availableFlowers = new List<int>();
        if (stage <= 2)
        {
            List<FlowerType> randFlowerOptions = new List<FlowerType>();
            foreach (KeyValuePair<FlowerType, bool> kvp in Stage12Flowers)
            {
                if (!kvp.Value)
                {
                    if (stage == 2 || (stage == 1 && kvp.Key != FlowerType.Orange))
                        randFlowerOptions.Add(kvp.Key);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                int rand = Random.Range(0, randFlowerOptions.Count);
                Debug.Log(rand);
                Debug.Log(randFlowerOptions.Count);
                availableFlowers.Add((int)randFlowerOptions[rand] - 2);
                ownedFlowers.Add(randFlowerOptions[rand]);
                randFlowerOptions.RemoveAt(rand);
            }

            foreach (int i in availableFlowers)
                Stage12Flowers[(FlowerType)i] = true;
        }
        else
        {
            List<FlowerType> randFlowerOptions = new List<FlowerType>();
            foreach (KeyValuePair<FlowerType, bool> kvp in Stage34Flowers)
                if (!kvp.Value)
                    randFlowerOptions.Add(kvp.Key);

            for (int i = 0; i < 3; i++)
            {
                int rand = Random.Range(0, randFlowerOptions.Count);
                availableFlowers.Add((int)randFlowerOptions[rand] - 2);
                ownedFlowers.Add(randFlowerOptions[rand]);
                randFlowerOptions.RemoveAt(rand);
            }

            foreach (int i in availableFlowers)
                Stage34Flowers[(FlowerType)i] = true;
        }

        GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().AddHoneyCards(availableFlowers);
        return availableFlowers;
    }
    
    public List<Tool> GetMaxedTools()
    {
        List<Tool> maxedTools = new List<Tool>();
        foreach (KeyValuePair<Tool, bool> kvp in toolUpgrades)
            if (kvp.Value)
                maxedTools.Add(kvp.Key);
        return maxedTools;
    }
}

//public interface ITool
//{
//    public Tool Tool
//    {
//        get;
//    }

//    public string Description
//    {
//        get;
//    }

//    public Texture2D Sprite
//    {
//        get;
//    }

//    public bool Unlocked
//    {
//        get;
//    }
//}

//public struct Shovel : ITool
//{
//    private Tool tool;
//    public Tool Tool
//    {
//        get { return tool; }
//    }

//    private string description;
//    public string Description
//    {
//        get { return description; }
//    }

//    private Texture2D sprite;
//    public Texture2D Sprite
//    {
//        get { return sprite; }
//    }

//    private bool unlocked;
//    public bool Unlocked
//    {
//        get { return unlocked; }
//    }
//}