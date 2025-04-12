using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private FlowerType flower;

    public FlowerType Flower
    {
        get { return flower; }
        set
        {
            flower = value;
            Debug.Log("Todo"); //Destroy flower gameobject
            if (value != FlowerType.Empty)
                Debug.Log("Todo"); //Instatiate flower above tile
        }
    }
}
