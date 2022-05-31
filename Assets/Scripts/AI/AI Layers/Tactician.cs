using System.Collections;
using System.Collections.Generic;
using AI.BehaviorStates;
using UnityEngine;

public class Tactician : Base
{
    private List<UnitState> orderlist = new List<UnitState>();

    private List<Unit> soldiers = new List<Unit>();

    public List<Unit> Soldiers { get => soldiers; private set => soldiers = value; }

    private TacticianState currentState;

    public int nbLightInCreation = 0;
    public int nbHeavyInCreation = 0;

    private void Start()
    {
        Team = ETeam.Neutral;
        currentState = new IdleTactician(this);

        foreach (Transform child in transform)
        {
            Unit unitLogic = child.GetComponent<Unit>();
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
        soldiers.ForEach(soldier => soldier.UnitLogic.SetState(new IdleUnit(soldier.UnitLogic)));
    }

    public void SetState(TacticianState order/*, Vector3 Target*/)
    {
        currentState.End();
        currentState = order;
        currentState.Start();
    }

    public void AddSoldier(Unit unitLogic)
    {
        soldiers.Add(unitLogic);
    }

    protected override float GetInfluence()
    {
        float influence = 0f;

        foreach (Unit unit in soldiers)
        {
            influence += unit.Influence;
        }

        return influence;
    }
}
