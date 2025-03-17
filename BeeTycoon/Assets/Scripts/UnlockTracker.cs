using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockTracker : MonoBehaviour
{
    public Dictionary<string, bool> species = new Dictionary<string, bool>()
    {
        {"Italian", true},
        {"Russian", true},
        {"Japanese", true},
        {"Carniolan", false}
    };

    public Dictionary<string, bool> quirks = new Dictionary<string, bool>()
    {
        {"Industrious", true},
        {"Greedy", true},
        {"Territorial", true},
        {"Rugged", true},
        {"Agile", true}
    };
}
