using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorStates
{
    public class UnitCapture : UnitState
    {
        public UnitCapture(UnitLogic unitLogic) : base(unitLogic) { }

        public override void Start() {}

        public override void Update()
        {
            if(unitLogic.associatedUnit)
                unitLogic.associatedUnit.CaptureUpdate();
        }

        public override void End()
        {
            if (unitLogic.associatedUnit)
                unitLogic.associatedUnit.StopCapture();
        }        
    }
}

partial class Unit
{
    TargetBuilding CaptureTarget = null;

    private bool isCapturing = false;

    // Capture Task
    public void StartCapture()
    {
        if (!CaptureTarget || CanCapture(CaptureTarget) == false || CaptureTarget.GetTeam() == GetTeam())
            return;

        if (NavMeshAgent)
            Stop();

        CaptureTarget.StartCapture(this);

        isCapturing = true;
    }
    public void StopCapture()
    {
        if (CaptureTarget)
        {
            CaptureTarget.StopCapture(this);
            CaptureTarget = null;
        }

        isCapturing = false;
    }

    public bool CanCapture(TargetBuilding target)
    {
        if (target is null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }

    public bool IsCapturing()
    {
        return isCapturing;
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target)
    {
        if (target is null)
            return;

        if (IsCapturing() && target != CaptureTarget)
            StopCapture();

        if (NavMeshAgent)
        {
            path = new NavMeshPath();
            NavMeshAgent.CalculatePath(target.transform.position, path);
        }

        EntityTarget = null;

        CaptureTarget = target;
    }

    public void CaptureUpdate()
    {
        if (!isCapturing && CanCapture(CaptureTarget))
            StartCapture();
    }
}