using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Unseen.Interaction
{
    [RequireComponent(typeof(Collider), typeof(XRSimpleInteractable), typeof(AudioSource))]
    public sealed class ElevatorPokeButton : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;
        [SerializeField, Range(0.005f, 0.08f)] private float pressDistance = 0.035f;

        [SerializeField] private AudioClip pressSound;

        private Vector3 releasedLocalPosition;
        private bool pressed;
        private XRSimpleInteractable interactable;
        private AudioSource audioSource;

        public void Configure(ElevatorController targetElevator)
        {
            elevator = targetElevator;
        }

        private void Awake()
        {
            releasedLocalPosition = transform.localPosition;
            GetComponent<Collider>().isTrigger = false;
            interactable = GetComponent<XRSimpleInteractable>();
            audioSource = GetComponent<AudioSource>();
            interactable.selectEntered.AddListener(OnPressed);
            interactable.selectExited.AddListener(OnReleased);
        }

        private void OnPressed(SelectEnterEventArgs args)
        {
            if (pressed)
                return;

            pressed = true;

            // ��ư�� ������ �� �Ҹ� �� �� ���
            if (pressSound != null)
            {
                audioSource.PlayOneShot(pressSound);
            }

            transform.localPosition = releasedLocalPosition + Vector3.back * pressDistance;
            elevator?.OpenDoor();
            transform.localPosition = releasedLocalPosition + Vector3.back * pressDistance;
            elevator?.OpenDoor();
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            pressed = false;
            transform.localPosition = releasedLocalPosition;
        }

        private void OnDestroy()
        {
            if (interactable == null)
                return;

            interactable.selectEntered.RemoveListener(OnPressed);
            interactable.selectExited.RemoveListener(OnReleased);
        }
    }
}
