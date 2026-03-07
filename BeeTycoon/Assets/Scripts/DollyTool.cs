using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollyTool : ToolScript
{
    public int usesPerTurn = 0;
    public int usesLeft = 0;
    public int carryCapcity = 2;
    private List<string> descriptions = new List<string>()
    {
        "Moves hives from one\ntile to another.\n\n1 Use per turn\n\nCan only carry hives\nup to 2 supers tall",
        "Super carry capacity:\n2 -> 5",
        "Uses per turn:\n1 -> 3"
    };

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override void Upgrade()
    {
        level++;
        if (level == 1)
        {
            usesPerTurn = 1;
            usesLeft = 1;
        }
        else if (level == 2)
            carryCapcity = 5;
        else
        {
            usesPerTurn = 3;
            usesLeft = 3;
        }

        base.Upgrade();
    }

    public override void TurnReset()
    {
        usesLeft = usesPerTurn;
    }
}