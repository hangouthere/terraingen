using UnityEngine;

namespace nfg.UnityEngine.Scripts {

    // Left Mouse Button = Activate
    // WASD = Expected Movement
    // Q = Move Down
    // E = Move Up
    // SPACE = Move on X/Z-Axis only

    public class FlyingMovement : MonoBehaviour {

        [Range(0.1f, 500f)]
        [SerializeField] private float FlySpeed = 100.0f;
        [Range(0.1f, 5f)]
        [SerializeField] private float ShiftMultiplier = 2.5f;
        [Range(0.1f, 4f)]
        [SerializeField] private float LookSensitivity = 0.25f;
        [SerializeField] private bool IsCamera;
        [SerializeField] private bool IsInverted;

        private Vector3 LastMousePosition;

        private bool IsMouseOverGameWindow {
            get {
                return !(
                    0 > Input.mousePosition.x
                    || 0 > Input.mousePosition.y
                    || Screen.width < Input.mousePosition.x
                    || Screen.height < Input.mousePosition.y
                );
            }
        }

        private Vector3 KeyboardInput {
            get {
                // WASD = Expected Movement
                Vector3 p_Velocity = new Vector3(
                    Input.GetAxisRaw("Horizontal"),
                    0,
                    Input.GetAxisRaw("Vertical")
                );

                // Q = Move Down
                if (Input.GetKey(KeyCode.Q)) {
                    p_Velocity.y = -1;
                }

                // E = Move Up
                if (Input.GetKey(KeyCode.E)) {
                    p_Velocity.y = 1;
                }

                // SPACE = Move on X/Z-Axis only
                if (Input.GetKey(KeyCode.Space)) {
                    p_Velocity.y = 0;
                }

                return p_Velocity;
            }
        }

        void Update() {
            if (!IsMouseOverGameWindow) {
                return;
            }

            if (Input.GetMouseButton(0)) {
                RotateAndTranslate();
            }

            LastMousePosition = Input.mousePosition;
        }

        private void RotateAndTranslate() {
            Vector3 inputVector = KeyboardInput;

            LastMousePosition = null != LastMousePosition ? Input.mousePosition - LastMousePosition : Input.mousePosition;

            // Invert Axis and Y-direction to match Unity inputs vs screen input
            if (IsCamera) {
                LastMousePosition = new Vector3((IsInverted ? 1 : -1) * LastMousePosition.y * LookSensitivity, LastMousePosition.x * LookSensitivity);
            } else {
                LastMousePosition = new Vector3((IsInverted ? -1 : 1) * LastMousePosition.y * LookSensitivity, LastMousePosition.x * LookSensitivity, 0);
            }

            // Change rotation of our object
            transform.eulerAngles = transform.eulerAngles + LastMousePosition;

            if (Input.GetKey(KeyCode.LeftShift)) {
                inputVector *= ShiftMultiplier;
            }

            inputVector *= FlySpeed * Time.deltaTime;

            transform.Translate(inputVector);
        }
    }

}