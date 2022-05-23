using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.StateMachine
{
    public class UnitCapture : UnitState
    {
        public UnitCapture(UnitLogic unitLogic) : base(unitLogic) { }
        public override void Start()
        {

        }

        public override void Update()
        {
            unitLogic.associatedUnit.CaptureUpdate();
        }

        public override void End()
        {
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
            NavMeshAgent.isStopped = true;

        CaptureTarget.StartCapture(this);

        isCapturing = true;
    }
    public void StopCapture()
    {
        if ( !(CaptureTarget is null) )
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

        if (CanCapture(target) == false)
        {
            NavMeshAgent.SetDestination(target.transform.position);
            NavMeshAgent.isStopped = false;
        }

        EntityTarget = null;

        CaptureTarget = target;
    }

    public void CaptureUpdate()
    {
        if (CaptureTarget && !isCapturing && CanCapture(CaptureTarget))
            StartCapture();
    }
}