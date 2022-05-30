using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State
{
    // Update is called once per frame
    public abstract void Update();

    // Called when the state is swiped out
    public abstract void End();
}

public abstract class TacticianState : State
{
    private Tactician tactician;
    private Base target;
    public TacticianState(Tactician _tactician) => tactician = _tactician;
}

public abstract class UnitState : State
{
    protected readonly UnitLogic unitLogic;
    protected readonly Unit unit;

    protected UnitState(UnitLogic _unitLogic)
    {
        unitLogic   = _unitLogic; 
        unit        = _unitLogic.associatedUnit;
    } 
}

