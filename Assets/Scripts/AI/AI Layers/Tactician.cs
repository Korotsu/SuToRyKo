using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    private List<UnitState> orderlist = new List<UnitState>();

    private List<Soldier> soldiers = new List<Soldier>();

    private TacticianState currentState = new IdleTactician();

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
        UnitState order = new IdleUnit();
        soldiers.ForEach(soldier => soldier.SetState(order));
    }

    public void SetState(TacticianState order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
