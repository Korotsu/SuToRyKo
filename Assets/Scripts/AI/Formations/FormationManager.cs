using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System;
using System.Linq;

namespace Formations
{
    public partial class FormationManager : MonoBehaviour
    {
        public enum EFormationTypes
        {
            Linear,
            VShaped,
            Curved,
        }

        private EFormationTypes formationType = EFormationTypes.Linear;

        private List<FormationNode> nodes = new List<FormationNode>();

        private FormationNode leaderNode;

        private Action UpdateFormation;

        private Tactician tactician;

        private NavMeshPath path;

        public Tactician Tactician => tactician;

        private NavMeshAgent navMeshAgent;

        private int formationSize;

        private Vector3 maxBounds = Vector3.zero;

        [SerializeField, Range(0, 10)]
        private int lineUnitNb = 5;

        private float lineSize = 0f;

        private int lineNb = 0;

        [SerializeField]
        private bool displayNodes = false;

        // Start is called before the first frame update
        void Start()
        {
            tactician = GetComponent<Tactician>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (!tactician && !navMeshAgent)
            {
                enabled = false;
                return;
            }

            leaderNode = new FormationNode(this, transform.position);

            UpdateFormation += UpdateLinearFormation;
        }

        public void SwitchFormationType(EFormationTypes newType)
        {
            nodes.Clear();

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
            UpdateFormation?.Invoke();

            if (path != null && navMeshAgent.path != path)
                navMeshAgent.SetPath(path);
        }

        private void OnDrawGizmos()
        {
            if (displayNodes)
            {
                foreach (FormationNode node in nodes)
                {
                    Gizmos.DrawCube(node.GetPosition(), maxBounds);
                }
            }
        }

        public void SetTargetPos(Vector3 pos)
        {
            SetFormationSpeed();

            if (!navMeshAgent.hasPath && path == null)
            {
                navMeshAgent.SetDestination(pos);
                navMeshAgent.isStopped = false;
            }

            else
            {
                navMeshAgent.SetDestination(pos);
                //path = new NavMeshPath();
                //navMeshAgent.CalculatePath(pos, path);
            }

            /*transform.position = pos;
            tactician.GetSoldiers().ForEach(soldier => soldier.Unit.UpdateTargetPos());*/
        }

        public void SetFormationSpeed()
        {
            List<Unit> units = tactician.GetSoldiers().Select(soldier => soldier.Unit).ToList();

            float maxSpeed          = float.MaxValue;
            float maxAngularSpeed   = float.MaxValue;
            float maxAcceleration   = float.MaxValue;

            foreach (Unit unit in units)
            {
                maxSpeed        = (unit.GetUnitData.Speed        < maxSpeed)         ? unit.GetUnitData.Speed         : maxSpeed;
                maxAngularSpeed = (unit.GetUnitData.AngularSpeed < maxAngularSpeed)  ? unit.GetUnitData.AngularSpeed  : maxAngularSpeed;
                maxAcceleration = (unit.GetUnitData.Acceleration < maxAcceleration)  ? unit.GetUnitData.Acceleration  : maxAcceleration;
            }

            foreach (Unit unit in units)
            {
                unit.NavMeshAgent.speed         = maxSpeed;
                unit.NavMeshAgent.angularSpeed  = maxAngularSpeed;
                unit.NavMeshAgent.acceleration  = maxAcceleration;
                unit.NavMeshAgent.radius        = maxBounds.x / 2;
            }

            navMeshAgent.speed          = maxSpeed * 0.9f;
            navMeshAgent.angularSpeed   = maxAngularSpeed * 0.9f;
            navMeshAgent.acceleration   = maxAcceleration;
            navMeshAgent.radius         = 0.1f;
            navMeshAgent.autoRepath     = true;
        }
    }
}
