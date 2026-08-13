using UnityEngine;
using UnityEngine.InputSystem;

namespace FishSpa.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FishPlayerController : MonoBehaviour
    {
        [Header("References")]
        public Transform cameraTransform;
        public Transform spawnPoint;

        [Header("Movement")]
        public float swimSpeed = 5.8f;
        public float verticalSpeed = 4.2f;
        public float dashSpeed = 11f;
        public float dashDuration = 0.16f;
        public float dashCooldown = 0.75f;
        public float turnSharpness = 12f;

        [Header("Bounds")]
        public bool constrainToPrototypeBounds = true;
        public Vector3 minBounds = new(-6f, -0.75f, -6.5f);
        public Vector3 maxBounds = new(6f, 4.25f, 14f);

        private CharacterController characterController;
        private Vector3 externalVelocity;
        private Vector3 dashDirection;
        private float dashTimer;
        private float dashCooldownTimer;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector3 moveDirection = ReadCameraRelativeMovement();
            float vertical = ReadVerticalInput();

            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);

            if (ReadDashPressed() && dashCooldownTimer <= 0f)
            {
                dashDirection = moveDirection.sqrMagnitude > 0.001f ? moveDirection.normalized : transform.forward;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
            }

            Vector3 velocity = moveDirection * swimSpeed;
            velocity += Vector3.up * (vertical * verticalSpeed);

            if (dashTimer > 0f)
            {
                velocity += dashDirection * dashSpeed;
                dashTimer -= deltaTime;
            }

            velocity += externalVelocity;
            characterController.Move(velocity * deltaTime);
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, 8f * deltaTime);

            if (constrainToPrototypeBounds)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
                    Mathf.Clamp(transform.position.z, minBounds.z, maxBounds.z));
            }

            Vector3 facing = velocity;
            facing.y *= 0.45f;
            if (facing.sqrMagnitude > 0.05f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-turnSharpness * deltaTime));
            }
        }

        public void AddImpulse(Vector3 impulse)
        {
            externalVelocity += impulse;
        }

        public void ResetToSpawn()
        {
            Vector3 targetPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion targetRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            characterController.enabled = false;
            transform.SetPositionAndRotation(targetPosition, targetRotation);
            externalVelocity = Vector3.zero;
            dashTimer = 0f;
            dashCooldownTimer = 0f;
            characterController.enabled = true;
        }

        private Vector3 ReadCameraRelativeMovement()
        {
            Vector2 input = ReadMoveInput();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Transform reference = cameraTransform != null ? cameraTransform : transform;
            Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = transform.forward;
            }

            return forward * input.y + right * input.x;
        }

        private static Vector2 ReadMoveInput()
        {
            Vector2 input = Vector2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input.y += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input.y -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input.x += 1f;
                }

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input.x -= 1f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                input += gamepad.leftStick.ReadValue();
            }

            return input;
        }

        private static float ReadVerticalInput()
        {
            float vertical = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.isPressed)
                {
                    vertical += 1f;
                }

                if (keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed)
                {
                    vertical -= 1f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.isPressed)
                {
                    vertical += 1f;
                }

                if (gamepad.buttonEast.isPressed)
                {
                    vertical -= 1f;
                }
            }

            return Mathf.Clamp(vertical, -1f, 1f);
        }

        private static bool ReadDashPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.leftStickButton.wasPressedThisFrame;
        }
    }
}
