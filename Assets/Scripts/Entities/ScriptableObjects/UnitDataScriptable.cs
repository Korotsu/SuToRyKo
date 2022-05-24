using UnityEngine;

[CreateAssetMenu(fileName = "Unit_Data", menuName = "RTS/UnitData", order = 0)]
public class UnitDataScriptable : EntityDataScriptable
{
    [Header("Combat")]
    public int DPS = 10;
    public float AttackFrequency = 1f;
    public float AttackDistanceMax = 10f;
    public float CaptureDistanceMax = 10f;
   

    [Header("Repairing")]
    public bool CanRepair = false;
    public int RPS = 10;
    public float RepairFrequency = 1f;
    public float RepairDistanceMax = 10f;

    [Header("Movement")]
    [Tooltip("Overrides NavMeshAgent steering settings")]
    public float Speed = 10f;
    public float AngularSpeed = 200f;
    public float Acceleration = 20f;
    public bool IsFlying = false;

    [Header("FX")]
    public GameObject BulletPrefab = null;
    public GameObject DeathFXPrefab = null;

    public override float GetPower()
    {
        float p =  DPS * AttackFrequency;
        if (CanRepair)
        {
            p += RPS * RepairFrequency;
            p *= 1.2f;
        }

        p *= 1 + (AttackDistanceMax / 100.0f);
        p *= 1 + (Speed / 100.0f);
        p *= (MaxHP / 50.0f);
        return p;
    }
    public override float GetInfluence(float Power)
    {
        float coef = (Cost * BuildDuration);
        if (Cost == 0)
            coef = BuildDuration;
        if (BuildDuration == 0)
            coef = Cost;
        
        return  Power -(coef *3.0f);
    }
   
}
