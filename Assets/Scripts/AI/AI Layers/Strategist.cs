using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Strategist : MonoBehaviour
{
    [SerializeField]
    private List<State> orderlist = new List<State>();

    private List<Tactician> tacticians = new List<Tactician>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            Tactician tactician = child.GetComponent<Tactician>();
            if (tactician)
                tacticians.Add(tactician);
        }
    }

    // Update is called once per frame
    void Update()
    {
        TakeDecision();
    }

    private void TakeDecision()
    {
        //Decisional code with influence and modifier Map;
        State order = new Idle();
        tacticians.ForEach(tactician => tactician.SetState(order));
    }
}
