using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "AreaConfig", menuName = "Wing/Configs/SubAreaSet", order = 6)]
    public class SubAreaSet : ScriptableObject
    {
        public CornerArea[] cornerAreas;
        public VEdgeArea[] vEdgeAreas;
        public HEdgeArea[] hEdgeAreas;
        public CentreArea[] centreAreas;
    }
}
