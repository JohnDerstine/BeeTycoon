using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cost : MonoBehaviour
{
    [SerializeField]
    public int Price = -1;

    public bool purchased;

    public FlowerType ftype = FlowerType.Empty;

    public bool OneTime;

    public bool Purchased
    {
        get { return purchased; }
        set 
        {
            purchased = value;
            Price = 0;
        }
    }
}