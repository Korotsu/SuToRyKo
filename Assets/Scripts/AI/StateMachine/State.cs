using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State
{
    public abstract void Start();

    public abstract void Update();

    public abstract void End();
}

public abstract class TacticianState : State
{
    private Tactician tactician;
    public TacticianState(Tactician _tactician) => tactician = _tactician;
}

public abstract class SoldierState : State
{
    private Soldier soldier;
    public SoldierState(Soldier _soldier) => soldier = _soldier;
}

