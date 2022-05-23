using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    private List<SoldierState> orderlist = new List<SoldierState>();

    private List<Soldier> soldiers = new List<Soldier>();

    private Formations.FormationManager formationManager;

    public ref List<Soldier> GetSoldiers() => ref soldiers;

    private TacticianState currentState;

    public TacticianState GetTacticianState() => currentState;

    public bool isFormationLocked = false;

    private void Start()
    {
        currentState = new IdleTactician(this);

        formationManager = GetComponent<Formations.FormationManager>();

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

    private void OnDestroy()
    {
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.Unit.mainTactician == this)
                soldier.Unit.mainTactician = null;

            if (soldier.Unit.tempTactician == this)
                soldier.Unit.tempTactician = null;
        }

        soldiers.Clear();
    }

    public void SetTargetPos(Vector3 pos)
    {
        formationManager.SetTargetPos(pos);
    }
}
