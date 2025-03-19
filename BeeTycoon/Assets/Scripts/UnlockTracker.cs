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

    //Add negative quirks that are opposite of 5 base
    public Dictionary<string, bool> quirks = new Dictionary<string, bool>()
    {
        {"Industrious", true},
        {"Greedy", true},
        {"Territorial", true},
        {"Rugged", true},
        {"Agile", true}
    };

    public Dictionary<string, string> quirkDescriptions = new Dictionary<string, string>()
    {
        {"Industrious", "This coloney is 50% more efficient at building comb"},
        {"Greedy", "This coloney is 50% more efficient at producing honey"},
        {"Territorial", "This coloney is 50% better at fighting pests and invaders"},
        {"Rugged", "This coloney is 50% more resistant disease"},
        {"Agile", "This coloney is 50% more efficient at collecting nectar"}
    };
}
