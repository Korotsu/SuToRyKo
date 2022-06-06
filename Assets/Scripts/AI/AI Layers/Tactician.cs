using System.Collections;
using System.Collections.Generic;
using AI.BehaviorStates;
using UnityEngine;
using UnityEngine.AI;

public class Tactician : Base
{
    private List<UnitState> orderlist = new List<UnitState>();

    private List<Unit> soldiers = new List<Unit>();

    public List<Unit> Soldiers { get => soldiers; private set => soldiers = value; }

    private NavMeshAgent navMeshAgent;

    private Formations.FormationManager formationManager;

    public ref Formations.FormationManager FormationManager => ref formationManager;

    public ref List<Unit> GetSoldiers() => ref soldiers;

    private TacticianState currentState;

    public TacticianState GetTacticianState() => currentState;

    public bool isFormationLocked = false;

    public int nbLightInCreation = 0;
    public int nbHeavyInCreation = 0;
    public int nbLight = 0;
    public int nbHeavy = 0;

    [SerializeField] private float maxDistanceToTarget = 20f;


    private Vector3 targetPosition = Vector3.zero;

    private void Start()
    {
        currentState = new IdleTactician(this);
        navMeshAgent = GetComponent<NavMeshAgent>();


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

        if (soldiers.Count < 2)
        {
            formationManager.enabled = false;
            
            if (navMeshAgent)
                navMeshAgent.enabled = false;
        }
    }

    public void UpdateUnits()
    {
        nbHeavy = 0;
        nbLight = 0;
        int count = soldiers.Count;
        for (int i = 0; i < count; ++i)
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

    public bool IsNearTarget()
    {
        if (currentState != null && currentState.Target)
        {
            float distance = (currentState.Target.transform.position - transform.position).magnitude;

            if (Mathf.Abs(distance) <= maxDistanceToTarget)
                return true;
        }

        return false;
    }


    // Update is called once per frame
    private void Update()
    {
        if (transform.position == Vector3.zero && soldiers.Count > 0)
        {
            transform.position = soldiers[0].transform.position;
        }

        if (formationManager && !formationManager.enabled && soldiers.Count >= 2)
        {
            formationManager.enabled = true;
            navMeshAgent.enabled = true;
        }

        if (!formationManager.enabled && soldiers.Count > 0)
            transform.position = soldiers[0].transform.position;

        currentState.Update();
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
        targetPosition = pos;
        formationManager.SetTargetPos(pos);
    }

    public void StopFollowFormations()
    {
        soldiers.ForEach(soldier => soldier.actions -= soldier.FollowFormation);
    }

    public void StartFormation()
    {
        if (currentState != null)
        {
            formationManager.SwitchFormationType(currentState.GetFormationType());
        }
    }

    //Return true if the tempTactician is pending kill;
    public bool RemoveAndCheck(Unit unit)
    {
        soldiers.Remove(unit);

        unit.tempTactician  = null;
        unit.actions        -= unit.FollowFormation;

        if (soldiers.Count == 1)
        {
            soldiers[0].tempTactician = null;
            soldiers[0].actions -= soldiers[0].FollowFormation;
            soldiers.Clear();
            Destroy(this.gameObject);

            return true;
        }

        else
            return false;
    }
}
