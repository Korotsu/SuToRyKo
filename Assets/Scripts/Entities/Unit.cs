﻿using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Unit : BaseEntity
{
    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;
    BaseEntity EntityTarget = null;
    TargetBuilding CaptureTarget = null;
    public NavMeshAgent NavMeshAgent;
    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }

    public Action actions;

    public Tactician mainTactician = null;

    public Tactician tempTactician = null;

    public Formations.FormationNode formationNode = null;

    private NavMeshPath path = null;

    private uint pathIndex = 0;

    private bool isCapturing = false;

    private float timeLeftForRaycast;

    [SerializeField]
    private readonly float raycastDelay = 5.0f;

    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }
    void Unit_OnDead()
    {
        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        Destroy(gameObject);
    }
    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;
        //NavMeshAgent.enabled        = false;

        timeLeftForRaycast = raycastDelay;
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {
        actions?.Invoke();

        // Attack / repair task debug test $$$ to be removed for AI implementation
        if (EntityTarget != null)
        {
            if (EntityTarget.GetTeam() != GetTeam())
                ComputeAttack();
            else
                ComputeRepairing();
        }

        if (CaptureTarget != null && !isCapturing && CanCapture(CaptureTarget))
            StartCapture(CaptureTarget);

    }
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos)
    {
        if (EntityTarget != null)
            EntityTarget = null;

        if (CaptureTarget != null)
            StopCapture();

        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }

    public void FollowFormation()
    {
        if (tempTactician || mainTactician)
        {
            timeLeftForRaycast -= Time.deltaTime;

            if (timeLeftForRaycast <= 0f)
            {
                NavMeshHit hit = new NavMeshHit();

                if (NavMeshAgent.Raycast(formationNode.GetPosition(), out hit) && (path == null || path.status == NavMeshPathStatus.PathInvalid || path.status == NavMeshPathStatus.PathPartial))
                {
                    path = new NavMeshPath();
                    if (NavMeshAgent.CalculatePath(formationNode.GetPosition(), path))
                        pathIndex = 0;
                    else
                        path = null;
                }

                timeLeftForRaycast = raycastDelay;
            }

            if (path != null && (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial))
            {
                Vector3 destination = path.corners[pathIndex] - transform.position;
                NavMeshAgent.Move(destination * NavMeshAgent.speed * Time.deltaTime);
                if ((destination).sqrMagnitude <= 0.1)
                {
                    pathIndex++;

                    if (path.corners.Length <= pathIndex)
                        path = null;
                }
            }

            else if (formationNode.FormationManager && (formationNode.GetPosition() - transform.position).sqrMagnitude >= 0.1)
            {
                Vector3 destination = formationNode.GetPosition() - transform.position;
                destination.Normalize();
                NavMeshAgent.Move(destination * NavMeshAgent.speed * Time.deltaTime);
            }
        }
    }

    public void UpdateTargetPos()
    {
        //transform.position = formationNode.GetPosition();
        if (formationNode.FormationManager)
            SetTargetPos(formationNode.GetPosition());
    }

    public void SetFormationNode(ref Formations.FormationNode _formationNode)
    {
        formationNode = _formationNode;
        actions += FollowFormation;
    }

    public List<Unit> GetAllUnitsInFormation()
    {
        List<Unit> units = new List<Unit>();
        Tactician tactician = tempTactician ?? mainTactician;

        if (tactician)
            tactician.GetSoldiers().ForEach(soldier => units.Add(soldier.Unit));

        return units;
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (target == null)
            return;

        //if (CanAttack(target) == false)
        //    SetTargetPos(target.transform.position);
        //
        if (target.GetTeam() != GetTeam())
            StartAttacking(target);



        if (CaptureTarget != null)
            StopCapture();
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target)
    {
        if (target == null)
            return;

        if (CanCapture(target) == false)
            SetTargetPos(target.transform.position);

        if (EntityTarget != null)
            EntityTarget = null;

        if (IsCapturing())
            StopCapture();

        CaptureTarget = target;
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity)
    {
        if (entity == null)
            return;

        if (entity.GetTeam() == GetTeam())
            StartRepairing(entity);

        if (CaptureTarget != null)
            StopCapture();
    }
    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
    }
    public void ComputeAttack()
    {
        if (CanAttack(EntityTarget) == false) //TODO: Check if you already have the current position of his target.
        {
            if (NavMeshAgent)
            {
                NavMeshAgent.SetDestination(EntityTarget.transform.position);
                NavMeshAgent.isStopped = false;
            }

            return;
        }

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(EntityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            EntityTarget.AddDamage(damages);
        }
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

    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false || target.GetTeam() == GetTeam())
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        CaptureTarget = target;
        CaptureTarget.StartCapture(this);

        isCapturing = true;
    }
    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;

        isCapturing = false;
    }

    public bool IsCapturing()
    {
        return isCapturing;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
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

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
        {
            LastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            EntityTarget.Repair(amount);
        }
    }
    #endregion
}
