using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ElevatorZone : MonoBehaviour
{
    [Tooltip("이 탑승 존이 속한 스테이지")]
    [SerializeField] private GameFlowManager.GameState belongsToStage = GameFlowManager.GameState.Stage1;

    [Tooltip("플레이어 태그 (기본: Player)")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggeredTransition = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        SoundStateManager.Instance?.SetInsideElevator(true);

        // 스테이지 전환은 한 번만
        if (!hasTriggeredTransition)
        {
            hasTriggeredTransition = true;
            GameFlowManager.Instance?.OnPlayerBoardedElevator(belongsToStage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        SoundStateManager.Instance?.SetInsideElevator(false);
    }
}