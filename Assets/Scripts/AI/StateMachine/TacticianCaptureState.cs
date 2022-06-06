using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticianCaptureState : TacticianState
{
    private bool alreadyStopMovement        = false;

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
            }
        }
    }

    public override void Update()
    {
        if (!tactician)
            return;

        bool isNotInFormation = tactician.FormationManager.nodes.Count < 2;

        if (tactician.IsNearTarget() || isNotInFormation)
        {
            if (!isNotInFormation)
                tactician.StopFollowFormations();

            if (!alreadyStopMovement)
            {
                alreadyStopMovement     = true;
                SetCaptureTarget();
            }


            base.Update();
        }

        else if (target.transform.position != tactician.targetPosition)
        {
            tactician.SetTargetPos(target.transform.position);
            alreadyStopMovement     = false;
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