using System;
using System.Collections;
using System.Collections.Generic;
using AI.StateMachine;
using UnityEngine;

public class UnitLogic : MonoBehaviour
{
    public Unit associatedUnit { get; private set; }

    public UnitState CurrentState { get; private set; }

    
    public UnitLogic(Unit _associatedUnit)
    {
        associatedUnit = _associatedUnit;
    }
    
    private void Start()
    {
        CurrentState = new IdleUnit(this);        
    }

    // Update is called once per frame
    void Update()
    {
        CurrentState.Update();
    }

    public void SetState(UnitState order/*, Vector3 Target*/)
    {
        if(CurrentState != null)
            CurrentState.End();

        CurrentState = order;
        CurrentState.Start();
    }

    public void SetUnit(Unit unit)
    {
        associatedUnit = unit;
    }
}