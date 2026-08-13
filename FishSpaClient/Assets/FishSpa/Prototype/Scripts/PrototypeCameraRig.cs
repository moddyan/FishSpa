using UnityEngine;
using UnityEngine.InputSystem;

namespace FishSpa.Prototype
{
    public sealed class PrototypeCameraRig : MonoBehaviour
    {
        public Transform target;
        public Transform cameraTransform;

        [Header("Orbit")]
        public float distance = 7f;
        public float targetHeight = 0.9f;
        public float mouseSensitivity = 0.08f;
        public float gamepadSensitivity = 95f;
        public float minPitch = -22f;
        public float maxPitch = 58f;
        public float positionSharpness = 16f;

        [Header("Collision")]
        public float collisionRadius = 0.25f;
        public float minDistance = 1.8f;
        public LayerMask collisionMask = ~0;

        private float yaw;
        private float pitch = 18f;

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (target != null)
            {
                Vector3 euler = transform.eulerAngles;
                yaw = euler.y;
            }

            LockCursor();
        }

        private void LateUpdate()
        {
            if (target == null || cameraTransform == null)
            {
                return;
            }

            ReadLookInput();

            Vector3 pivot = target.position + Vector3.up * targetHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredOffset = rotation * (Vector3.back * distance);
            Vector3 desiredPosition = pivot + desiredOffset;

            Vector3 toDesired = desiredPosition - pivot;
            float desiredDistance = toDesired.magnitude;
            if (Physics.SphereCast(pivot, collisionRadius, toDesired.normalized, out RaycastHit hit, desiredDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = pivot + toDesired.normalized * Mathf.Max(minDistance, hit.distance - 0.15f);
            }

            float blend = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, blend);
            cameraTransform.rotation = Quaternion.LookRotation(pivot - cameraTransform.position, Vector3.up);
        }

        private void ReadLookInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                yaw += mouseDelta.x * mouseSensitivity;
                pitch -= mouseDelta.y * mouseSensitivity;

                if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
                {
                    LockCursor();
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 look = gamepad.rightStick.ReadValue();
                yaw += look.x * gamepadSensitivity * Time.deltaTime;
                pitch -= look.y * gamepadSensitivity * Time.deltaTime;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
