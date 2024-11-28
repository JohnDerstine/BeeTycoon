using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoneyMarket : MonoBehaviour
{
    Dictionary<FlowerType, List<float>> marketValues = new Dictionary<FlowerType, List<float>>();
    Dictionary<FlowerType, float> amountSold = new Dictionary<FlowerType, float>();
    int turn = 0;
    System.Array values = System.Enum.GetValues(typeof(FlowerType));

    // Start is called before the first frame update
    void Start()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            marketValues.Add(fType, new List<float>() { 0, 0, 0}); //Current Value, Growth per Turn, Base Value
            amountSold.Add(fType, 0);
        }

        marketValues[FlowerType.Clover][2] = 10;
        marketValues[FlowerType.Alfalfa][2] = 12;
        marketValues[FlowerType.Blossom][2] = 15;
        marketValues[FlowerType.Buckwheat][2] = 17;

        ResetToBaseValue();
    }

    public void UpdateMarket()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            if (marketValues[fType][1] == 0 || turn % 4 == 0)
            {
                float randValue = Random.Range(1, 5);
                if (Random.Range(0, 2) == 0)
                    randValue *= -1;
                marketValues[fType][1] = randValue;
            }
            else
                marketValues[fType][1] += Random.Range(-1, 1);


            if (turn % 8 == 0)
                ResetToBaseValue();
            else if (marketValues[fType][0] > marketValues[fType][2] / 2f)
                marketValues[fType][0] += marketValues[fType][1] - amountSold[fType] / 10f;

            if (marketValues[fType][0] < marketValues[fType][2] / 2f)
                marketValues[fType][0] = marketValues[fType][2] / 2f;
        }
        turn++;
        LogValues();
    }

    private void ResetToBaseValue()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            marketValues[fType][0] = marketValues[fType][2];
        }
    }

    private void LogValues()
    {
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            Debug.Log(marketValues[fType][0] + " " + marketValues[fType][1]);
        }
    }
}
