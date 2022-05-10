using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FormationManager : MonoBehaviour
{
    private enum EFormationTypes
    {
        Linear,
        VShaped,
        Curved,
    }

    private EFormationTypes formationType = EFormationTypes.Linear;
     
    private List<FormationNode> nodes = new List<FormationNode>();

    public Action UpdateFormation;

    // Start is called before the first frame update
    void Start()
    {
        Tactician tactician = GetComponent<Tactician>();

        if (!tactician)
        {
            enabled = false;
            return;
        }

        UpdateFormation += UpdateLinearFormation;
    }

    private void SwitchFormationType(EFormationTypes newType)
    {
        switch (formationType)
        {
            case EFormationTypes.Linear:
                UpdateFormation -= UpdateLinearFormation;
                break;
            case EFormationTypes.VShaped:
                UpdateFormation -= UpdateVShapedFormation;
                break;
            case EFormationTypes.Curved:
                UpdateFormation -= UpdateCurvedFormation;
                break;
            default:
                break;
        }

        switch (newType)
        {
            case EFormationTypes.Linear:
                UpdateFormation += UpdateLinearFormation;
                break;
            case EFormationTypes.VShaped:
                UpdateFormation += UpdateVShapedFormation;
                break;
            case EFormationTypes.Curved:
                UpdateFormation += UpdateCurvedFormation;
                break;
            default:
                break;
        }

        formationType = newType;
    }

    private void UpdateLinearFormation()
    {

    }

    private void UpdateVShapedFormation()
    {

    }

    private void UpdateCurvedFormation()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateFormation();
    }
}
