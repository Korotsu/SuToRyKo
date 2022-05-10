using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Formations
{
    public partial class FormationManager
    {
        private void CreateCurvedFormation()
        {
            throw new System.NotImplementedException();
        }

        private void UpdateCurvedFormation()
        {
            throw new System.NotImplementedException();
        }

        /*note for me : the Z offset : 
            center = nbofUnitInline / 2;
            offset = abs(rankInLine - center) * angle;
            node.z -= offset;
            repeat for all nodes;
        */
    }
}
