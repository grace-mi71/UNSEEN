using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Unseen.Interaction
{
    [RequireComponent(typeof(Collider), typeof(UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable))]
    public sealed class LadderVentTransition : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)] private float topThreshold = 0.35f;
        [SerializeField, Range(0.5f, 2f)] private float ventDepth = 0.9f;
        [SerializeField, Range(0.5f, 2f)] private float exitDistance = 1f;

        private XROrigin xrOrigin;
        private Camera targetCamera;
        private Collider ladderCollider;
        private UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable climbInteractable;
        private InputAction exitAction;
        private GameObject exitPrompt;
        private Vector3 exitCameraPosition;
        private bool insideVent;
        private bool wasClimbing;

        private void Awake()
        {
            xrOrigin = FindFirstObjectByType<XROrigin>();
            targetCamera = Camera.main;
            ladderCollider = GetComponent<Collider>();
            climbInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable>();

            exitAction = new InputAction("Exit Vent", InputActionType.Button);
            exitAction.AddBinding("<XRController>{RightHand}/primaryButton");
            exitAction.AddBinding("<XRController>{LeftHand}/primaryButton");
            exitAction.AddBinding("<Keyboard>/a");
            exitAction.Enable();

            CreateExitPrompt();
        }

        private void Update()
        {
            if (xrOrigin == null || targetCamera == null)
                return;

            if (!insideVent)
            {
                var isClimbingNow = climbInteractable.isSelected;
                if (isClimbingNow != wasClimbing)
                {
                    wasClimbing = isClimbingNow;
                    SoundStateManager.Instance?.SetClimbing(isClimbingNow);
                }

                TryEnterVent();
                return;
            }

            UpdatePromptPosition();
            if (exitAction.WasPressedThisFrame())
                ExitVent();
        }

        private void TryEnterVent()
        {
            if (!climbInteractable.isSelected)
                return;

            var bounds = ladderCollider.bounds;
            var cameraPosition = targetCamera.transform.position;
            var horizontalOffset = Vector3.ProjectOnPlane(cameraPosition - bounds.center, Vector3.up);
            if (horizontalOffset.sqrMagnitude > 2.25f || cameraPosition.y < bounds.max.y - topThreshold)
                return;

            var approachDirection = horizontalOffset.sqrMagnitude > 0.01f
                ? horizontalOffset.normalized
                : -transform.forward;

            exitCameraPosition = new Vector3(
                bounds.center.x,
                bounds.min.y + xrOrigin.CameraYOffset,
                bounds.center.z) + approachDirection * exitDistance;

            var ventCameraPosition = new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z)
                                     - approachDirection * ventDepth;
            MoveCameraTo(ventCameraPosition);

            insideVent = true;
            climbInteractable.enabled = false;
            exitPrompt.SetActive(true);

            SoundStateManager.Instance?.SetClimbing(false);
            SoundStateManager.Instance?.SetInsideVent(true);
            wasClimbing = false;
        }

        private void ExitVent()
        {
            MoveCameraTo(exitCameraPosition);
            insideVent = false;
            climbInteractable.enabled = true;
            exitPrompt.SetActive(false);

            SoundStateManager.Instance?.SetInsideVent(false);
        }

        private void MoveCameraTo(Vector3 destination)
        {
            xrOrigin.transform.position += destination - targetCamera.transform.position;
        }

        private void CreateExitPrompt()
        {
            exitPrompt = new GameObject("Vent Exit Prompt");
            var canvas = exitPrompt.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rect = exitPrompt.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 100f);
            rect.localScale = Vector3.one * 0.0015f;

            var background = exitPrompt.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.75f);

            var textObject = new GameObject("Text");
            textObject.transform.SetParent(exitPrompt.transform, false);
            var text = textObject.AddComponent<Text>();
            text.text = "A 버튼을 눌러 내려가기";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 42;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            textObject.GetComponent<RectTransform>().anchorMax = Vector2.one;
            textObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            exitPrompt.SetActive(false);
        }

        private void UpdatePromptPosition()
        {
            var cameraTransform = targetCamera.transform;
            exitPrompt.transform.position = cameraTransform.position + cameraTransform.forward * 0.8f - cameraTransform.up * 0.25f;
            exitPrompt.transform.rotation = Quaternion.LookRotation(exitPrompt.transform.position - cameraTransform.position);
        }

        private void OnDestroy()
        {
            SoundStateManager.Instance?.SetClimbing(false);
            SoundStateManager.Instance?.SetInsideVent(false);
            exitAction?.Dispose();
        }
    }
}