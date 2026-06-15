/*
 * Owner: Eunyeong Choi
 * Function of this code: Opens and closes elevator doors with tweened movement, audio, and Unity events.
 * Additional notes: Door transforms must be assigned in the Inspector; closed positions are captured during Awake.
 */
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

// Controls elevator door movement and playback of door sounds.
[RequireComponent(typeof(AudioSource))]
public class ElevatorController : MonoBehaviour
{
    [Header("Door References")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Movement Settings")]
    public float openDistance = 1.0f;
    public float openDuration = 1.5f;
    public float closeDuration = 1.5f;
    public Ease easeType = Ease.InOutQuad;

    [Header("Audio Settings")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    [Header("Elevator Events")]
    public UnityEvent onOpenStart;
    public UnityEvent onOpenComplete;
    public UnityEvent onCloseStart;
    public UnityEvent onCloseComplete;

    private Vector3 leftDoorClosedLocalPos;
    private Vector3 rightDoorClosedLocalPos;
    private bool isOpen = false;

    void Awake()
    {
        if (leftDoor != null) leftDoorClosedLocalPos = leftDoor.localPosition;
        if (rightDoor != null) rightDoorClosedLocalPos = rightDoor.localPosition;

        // Reuse the attached audio source or create one when missing.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    [ContextMenu("Test Open Door")]
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        // Play the opening sound once when the door begins to open.
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        onOpenStart?.Invoke();

        leftDoor.DOLocalMoveX(leftDoorClosedLocalPos.x - openDistance, openDuration).SetEase(easeType);
        rightDoor.DOLocalMoveX(rightDoorClosedLocalPos.x + openDistance, openDuration).SetEase(easeType)
            .OnComplete(() =>
            {
                onOpenComplete?.Invoke();
            });
    }

    [ContextMenu("Test Close Door")]
    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        // Play the closing sound once when the door begins to close.
        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        onCloseStart?.Invoke();

        leftDoor.DOLocalMoveX(leftDoorClosedLocalPos.x, closeDuration).SetEase(easeType);
        rightDoor.DOLocalMoveX(rightDoorClosedLocalPos.x, closeDuration).SetEase(easeType)
            .OnComplete(() =>
            {
                onCloseComplete?.Invoke();
            });
    }
}
