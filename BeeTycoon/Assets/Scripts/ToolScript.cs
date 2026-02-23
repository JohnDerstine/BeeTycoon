using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToolScript : MonoBehaviour
{
    public int level = 0;
    public abstract void Upgrade();

    public void SetLevel(int level)
    {
        this.level = level;
    }
}
