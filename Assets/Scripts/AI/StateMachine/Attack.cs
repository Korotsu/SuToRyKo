using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using AI.BehaviorStates;
using UnityEngine;

namespace AI.BehaviorStates
{
    public class TacticianAttackState : TacticianState
    {
        readonly List<UnitLogic> units;

        public TacticianAttackState(Tactician _tactician) : base(_tactician)
        {
            // Cycle through all units and set their states as attack!

            // Additionally, keep track of all enemies by binding their death state to a countdown!
            
            // Allies
            foreach (UnitLogic unit in units)
            {
                unit.SetState(new UnitCombatState(unit));
            }
            
            // Opponents
            foreach (UnitLogic unit in units)
                unit.associatedUnit.OnDeadEvent += CheckAttackEnding;
            
        }


        private void CheckAttackEnding()
        {
            // Every time an opposite enemy dies, check if everything is still functioning in the adverse tactician.
            
            // If not, go into a neutral state. If yes, keep going.
            
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
        private Unit currentTarget;
        

        public UnitCombatState(UnitLogic unitLogic, Unit givenTarget) : base(unitLogic)
        {
            currentTarget = givenTarget;

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
                unit.StartAttacking(currentTarget);
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
            
            unit.StartAttacking(currentTarget);
            currentTarget.OnDeadEvent += SearchNewTarget;
        }

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
    public void StartAttacking(BaseEntity target)
    {
        NavMeshAgent.isStopped = true;
        
        EntityTarget = target;
    }

    private bool CanAttack(BaseEntity target)
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
