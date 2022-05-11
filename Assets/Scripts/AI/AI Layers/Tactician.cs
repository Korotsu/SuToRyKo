using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    private List<SoldierState> orderlist = new List<SoldierState>();

    private List<Soldier> soldiers = new List<Soldier>();

    public ref List<Soldier> GetSoldiers() => ref soldiers;

    private TacticianState currentState;

    public bool isFormationLocked = false;

    private void Start()
    {
        currentState = new IdleTactician(this);

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
}
