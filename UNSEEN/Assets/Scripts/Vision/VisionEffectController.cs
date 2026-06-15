/*
 * Owner: Gangmin Lee
 * Function of this code: Applies normal, cataract, tunnel-vision, and darkness effects to the active XR camera.
 * Additional notes: Stage 4 darkness uses a semi-transparent black overlay; F1-F4 enable editor testing.
 */
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Unseen.Vision
{
    public enum VisionMode
    {
        Normal,
        Cataract,
        TunnelVision,
        Darkness
    }

    [DisallowMultipleComponent]
    public sealed class VisionEffectController : MonoBehaviour
    {
        private static readonly int ModeId = Shader.PropertyToID("_Mode");
        private static readonly int TunnelRadiusId = Shader.PropertyToID("_TunnelRadius");
        private static readonly int TunnelFeatherId = Shader.PropertyToID("_TunnelFeather");
        private static readonly int CataractHazeId = Shader.PropertyToID("_CataractHaze");
        private static readonly int DarknessOpacityId = Shader.PropertyToID("_DarknessOpacity");

        [SerializeField] private VisionMode initialMode = VisionMode.Normal;
        [SerializeField, Range(0.05f, 0.8f)] private float tunnelRadius = 0.13f;
        [SerializeField, Range(0.01f, 0.4f)] private float tunnelFeather = 0.07f;
        [SerializeField, Range(0f, 0.9f)] private float cataractHaze = 0.52f;
        [SerializeField, Range(0f, 1f)] private float darknessOpacity = 0.94f;
        [SerializeField] private bool enableNumberKeyTesting = true;

        private Camera targetCamera;
        private Transform overlayTransform;
        private Material overlayMaterial;
        private Volume cataractVolume;
        private DepthOfField depthOfField;
        private VisionMode currentMode;

        public VisionMode CurrentMode => currentMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllerExists()
        {
            if (FindFirstObjectByType<VisionEffectController>() != null)
                return;

            var host = new GameObject("Vision Effect Controller");
            host.AddComponent<VisionEffectController>();
        }

        private void Awake()
        {
            SetupCataractVolume();
            currentMode = initialMode;
        }

        private void Start()
        {
            TryAttachToCamera();
            SetVisionMode(initialMode);
        }

        private void Update()
        {
            if (targetCamera == null)
                TryAttachToCamera();

            UpdateOverlayScale();
            HandleTestInput();
        }

        public void SetVisionMode(VisionMode mode)
        {
            currentMode = mode;

            if (overlayMaterial != null)
            {
                overlayMaterial.SetFloat(ModeId, (float)mode);
                overlayMaterial.SetFloat(TunnelRadiusId, tunnelRadius);
                overlayMaterial.SetFloat(TunnelFeatherId, tunnelFeather);
                overlayMaterial.SetFloat(CataractHazeId, cataractHaze);
                overlayMaterial.SetFloat(DarknessOpacityId, darknessOpacity);
            }

            if (cataractVolume != null)
                cataractVolume.enabled = mode == VisionMode.Cataract;

        }

        public void SetNormal() => SetVisionMode(VisionMode.Normal);
        public void SetCataract() => SetVisionMode(VisionMode.Cataract);
        public void SetTunnelVision() => SetVisionMode(VisionMode.TunnelVision);
        public void SetDarkness() => SetVisionMode(VisionMode.Darkness);

        private void TryAttachToCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                camera = FindFirstObjectByType<Camera>();
            if (camera == null || camera == targetCamera)
                return;

            targetCamera = camera;
            targetCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            CreateOverlay();
            SetVisionMode(currentMode);
        }

        private void CreateOverlay()
        {
            if (overlayTransform != null)
                Destroy(overlayTransform.gameObject);

            var shader = Resources.Load<Shader>("VisionOverlay");
            if (shader == null)
                shader = Shader.Find("UNSEEN/VisionOverlay");
            if (shader == null)
            {
                Debug.LogError("UNSEEN/VisionOverlay shader was not found.");
                return;
            }

            var overlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            overlay.name = "Vision Overlay";
            overlay.layer = targetCamera.gameObject.layer;
            overlayTransform = overlay.transform;
            overlayTransform.SetParent(targetCamera.transform, false);
            overlayTransform.localRotation = Quaternion.identity;

            Destroy(overlay.GetComponent<Collider>());

            overlayMaterial = new Material(shader)
            {
                name = "Vision Overlay (Runtime)",
                renderQueue = 5000
            };
            overlay.GetComponent<MeshRenderer>().sharedMaterial = overlayMaterial;
            UpdateOverlayScale();
        }

        private void UpdateOverlayScale()
        {
            if (targetCamera == null || overlayTransform == null)
                return;

            var distance = Mathf.Max(targetCamera.nearClipPlane + 0.03f, 0.08f);
            var height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var width = height * targetCamera.aspect;

            overlayTransform.localPosition = new Vector3(0f, 0f, distance);
            overlayTransform.localScale = new Vector3(width * 1.35f, height * 1.35f, 1f);
        }

        private void SetupCataractVolume()
        {
            var volumeObject = new GameObject("Cataract Blur Volume");
            volumeObject.transform.SetParent(transform, false);

            cataractVolume = volumeObject.AddComponent<Volume>();
            cataractVolume.isGlobal = true;
            cataractVolume.priority = 1000f;
            cataractVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            depthOfField = cataractVolume.profile.Add<DepthOfField>();
            depthOfField.active = true;
            depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            depthOfField.gaussianStart.Override(0f);
            depthOfField.gaussianEnd.Override(3f);
            depthOfField.gaussianMaxRadius.Override(2f);
            depthOfField.highQualitySampling.Override(false);

            cataractVolume.enabled = false;
        }

        private void HandleTestInput()
        {
            if (!enableNumberKeyTesting)
                return;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            // The XR Device Simulator reserves the number row for device controls.
            if (keyboard.f1Key.wasPressedThisFrame) SetNormal();
            if (keyboard.f2Key.wasPressedThisFrame) SetCataract();
            if (keyboard.f3Key.wasPressedThisFrame) SetTunnelVision();
            if (keyboard.f4Key.wasPressedThisFrame) SetDarkness();
#endif
        }

        private void OnDestroy()
        {
            if (overlayMaterial != null)
                Destroy(overlayMaterial);
            if (cataractVolume != null && cataractVolume.profile != null)
                Destroy(cataractVolume.profile);
        }
    }
}
