using System;
using System.Collections;
using System.Collections.Generic;
using AI.StateMachine;
using UnityEngine;

public class UnitLogic : MonoBehaviour
{
    private UnitState currentState;

    public Unit associatedUnit;

    public UnitState CurrentState { get => currentState; }
    public Unit AssociatedUnit { get => associatedUnit; }

    public UnitLogic(Unit _associatedUnit)
    {
        associatedUnit = _associatedUnit;
    }
    
    private void Start()
    {
        currentState = new IdleUnit(this);        
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
    }

    public void SetState(UnitState order/*, Vector3 Target*/)
    {
        if(currentState != null)
            currentState.End();

        currentState = order;
        currentState.Start();
    }

    public void SetUnit(Unit unit)
    {
        associatedUnit = unit;
    }
}