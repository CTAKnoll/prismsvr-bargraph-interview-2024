using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Graph
{
    public class GraphElementView : UIView<GraphElementModel>
    {
        [Header("Child Views")]
        [SerializeField] private UIInteractable GraphBarInteractable;
        [SerializeField] private Image GraphBarImage;
        [SerializeField] private TextMeshProUGUI IndexText;
        [SerializeField] private TextMeshProUGUI ValueText;
        
        [HideInInspector] public RectTransform RectTransform;
        private float BaseHeight;

        public void Awake()
        {
            RectTransform = (RectTransform)GraphBarInteractable.transform;
            BaseHeight = RectTransform.rect.height;
            //Debug.Log(Time.frameCount); // whats up with the one frame popin, need to investigate...
        }

        public UIInteractable BarInteractable => GraphBarInteractable;
        public override void UpdateViewWithModel(GraphElementModel model)
        {
            //Debug.Log(Time.frameCount);
            var yScale = model.Value / model.Scalar;
            RectTransform.sizeDelta = new Vector2(model.Width, BaseHeight * yScale);

            GraphBarImage.color = model.Color;
            
            IndexText.text = model.Index.ToString();
            
            ValueText.text = model.Value.ToString();
            // Set the position of the ValueText based on the height of the element
            ValueText.transform.localPosition = (RectTransform.localPosition.y +
                                                 RectTransform.localScale.y *
                                                 RectTransform.rect.height) * Vector3.up +
                                                model.ValueTextNudge;
        }
    }
}