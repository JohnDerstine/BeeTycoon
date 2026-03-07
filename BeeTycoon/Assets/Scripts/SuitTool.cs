using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuitTool : ToolScript
{
    public bool allowHarvest;
    public int cureChance = -1;
    private List<string> descriptions = new List<string>()
    {
        "Allows harvesting of hives with 2 or more stress",
        "Grants 25% chance to cure a random affliction when harvesting an almost full hive",
        "Affliction cure chance:\n25% -> 50%"
    };

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override void Upgrade()
    {
        level++;
        if (level == 1)
            allowHarvest = true;
        else if (level == 2)
            cureChance = 4;
        else
            cureChance = 2;

        base.Upgrade();
    }

    public override void TurnReset()
    {
        //Nothing
    }
}
