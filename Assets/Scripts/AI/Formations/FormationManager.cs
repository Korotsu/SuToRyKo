using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    private List<FormationNode> nodes = new List<FormationNode>();

    // Start is called before the first frame update
    void Start()
    {
        Tactician tactician = GetComponent<Tactician>();

        if (!tactician)
        {
            enabled = false;
            return;
        }


        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
