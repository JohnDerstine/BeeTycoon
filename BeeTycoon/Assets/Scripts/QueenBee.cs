using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBee : MonoBehaviour
{
    public bool nullQueen;

    //stat multipliers;
    public float productionMult = 0.5f;
    public float constructionMult = 0.5f;
    public float collectionMult = 0.5f;
    public float resilienceMult = 0.5f;
    public float aggressivnessMult = 0.5f;

    public QueenBee(bool nullQueen)
    {
        this.nullQueen = nullQueen;
    }

    void Start()
    {
        if (nullQueen != true)
        {
            productionMult = Random.Range(.75f, 2.0f);
            constructionMult = Random.Range(.75f, 2.0f);
            collectionMult = Random.Range(.75f, 2.0f);
            resilienceMult = Random.Range(.75f, 2.0f);
            aggressivnessMult = Random.Range(.75f, 2.0f);
        }
    }
}
