using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishSpa.Prototype
{
    public sealed class CleanableResidue : MonoBehaviour
    {
        private static readonly List<CleanableResidue> ActiveResidues = new();

        [Header("Cleaning")]
        public string displayName = "Residue";
        public float cleanDuration = 1.5f;
        public float progressDecayPerSecond = 0.12f;

        [Header("Visuals")]
        public Renderer residueRenderer;
        public Transform progressFill;
        public Color dirtyColor = new(0.35f, 0.2f, 0.08f);
        public Color cleaningColor = new(0.12f, 0.85f, 0.48f);

        public event Action<CleanableResidue> Cleaned;

        private readonly List<Collider> colliders = new();
        private readonly List<Renderer> renderers = new();
        private Vector3 initialScale;
        private Vector3 initialProgressScale;
        private float lastCleanTime = -999f;
        private float progress;
        private bool isCleaned;

        public static IReadOnlyList<CleanableResidue> Instances => ActiveResidues;
        public float Progress => progress;
        public bool IsCleaned => isCleaned;

        private void Awake()
        {
            initialScale = transform.localScale;

            if (residueRenderer == null)
            {
                residueRenderer = GetComponentInChildren<Renderer>();
            }

            GetComponentsInChildren(true, renderers);
            GetComponentsInChildren(true, colliders);

            if (progressFill != null)
            {
                initialProgressScale = progressFill.localScale;
            }

            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (!ActiveResidues.Contains(this))
            {
                ActiveResidues.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveResidues.Remove(this);
        }

        private void Update()
        {
            if (isCleaned || progress <= 0f)
            {
                return;
            }

            if (Time.time - lastCleanTime < 0.15f)
            {
                return;
            }

            progress = Mathf.Max(0f, progress - progressDecayPerSecond * Time.deltaTime);
            UpdateVisuals();
        }

        public void ApplyCleaning(float deltaTime)
        {
            if (isCleaned)
            {
                return;
            }

            lastCleanTime = Time.time;
            progress = Mathf.Clamp01(progress + deltaTime / Mathf.Max(0.05f, cleanDuration));
            UpdateVisuals();

            if (progress >= 1f)
            {
                CompleteCleaning();
            }
        }

        public void ResetResidue()
        {
            isCleaned = false;
            progress = 0f;
            lastCleanTime = -999f;

            foreach (Renderer residueRenderer in renderers)
            {
                residueRenderer.enabled = true;
            }

            foreach (Collider residueCollider in colliders)
            {
                residueCollider.enabled = true;
            }

            UpdateVisuals();
        }

        private void CompleteCleaning()
        {
            isCleaned = true;
            progress = 1f;
            UpdateVisuals();

            foreach (Renderer residueRenderer in renderers)
            {
                residueRenderer.enabled = false;
            }

            foreach (Collider residueCollider in colliders)
            {
                residueCollider.enabled = false;
            }

            Cleaned?.Invoke(this);
        }

        private void UpdateVisuals()
        {
            transform.localScale = initialScale * Mathf.Lerp(1f, 0.45f, progress);

            if (residueRenderer != null)
            {
                residueRenderer.material.color = Color.Lerp(dirtyColor, cleaningColor, progress);
            }

            if (progressFill == null)
            {
                return;
            }

            bool showProgress = progress > 0.01f && !isCleaned;
            progressFill.gameObject.SetActive(showProgress);

            if (showProgress)
            {
                float scale = Mathf.Lerp(0.08f, 1f, progress);
                progressFill.localScale = new Vector3(
                    initialProgressScale.x * scale,
                    initialProgressScale.y,
                    initialProgressScale.z * scale);
            }
        }
    }
}
