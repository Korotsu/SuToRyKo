using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Formations
{
    public class FormationNode
    {
        private FormationManager manager;

        public FormationManager FormationManager => manager;

        private Vector3 relativePosition;

        public FormationNode(FormationManager _manager, Vector3 startPosition)
        {
            manager = _manager;
            relativePosition = startPosition;
        }

        public void SetRelativePosition(Vector3 newRelativePos) => relativePosition = newRelativePos;

        public Vector3 GetPosition() => (manager.transform.rotation * relativePosition) + manager.transform.position;
    }
}