using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelTool : ToolScript
{
    public int uses = 3;
    public override void Upgrade()
    {
        level++;
    }
}
