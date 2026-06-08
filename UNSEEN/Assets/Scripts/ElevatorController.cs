using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

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
    }

    // 임시 테스트용 키보드 입력 감지
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            OpenDoor();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CloseDoor();
        }
    }

    // 인스펙터 우클릭 테스트용 속성 추가
    [ContextMenu("Test Open Door")]
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

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

        onCloseStart?.Invoke();

        leftDoor.DOLocalMoveX(leftDoorClosedLocalPos.x, closeDuration).SetEase(easeType);
        rightDoor.DOLocalMoveX(rightDoorClosedLocalPos.x, closeDuration).SetEase(easeType)
            .OnComplete(() =>
            {
                onCloseComplete?.Invoke();
            });
    }
}