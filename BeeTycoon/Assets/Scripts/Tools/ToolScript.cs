using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ToolScript : MonoBehaviour
{
    [SerializeField]
    private ToolManager toolManager;

    [SerializeField]
    private VisualTreeAsset pip;

    [SerializeField]
    public int toolID;

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

    public abstract string GetCurrentDescription();

    public void SetLevel(int level)
    {
        for (int i = 0; i < level; i++)
            Upgrade();
    }
    
    protected void SpawnPips(string tool, int usesPerTurn)
    {
        for (int i = 0; i < usesPerTurn; i++)
            GameObject.Find("UIDocument").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(tool).Q<VisualElement>("Pips").Add(pip.Instantiate());
    }

    protected void RemovePip(string tool)
    {
        GameObject.Find("UIDocument").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(tool).Q<VisualElement>("Pips").RemoveAt(0);
    }
}
