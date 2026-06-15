/*
 * Owner: Gangmin Lee
 * Function of this code: Configures XR hardware and installs ladder, poke, elevator-button, and debug interactions at runtime.
 * Additional notes: Installation runs after each scene load and relies on established object naming conventions.
 */
using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

namespace Unseen.Interaction
{
    public static class XRInteractionAutoInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            ConfigureHardwareRig();
            InstallLadders();
            InstallPokeInteractors();
            InstallElevatorButtons();

            var bootstrapObject = new GameObject("XR Scene Bootstrap");
            bootstrapObject.AddComponent<XRSceneBootstrap>();
        }

        internal static void ConfigureHardwareRig()
        {
            ConfigureHardwareRig(FindActiveSceneOrigin());
        }

        internal static void ConfigureHardwareRig(XROrigin xrOrigin)
        {
            if (xrOrigin == null)
                return;

            foreach (var device in InputSystem.devices)
            {
                if (device is TrackedDevice && !device.enabled)
                    InputSystem.EnableDevice(device);
            }

            var actionManager = xrOrigin.GetComponent<InputActionManager>();
            if (actionManager != null)
            {
                actionManager.enabled = true;
                foreach (var actionAsset in actionManager.actionAssets)
                    actionAsset?.Enable();
            }

            var cameraPoseDriver = xrOrigin.Camera?.GetComponent<TrackedPoseDriver>();
            if (cameraPoseDriver != null)
                cameraPoseDriver.enabled = true;

            foreach (var child in xrOrigin.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "Left Controller")
                    RestoreController(child, new Color(0.08f, 0.45f, 0.9f));
                else if (child.name == "Right Controller")
                    RestoreController(child, new Color(0.9f, 0.28f, 0.08f));
            }
        }

        private static void RestoreController(Transform controller, Color color)
        {
            controller.gameObject.SetActive(true);

            var poseDriver = controller.GetComponent<TrackedPoseDriver>();
            if (poseDriver != null)
                poseDriver.enabled = true;

            CreateControllerVisual(controller, color);
            var visual = controller.Find("Controller Visual");
            if (visual != null)
                visual.gameObject.SetActive(true);
        }

        private static void CreateControllerVisual(Transform controller, Color color)
        {
            if (controller.Find("Controller Visual") != null)
                return;

            var visualRoot = new GameObject("Controller Visual");
            visualRoot.transform.SetParent(controller, false);

            var handle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            handle.name = "Handle";
            handle.transform.SetParent(visualRoot.transform, false);
            handle.transform.localPosition = new Vector3(0f, -0.055f, 0.015f);
            handle.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.035f, 0.075f, 0.035f);

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Interaction Tip";
            tip.transform.SetParent(visualRoot.transform, false);
            tip.transform.localPosition = new Vector3(0f, 0f, 0.07f);
            tip.transform.localScale = Vector3.one * 0.055f;

            ConfigureVisualPart(handle, color);
            ConfigureVisualPart(tip, Color.Lerp(color, Color.white, 0.35f));
        }

        private static void ConfigureVisualPart(GameObject part, Color color)
        {
            UnityEngine.Object.Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().material.color = color;
        }

        private static void InstallLadders()
        {
            var xrOrigin = FindActiveSceneOrigin();
            if (xrOrigin == null)
                return;

            var climbProvider = xrOrigin.GetComponent<ClimbProvider>();
            if (climbProvider == null)
                climbProvider = xrOrigin.gameObject.AddComponent<ClimbProvider>();
            climbProvider.climbSettings = CreateFreeClimbSettings();

            foreach (var meshRenderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var ladder = meshRenderer.gameObject;
                if (!ladder.name.Contains("ladder", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ladder.GetComponent<Collider>() == null)
                    ladder.AddComponent<BoxCollider>();

                var rigidbody = ladder.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = ladder.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                var climbInteractable = ladder.GetComponent<ClimbInteractable>();
                if (climbInteractable == null)
                    climbInteractable = ladder.AddComponent<ClimbInteractable>();
                climbInteractable.climbProvider = climbProvider;
                climbInteractable.filterInteractionByDistance = false;
                climbInteractable.climbSettingsOverride = CreateFreeClimbSettings();

                if (ladder.GetComponent<LadderVentTransition>() == null)
                    ladder.AddComponent<LadderVentTransition>();

                var debugVisual = ladder.GetComponent<XRInteractionDebugVisual>();
                if (debugVisual == null)
                    debugVisual = ladder.AddComponent<XRInteractionDebugVisual>();
                debugVisual.Configure(new Color(0.4f, 0.4f, 0.4f), new Color(1f, 0.75f, 0.05f), new Color(0.1f, 1f, 0.25f));
            }
        }

        internal static XROrigin FindActiveSceneOrigin()
        {
            var activeScene = SceneManager.GetActiveScene();
            foreach (var origin in UnityEngine.Object.FindObjectsByType<XROrigin>(
                         FindObjectsInactive.Exclude,
                         FindObjectsSortMode.None))
            {
                if (origin.gameObject.scene == activeScene)
                    return origin;
            }

            return null;
        }

        private static void InstallElevatorButtons()
        {
            foreach (var elevator in UnityEngine.Object.FindObjectsByType<ElevatorController>(FindObjectsSortMode.None))
            {
                if (elevator.GetComponentInChildren<ElevatorPokeButton>() != null)
                    continue;

                SetupElevatorKey(elevator);
            }
        }

        private static void InstallPokeInteractors()
        {
            foreach (var directInteractor in UnityEngine.Object.FindObjectsByType<XRDirectInteractor>(FindObjectsSortMode.None))
            {
                ConfigureDirectGrabInput(directInteractor);

                if (directInteractor.transform.Find("Poke Interactor") != null)
                    continue;

                var pokeObject = new GameObject("Poke Interactor");
                pokeObject.transform.SetParent(directInteractor.transform, false);
                pokeObject.transform.localPosition = new Vector3(0f, 0f, 0.08f);

                var pokeInteractor = pokeObject.AddComponent<XRPokeInteractor>();
                pokeInteractor.pokeDepth = 0.12f;
                pokeInteractor.pokeHoverRadius = 0.025f;
                pokeInteractor.pokeSelectWidth = 0.025f;

                var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "Poke Tip Visual";
                tip.transform.SetParent(pokeObject.transform, false);
                tip.transform.localScale = Vector3.one * 0.035f;
                ConfigureVisualPart(tip, new Color(1f, 0.1f, 0.75f));
            }
        }

        private static void ConfigureDirectGrabInput(XRDirectInteractor directInteractor)
        {
            var side = directInteractor.handedness == InteractorHandedness.Left ? "Left" : "Right";
            var actionManager = directInteractor.GetComponentInParent<InputActionManager>();
            if (actionManager == null || actionManager.actionAssets.Count == 0)
                return;

            var actionAsset = actionManager.actionAssets[0];
            var selectAction = actionAsset.FindAction($"XRI {side} Interaction/Select");
            var selectValueAction = actionAsset.FindAction($"XRI {side} Interaction/Select Value");
            if (selectAction != null)
                directInteractor.selectInput.inputActionReferencePerformed = InputActionReference.Create(selectAction);
            if (selectValueAction != null)
                directInteractor.selectInput.inputActionReferenceValue = InputActionReference.Create(selectValueAction);

            var grabCollider = directInteractor.GetComponent<SphereCollider>();
            if (grabCollider != null)
                grabCollider.radius = 0.1f;
        }

        private static ClimbSettingsDatumProperty CreateFreeClimbSettings()
        {
            return new ClimbSettingsDatumProperty(new ClimbSettings
            {
                allowFreeXMovement = true,
                allowFreeYMovement = true,
                allowFreeZMovement = true,
            });
        }

        private static void SetupElevatorKey(ElevatorController elevator)
        {
            Renderer targetRenderer = null;
            foreach (var child in elevator.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.Contains("elevator key", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
                {
                    if (targetRenderer == null || renderer.bounds.size.sqrMagnitude > targetRenderer.bounds.size.sqrMagnitude)
                        targetRenderer = renderer;
                }
            }

            if (targetRenderer == null)
            {
                Debug.LogWarning($"Could not find an elevator key model below {elevator.name}.", elevator);
                return;
            }

            var key = targetRenderer.gameObject;
            var collider = key.GetComponent<Collider>();
            if (collider == null)
                collider = key.AddComponent<BoxCollider>();

            var interactable = key.GetComponent<XRSimpleInteractable>();
            if (interactable == null)
                interactable = key.AddComponent<XRSimpleInteractable>();

            var pokeFilter = key.GetComponent<XRPokeFilter>();
            if (pokeFilter == null)
                pokeFilter = key.AddComponent<XRPokeFilter>();
            pokeFilter.pokeInteractable = interactable;
            pokeFilter.pokeCollider = collider;

            var pokeButton = key.AddComponent<ElevatorPokeButton>();
            pokeButton.Configure(elevator);

            var debugVisual = key.AddComponent<XRInteractionDebugVisual>();
            debugVisual.Configure(new Color(0.65f, 0.12f, 0.04f), new Color(1f, 0.65f, 0.05f), new Color(0.1f, 1f, 0.25f));
        }

    }
}
