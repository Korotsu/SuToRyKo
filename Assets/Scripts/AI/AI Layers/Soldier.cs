using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private State currentState = new Idle();

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
    }

    public void SetState(State order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
