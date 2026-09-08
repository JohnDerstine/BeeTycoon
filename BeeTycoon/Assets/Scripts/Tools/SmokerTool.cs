using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class SmokerTool : ToolScript
{
    public int usesPerTurn = 0;
    public int usesLeft = 0;
    public bool calming;
    private List<string> descriptions = new List<string>()
    {
        "Cures hives from the aggressive affliction\n\n1 Use per turn",
        "Uses per turn: 1 -> 2",
        "Using on a hive reduces its stress by 1"
    };

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override string GetCurrentDescription()
    {
        string calm = "";
        if (calming)
            calm = " and reduces stress by 1";
        int uses = usesPerTurn;
        if (uses == 0)
            uses = 1;

        string description = "Cures hives from the aggressive affliction" + calm + "\n\n" + uses + " Uses per turn";

        return description;
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
            usesPerTurn += 1;
        else if (level == 3)
            calming = true;

        base.Upgrade();
    }

    public override void TurnReset()
    {
        usesLeft = usesPerTurn;
    }
}
