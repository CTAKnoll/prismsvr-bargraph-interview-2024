using UnityEngine;

namespace UI.Graph
{
    public struct GraphElementModel : IUIModel
    {
        public float Index;
        public float Value;
        
        public float Scalar;
        public float Width; 
        
        public Vector3 ValueTextNudge;
        public Color Color;
    }
}