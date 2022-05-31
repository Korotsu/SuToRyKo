using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State
{
    protected Base target;
    public Base Target { get => target; private set => target = value; }

    public void SetTarget(Base _target)
    {
        target = _target;
    }

    // Called once at the start of the state
    public abstract void Start();

    // Update is called once per frame
    public abstract void Update();

    // Called when the state is swiped out
    public abstract void End();
}

public abstract class TacticianState : State
{
    protected Tactician tactician;
    
    public TacticianState(Tactician _tactician) => tactician = _tactician;

    public override void Update()
    {
        foreach (Unit unit in tactician.Soldiers)
        {
            unit.UnitLogic.Update();
        }
    }
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

