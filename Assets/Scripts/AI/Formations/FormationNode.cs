using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationNode
{
    private FormationManager manager;
    private Vector3 relativePosition;
    
    public Vector3 GetPosition()
    {
        return relativePosition + manager.transform.position;
    }
    
}
