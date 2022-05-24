using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Formation_Data", menuName = "RTS/FormationData", order = 2)]
    public class FormationData : ScriptableObject
    {
        [Tooltip("Number of light units in %"), Range(0f, 1f)] public float lightUnit = 0f;
        [Tooltip("Nunber of heavy units in  %"), Range(0f, 1f)] public float HeavyUnit = 0f;
    }
}
