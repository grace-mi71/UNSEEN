using UnityEngine;

public sealed class ElevatorZone : MonoBehaviour
{
    [Tooltip("이 탑승 존이 속한 스테이지")]
    [SerializeField] private GameFlowManager.GameState belongsToStage = GameFlowManager.GameState.Stage1;

    [Tooltip("플레이어 태그")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("감지 반경 (Inspector에서 Scene뷰 기즈모로 확인 가능)")]
    [SerializeField] private float detectionRadius = 3f;

    private bool hasTriggeredTransition = false;
    private bool playerIsInside = false;
    private Transform playerTransform;

    private void Start()
    {
        var cam = Camera.main;
        if (cam != null)
            playerTransform = cam.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isInside = distance <= detectionRadius;

        if (isInside && !playerIsInside)
        {
            playerIsInside = true;
            SoundStateManager.Instance?.SetInsideElevator(true);

            if (!hasTriggeredTransition)
            {
                hasTriggeredTransition = true;
                GameFlowManager.Instance?.OnPlayerBoardedElevator(belongsToStage);
            }
        }
        else if (!isInside && playerIsInside)
        {
            playerIsInside = false;
            SoundStateManager.Instance?.SetInsideElevator(false);
        }
    }

    // Scene뷰에서 감지 범위 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}