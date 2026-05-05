using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelTool : ToolScript
{
    public int usesPerTurn = 0;
    public int usesLeft = 0;
    private List<string> descriptions = new List<string>()
    {
        "Moves flowers from one tile to another.\n\n3 Uses per turn",
        "Uses per turn:\n3 -> 5",
        "Uses per turn:\n5 -> 7"
    };

    private void Awake()
    {
        Upgrade();
    }

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override void Upgrade()
    {
        level++;
        if (level == 1)
        {
            usesPerTurn = 3;
            usesLeft = 3;
        }
        else
            usesPerTurn += 2;

        base.Upgrade();
    }

    public override void TurnReset()
    {
        usesLeft = usesPerTurn;
    }
}
