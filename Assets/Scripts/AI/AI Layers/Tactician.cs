using System.Collections;
using System.Collections.Generic;
using AI.BehaviorStates;
using UnityEngine;

public class Tactician : Base
{
    private List<UnitState> orderlist = new List<UnitState>();

    private List<Unit> soldiers = new List<Unit>();

    public List<Unit> Soldiers { get => soldiers; private set => soldiers = value; }

    private Formations.FormationManager formationManager;

    public ref List<Unit> GetSoldiers() => ref soldiers;

    private TacticianState currentState;

    public TacticianState GetTacticianState() => currentState;

    public bool isFormationLocked = false;

    public int nbLightInCreation = 0;
    public int nbHeavyInCreation = 0;
    public int nbLight = 0;
    public int nbHeavy = 0;
    private void Start()
    {
        
        currentState = new IdleTactician(this);

        formationManager = GetComponent<Formations.FormationManager>();
		foreach (Transform child in transform)
		{
			Unit unitLogic = child.GetComponent<Unit>();
            if (unitLogic)
                soldiers.Add(unitLogic);
		}
        if (!formationManager)
        {
            
            enabled = false;
            return;
        }

        //soldiers.ForEach(soldier => soldier.Unit)
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
        //TakeDecision();
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
    private void OnDestroy()
    {
        foreach (Unit soldier in soldiers)
        {
            if (soldier.mainTactician == this)
                soldier.mainTactician = null;

            if (soldier.tempTactician == this)
                soldier.tempTactician = null;
        }

        soldiers.Clear();
    }

    public void SetTargetPos(Vector3 pos)
    {
        formationManager.SetTargetPos(pos);
    }
}
