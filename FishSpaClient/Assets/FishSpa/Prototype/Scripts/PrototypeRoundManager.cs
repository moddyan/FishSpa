using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FishSpa.Prototype
{
    public sealed class PrototypeRoundManager : MonoBehaviour
    {
        public FishPlayerController player;
        public FishCleaner cleaner;

        private readonly List<CleanableResidue> residues = new();
        private float elapsedTime;
        private bool isComplete;

        public int TotalResidues => residues.Count;
        public int CleanedResidues { get; private set; }
        public float CleanProgress => TotalResidues > 0 ? CleanedResidues / (float)TotalResidues : 1f;
        public float ElapsedTime => elapsedTime;
        public bool IsComplete => isComplete;

        private void Start()
        {
            CacheResidues();
            ResetRound();
        }

        private void OnDestroy()
        {
            foreach (CleanableResidue residue in residues)
            {
                if (residue != null)
                {
                    residue.Cleaned -= HandleResidueCleaned;
                }
            }
        }

        private void Update()
        {
            if (ReadResetPressed())
            {
                ResetRound();
            }

            if (!isComplete)
            {
                elapsedTime += Time.deltaTime;
            }
        }

        public void ResetRound()
        {
            elapsedTime = 0f;
            isComplete = false;
            CleanedResidues = 0;

            foreach (CleanableResidue residue in residues)
            {
                if (residue != null)
                {
                    residue.ResetResidue();
                }
            }

            if (player != null)
            {
                player.ResetToSpawn();
            }

            if (cleaner != null)
            {
                cleaner.ClearTarget();
            }
        }

        private void CacheResidues()
        {
            residues.Clear();
            residues.AddRange(FindObjectsByType<CleanableResidue>(FindObjectsInactive.Exclude));

            foreach (CleanableResidue residue in residues)
            {
                residue.Cleaned -= HandleResidueCleaned;
                residue.Cleaned += HandleResidueCleaned;
            }
        }

        private void HandleResidueCleaned(CleanableResidue residue)
        {
            CleanedResidues = 0;
            foreach (CleanableResidue cleanableResidue in residues)
            {
                if (cleanableResidue != null && cleanableResidue.IsCleaned)
                {
                    CleanedResidues++;
                }
            }

            if (CleanedResidues >= TotalResidues)
            {
                isComplete = true;
            }
        }

        private static bool ReadResetPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.startButton.wasPressedThisFrame;
        }
    }
}
