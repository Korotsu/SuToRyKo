using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticianCaptureState : TacticianState
{
    private bool isStoppedFollowFormation = false;
    public TacticianCaptureState(Tactician _tactician, Base _target = null) : base(_tactician) 
    {
        target = _target;
    }

    public override void Start()
    {
        if (target && tactician)
        {
            foreach (Unit unit in tactician.Soldiers)
            {
                unit.UnitLogic.SetState(new AI.BehaviorStates.UnitCapture(unit.UnitLogic));
                unit.UnitLogic.CurrentState.SetTarget(target);

                if (target is TargetBuilding targetBuilding)
                    unit.SetCaptureTarget(targetBuilding);
            }
        }
    }

    public override void Update()
    {
        if (!tactician)
            return;

        if (!tactician.IsNearTarget())
        {
            tactician.SetTargetPos(target.transform.position);

            if(isStoppedFollowFormation)
                isStoppedFollowFormation = false;
        }
        else
        {
            if (!isStoppedFollowFormation)
            {
                isStoppedFollowFormation = true;
                tactician.StopFollowFormations();
                SetCaptureTarget();
            }

            base.Update();
        }
    }

    public override void End() {}

    private void SetCaptureTarget()
    {
        if (target && tactician)
        {
            foreach (Unit unit in tactician.Soldiers)
            {
                if (target is TargetBuilding targetBuilding)
                    unit.SetCaptureTarget(targetBuilding);
            }
        }
    }
}