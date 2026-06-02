using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        {"Carniolan", false},
        {"Caucasian", false},
        {"Himalayan", false},
        {"Cordovan", false},
        {"Buckfast", false},
        {"Killer", false},
    };

    public Dictionary<string, bool> majorTechs = new Dictionary<string, bool>()
    {
        {"HoneySelect", false},
        {"FlowerSelect", false},
        {"SizeSelect", false},
        {"Composte", false},
        {"Orders", false},
    };

    public Dictionary<string, bool> toolUpgrades = new Dictionary<string, bool>()
    {
        {"Smoker1", false},
        {"Shovel1", false},
        {"Dolly1", false},
        {"HiveTool1", false},
        {"BeeSuit1", false},
        {"Extractor1", false},
        {"Smoker2", false},
        {"Shovel2", false},
        {"Dolly2", false},
        {"HiveTool2", false},
        {"BeeSuit2", false},
        {"Extractor2", false}
    };

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

    public Dictionary<FlowerType, bool> Stage12FlowersUnlocked = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Clover, true},
        {FlowerType.Buckwheat, true},
        {FlowerType.Alfalfa, true},
        {FlowerType.Dandelion, true},
        {FlowerType.Sunflower, true},
        {FlowerType.Orange, true},
    };

    public Dictionary<FlowerType, bool> Stage34FlowersUnlocked = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Fireweed, true},
        {FlowerType.Goldenrod, true},
        {FlowerType.Daisy, true},
        {FlowerType.Thistle, true},
        {FlowerType.Blueberry, true},
        {FlowerType.Tupelo, true},
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

    public int stage = 0;

    public List<FlowerType> GetUnlockedFlowers()
    {
        List<FlowerType> unlockedFlowers = new List<FlowerType>();
        foreach(KeyValuePair<FlowerType, bool> kvp in Stage12FlowersUnlocked)
            if (kvp.Value)
                unlockedFlowers.Add(kvp.Key);

        foreach (KeyValuePair<FlowerType, bool> kvp in Stage34FlowersUnlocked)
            if (kvp.Value)
                unlockedFlowers.Add(kvp.Key);

        return unlockedFlowers;
    }

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
                if (!kvp.Value && Stage12FlowersUnlocked[kvp.Key])
                {
                    if (stage == 2 || (stage == 1 && kvp.Key != FlowerType.Orange))
                        randFlowerOptions.Add(kvp.Key);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                int rand = Random.Range(0, randFlowerOptions.Count);
                availableFlowers.Add((int)randFlowerOptions[rand] - 2);
                ownedFlowers.Add(randFlowerOptions[rand]);
                randFlowerOptions.RemoveAt(rand);
            }

            foreach (int i in availableFlowers)
            {
                Stage12Flowers[(FlowerType)(i + 2)] = true;
            }
        }
        else
        {
            List<FlowerType> randFlowerOptions = new List<FlowerType>();
            foreach (KeyValuePair<FlowerType, bool> kvp in Stage34Flowers)
                if (!kvp.Value && Stage34FlowersUnlocked[kvp.Key])
                    randFlowerOptions.Add(kvp.Key);

            for (int i = 0; i < 3; i++)
            {
                int rand = Random.Range(0, randFlowerOptions.Count);
                availableFlowers.Add((int)randFlowerOptions[rand] - 2);
                ownedFlowers.Add(randFlowerOptions[rand]);
                randFlowerOptions.RemoveAt(rand);
            }

            foreach (int i in availableFlowers)
                Stage34Flowers[(FlowerType)(i + 2)] = true;
        }

        GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().AddHoneyCards(availableFlowers);
        return availableFlowers;
    }

    public void ResetToStart()
    {
        stage = 0;
        ownedFlowers.Clear();
        foreach (FlowerType key in Stage12Flowers.Keys.ToList())
            Stage12Flowers[key] = false;
        foreach (FlowerType key in Stage34Flowers.Keys.ToList())
            Stage34Flowers[key] = false;
    }
}