using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base : MonoBehaviour
{
    [SerializeField]
    protected ETeam Team = ETeam.Neutral;

    public float Influence => GetInfluence();
    protected abstract float GetInfluence();

    public virtual ETeam GetTeam()
    {
        return Team;
    }

    public void SetTeam(ETeam newTeam)
    {
        Team = newTeam;
    }
}
