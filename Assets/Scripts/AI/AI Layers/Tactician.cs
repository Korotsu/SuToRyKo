using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    private List<SoldierState> orderlist = new List<SoldierState>();

    private List<Soldier> soldiers = new List<Soldier>();

    private TacticianState currentState;

    private void Start()
    {
        currentState = new IdleTactician(this);

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
        soldiers.ForEach(soldier => soldier.SetState(new IdleSoldier(soldier)));
    }

    public void SetState(TacticianState order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }
}
