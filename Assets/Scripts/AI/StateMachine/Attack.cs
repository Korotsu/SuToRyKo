using UnityEngine;

namespace AI.StateMachine
{
    public class UnitAttack : UnitState
    {
        private readonly BaseEntity givenTarget;

        public UnitAttack(UnitLogic _targetLogic, BaseEntity _givenTarget) : base(_targetLogic)
        { givenTarget = _givenTarget; }
        
        public override void Start()
        {
            if (givenTarget is null)
                Debug.LogWarning("An attack was ordered, but the target is null!", targetUnit);

            else if (givenTarget.GetTeam() == targetUnit.GetTeam())
                Debug.LogWarning("An attack was ordered, but the target is of the same team!", targetUnit);

            else targetUnit.SetAttackTarget(givenTarget);
        }

        public override void Update()
        {
            targetUnit.ComputeAttack();
        }

        public override void End()
        {
            throw new System.NotImplementedException();
        }
    }
}


public partial class Unit
{
    // Begin attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (target is null)
            return;

        // TODO: Remove this as soon as the capture state is implemented
        if ( !(CaptureTarget is null) )
            StopCapture();
    }
    
    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
    } 
    
    public bool CanAttack(BaseEntity target)
    {
        if (target is null)
            return false;

        // distance check
        return (target.transform.position - transform.position).sqrMagnitude < GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax;
    }

    public void ComputeAttack()
    {
        // Efficient moving system towards target
        if (CanAttack(EntityTarget) == false)
        {
            if ( !(NavMeshAgent is null) && NavMeshAgent.isStopped )
            {
                NavMeshAgent.SetDestination(EntityTarget.transform.position);
                NavMeshAgent.isStopped = false;
            }
        }

        LookAtTarget();
        
        
        if (ActionCooldown <= 0f)
        {
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
}
