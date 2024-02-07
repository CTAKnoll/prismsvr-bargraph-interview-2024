using UI.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Services
{
    public class UIRoot : UIInteractable
    {
        [Header("Core Objects")]
        public Camera camera;
        public GraphicRaycaster raycaster;
        public PlayerInput input;
        
        [Header("Input Actions")]
        public InputActionReference point; 
        public InputActionReference navigate; 
        public InputActionReference tap; 
        public InputActionReference hold; 
        public InputActionReference altTap;
        public InputActionReference altHold;
        public InputActionReference scroll;
        public InputActionReference back;

        [Header("Top Level Views")] 
        public SimpleButtonView TestButton;

        private UIDriver Controller;
        

        // The UIRoot inverts the normal controller/view power balance because it needs to bootstrap the UI system
        public void Awake()
        { 
            Controller = new UIDriver(this);
        }
    }
}