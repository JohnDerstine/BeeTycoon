using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : MonoBehaviour
{
    private int size = 1;
    private float population = 5000;
    private float popCap = 20000; //what population the hive can currently house
    private float popSizeCap = 20000; //how much each level of size changes the popCap
    private float comb;
    private float combCap = 8; //how much honey the hive can currently store
    private float combSizeCap = 8; //how much each level of size changes the honeyCap
    private float nectar;
    private float honey;

    private float storage = 0; //how much storage the hive has
    private float storagePerComb = 1000; //how much each level of size changes the storage

    private float birthRate = 2500;

    private float hiveEfficency; //Efficiency is a multiplier to all the hive's actions and is calculated by the population / total population * size of the hive

    //stats 0-1f
    private float production = 400;
    private float construction = 0.5f;
    private float collection = 400;
    private float resilience = 1;
    private float aggressivness = 1;

    private QueenBee queen;

    public int Size
    {
        get { return size; }
        set
        {
            size += value;
            popCap = popSizeCap * size;
            combCap = combSizeCap * size;
        }
    }

    void Start()
    {
        queen = gameObject.GetComponent<QueenBee>();
    }

    void Update()
    {

    }

    public void UpdateHive()
    {
        float possibleComb = construction * queen.constructionMult * hiveEfficency;
        if (possibleComb + comb > combCap)
            possibleComb = combCap - comb;
        comb += possibleComb;
        storage = storagePerComb * comb;

        float possibleHoney = production * queen.productionMult * hiveEfficency;
        if (possibleHoney > nectar)
            possibleHoney = nectar;
        honey += possibleHoney;

        float possibleNectar = collection * queen.collectionMult * hiveEfficency;
        if (possibleNectar + nectar + honey > storage)
            possibleNectar = storage - (nectar + honey);
        nectar += possibleNectar;

        float possiblePop = birthRate;
        if (possiblePop + population > popCap)
            possiblePop = popCap - population;
        population += possiblePop;

        hiveEfficency = (population / popCap) * size;

        Debug.Log("Population: " + population);
        Debug.Log("Comb: " + comb);
        Debug.Log("Nectar: " + nectar);
        Debug.Log("Honey: " + honey);
        Debug.Log("Storage: " + storage);
        Debug.Log("Efficiency: " + hiveEfficency);
    }
}