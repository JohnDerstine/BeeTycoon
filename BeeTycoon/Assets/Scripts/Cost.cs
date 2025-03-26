using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cost : MonoBehaviour
{
    [SerializeField]
    public int Price = -1;

    private bool purchased;

    public bool Purchased
    {
        get { return purchased; }
        set 
        {
            purchased = value;
        }
    }
}