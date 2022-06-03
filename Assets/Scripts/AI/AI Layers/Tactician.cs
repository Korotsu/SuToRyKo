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

        

        if (soldiers.Count > 0)
            transform.position = soldiers[0].transform.position;

        if (!formationManager)
        {
            enabled = false;
            return;
        }
        
        //soldiers.ForEach(soldier => soldier.Unit)
    }

    public void UpdateUnits()
    {
        nbHeavy = 0;
        nbLight = 0;
        int count = soldiers.Count;
        for (int i = 0; i < count ; ++i)
        {
            Unit unit = soldiers[i];
            if (!unit)
            {
                soldiers.RemoveAt(i);
                i--;
                continue;
            }
            if (unit.GetUnitData.type == EntityDataScriptable.Type.Heavy)
                nbHeavy++;
            else
                nbLight++;
        }
    }
    // Update is called once per frame
    void Update()
    {
       
        currentState.Update();
        
        if (!formationManager.enabled && soldiers.Count > 0)
            transform.position = soldiers[0].transform.position;
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
