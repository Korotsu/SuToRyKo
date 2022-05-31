using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Formations
{
    public partial class FormationManager
    {
        private void CreateFormation(float angle)
        {
            List<Soldier> soldiers = tactician.GetSoldiers();
            formationSize = soldiers.Count;

            List<Renderer> renderers = new List<Renderer>();

            foreach (Soldier soldier in soldiers)
            {
                GetRenderers(ref renderers, soldier.transform, true);
                renderers.ForEach(renderer => maxBounds = Vector3.Max(maxBounds, renderer.bounds.size));
                renderers.Clear();
            }

            lineSize = maxBounds.x * lineUnitNb;
            lineNb = Mathf.CeilToInt((float)soldiers.Count / lineUnitNb);
            float lastLineSize = maxBounds.x * (soldiers.Count - (lineUnitNb * (lineNb - 1)));
            float centerX = lineSize / 2;

            for (int i = 0; i < lineNb; i++)
            {
                for (int j = 0; j < lineUnitNb; j++)
                {
                    if (i * lineUnitNb + j >= soldiers.Count)
                        return;

                    if (i == lineNb - 1 && lastLineSize != 0)
                        centerX = lastLineSize / 2;

                    float distanceFromCenter = (maxBounds.x * (j + 0.5f)) - centerX;
                    float zOffset = Mathf.Abs(distanceFromCenter) * Mathf.Tan(Mathf.PI * (angle / 180));
                    float tempPreZ = (i + 1) * maxBounds.z;
                    float tempZ = -(tempPreZ + zOffset);
                    FormationNode formationNode = new FormationNode(this, new Vector3(distanceFromCenter, 0, tempZ));
                    soldiers[i * lineUnitNb + j].Unit.SetFormationNode(ref formationNode);
                    nodes.Add(formationNode);
                }
            }
        }

        private void GetRenderers(ref List<Renderer> renderers, Transform obj, bool includeChildren = false)
        {
            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer)
                renderers.Add(renderer);

            if (!includeChildren)
                return;

            foreach (Transform child in obj)
                GetRenderers(ref renderers, child, includeChildren);
        }

        private void CreateLinearFormation()
        {
            CreateFormation(0);
        }

        private void CreateVShapedFormation()
        {
            CreateFormation(45);
        }

        private void CreateCurvedFormation()
        {
            CreateFormation(30);
        }
    }
}
