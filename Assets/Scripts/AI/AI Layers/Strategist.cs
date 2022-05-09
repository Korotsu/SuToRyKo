using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Strategist : MonoBehaviour
{
    private List<TacticianState> orderlist = new List<TacticianState>();

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
        TacticianState order = new IdleTactician();
        tacticians.ForEach(tactician => tactician.SetState(order));
    }
}
