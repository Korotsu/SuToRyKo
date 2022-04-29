using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    private List<State> orderlist = new List<State> { new Idle() };

    private List<Soldier> soldiers = new List<Soldier>();

    private State currentState = new Idle();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            Soldier soldier = child.GetComponent<Soldier>();
            if (soldier)
                soldiers.Add(soldier);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
        TakeDecision();
    }

    private void TakeDecision()
    {
        //Decisional code with influence and modifier Map;
        State order = orderlist[0];
        soldiers.ForEach(soldier => soldier.SetState(order));
    }

    public void SetState(State order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
