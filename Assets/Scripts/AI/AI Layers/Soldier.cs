using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private SoldierState currentState;

    private Unit unit;

    public ref Unit Unit => ref unit;

    public Soldier(Unit _unit) => unit = _unit;

    private void Start()
    {
        currentState = new IdleSoldier(this);

        unit = GetComponent<Unit>();
        if (!unit)
        {
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
    }

    public void SetState(SoldierState order/*, Vector3 Target*/)
    {
        if (currentState != null)
            currentState.End();

        currentState = order;
        currentState.Start();
    }
}
