using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State
{
    // Called when the state is swapped in
    public abstract void Start();

    // Update is called once per frame
    public abstract void Update();

    // Called when the state is swiped out
    public abstract void End();
}

public abstract class TacticianState : State
{
    private Tactician tactician;
    public TacticianState(Tactician _tactician) => tactician = _tactician;
}

public abstract class UnitState : State
{
    protected readonly UnitLogic targetLogic;
    protected readonly Unit targetUnit;

    protected UnitState(UnitLogic _targetLogic)
    {
        targetLogic = _targetLogic; 
        targetUnit = _targetLogic.associatedUnit;
    } 
}

