using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace NebulaDefender
{
    public sealed class PilotInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        private InputActionMap flight;
        private InputAction throttle, turn, fire, shield, pause, help, view, point, pointerFire, pointerThrust, pointerShield;
        private bool pointerReady, mouseSteering;
        private Vector2 lastPointer;
        private PointerEventData pointerEvent;
        private readonly System.Collections.Generic.List<RaycastResult> uiHits = new System.Collections.Generic.List<RaycastResult>();
        private int raycastFrame = -1;
        private Vector2 raycastPosition;
        private bool pointerOverUI;
        public Vector2 PointerPosition => point != null ? point.ReadValue<Vector2>() : Vector2.zero;
        public bool PointerAvailable => FlightEnabled && pointerReady && PointerInside && !PointerOverUI;
        public bool MouseSteering => mouseSteering && PointerAvailable;
        public bool PointerOverUI
        {
            get
            {
                var events = EventSystem.current;
                if (events == null) return false;
                var position = PointerPosition;
                if (raycastFrame != Time.frameCount || raycastPosition != position)
                {
                    if (pointerEvent == null) pointerEvent = new PointerEventData(events);
                    pointerEvent.position = position; uiHits.Clear(); events.RaycastAll(pointerEvent,uiHits);
                    pointerOverUI = uiHits.Count > 0; raycastFrame = Time.frameCount; raycastPosition = position;
                }
                return pointerOverUI;
            }
        }
        private bool PointerInside => PointerPosition.x >= 0 && PointerPosition.x <= Screen.width && PointerPosition.y >= 0 && PointerPosition.y <= Screen.height;
        public float Throttle => FlightEnabled ? (Mathf.Abs(throttle.ReadValue<float>()) > 0.01f ? throttle.ReadValue<float>() : PointerAvailable && pointerThrust.IsPressed() ? 1 : 0) : 0;
        public float Turn => flight != null && flight.enabled ? turn.ReadValue<float>() : 0;
        public bool Fire => FlightEnabled && (fire.IsPressed() || PointerAvailable && pointerFire.IsPressed());
        public bool ShieldPressed => FlightEnabled && (shield.WasPressedThisFrame() || PointerAvailable && pointerShield.WasPressedThisFrame());
        public bool PausePressed => pause != null && pause.WasPressedThisFrame();
        public bool HelpPressed => help != null && help.WasPressedThisFrame();
        public bool ViewPressed => view != null && view.WasPressedThisFrame();
        public bool FlightEnabled => flight != null && flight.enabled;
        public void Configure(InputActionAsset asset) => actions = asset;
        private void Awake()
        {
            actions = Instantiate(actions);
            flight = actions.FindActionMap("Flight", true);
            throttle = flight.FindAction("Throttle", true); turn = flight.FindAction("Turn", true);
            fire = flight.FindAction("Fire", true); shield = flight.FindAction("Shield", true);
            pause = actions.FindAction("System/Pause", true);
            point = flight.AddAction("MousePoint", InputActionType.PassThrough, "<Mouse>/position", expectedControlLayout: "Vector2");
            pointerFire = flight.AddAction("MouseFire", InputActionType.Button, "<Mouse>/leftButton");
            pointerThrust = flight.AddAction("MouseThrust", InputActionType.Button, "<Mouse>/rightButton");
            pointerShield = flight.AddAction("MouseShield", InputActionType.Button, "<Mouse>/middleButton");
            var system = actions.FindActionMap("System");
            help = system.FindAction("Help") ?? system.AddAction("Help", InputActionType.Button, "<Keyboard>/f1");
            view = system.FindAction("View") ?? system.AddAction("View", InputActionType.Button, "<Keyboard>/c");
            system.Enable();
        }
        private void Update()
        {
            if (!FlightEnabled) return;
            var position = PointerPosition;
            if (!pointerReady && !pointerFire.IsPressed() && !pointerThrust.IsPressed()) pointerReady = true;
            if (Mathf.Abs(Turn) > 0.1f) mouseSteering = false;
            else if (PointerAvailable && ((position - lastPointer).sqrMagnitude > 4 || pointerFire.WasPressedThisFrame() || pointerThrust.WasPressedThisFrame())) mouseSteering = true;
            lastPointer = position;
        }
        public void SetFlight(bool value)
        {
            pointerReady = false; mouseSteering = false;
            if (value) { flight.Enable(); lastPointer = PointerPosition; } else flight.Disable();
        }
        private void OnDestroy() { if (actions != null) { actions.Disable(); Destroy(actions); } }
    }
}
