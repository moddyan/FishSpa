using UnityEngine;

namespace FishSpa.Prototype
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        public PrototypeRoundManager roundManager;
        public FishCleaner cleaner;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle completeStyle;

        private void OnGUI()
        {
            EnsureStyles();
            EnsureReferences();

            if (roundManager == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 430f, 230f), panelStyle);
            GUILayout.Label("Fish Spa Prototype V0.1", titleStyle);
            GUILayout.Label("WASD swim | Space up | C down | Shift dash | E/Left Mouse clean | R reset", labelStyle);
            GUILayout.Space(8f);

            GUILayout.Label($"Time: {roundManager.ElapsedTime:0.0}s", labelStyle);
            GUILayout.Label($"Cleaned: {roundManager.CleanedResidues}/{roundManager.TotalResidues}", labelStyle);
            DrawBar(roundManager.CleanProgress, new Color(0.2f, 0.75f, 0.95f), "Mouth cleanliness");

            GUILayout.Space(8f);
            CleanableResidue target = cleaner != null ? cleaner.CurrentTarget : null;
            if (target != null)
            {
                string verb = cleaner.IsCleaningHeld ? "Cleaning" : "Hold E to clean";
                GUILayout.Label($"{verb}: {target.displayName}", labelStyle);
                DrawBar(target.Progress, new Color(0.25f, 0.95f, 0.45f), "Residue progress");
            }
            else
            {
                GUILayout.Label("Move close to a brown residue spot to clean it.", labelStyle);
            }

            GUILayout.EndArea();

            if (roundManager.IsComplete)
            {
                Rect completeRect = new(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 80f, 440f, 160f);
                GUILayout.BeginArea(completeRect, panelStyle);
                GUILayout.Label("Cleaning Complete", completeStyle);
                GUILayout.Label($"All residue cleared in {roundManager.ElapsedTime:0.0}s", labelStyle);
                GUILayout.Label("Press R to reset the prototype.", labelStyle);
                GUILayout.EndArea();
            }
        }

        private void DrawBar(float value, Color fillColor, string label)
        {
            Rect rect = GUILayoutUtility.GetRect(390f, 18f);
            GUI.Box(rect, GUIContent.none);

            Rect fillRect = new(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
            Color previous = GUI.color;
            GUI.color = fillColor;
            GUI.Box(fillRect, GUIContent.none);
            GUI.color = previous;

            GUI.Label(rect, $"{label}: {Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%", labelStyle);
        }

        private void EnsureReferences()
        {
            if (roundManager == null)
            {
                roundManager = FindAnyObjectByType<PrototypeRoundManager>();
            }

            if (cleaner == null)
            {
                cleaner = FindAnyObjectByType<FishCleaner>();
            }
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            completeStyle = new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28
            };
        }
    }
}
