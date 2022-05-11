using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.StateMachine
{
    public class UnitCapture : UnitState
    {
        public UnitCapture(UnitLogic _targetLogic) : base(_targetLogic) { }
        public override void Start()
        {
            //targetLogic.AssociatedUnit.StartCapture();
        }

        public override void Update()
        {
            targetLogic.AssociatedUnit.CaptureUpdate();
        }

        public override void End()
        {
            targetLogic.AssociatedUnit.StopCapture();
        }
    }
}

public partial class Unit
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
        if (CaptureTarget != null)
        {
            CaptureTarget.StopCapture(this);
            CaptureTarget = null;
        }

        isCapturing = false;
    }

    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
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
        if (target == null)
            return;

        if (IsCapturing() && target != CaptureTarget)
            StopCapture();

        if (CanCapture(target) == false)
        {
            NavMeshAgent.SetDestination(target.transform.position);
            NavMeshAgent.isStopped = false;
        }

            if (EntityTarget != null)
            EntityTarget = null;

        CaptureTarget = target;
    }

    public void CaptureUpdate()
    {
        if (CaptureTarget && !isCapturing && CanCapture(CaptureTarget))
            StartCapture();
    }
}