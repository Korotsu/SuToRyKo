using System.Collections;
using System.Collections.Generic;
using Entities;
using UnityEngine;


namespace AI.StateMachine
{
    public class UnitRepair : UnitState
    {
        public UnitRepair(UnitLogic _targetLogic) : base(_targetLogic) { }

        public override void Start()
        {

        }

        public override void Update()
        {
            unitLogic.associatedUnit.ComputeRepairing();
        }

        public override void End()
        {

        }
    }
}

public partial class Unit
{
    #region IRepairable
    public override bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    public override void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    public override void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Task Methods
    // Targetting Task - repairing
    public void SetRepairTarget(InteractableEntity entity)
    {
        if (entity == null)
            return;

        if (entity.GetTeam() == GetTeam())
            StartRepairing(entity);

        if (CaptureTarget != null)
            StopCapture();
    }

    // Repairing Task
    public bool CanRepair(InteractableEntity target)
    {
        if (GetUnitData.CanRepair == false || target is null)
            return false;

        // distance check
        return (target.transform.position - transform.position).sqrMagnitude < GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax;
    }
    public void StartRepairing(InteractableEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            EntityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        if (CanRepair(EntityTarget) == false)
        {
            NavMeshAgent.SetDestination(EntityTarget.transform.position);
            NavMeshAgent.isStopped = false;
            return;
        }

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        LookAtTarget();

        if ((Time.time - ActionCooldown) > UnitData.RepairFrequency)
        {
            ActionCooldown = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            EntityTarget.Repair(amount);
        }
    }
    #endregion
}