using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace AI.BehaviorStates
{
    public class TacticianAttackState : TacticianState
    {
        readonly List<UnitLogic> units;

        public TacticianAttackState(Tactician _tactician, Base _target = null) : base(_tactician)
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
            switch (target)
            {
                case Tactician _tactician:
                    break;
                case Factory _factory:
                    foreach (Unit unit in tactician.Soldiers)
                    {
                        //unit.UnitLogic.SetState(new AI.BehaviorStates.UnitCombatState(unit));
                    }
                    break;
                default:
                    break;
            }
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override void End()
        {
            foreach (UnitLogic unit in units)
                unit.associatedUnit.OnDeadEvent -= CheckAttackEnding;
        }

    }
    
    public class UnitCombatState : UnitState
    {
        private BaseEntity currentTarget;
        

        public UnitCombatState(UnitLogic unitLogic, BaseEntity target) : base(unitLogic)
        {
            currentTarget = target;

            if (currentTarget is null)
            {
                Debug.LogWarning("An attack was ordered, but the target is null!", unit);
                SearchNewTarget();
            }

            else if (currentTarget.GetTeam() == unit.GetTeam())
            {
                Debug.LogWarning("An attack was ordered, but the target is of the same team!", unit);
                SearchNewTarget();
            }

            else
            {
                if(target is InteractableEntity entity)
                    unit.StartAttacking(entity);

                currentTarget.OnDeadEvent += SearchNewTarget;   
            }
        }
        
        public UnitCombatState(UnitLogic unitLogic) : base(unitLogic)
        {
            SearchNewTarget();
        }

        private Unit SearchTarget()
        {
            // Reach into your Tactician,
            // Reach into the enemy Tactician,
            // Analyze the closest enemy of their group.

            var opposingUnits = new List<Unit>();

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

            return closestUnit;
        }

        private void SearchNewTarget()
        {
            currentTarget = SearchTarget();

            if (currentTarget is null)
            {
                Debug.Log("Unit attack can't pass! There is no attack ongoing..", unit);
                return;                
            }
            
            //unit.StartAttacking(currentTarget);
            currentTarget.OnDeadEvent += SearchNewTarget;
        }
        public override void Start() {}

        public override void Update()
        {
            unit.ComputeAttack();
        }

        public override void End()
        {
            if (!(currentTarget is null))
                currentTarget.OnDeadEvent -= SearchNewTarget;
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
