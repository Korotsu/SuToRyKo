using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class State
{
    public abstract void Start();

    public abstract void Update();

    public abstract void End();
}
