using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Formations
{
    public partial class FormationManager : MonoBehaviour
    {
        private enum EFormationTypes
        {
            Linear,
            VShaped,
            Curved,
        }

        private EFormationTypes formationType = EFormationTypes.Linear;

        private List<FormationNode> nodes;

        private FormationNode leaderNode;

        private Action UpdateFormation;

        private Tactician tactician;

        private int formationSize;

        [SerializeField, Range(0, 10)]
        private int lineUnitNb = 5;

        private float lineSize = 0f;

        private int lineNb = 0;


        // Start is called before the first frame update
        void Start()
        {
            tactician = GetComponent<Tactician>();

            if (!tactician)
            {
                enabled = false;
                return;
            }

            leaderNode = new FormationNode(this, transform.position);

            CreateLinearFormation();
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
                    CreateLinearFormation();
                    UpdateFormation += UpdateLinearFormation;
                    break;
                case EFormationTypes.VShaped:
                    CreateVShapedFormation();
                    UpdateFormation += UpdateVShapedFormation;
                    break;
                case EFormationTypes.Curved:
                    CreateCurvedFormation();
                    UpdateFormation += UpdateCurvedFormation;
                    break;
                default:
                    break;
            }

            formationType = newType;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFormation();
        }
    }
}
