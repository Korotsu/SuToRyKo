using Entities;
using UnityEngine;

namespace AI.StateMachine
{
    public class UnitAttack : UnitState
    {
        private Unit givenTarget;
        

        public UnitAttack(UnitLogic unitLogic, Unit _givenTarget) : base(unitLogic)
        {
            givenTarget = _givenTarget;
        }
        
        public override void Start()
        {
            if (givenTarget is null)
                Debug.LogWarning("An attack was ordered, but the target is null!", unit);

            else if (givenTarget.GetTeam() == unit.GetTeam())
                Debug.LogWarning("An attack was ordered, but the target is of the same team!", unit);

            else
            {
                unit.StartAttacking(givenTarget);
                givenTarget.OnDeadEvent += () => unitLogic.SetState(new IdleUnit(unitLogic));

            }
        }

        public override void Update()
        {
            unit.ComputeAttack();
        }

        public override void End()
        {
            
        }
    }
}


public partial class Unit
{
    // Begin Attack, stops movements!
    public void StartAttacking(InteractableEntity target)
    {
        NavMeshAgent.isStopped = true;
        
        EntityTarget = target;
    }

    private bool CanAttack(InteractableEntity target)
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
