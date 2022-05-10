using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Formations
{
    public partial class FormationManager
    {
        private void CreateLinearFormation()
        {
            List<Soldier> soldiers = tactician.GetSoldiers();
            formationSize = soldiers.Count;

            Vector3 maxBounds = Vector3.zero;

            foreach (Soldier soldier in soldiers)
            {
                Bounds bounds = soldier.gameObject.GetComponent<Mesh>().bounds;
                maxBounds = Vector3.Max(maxBounds, bounds.size);
            }

            lineSize = maxBounds.x * lineUnitNb;
            lineNb = Mathf.CeilToInt((float)soldiers.Count / lineUnitNb);

        }

        private void UpdateLinearFormation()
        {
            throw new System.NotImplementedException();
        }
    }
}