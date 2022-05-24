using UnityEngine;

public class EntityDataScriptable : ScriptableObject
{
    public enum Type
    {
        Light,
        Heavy
    }
    [Header("Build Data")]
    public int TypeId = 0;
    public Type type = Type.Light;
    public string Caption = "Unknown Unit";
    public int Cost = 1;
    public float BuildDuration = 1f;

    [Header("Health Points")]
    public int MaxHP = 100;


    public virtual float GetPower()
    {
        return 0;
    }
    public virtual float GetInfluence(float Power)
    {
        return Power;
    }
    public float GetInfluence()
    {
        return GetInfluence(GetPower());
    }
    
}
