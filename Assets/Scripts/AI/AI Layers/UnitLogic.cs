using System.Collections;
using System.Collections.Generic;
using AI.StateMachine;
using UnityEngine;

public class UnitLogic : MonoBehaviour
{
    private UnitState currentState;

    private Unit associatedUnit;

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
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
