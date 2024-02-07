using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Graph
{
    public class Graph : MonoBehaviour, IService
    {
        [Header("Child Views")]
        [SerializeField] private LayoutGroup BarGroup;
        [SerializeField] private SimpleButtonView ResetButton;
        [SerializeField] private SimpleButtonView SortButton;
        [SerializeField] private SimpleButtonView ColorButton;

        [Header("Tunable Fields")] 
        public float TransitionTime = 1.0f;
        public int TransitionIterations = 50;
        public float SwapThresholdMultiplier = 2f;
        public float ScaleToHeight = 5f;
        public Color DefaultColor = Color.red;
        public Color SuccessColor = Color.green;
        public float SortColorChangeInterval = 0.1f;

        private List<GraphElementController> GraphElements;
        public int Size => GraphElements.Count;
        
        private TemplateServer TemplateServer;

        private bool LockInteractions = false;
        
        [Serializable]
        public struct GraphData
        {
            public float Index;
            public float Value;
        }

        [Header("Graph Data")]
        public List<GraphData> Data;

        private bool IsUniqueColored = false;
        private static List<Color> ColoringColors = new()
        {
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            new(1f, 0.5f, 0.2f), // orange
            new(0.6f, 0.4f, 0.4f), // brown
            Color.green,
        };
        
        public GraphElementModel GetModelFromData(GraphData data)
        {
            return new GraphElementModel
            {
                Index = data.Index,
                Value = data.Value,
                Color = DefaultColor,
                Scalar = 1f,
                ValueTextNudge = 25 * Vector3.up
            };
        }

        private void Awake()
        {
            ServiceLocator.RegisterService(this);
        }

        public void Start()
        {
            ServiceLocator.TryGetService(out TemplateServer);

            _ = new SimpleButtonController(ResetButton, ResetGraph);
            _ = new SimpleButtonController(SortButton, DoAnimatedSort);
            _ = new SimpleButtonController(ColorButton, ToggleColorGraph);
            SetupGraphElements();
        }

        private void ResetGraph()
        {
            foreach (var elem in GraphElements)
            {
                elem.Close();
            }
            GraphElements.Clear();
            SetupGraphElements();
        }

        private void SetupGraphElements()
        {
            if(GraphElements == null)
                GraphElements = new();

            float maxValue = ScaleToHeight;
            for (int i = 0; i < Data.Count; i++)
            {
                var element = new GraphElementController(TemplateServer.GraphElement, transform,
                    GetModelFromData(Data[i]), this, SwapThresholdMultiplier);

                if (Data[i].Value > maxValue)
                    maxValue = Data[i].Value;
                
                element.SetWidth(((RectTransform)transform).rect.width / (2 * Data.Count)); // set width correctly
                GraphElements.Add(element);
            }

            // set height scalar correctly
            foreach (var elem in GraphElements)
            {
                elem.SetScalar(maxValue/ScaleToHeight);
            }
        }
        
        public void SwapLeft(int index)
        {
            if (LockInteractions)
                return;
            
            StartCoroutine(Swap(index, index - 1));
        }
        
        public void SwapRight(int index)
        {
            if (LockInteractions)
                return;
            
            StartCoroutine(Swap(index, index + 1));
        }

        private void DoAnimatedSort()
        {
            LockInteractions = true;
            StartCoroutine(AnimatedSort());
        }

        // Crappy bubblesort but we animate it yaay!
        private IEnumerator AnimatedSort()
        {
            for (int i = 0; i < GraphElements.Count - 1; i++)
            {
                for (int j = 0; j < GraphElements.Count - 1; j++)
                {
                    if (GraphElements[j].Value > GraphElements[j + 1].Value)
                    {
                        yield return StartCoroutine(Swap(j, j + 1));
                    }
                }
            }

            // Let's do some pretty coloring to show that we're done
            var WaitForInterval = new WaitForSeconds(SortColorChangeInterval);
            for (int i = 0; i < GraphElements.Count; i++)
            {
                GraphElements[i].SetColor(SuccessColor);
                yield return WaitForInterval;
            }
            for (int i = 0; i < GraphElements.Count; i++)
            {
                GraphElements[i].SetColor(DefaultColor);
            }
            
            IsUniqueColored = false;
            LockInteractions = false;
        }

        private IEnumerator Swap(int indexOne, int indexTwo)
        {
            BarGroup.enabled = false; // stop the layout group from being difficult
            GraphElementController controllerOne = GraphElements[indexOne];
            GraphElementController controllerTwo = GraphElements[indexTwo];
            var controllerOneInitIndex = controllerOne.Element.GetSiblingIndex();
            var controllerTwoInitIndex = controllerTwo.Element.GetSiblingIndex();
            var controllerOneInitPos = controllerOne.Element.position;
            var controllerTwoInitPos = controllerTwo.Element.position;
            controllerOne.SwapLocked = true;
            controllerTwo.SwapLocked = true;

            float lerpVal = 0f;
            var transitionInterval = new WaitForSeconds(TransitionTime / TransitionIterations);
            while (lerpVal < 1f)
            {
                lerpVal += (float) 1 / TransitionIterations;
                controllerOne.Element.position = Vector3.Lerp(controllerOneInitPos, controllerTwoInitPos, lerpVal);
                controllerTwo.Element.position = Vector3.Lerp(controllerTwoInitPos, controllerOneInitPos, lerpVal);
                yield return transitionInterval;
            }
            controllerOne.Element.position = controllerTwoInitPos;
            controllerTwo.Element.position = controllerOneInitPos;

            controllerOne.Element.SetSiblingIndex(controllerTwoInitIndex);
            controllerTwo.Element.SetSiblingIndex(controllerOneInitIndex);
            (GraphElements[indexOne], GraphElements[indexTwo]) = (GraphElements[indexTwo], GraphElements[indexOne]);

            controllerOne.SwapLocked = false;
            controllerTwo.SwapLocked = false;
            BarGroup.enabled = true;
        }

        private void ToggleColorGraph()
        {
            if(IsUniqueColored)
                UncolorGraph();
            else
                ColorGraph();
            IsUniqueColored = !IsUniqueColored;
        }
        
        //TODO: Will fail when they are more than 8 unique elements! Not scalable!!
        private void ColorGraph()
        {
            using List<Color>.Enumerator colors = ColoringColors.GetEnumerator();
            Dictionary<float, Color> chosenColors = new();
            foreach(var element in GraphElements)
            {
                bool novel = true;
                foreach (var key in chosenColors.Keys)
                {
                    if (element.Value.ApproxEquals(key))
                    {
                        novel = false;
                        element.SetColor(chosenColors[key]);
                    }
                }

                if (novel)
                {
                    if (colors.MoveNext())
                    {
                        Color nextColor = colors.Current;
                        chosenColors[element.Value] = nextColor;
                        element.SetColor(nextColor);
                    }
                    else
                    {
                        throw new IndexOutOfRangeException($"Graph :: Not enough colors ({ColoringColors.Count}) to color unique objects!");
                    }
                }
            }
        }

        private void UncolorGraph()
        {
            foreach (var element in GraphElements)
            {
                element.SetColor(DefaultColor);
            }
        }
    }
}
