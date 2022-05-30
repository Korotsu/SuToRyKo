using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base : MonoBehaviour
{
    [SerializeField]
    protected ETeam Team;

    public float Influence => GetInfluence();
    protected abstract float GetInfluence();

    public ETeam GetTeam()
    {
        return Team;
    }
}
