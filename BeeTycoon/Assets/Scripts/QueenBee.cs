using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBee : MonoBehaviour
{
    //stat multipliers;
    public float productionMult;
    public float constructionMult;
    public float collectionMult;
    public float resilienceMult;
    public float aggressivnessMult;

    void Start()
    {
        productionMult = Random.Range(.6f,2.5f);
        constructionMult = Random.Range(.6f, 2.5f);
        collectionMult = Random.Range(.6f, 2.5f);
        resilienceMult = Random.Range(.6f, 2.5f);
        aggressivnessMult = Random.Range(.6f, 2.5f);
    }
}
