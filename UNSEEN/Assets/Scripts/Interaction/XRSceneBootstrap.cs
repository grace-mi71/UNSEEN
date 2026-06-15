/*
 * Owner: Gangmin Lee
 * Function of this code: Stabilizes real-device XR tracking, player height, and controller input after scene transitions.
 * Additional notes: Re-enables shared input actions after the previous scene's XR Origin has finished shutting down.
 */
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Unseen.Interaction
{
    public sealed class XRSceneBootstrap : MonoBehaviour
    {
        private const float RealDeviceCameraHeight = 3f;
        private readonly List<XRInputSubsystem> inputSubsystems = new();

        private IEnumerator Start()
        {
            var playerRealigned = false;

            ConfigureSimulatorForCurrentEnvironment();
            yield return null;
            ForceRebindHardwareRig();

            // The previous scene's InputActionManager can disable the shared action asset
            // after the new scene starts. Re-enable it across the transition window.
            for (var frame = 0; frame < 120; frame++)
            {
                ConfigureTrackingOrigin();
                XRInteractionAutoInstaller.ConfigureHardwareRig();

                var xrOrigin = FindFirstObjectByType<XROrigin>();
                var actionManager = xrOrigin != null ? xrOrigin.GetComponent<InputActionManager>() : null;
                if (actionManager != null)
                {
                    actionManager.enabled = true;
                    foreach (var actionAsset in actionManager.actionAssets)
                        actionAsset?.Enable();
                }

                if (!playerRealigned && IsHeadTracked())
                {
                    FindFirstObjectByType<GameFlowManager>()?.RealignPlayerToCurrentStage();
                    playerRealigned = true;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private static void ConfigureSimulatorForCurrentEnvironment()
        {
            var simulatorManagers = FindObjectsByType<SimulatedDeviceLifecycleManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var simulatorManager in simulatorManagers)
            {
#if UNITY_EDITOR
                // Keep the simulator scene object inactive by default so it cannot remove
                // real XR devices in a player build. Activate it only for Editor testing.
                simulatorManager.gameObject.SetActive(true);
#else
                simulatorManager.gameObject.SetActive(false);
#endif
            }
        }

        private static void ForceRebindHardwareRig()
        {
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
                return;

            var actionManager = xrOrigin.GetComponent<InputActionManager>();
            if (actionManager != null)
            {
                actionManager.enabled = true;
                foreach (var actionAsset in actionManager.actionAssets)
                {
                    if (actionAsset == null)
                        continue;

                    actionAsset.Disable();
                    actionAsset.Enable();
                }
            }

            // Re-enabling the pose drivers forces them to bind to the controller devices
            // created by the current scene's simulator or the connected XR runtime.
            foreach (var poseDriver in xrOrigin.GetComponentsInChildren<TrackedPoseDriver>(true))
            {
                poseDriver.enabled = false;
                poseDriver.enabled = true;
            }

            XRInteractionAutoInstaller.ConfigureHardwareRig();
        }

        private void ConfigureTrackingOrigin()
        {
            inputSubsystems.Clear();
            SubsystemManager.GetSubsystems(inputSubsystems);
            foreach (var subsystem in inputSubsystems)
            {
                if (subsystem.running && subsystem.GetTrackingOriginMode() != TrackingOriginModeFlags.Device)
                    subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            }

            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
                return;

            if (xrOrigin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Device)
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            if (!Mathf.Approximately(xrOrigin.CameraYOffset, RealDeviceCameraHeight))
                xrOrigin.CameraYOffset = RealDeviceCameraHeight;
        }

        private static bool IsHeadTracked()
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.isValid
                && head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out var tracked)
                && tracked)
            {
                return true;
            }

            foreach (var device in InputSystem.devices)
            {
                if (device is XRHMD hmd && hmd.isTracked.ReadValue() > 0f)
                    return true;
            }

            return false;
        }
    }
}
