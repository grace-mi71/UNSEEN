using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ElevatorBoardingZone : MonoBehaviour
{
    [Tooltip("이 탑승 존이 속한 스테이지")]
    [SerializeField] private GameFlowManager.GameState belongsToStage = GameFlowManager.GameState.Stage1;

    [Tooltip("플레이어 태그 (기본: Player)")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        GameFlowManager.Instance?.OnPlayerBoardedElevator(belongsToStage);

        // 한 번만 동작
        enabled = false;
    }
}