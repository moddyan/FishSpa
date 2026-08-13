using UnityEngine;
using UnityEngine.InputSystem;

namespace FishSpa.Prototype
{
    public sealed class FishCleaner : MonoBehaviour
    {
        [Header("Cleaning")]
        public float cleanRadius = 1.85f;
        public float targetRefreshInterval = 0.08f;

        private float nextTargetRefreshTime;
        private CleanableResidue currentTarget;
        private bool isCleaningHeld;

        public CleanableResidue CurrentTarget => currentTarget;
        public bool IsCleaningHeld => isCleaningHeld;
        public float CurrentTargetProgress => currentTarget != null ? currentTarget.Progress : 0f;

        private void Update()
        {
            if (Time.time >= nextTargetRefreshTime)
            {
                nextTargetRefreshTime = Time.time + targetRefreshInterval;
                currentTarget = FindNearestTarget();
            }

            isCleaningHeld = ReadCleanHeld();

            if (isCleaningHeld && currentTarget != null)
            {
                currentTarget.ApplyCleaning(Time.deltaTime);

                if (currentTarget.IsCleaned)
                {
                    currentTarget = null;
                }
            }
        }

        public void ClearTarget()
        {
            currentTarget = null;
            isCleaningHeld = false;
        }

        private CleanableResidue FindNearestTarget()
        {
            CleanableResidue bestTarget = null;
            float bestDistanceSquared = cleanRadius * cleanRadius;
            Vector3 origin = transform.position;

            foreach (CleanableResidue residue in CleanableResidue.Instances)
            {
                if (residue == null || residue.IsCleaned)
                {
                    continue;
                }

                float distanceSquared = (residue.transform.position - origin).sqrMagnitude;
                if (distanceSquared <= bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestTarget = residue;
                }
            }

            return bestTarget;
        }

        private static bool ReadCleanHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.isPressed)
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonNorth.isPressed;
        }
    }
}
