using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorStates
{
    public class IdleUnit : UnitState
    {
        public IdleUnit(UnitLogic unitLogic) : base(unitLogic) {}

        public override void Update() {}
        
        public override void End() {}
    }

    public class IdleTactician : TacticianState
    {
        public IdleTactician(Tactician _tactician) : base(_tactician){}

        public override void Update() {}

        public override void End() {}
    }
}