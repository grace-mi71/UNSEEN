#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unseen.Interaction
{
    public sealed class XRSimulatorTestShortcuts : MonoBehaviour
    {
        [SerializeField] private float verticalSpeed = 2f;

        private XROrigin xrOrigin;
        private Camera targetCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<XRSimulatorTestShortcuts>() != null)
                return;

            new GameObject("XR Simulator Test Shortcuts").AddComponent<XRSimulatorTestShortcuts>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (xrOrigin == null)
                xrOrigin = FindFirstObjectByType<XROrigin>();
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (xrOrigin != null)
            {
                var direction = 0f;
                if (keyboard.pageUpKey.isPressed) direction += 1f;
                if (keyboard.pageDownKey.isPressed) direction -= 1f;
                xrOrigin.transform.position += Vector3.up * (direction * verticalSpeed * Time.deltaTime);
            }

            if (keyboard.f6Key.wasPressedThisFrame)
                FindNearestElevator()?.OpenDoor();
            if (keyboard.f7Key.wasPressedThisFrame)
                FindNearestElevator()?.CloseDoor();
        }

        private ElevatorController FindNearestElevator()
        {
            if (targetCamera == null)
                return null;

            ElevatorController nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var elevator in FindObjectsByType<ElevatorController>(FindObjectsSortMode.None))
            {
                var distance = (elevator.transform.position - targetCamera.transform.position).sqrMagnitude;
                if (distance >= nearestDistance)
                    continue;

                nearest = elevator;
                nearestDistance = distance;
            }

            return nearest;
        }
    }
}
#endif
