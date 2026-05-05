using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToolScript : MonoBehaviour
{
    [SerializeField]
    private ToolManager toolManager;

    [SerializeField]
    private int toolID;

    protected int level = 0;
    public int Level
        { get { return level; } }

    public virtual void Upgrade()
    {
        if (Level == 3)
            toolManager.toolsMaxed[(Tool)toolID] = true;
    }

    public abstract void TurnReset();

    public abstract string GetDescription();

    public void SetLevel(int level)
    {
        for (int i = 0; i < level; i++)
            Upgrade();
    }
}
