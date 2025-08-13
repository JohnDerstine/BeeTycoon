using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBee : MonoBehaviour
{
    public bool nullQueen;
    public bool finishedGenerating;
    public bool fromSave;
    private UnlockTracker unlocks;

    //stat multipliers;
    public float productionMult = 0.0f;
    public float constructionMult = 0.0f;
    public float collectionMult = 0.0f;
    public float resilienceMult = 0.0f;
    public float aggressivnessMult = 0.0f;

    public string species;
    public int age;
    public float grade;
    public List<string> quirks = new List<string>();

    void Start()
    {
        unlocks = GameObject.Find("UnlockTracker").GetComponent<UnlockTracker>();

        if (fromSave)
            return;

        if (!nullQueen)
        {
            productionMult = Random.Range(.75f, 2.0f);
            constructionMult = Random.Range(.75f, 2.0f);
            collectionMult = Random.Range(.75f, 2.0f);
            resilienceMult = Random.Range(.75f, 2.0f);
            aggressivnessMult = Random.Range(.75f, 2.0f);
            GenerateStats();
        }
        else
            finishedGenerating = true;
    }

    private void GenerateStats()
    {
        List<string> possibilites = new List<string>();
        foreach (KeyValuePair<string, bool> kvp in unlocks.species)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }
        species = possibilites[Random.Range(0, possibilites.Count)];

        age = Random.Range(0, 37);
        grade = Mathf.Round((productionMult + constructionMult + collectionMult + resilienceMult + aggressivnessMult) * 10) / 10.0f;

        int quirkNum;
        int quirkRand = Random.Range(0, 10);
        if (quirkRand <= 2)
            quirkNum = 0;
        else if (quirkRand > 2 && quirkRand <= 6)
            quirkNum = 1;
        else if (quirkRand > 6 && quirkRand <= 8)
            quirkNum = 2;
        else
            quirkNum = 3;
            
        possibilites.Clear();
        foreach (KeyValuePair<string, bool> kvp in unlocks.quirks)
        {
            if (kvp.Value == true)
                possibilites.Add(kvp.Key);
        }

        for (int i = 0; i < quirkNum; i++)
        {
            int index = Random.Range(0, possibilites.Count);
            quirks.Add(possibilites[index]);
            possibilites.RemoveAt(index);
        }

        GetComponent<Cost>().Price = (int)grade;
        finishedGenerating = true;
    }

    public IEnumerator TransferStats(QueenBee newQueen)
    {
        yield return new WaitUntil(() => finishedGenerating);
        productionMult = newQueen.productionMult;
        collectionMult = newQueen.collectionMult;
        constructionMult = newQueen.constructionMult;
        aggressivnessMult = newQueen.aggressivnessMult;
        resilienceMult = newQueen.resilienceMult;
        age = newQueen.age;
        species = newQueen.species;
        quirks = newQueen.quirks;
        grade = newQueen.grade;
        nullQueen = false;
    }
}
