using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveTool : ToolScript
{
    public int usesPerTurn = 0;
    public int usesLeft = 0;
    private List<string> descriptions = new List<string>()
    {
        "Cures hives from the glued affliction\n\n1 Use per turn",
        "Increases all tools uses per turn by 1",
        "Increases all tools uses per turn by 1"
    };

    public override string GetDescription()
    {
        return descriptions[level].ToString();
    }

    public override string GetCurrentDescription()
    {
        int upt = usesPerTurn;
        if (upt == 0)
            upt = 1;
        string upgraded = "";
        int num = level - 1;

        if (level == 2 || level == 3)
            upgraded = "\n\nAdditionaly, all other tools get an extra " + num + " uses per turn";
        string description = "Cures hives from glued affliction\n\n" + upt + "Uses per turn" + upgraded;

        return description;
    }

    public override void Upgrade()
    {
        ToolManager tManager = GameObject.Find("ToolManager").GetComponent<ToolManager>();
        level++;
        if (level == 1)
        {
            usesPerTurn = 1;
            usesLeft = 1;
        }
        else if (level == 2 || level == 3)
        {
            tManager.shovel.usesPerTurn++;
            tManager.dolly.usesPerTurn++;
            tManager.smoker.usesPerTurn++;
            usesPerTurn++;
        }

        base.Upgrade();
    }

    public override void TurnReset()
    {
        usesLeft = usesPerTurn;
    }
}
