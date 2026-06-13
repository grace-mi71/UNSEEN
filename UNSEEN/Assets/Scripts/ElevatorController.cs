using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

// 엘리베이터 문을 제어하고 소리를 재생하는 클래스
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

        // 오디오 소스 컴포넌트 가져오기 (없으면 자동 추가)
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

        // 문이 열릴 때 소리 한 번 재생
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

        // 문이 닫힐 때 소리 한 번 재생
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