using UnityEngine;
using UnityEngine.AI;
using System;
using Entities;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public partial class Unit : InteractableEntity
{
    [SerializeField] private UnitDataScriptable UnitData = null;

    private Transform BulletSlot;
    private float ActionCooldown = 0f;
    private InteractableEntity entityTarget = null;
    public InteractableEntity EntityTarget { get => entityTarget;  set => entityTarget = value; }
    public NavMeshAgent NavMeshAgent;
    public UnitDataScriptable GetUnitData => UnitData;
    public int Cost => UnitData.Cost;
    public int GetTypeId => UnitData.TypeId;

    public Action actions;

    public Tactician mainTactician = null;

    public Tactician tempTactician = null;

    public Formations.FormationNode formationNode = null;

    private NavMeshPath path = null;

    private uint pathIndex = 0;

    private float timeLeftForRaycast;

    [SerializeField]
    private readonly float raycastDelay = 5.0f;

    private UnitLogic unitLogic = null;

    public bool recovery = false;
    public UnitLogic UnitLogic { get => unitLogic; }

    float GetPower()
    {
        float p = UnitData.GetPower();
        p *= (HP / (float)UnitData.MaxHP);
        return p;
    }
    protected override float GetInfluence()
    {
        return UnitData.GetInfluence(GetPower());
    }

    public override void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
        unitLogic ??= new UnitLogic(this);
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

        if (tempTactician && tempTactician != mainTactician)
            tempTactician.Soldiers.Remove(this);

        if(mainTactician)
            mainTactician.Soldiers.Remove(this);

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
        //NavMeshAgent.enabled        = false;

        timeLeftForRaycast = raycastDelay;
    }
    protected override void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    protected override void Update()
    {
        actions?.Invoke();

        if (ActionCooldown > 0f)
            ActionCooldown -= Time.deltaTime;

        if(!mainTactician && !tempTactician)
            unitLogic.Update();

        if (path != null && path.status != NavMeshPathStatus.PathInvalid && NavMeshAgent.path != path)
        {
            NavMeshAgent.SetDestination(path.corners[path.corners.Length-1]);
            NavMeshAgent.isStopped = false;
        }
    }
    #endregion



    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos)
    {

        if (!(CaptureTarget is null))
            StopCapture();

        if (NavMeshAgent)
        {
            path = new NavMeshPath();
            NavMeshAgent.CalculatePath(pos, path);
            pathIndex = 0;
        }
    }

    public void Stop()
    {
        NavMeshAgent.isStopped  = true;
        path                    = null;
        pathIndex               = 0;
        NavMeshAgent.ResetPath();
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
                    recovery = true;
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
                transform.rotation = Quaternion.LookRotation(destination);
                if ((destination).sqrMagnitude <= 0.1)
                {
                    pathIndex++;

                    if (pathIndex >= path.corners.Length)
                    {
                        pathIndex   = 0;
                        path        = null;
                        recovery    = false;
                    }
                }
            }

            else if (formationNode.FormationManager && (formationNode.GetPosition() - transform.position).sqrMagnitude >= 0.1)
            {
                Vector3 destination = formationNode.GetPosition() - transform.position;
                destination.Normalize();
                NavMeshAgent.Move(destination * NavMeshAgent.speed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(destination);
            }
        }
    }

    public void SetFormationNode(ref Formations.FormationNode _formationNode)
    {
        formationNode = _formationNode;
        if (actions == null || actions.GetInvocationList().Length == 0)
            actions += FollowFormation;
    }

    public void CheckRecovery()
    {
        if (!recovery)
            Stop();

        if (actions == null || actions.GetInvocationList().Length == 0)
        {
            actions += FollowFormation;
            NavMeshAgent.isStopped = false;
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
