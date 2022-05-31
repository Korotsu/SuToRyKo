using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace AI.BehaviorStates
{
    public class TacticianAttackState : TacticianState
    {
        public TacticianAttackState(Tactician _tactician, BaseEntity _target = null) : base(_tactician)
        {
            target = _target;
        }


        private void CheckAttackEnding()
        {
            // Every time an opposite enemy dies, check if everything is still functioning in the adverse tactician.
            
            // If not, go into a neutral state. If yes, keep going.
            
        }
        public override void Start() 
        {
            if (target is BaseEntity entity)
            {
                foreach (Unit unit in tactician.Soldiers)
                {
                    unit.UnitLogic.SetState(new AI.BehaviorStates.UnitCombatState(unit.UnitLogic, entity));
                }
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void End()
        {
            foreach (Unit unit in tactician.Soldiers)
                unit.UnitLogic.associatedUnit.OnDeadEvent -= CheckAttackEnding;
        }

    }
    
    public class UnitCombatState : UnitState
    {
        public UnitCombatState(UnitLogic unitLogic, BaseEntity _target = null) : base(unitLogic)
        {
            target = _target;
        }
        

        private bool SearchTarget(Unit[] opposingUnits)
        {
            // Reach into your Tactician,
            // Reach into the enemy Tactician,
            // Analyze the closest enemy of their group.

            float shortestDist = float.MaxValue;
            Unit closestUnit = null;

            foreach (Unit opposingUnit in opposingUnits)
            {
                float dist = Vector3.Distance(unit.transform.position, opposingUnit.transform.position);
                
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    closestUnit = opposingUnit;
                }
            }
            if (closestUnit)
            {
                target = closestUnit;
                unit.StartAttacking(closestUnit);
                return true;
            }
            
            return false;
        }

        public override void Start() 
        {
            switch (unitLogic.CurrentState.Target)
            {
                case Tactician _tactician:
                    SearchTarget(_tactician.Soldiers.ToArray());
                    break;
                case Unit _unit:
                    target = _unit;
                    unit.StartAttacking(_unit);
                    break;
                case Factory factory:
                    target = factory;
                    unit.StartAttacking(factory);
                    break;
                default:
                    break;
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
        if (!EntityTarget)
            return;

        LookAtTarget();

        // Efficient moving system towards target
        if (CanAttack(EntityTarget) == false)
        {
            if ( !(NavMeshAgent is null) && NavMeshAgent.isStopped )
            {
                NavMeshAgent.SetDestination(EntityTarget.transform.position);
                NavMeshAgent.isStopped = false;
            }
            return;
        }

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
