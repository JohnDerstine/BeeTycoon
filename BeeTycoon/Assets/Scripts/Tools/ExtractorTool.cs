using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractorTool : ToolScript
{
    public bool noCombLoss;
    public float extractorBonus = 1f;
    private List<string> descriptions = new List<string>()
    {
        "Increases harvest efficiency which grants 10% more honey",
        "Honey bonus:\n10% -> 25%",
        "Removes comb loss when harvesting"
    };

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override void Upgrade()
    {
        level++;
        if (level == 1)
            extractorBonus = 1.1f;
        else if (level == 2)
            extractorBonus = 1.25f;
        else
            noCombLoss = true;

        base.Upgrade();
    }

    public override void TurnReset()
    {
        //Nothing
    }
}
