using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public override string GetCurrentDescription()
    {
        string description = descriptions[0];
        int chance = 25;
        if (level == 3)
            chance = 50;

        if (level == 2 || level == 3)
            description += "When harvesting a full hive, " + chance + "% chance to cure random affliction.";
        return description;
    }

    public override void Upgrade()
    {
        level++;
        if (level == 1)
        {
            allowHarvest = true;
            foreach (Hive h in GameObject.Find("PlayerController").GetComponent<PlayerController>().hives)
                if (h.StressLevel >= 2)
                    h.EnableHarvestButtons();
        }
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
