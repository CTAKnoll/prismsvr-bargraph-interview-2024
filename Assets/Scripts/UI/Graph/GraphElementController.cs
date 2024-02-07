using UI.Model.Templates;
using UnityEngine;

namespace UI.Graph
{
    public class GraphElementController : UIController<GraphElementView, GraphElementModel>
    {
        public bool SwapLocked;
        public int Index => View.transform.GetSiblingIndex();
        public float Value => Model.Value;

        private Graph LayoutGraph;
        private int MoveThreshold;
        private float ThresholdScalar;
        
        public GraphElementController(GraphElementTemplate template, Transform parent, GraphElementModel model, Graph graph, float threshold) : base(template, parent, model)
        {
            LayoutGraph = graph;
            ThresholdScalar = threshold;
            MoveThreshold = (int) (View.RectTransform.rect.width * threshold);
            
            UiDriver.RegisterForHold(View.BarInteractable, null, null, MonitorForSwap);
            View.UpdateViewWithModel(model);
        }

        public Transform Element => View.gameObject.transform;

        public void SetWidth(float width)
        {
            Model.Width = width;
            MoveThreshold = (int) (width * ThresholdScalar);
            UpdateViewAtEndOfFrame();
        }

        public void SetScalar(float scalar)
        {
            Model.Scalar = scalar;
            UpdateViewAtEndOfFrame();
        }

        public void SetColor(Color color)
        {
            Model.Color = color;
            UpdateViewAtEndOfFrame();
        }
        
        private void MonitorForSwap()
        {
            Vector3 holdMagnitude = UiDriver.PointerWorldPosition - View.transform.position;
            if (Index != 0 && holdMagnitude.x <= -MoveThreshold && !SwapLocked)
            {
                LayoutGraph.SwapLeft(Index);
            }

            if (Index != LayoutGraph.Size-1 && holdMagnitude.x >= MoveThreshold && !SwapLocked)
            {
                LayoutGraph.SwapRight(Index);
            }
        }
    }
}