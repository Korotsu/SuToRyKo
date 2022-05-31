using Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticianCaptureState : TacticianState
{
    public TacticianCaptureState(Tactician _tactician, InteractableEntity _target = null) : base(_tactician) 
    {
        target = _target;
    }

    public override void Start()
    {
        if (target)
        {
            foreach (Unit unit in tactician.Soldiers)
            {
                unit.UnitLogic.SetState(new AI.BehaviorStates.UnitCapture(unit.UnitLogic));
                unit.UnitLogic.CurrentState.SetTarget(target);

                if(target is TargetBuilding targetBuilding)
                    unit.SetCaptureTarget(targetBuilding);
            }
        }
    }

    public override void Update()
    {
        foreach (Unit unit in tactician.Soldiers)
        {
            unit.UnitLogic.Update();
        }
    }

    public override void End()
    {
        
    }
}