using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private SoldierState currentState;

    public Unit unit;

    private void Start()
    {
        currentState = new IdleSoldier(this);        
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
    }

    public void SetState(SoldierState order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
