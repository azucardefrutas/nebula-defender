using UnityEngine;
using UnityEngine.InputSystem;

namespace NebulaDefender
{
    public sealed class PilotInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        private InputActionMap flight;
        private InputAction throttle, turn, fire, shield, pause;
        public float Throttle => flight != null && flight.enabled ? throttle.ReadValue<float>() : 0;
        public float Turn => flight != null && flight.enabled ? turn.ReadValue<float>() : 0;
        public bool Fire => flight != null && flight.enabled && fire.IsPressed();
        public bool ShieldPressed => flight != null && flight.enabled && shield.WasPressedThisFrame();
        public bool PausePressed => pause != null && pause.WasPressedThisFrame();
        public bool FlightEnabled => flight != null && flight.enabled;
        public void Configure(InputActionAsset asset) => actions = asset;
        private void Awake()
        {
            actions = Instantiate(actions);
            flight = actions.FindActionMap("Flight", true);
            throttle = flight.FindAction("Throttle", true); turn = flight.FindAction("Turn", true);
            fire = flight.FindAction("Fire", true); shield = flight.FindAction("Shield", true);
            pause = actions.FindAction("System/Pause", true);
            actions.FindActionMap("System").Enable();
        }
        public void SetFlight(bool value) { if (value) flight.Enable(); else flight.Disable(); }
        private void OnDestroy() { if (actions != null) { actions.Disable(); Destroy(actions); } }
    }
}
