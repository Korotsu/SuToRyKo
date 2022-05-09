using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private UnitState currentState = new IdleUnit();

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
