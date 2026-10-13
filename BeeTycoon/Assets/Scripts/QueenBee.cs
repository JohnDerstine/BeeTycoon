using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBee : MonoBehaviour
{
    public bool nullQueen;
    public bool finishedGenerating;
    public bool fromSave;
    private UnlockTracker unlocks;

    public string species;
    public string radiusType;
    public FlowerType favorite;
    public List<string> quirks = new List<string>();

    private List<string> rTypes = new List<string>() { "Square", "Long", "L-Shaped"};

    public bool transferComplete = false;

    public bool japaneseInherited = false;

    public Texture2D speciesSprite;
    public Color myColor;

    void Start()
    {
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();

        if (fromSave)
            return;

        if (!nullQueen)
            GenerateStats();
        else
            finishedGenerating = true;
    }

    private void GenerateStats()
    {
        List<string> possibilites = new List<string>();

        radiusType = rTypes[Random.Range(0, 3)];

        int quirkNum;
        int quirkRand = Random.Range(0, 10);
        if (quirkRand <= 2)
            quirkNum = 0;
        else if (quirkRand > 2 && quirkRand <= 8)
            quirkNum = 1;
        else
            quirkNum = 2;
        
        foreach (KeyValuePair<string, bool> kvp in unlocks.quirks)
        {
            if (kvp.Value)
                possibilites.Add(kvp.Key);
        }

        for (int i = 0; i < quirkNum; i++)
        {
            int index = Random.Range(0, possibilites.Count);
            quirks.Add(possibilites[index]);
            possibilites.RemoveAt(index);
        }

        List<FlowerType> unlockedFlowers = unlocks.GetUnlockedFlowers();
        favorite = unlockedFlowers[Random.Range(0, unlockedFlowers.Count)];

        finishedGenerating = true;
    }

    public IEnumerator TransferStats(QueenBee newQueen)
    {
        yield return new WaitUntil(() => finishedGenerating);
        radiusType = newQueen.radiusType;
        favorite = newQueen.favorite;
        species = newQueen.species;
        quirks = newQueen.quirks;
        GetSpeciesIcon();
        nullQueen = false;
        transferComplete = true;
    }

    public void GetSpeciesIcon()
    {
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();
        switch (species)
        {
            case "Italian":
                speciesSprite = unlocks.ItalianIcon;
                myColor = Color.yellow;
                break;
            case "Russian":
                speciesSprite = unlocks.RussianIcon;
                myColor = Color.gray;
                break;
            case "Japanese":
                speciesSprite = unlocks.JapaneseIcon;
                myColor = new Color(0.97f, 0.61f, 1);
                break;
            case "Caucasian":
                speciesSprite = unlocks.CaucasianIcon;
                myColor = Color.white;
                break;
            case "Cordovan":
                speciesSprite = unlocks.CordovanIcon;
                myColor = Color.blue;
                break;
            case "Carniolan":
                speciesSprite = unlocks.CarniolanIcon;
                myColor = new Color(0.61f, 0.2f, 0.77f);
                break;
            case "Himalayan":
                speciesSprite = unlocks.HimalayanIcon;
                myColor = new Color(0.8f, 0.1f, 0.033f);
                break;
            case "Buckfast":
                speciesSprite = unlocks.BuckfastIcon;
                myColor = new Color(0.48f, 0.18f, 0);
                break;
            case "Killer":
                speciesSprite = unlocks.KillerIcon;
                myColor = Color.red;
                break;
        }
    }
}
