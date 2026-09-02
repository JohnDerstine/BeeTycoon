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
        foreach (KeyValuePair<string, bool> kvp in unlocks.species)
        {
            if (kvp.Value)
                possibilites.Add(kvp.Key);
        }
        species = possibilites[Random.Range(0, possibilites.Count)];

        radiusType = rTypes[Random.Range(0, 3)];

        int quirkNum;
        int quirkRand = Random.Range(0, 10);
        if (quirkRand <= 2)
            quirkNum = 0;
        else if (quirkRand > 2 && quirkRand <= 8)
            quirkNum = 1;
        else
            quirkNum = 2;
            
        possibilites.Clear();
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

        //GetComponent<Cost>().Price = (int)grade;
        finishedGenerating = true;
    }

    public IEnumerator TransferStats(QueenBee newQueen)
    {
        yield return new WaitUntil(() => finishedGenerating);
        radiusType = newQueen.radiusType;
        favorite = newQueen.favorite;
        species = newQueen.species;
        quirks = newQueen.quirks;
        nullQueen = false;
        transferComplete = true;
    }
}
