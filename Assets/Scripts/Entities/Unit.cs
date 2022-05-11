using UnityEngine;
using UnityEngine.AI;
using System;

public partial class Unit : BaseEntity
{
    [SerializeField] private UnitDataScriptable UnitData = null;

    private Transform BulletSlot;
    private float ActionCooldown = 0f;
    private BaseEntity EntityTarget = null;
    private NavMeshAgent NavMeshAgent;
    public UnitDataScriptable GetUnitData => UnitData;
    public int Cost => UnitData.Cost;
    public int GetTypeId => UnitData.TypeId;

    public Action actions;

    public FormationNode formationNode = null;

    private UnitLogic unitLogic = null;

    public UnitLogic UnitLogic { get => unitLogic; }
    
    public override void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }

    private void Unit_OnDead()
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
    protected override void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;
    }
    protected override void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        if (!unitLogic)
        {
            unitLogic = gameObject.AddComponent<UnitLogic>();
            unitLogic.SetUnit(this);
        }
            

        base.Start();
    }
    protected override void Update()
    {
        if (ActionCooldown > 0f)
            ActionCooldown -= Time.time;

        CaptureUpdate();
    }
    #endregion

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

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos)
    {
        EntityTarget = null;

        if ( !(CaptureTarget is null) )
            StopCapture();

        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }
    
    public void FollowFormation()
    {
        SetTargetPos(formationNode.GetPosition());
    }

    public void SetFormationNode(ref FormationNode _formationNode)
    {
        formationNode = _formationNode;
        actions += FollowFormation;
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

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target is null)
            return false;

        // distance check
        return (target.transform.position - transform.position).sqrMagnitude < GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax;
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
    
    private void LookAtTarget()
    {
        Transform _transform = transform;
        
        _transform.LookAt(EntityTarget.transform);
        
        // only keep Y axis
        Vector3 eulerRotation = _transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        
        _transform.eulerAngles = eulerRotation;
    }
}
