using System.IO;
using FishSpa.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishSpa.Prototype.Editor
{
    public static class FishSpaPrototypeBootstrap
    {
        private const string PrototypeRoot = "Assets/FishSpa/Prototype";
        private const string ScenesPath = PrototypeRoot + "/Scenes";
        private const string MaterialsPath = PrototypeRoot + "/Materials";
        private const string ScenePath = ScenesPath + "/WhaleMouthPrototype.unity";

        [MenuItem("Fish Spa/Prototype/Rebuild Whale Mouth Prototype")]
        public static void RebuildPrototypeScene()
        {
            EnsureFolders();

            Material mouthMaterial = CreateMaterial("MAT_Prototype_Mouth", new Color(0.75f, 0.24f, 0.32f));
            Material gumMaterial = CreateMaterial("MAT_Prototype_Gum", new Color(0.95f, 0.34f, 0.42f));
            Material tongueMaterial = CreateMaterial("MAT_Prototype_Tongue", new Color(0.92f, 0.22f, 0.45f));
            Material toothMaterial = CreateMaterial("MAT_Prototype_Tooth", new Color(0.95f, 0.92f, 0.78f));
            Material fishMaterial = CreateMaterial("MAT_Prototype_Fish", new Color(0.08f, 0.62f, 1f));
            Material tailMaterial = CreateMaterial("MAT_Prototype_FishTail", new Color(0.02f, 0.32f, 0.85f));
            Material residueMaterial = CreateMaterial("MAT_Prototype_Residue", new Color(0.36f, 0.2f, 0.08f));
            Material progressMaterial = CreateMaterial("MAT_Prototype_CleanProgress", new Color(0.1f, 0.9f, 0.42f));
            Material markerMaterial = CreateMaterial("MAT_Prototype_Marker", new Color(0.0f, 0.85f, 0.95f));
            Material throatMaterial = CreateMaterial("MAT_Prototype_Throat", new Color(0.06f, 0.02f, 0.05f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new("FishSpa_PrototypeRoot");
            GameObject mouthRoot = new("WhaleMouth_Blockout");
            mouthRoot.transform.SetParent(root.transform);

            CreateLighting();
            Transform playerSpawn = CreateSpawnMarker(root.transform, markerMaterial);
            FishPlayerController player = CreatePlayer(root.transform, playerSpawn, fishMaterial, tailMaterial);
            PrototypeCameraRig cameraRig = CreateCameraRig(root.transform, player.transform);
            player.cameraTransform = cameraRig.cameraTransform;

            CreateMouthBlockout(mouthRoot.transform, mouthMaterial, gumMaterial, toothMaterial, tongueMaterial, throatMaterial);
            CreateResidues(mouthRoot.transform, residueMaterial, progressMaterial);

            GameObject systems = new("PrototypeSystems");
            systems.transform.SetParent(root.transform);
            PrototypeRoundManager roundManager = systems.AddComponent<PrototypeRoundManager>();
            PrototypeHud hud = systems.AddComponent<PrototypeHud>();
            roundManager.player = player;
            roundManager.cleaner = player.GetComponent<FishCleaner>();
            hud.roundManager = roundManager;
            hud.cleaner = roundManager.cleaner;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Fish Spa Prototype", "WhaleMouthPrototype scene rebuilt.", "OK");
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ToFileSystemPath(ScenesPath));
            Directory.CreateDirectory(ToFileSystemPath(MaterialsPath));
            AssetDatabase.Refresh();
        }

        private static string ToFileSystemPath(string assetPath)
        {
            string relativeToAssets = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativeToAssets);
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            string path = $"{MaterialsPath}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader)
                {
                    name = materialName
                };

                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            GameObject fillLightObject = new("Mouth Fill Light");
            Light fillLight = fillLightObject.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 9f;
            fillLight.range = 18f;
            fillLight.color = new Color(1f, 0.55f, 0.62f);
            fillLightObject.transform.position = new Vector3(0f, 2f, 4f);
        }

        private static Transform CreateSpawnMarker(Transform parent, Material markerMaterial)
        {
            GameObject spawn = new("PlayerSpawn");
            spawn.transform.SetParent(parent);
            spawn.transform.SetPositionAndRotation(new Vector3(0f, 1.4f, -5.2f), Quaternion.Euler(0f, 0f, 0f));

            GameObject marker = CreatePrimitive("EntryMarker", PrimitiveType.Cylinder, spawn.transform, markerMaterial, false);
            marker.transform.localPosition = new Vector3(0f, -1.05f, 0f);
            marker.transform.localScale = new Vector3(1.3f, 0.03f, 1.3f);

            return spawn.transform;
        }

        private static FishPlayerController CreatePlayer(Transform parent, Transform spawn, Material bodyMaterial, Material tailMaterial)
        {
            GameObject player = new("PlayerFish");
            player.transform.SetParent(parent);
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.radius = 0.36f;
            characterController.height = 0.95f;
            characterController.center = Vector3.zero;

            FishPlayerController controller = player.AddComponent<FishPlayerController>();
            controller.spawnPoint = spawn;
            controller.swimSpeed = 5.8f;
            controller.verticalSpeed = 4.2f;
            controller.dashSpeed = 11f;

            FishCleaner cleaner = player.AddComponent<FishCleaner>();
            cleaner.cleanRadius = 1.85f;

            GameObject body = CreatePrimitive("Body", PrimitiveType.Capsule, player.transform, bodyMaterial, false);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.52f, 0.72f, 0.52f);

            GameObject nose = CreatePrimitive("Nose", PrimitiveType.Sphere, player.transform, bodyMaterial, false);
            nose.transform.localPosition = new Vector3(0f, 0f, 0.55f);
            nose.transform.localScale = new Vector3(0.34f, 0.24f, 0.24f);

            GameObject tail = CreatePrimitive("Tail", PrimitiveType.Cube, player.transform, tailMaterial, false);
            tail.transform.localPosition = new Vector3(0f, 0f, -0.7f);
            tail.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            tail.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);

            return controller;
        }

        private static PrototypeCameraRig CreateCameraRig(Transform parent, Transform target)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 120f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 2.6f, -11f);
            cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

            GameObject rigObject = new("ThirdPersonCameraRig");
            rigObject.transform.SetParent(parent);
            PrototypeCameraRig rig = rigObject.AddComponent<PrototypeCameraRig>();
            rig.target = target;
            rig.cameraTransform = cameraObject.transform;
            rig.distance = 7f;
            rig.targetHeight = 0.75f;
            rig.mouseSensitivity = 0.08f;
            return rig;
        }

        private static void CreateMouthBlockout(
            Transform parent,
            Material mouthMaterial,
            Material gumMaterial,
            Material toothMaterial,
            Material tongueMaterial,
            Material throatMaterial)
        {
            GameObject lowerJaw = CreatePrimitive("LowerJaw", PrimitiveType.Cube, parent, mouthMaterial, true);
            lowerJaw.transform.localPosition = new Vector3(0f, -1.65f, 4.5f);
            lowerJaw.transform.localScale = new Vector3(13.5f, 0.55f, 20f);

            GameObject upperJaw = CreatePrimitive("UpperJaw", PrimitiveType.Cube, parent, mouthMaterial, true);
            upperJaw.transform.localPosition = new Vector3(0f, 5.1f, 4.5f);
            upperJaw.transform.localScale = new Vector3(13.5f, 0.55f, 20f);

            GameObject leftGum = CreatePrimitive("LeftGumWall", PrimitiveType.Cube, parent, gumMaterial, true);
            leftGum.transform.localPosition = new Vector3(-6.75f, 1.7f, 4.5f);
            leftGum.transform.localScale = new Vector3(0.55f, 6.8f, 20f);

            GameObject rightGum = CreatePrimitive("RightGumWall", PrimitiveType.Cube, parent, gumMaterial, true);
            rightGum.transform.localPosition = new Vector3(6.75f, 1.7f, 4.5f);
            rightGum.transform.localScale = new Vector3(0.55f, 6.8f, 20f);

            GameObject throat = CreatePrimitive("DarkThroatExit", PrimitiveType.Cylinder, parent, throatMaterial, false);
            throat.transform.localPosition = new Vector3(0f, 1.7f, 14.2f);
            throat.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            throat.transform.localScale = new Vector3(2.8f, 0.28f, 2.2f);

            GameObject tongue = CreatePrimitive("Tongue_LowIntensityHazard", PrimitiveType.Cube, parent, tongueMaterial, true);
            tongue.transform.localPosition = new Vector3(0f, -0.9f, 4.9f);
            tongue.transform.localScale = new Vector3(8.2f, 0.55f, 9.5f);
            BoxCollider tongueCollider = tongue.GetComponent<BoxCollider>();
            tongueCollider.isTrigger = true;
            tongue.AddComponent<TongueWaggle>();

            CreateTeeth(parent, toothMaterial);
        }

        private static void CreateTeeth(Transform parent, Material toothMaterial)
        {
            float[] zPositions = { -3.4f, -1.25f, 0.9f, 3.05f, 5.2f, 7.35f, 9.5f, 11.65f };
            float[] xPositions = { -5.75f, 5.75f };

            foreach (float z in zPositions)
            {
                foreach (float x in xPositions)
                {
                    GameObject lowerTooth = CreatePrimitive($"LowerTooth_{x}_{z}", PrimitiveType.Capsule, parent, toothMaterial, true);
                    lowerTooth.transform.localPosition = new Vector3(x, -0.92f, z);
                    lowerTooth.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0f ? -10f : 10f);
                    lowerTooth.transform.localScale = new Vector3(0.42f, 0.85f, 0.42f);

                    GameObject upperTooth = CreatePrimitive($"UpperTooth_{x}_{z}", PrimitiveType.Capsule, parent, toothMaterial, true);
                    upperTooth.transform.localPosition = new Vector3(x, 4.35f, z);
                    upperTooth.transform.localRotation = Quaternion.Euler(180f, 0f, x < 0f ? 10f : -10f);
                    upperTooth.transform.localScale = new Vector3(0.42f, 0.95f, 0.42f);
                }
            }
        }

        private static void CreateResidues(Transform parent, Material residueMaterial, Material progressMaterial)
        {
            Vector3[] positions =
            {
                new(-3.8f, 0.08f, -1.8f),
                new(3.65f, 0.08f, -0.2f),
                new(-4.9f, 1.6f, 2.3f),
                new(4.95f, 1.9f, 4.8f),
                new(-2.3f, 4.72f, 3.6f),
                new(2.2f, 4.72f, 6.7f),
                new(-1.9f, -0.47f, 7.8f),
                new(2.7f, -0.47f, 9.9f)
            };

            Quaternion[] rotations =
            {
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(0f, 0f, -90f),
                Quaternion.Euler(0f, 0f, 90f),
                Quaternion.Euler(180f, 0f, 0f),
                Quaternion.Euler(180f, 0f, 0f),
                Quaternion.identity,
                Quaternion.identity
            };

            Vector3[] scales =
            {
                new(0.75f, 0.12f, 0.75f),
                new(0.75f, 0.12f, 0.75f),
                new(0.12f, 0.72f, 0.72f),
                new(0.12f, 0.72f, 0.72f),
                new(0.82f, 0.12f, 0.82f),
                new(0.82f, 0.12f, 0.82f),
                new(0.9f, 0.12f, 0.9f),
                new(0.9f, 0.12f, 0.9f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject residue = CreatePrimitive($"Residue_{i + 1:00}", PrimitiveType.Sphere, parent, residueMaterial, true);
                residue.transform.localPosition = positions[i];
                residue.transform.localRotation = rotations[i];
                residue.transform.localScale = scales[i];

                CleanableResidue cleanable = residue.AddComponent<CleanableResidue>();
                cleanable.displayName = $"Residue {i + 1}";
                cleanable.cleanDuration = 1.45f;
                cleanable.progressDecayPerSecond = 0.08f;
                cleanable.residueRenderer = residue.GetComponent<Renderer>();

                GameObject progress = CreatePrimitive("ProgressFill", PrimitiveType.Cylinder, residue.transform, progressMaterial, false);
                progress.transform.localPosition = new Vector3(0f, 0.72f, 0f);
                progress.transform.localScale = new Vector3(0.85f, 0.05f, 0.85f);
                cleanable.progressFill = progress.transform;
            }
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Material material, bool keepCollider)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent);

            if (material != null)
            {
                primitive.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = primitive.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return primitive;
        }

    }
}
