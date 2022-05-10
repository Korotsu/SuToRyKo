using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleSoldier : SoldierState
{
    public IdleSoldier(Soldier _soldier) : base(_soldier) {}

    // Start is called before the first frame update
    public override void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }

    public override void End()
    {

    }
}

public class IdleTactician : TacticianState
{
    public IdleTactician(Tactician _tactician) : base(_tactician){}

    // Start is called before the first frame update
    public override void Start()
    {

    }

    // Update is called once per frame
    public override void Update()
    {

    }

    public override void End()
    {

    }
}
