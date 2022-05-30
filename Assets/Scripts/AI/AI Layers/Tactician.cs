using System.Collections;
using System.Collections.Generic;
using AI.StateMachine;
using UnityEngine;

public class Tactician : Base
{
    private List<UnitState> orderlist = new List<UnitState>();

    private List<UnitLogic> soldiers = new List<UnitLogic>();

    private TacticianState currentState;

    public ETeam Team = ETeam.Neutral;

    private void Start()
    {
        currentState = new IdleTactician(this);

        foreach (Transform child in transform)
        {
            UnitLogic unitLogic = child.GetComponent<UnitLogic>();
            if (unitLogic)
                soldiers.Add(unitLogic);
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
        soldiers.ForEach(soldier => soldier.SetState(new IdleUnit(soldier)));
    }

    public void SetState(TacticianState order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }

    public void AddSoldier(UnitLogic unitLogic)
    {
        soldiers.Add(unitLogic);
    }

    protected override float GetInfluence()
    {
        float influence = 0f;

        foreach (UnitLogic unitLogic in soldiers)
        {
            influence += unitLogic.associatedUnit.Influence;
        }

        return influence;
    }
}
