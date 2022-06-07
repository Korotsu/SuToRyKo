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
            Custom,
        }

        public EFormationTypes formationType = EFormationTypes.Linear;

        public List<FormationNode> nodes = new List<FormationNode>();

        private FormationNode leaderNode;

        private Action UpdateFormation;

        private Tactician tactician;

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

        [SerializeField, Range(0, 180)]
        private float formationAngle = 0;

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
        }

        public void SwitchFormationType(EFormationTypes newType)
        {
            nodes.Clear();

            switch (newType)
            {
                case EFormationTypes.Linear:
                    CreateFormation(0);
                    break;
                case EFormationTypes.VShaped:
                    CreateFormation(45);
                    break;
                case EFormationTypes.Curved:
                    CreateFormation(30);
                    break;
                case EFormationTypes.Custom:
                    CreateFormation(formationAngle);
                    break;
                default:
                    break;
            }

            formationType = newType;
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
            if (!tactician)
                return;

            SetFormationSpeed();

            if (!navMeshAgent.hasPath)
            {
                bool result = navMeshAgent.SetDestination(pos);
                navMeshAgent.isStopped = false;
            }

            else
                navMeshAgent.SetDestination(pos);

            if (nodes.Count <= 1)
                tactician.GetSoldiers().ForEach(soldier => soldier.SetTargetPos(pos));
            else
                tactician.GetSoldiers().ForEach(soldier => soldier.CheckRecovery());
        }

        public void SetFormationSpeed()
        {
            if (!tactician)
                return;

            List<Unit> units = tactician.GetSoldiers().Select(soldier => soldier).ToList();

            float maxSpeed = float.MaxValue;
            float maxAngularSpeed = float.MaxValue;
            float maxAcceleration = float.MaxValue;
            float CaptureDistanceMax = float.MaxValue;

            foreach (Unit unit in units)
            {
                maxSpeed = (unit.GetUnitData.Speed < maxSpeed) ? unit.GetUnitData.Speed : maxSpeed;
                maxAngularSpeed = (unit.GetUnitData.AngularSpeed < maxAngularSpeed) ? unit.GetUnitData.AngularSpeed : maxAngularSpeed;
                maxAcceleration = (unit.GetUnitData.Acceleration < maxAcceleration) ? unit.GetUnitData.Acceleration : maxAcceleration;
                CaptureDistanceMax = (unit.GetUnitData.CaptureDistanceMax < CaptureDistanceMax) ? unit.GetUnitData.CaptureDistanceMax : CaptureDistanceMax;
            }

            foreach (Unit unit in units)
            {
                unit.NavMeshAgent.speed = maxSpeed;
                unit.NavMeshAgent.angularSpeed = maxAngularSpeed;
                unit.NavMeshAgent.acceleration = maxAcceleration;
                unit.NavMeshAgent.radius = 0.001f; //float.Epsilon;//maxBounds.x / 2;
            }

            navMeshAgent.speed              = maxSpeed * 0.9f;
            navMeshAgent.angularSpeed       = maxAngularSpeed * 0.9f;
            navMeshAgent.acceleration       = maxAcceleration;
            navMeshAgent.radius             = 0.1f;
            navMeshAgent.autoRepath         = true;
            tactician.maxDistanceToTarget   = CaptureDistanceMax;
        }
    }
}
