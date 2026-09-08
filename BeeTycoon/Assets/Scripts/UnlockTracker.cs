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
        {"ToolSelect", false},
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
        {FlowerType.Goldenrod, false},
        {FlowerType.Sunflower, false},
        {FlowerType.Orange, false},
    };

    public Dictionary<FlowerType, bool> Stage34Flowers = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Fireweed, false},
        {FlowerType.Dandelion, false},
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
        {FlowerType.Goldenrod, true},
        {FlowerType.Sunflower, true},
        {FlowerType.Orange, true},
    };

    public Dictionary<FlowerType, bool> Stage34FlowersUnlocked = new Dictionary<FlowerType, bool>()
    {
        {FlowerType.Fireweed, true},
        {FlowerType.Dandelion, true},
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
        {"Docile", true},
        {"Rugged", true},
        {"Agile", true},
        {"Motherly", true},
        {"Picky", true}
    };

    public Dictionary<string, string> quirkDescriptions = new Dictionary<string, string>()
    {
        {"Industrious", "This colony can store 6 lbs of honey more, per hive level"},
        {"Greedy", "Increase maximum honey production by 10%"},
        {"Docile", "Decrese chance of becoming aggrevated from 20% to 16.6%"},
        {"Rugged", "This colony is requires 25% less honey to survive winter"},
        {"Agile", "Raise the maximum hive efficiency from 150% to 160%"},
        {"Motherly", "Increase birthrate from 2500 to 3000 per turn"},
        {"Picky", "Honey purity is 5% higher"}
    };

    public Dictionary<string, float> quirkValues = new Dictionary<string, float>()
    {
        {"Industrious", 1},
        {"Greedy", 1.1f},
        {"Docile", 1},
        {"Rugged", 1.25f},
        {"Agile", 0.1f},
        {"Motherly", 500},
        {"Picky", 0.05f}
    };

    public Dictionary<string, string> speciesDetails = new Dictionary<string, string>()
    {
        {"Italian", "Italian honey bees increase their maximum honey production by 10% for each Italian colony."},
        {"Russian", "Russian honey bees don't share flowers in their radius with other species, taking all the nectar for themselves. When mutlile Russian colonies share a flower, they all get the maximum benefit."},
        {"Japanese", "Japanese honey bees have a 33% chance to cure themselves of a negative condition each turn. Other hives in their radius have a 10% chance to gain this passive when cured of a negative condition."},
        {"Carniolan", "Carniolan honey bees provide a calming effect to other hives in their radius, reducing the other hives' stress by 1."},
        {"Caucasian", "Caucasian honey bees prefer open space. When over half the tiles in their radius are empty, all flowers in their radius produce double the nectar. This effect persists for other hives who score those flowers."},
        {"Himalayan", "Himalayan honey bees' maximum honey production is increased by 25%, and all honey is treated as pure when there are no other hives in their radius."},
        {"Cordovan", "Cordovan honey bees recieve -50% maximum honey production. For each other populated hive you own, increase this hive's maixmum honey product by 25%, up to +150%."},
        {"Buckfast", "Buckfast honey bees' honey type and purity are dictated only by the flowers diagonal and adjacent to the hive. All other flowers are scored normally."},
        {"Killer", "Killer honey bees do not halt production at stress level 4. Additionally, increase nectar gained from flowers by 50% for each stress level."}
    };

    public Dictionary<FlowerType, string> flowerDetails = new Dictionary<FlowerType, string>()
    { 
        {FlowerType.Clover, "Clovers produce 0.10 lbs of nectar for each adjacent or diagonal clover."},
        {FlowerType.Alfalfa, "Alfalfas produce 0.20 lbs of nectar for each diagonal alfalfa."},
        {FlowerType.Buckwheat, "Buckwheats produce 0.10 lbs of nectar, and has a 1 in 7 chance to spread to tiles adjacent to it."},
        {FlowerType.Goldenrod, "Goldenrods produce 0.50 lbs of nectar."},
        {FlowerType.Fireweed, "Fireweeds produce 0.30 lbs of nectar, and has a 1 in 3 chance to replace an adjacent flower. If it has 3 or more adjacent flowers, it dies."},
        {FlowerType.Dandelion, "Dandelions produce 0.10 lbs of nectar. At the end of every turn, dandelions move to to a random tile. Increase the nectar gain by 0.01 lbs for each time a dandelion has moved."},
        {FlowerType.Sunflower, "Sunflowers produce 0.07 lbs of nectar for each empty tile adjacent and diagonal to it."},
        {FlowerType.Daisy, "Daisies produce 0.30 lbs of nectar for each unique flower adjacent and diagonal to it."},
        {FlowerType.Thistle, "Thistles kill an adjacent or diagonal flower, and produce nectar equal to 3x the amount that tile last produced."},
        {FlowerType.Blueberry, "Blueberries produce 2 lbs of nectar, but only in Summer."},
        {FlowerType.Orange, "Orange trees take up 4 tiles, but score on all of those tiles. They produce 0.50 lbs of nectar per tile."},
        {FlowerType.Tupelo, "Tueplo trees take up 4 tiles, but score on all of those tiles. They produce 0.75 lbs of nectar per tile. Flowers adjacent can not die."},
        {FlowerType.Tulip, "Produces double a random honey type. This type changes each season."},
        {FlowerType.TulipPoplar, "Tulip Poplar trees take up 4 tiles, but score on all of those tiles. They produce 0.05 lbs of nectar per tile. Extremely high & stable sell price."},
        {FlowerType.Hydrangea, "Hydrangeas produce 0.30 lbs of nectar, and produce 3x that when adjacent to water."},
        {FlowerType.WaterLily, "Water Lillies produce 0.50 lbs of nectar and can be place on water."},
        {FlowerType.Sundew, "Sundews produce 0.20 lbs of nectar. Any tiles with a sundew counts as 'water'."},
        {FlowerType.PitcherPlant, "Pitcher Plants produce 1 lb of nectar. When not scored, pitcher plants save their nectar. Up to 3 lbs can be saved."},
        {FlowerType.Lavendar, "Lavendars produce 0.15 lbs of nectar. If the hive is above 0 stress, apply the soothing condition. Otherwise, produce an additional 0.10 lbs of nectar."},
        {FlowerType.Hibiscus, "Produces 0.20 lbs of nectar for each condition the hive has."},
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