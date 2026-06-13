using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.XR.CoreUtils;
using Unseen.Vision;

[RequireComponent(typeof(AudioSource))]
public class GameFlowManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  열거형
    // ─────────────────────────────────────────

    public enum GameState { Stage1, Stage2, Stage3, Stage4 }

    // ─────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────

    [Header("=== Current State ===")]
    [SerializeField] private GameState currentState = GameState.Stage1;

    [Header("=== Spawn Points ===")]
    [SerializeField] private Transform startPoint1;
    [SerializeField] private Transform startPoint2;
    [SerializeField] private Transform startPoint3;
    [SerializeField] private Transform startPoint4;

    [Header("=== Elevators (적이 나오는 / 탑승 후 닫힐 엘리베이터) ===")]
    [SerializeField] private ElevatorController stage1Elevator;
    [SerializeField] private ElevatorController stage2Elevator;
    [SerializeField] private ElevatorController stage3Elevator;
    [SerializeField] private ElevatorController stage4Elevator;

    [Header("=== Monsters (씬에 비활성 상태로 배치) ===")]
    [SerializeField] private MonsterAI stage1Monster;
    [SerializeField] private MonsterAI stage2Monster;
    [SerializeField] private MonsterAI stage3Monster;
    [SerializeField] private MonsterAI stage4Monster;

    [Header("=== Transition Settings ===")]
    [Tooltip("플레이어 탑승 후 문이 닫히기까지의 대기 시간(초)")]
    [SerializeField] private float doorCloseDelay = 3f;
    [Tooltip("ElevatorController.closeDuration 과 맞춰주세요 (기본 1.5s)")]
    [SerializeField] private float closeDuration = 1.5f;

    [Header("=== Monster Spawn Door Settings ===")]
    [Tooltip("몬스터가 엘리베이터에서 나온 뒤 문이 닫히기까지 대기 시간(초)")]
    [SerializeField] private float monsterExitDoorCloseDelay = 3f;

    [Header("=== Stage4 Clear Event ===")]
    [Tooltip("Stage4 엘리베이터 탑승 시 발동할 이벤트 (게임 클리어 UI 등)")]
    public UnityEvent onStage4Clear;

    [Header("=== Audio Settings ===")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip bgmClip;

    // ─────────────────────────────────────────
    //  프로퍼티 / 싱글톤
    // ─────────────────────────────────────────

    public static GameFlowManager Instance { get; private set; }
    public GameState CurrentState => currentState;

    // 전환 중 중복 호출 방지
    private bool isTransitioning = false;

    private XROrigin xrOrigin;
    private Camera mainCamera;
    private AudioSource audioSource;
    private AudioSource bgmSource;

    // ─────────────────────────────────────────
    //  Unity 생명주기
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        xrOrigin = FindFirstObjectByType<XROrigin>();
        mainCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // 모든 몬스터 비활성으로 시작
        SetMonsterActive(stage1Monster, false);
        SetMonsterActive(stage2Monster, false);
        SetMonsterActive(stage3Monster, false);
        SetMonsterActive(stage4Monster, false);
    }

    private void Start()
    {
        InitStage(currentState);
    }

    // ─────────────────────────────────────────
    //  스테이지 초기화 (공통 진입점)
    // ─────────────────────────────────────────

    private void InitStage(GameState state)
    {
        currentState = state;

        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Stop();
            bgmSource.Play();
        }

        // 모든 몬스터 먼저 끄기
        SetMonsterActive(stage1Monster, false);
        SetMonsterActive(stage2Monster, false);
        SetMonsterActive(stage3Monster, false);
        SetMonsterActive(stage4Monster, false);

        switch (state)
        {
            case GameState.Stage1:
                TeleportPlayer(startPoint1);
                SetVisionMode(VisionMode.Normal);
                OpenElevatorAndSpawnMonster(stage1Elevator, stage1Monster);
                break;

            case GameState.Stage2:
                TeleportPlayer(startPoint2);
                SetVisionMode(VisionMode.TunnelVision);
                OpenElevatorAndSpawnMonster(stage2Elevator, stage2Monster);
                break;

            case GameState.Stage3:
                TeleportPlayer(startPoint3);
                SetVisionMode(VisionMode.Cataract);
                OpenElevatorAndSpawnMonster(stage3Elevator, stage3Monster);
                break;

            case GameState.Stage4:
                TeleportPlayer(startPoint4);
                SetVisionMode(VisionMode.Darkness);
                OpenElevatorAndSpawnMonster(stage4Elevator, stage4Monster);
                break;
        }
    }

    // ─────────────────────────────────────────
    //  ElevatorBoardingZone → 탑승 콜백
    // ─────────────────────────────────────────

    /// <summary>
    /// ElevatorBoardingZone 이 호출합니다.
    /// boardedStage: 플레이어가 탑승한 엘리베이터가 속한 스테이지
    /// </summary>
    public void OnPlayerBoardedElevator(GameState boardedStage)
    {
        // 현재 스테이지와 다른 탑승 이벤트는 무시
        if (boardedStage != currentState) return;
        if (isTransitioning) return;

        // Stage4 탑승 → 클리어 처리
        if (currentState == GameState.Stage4)
        {
            StartCoroutine(HandleStage4Clear());
            return;
        }

        StartCoroutine(TransitionToNextStage());
    }

    // ─────────────────────────────────────────
    //  전환 코루틴
    // ─────────────────────────────────────────

    private IEnumerator TransitionToNextStage()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(doorCloseDelay);

        // 현재 스테이지 엘리베이터 문 닫기
        GetCurrentElevator()?.CloseDoor();

        yield return new WaitForSeconds(closeDuration);

        // 다음 스테이지로
        var next = (GameState)((int)currentState + 1);
        isTransitioning = false;
        InitStage(next);
    }

    private IEnumerator HandleStage4Clear()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(doorCloseDelay);

        stage4Elevator?.CloseDoor();

        yield return new WaitForSeconds(closeDuration);

        isTransitioning = false;

        // Inspector에서 연결한 클리어 이벤트 발동 (UI 표시 등)
        onStage4Clear?.Invoke();
    }

    // ─────────────────────────────────────────
    //  헬퍼
    // ─────────────────────────────────────────

    private void OpenElevatorAndSpawnMonster(ElevatorController elevator, MonsterAI monster)
    {
        StartCoroutine(SpawnSequence(elevator, monster));
    }

    /// <summary>
    /// 1) 엘리베이터 오픈
    /// 2) 몬스터 활성화
    /// 3) monsterExitDoorCloseDelay 초 대기
    /// 4) 엘리베이터 문 닫기
    /// </summary>
    private IEnumerator SpawnSequence(ElevatorController elevator, MonsterAI monster)
    {
        elevator?.OpenDoor();
        SetMonsterActive(monster, true);

        yield return new WaitForSeconds(monsterExitDoorCloseDelay);

        elevator?.CloseDoor();
    }

    private ElevatorController GetCurrentElevator()
    {
        return currentState switch
        {
            GameState.Stage1 => stage1Elevator,
            GameState.Stage2 => stage2Elevator,
            GameState.Stage3 => stage3Elevator,
            GameState.Stage4 => stage4Elevator,
            _ => null
        };
    }

    private static void SetMonsterActive(MonsterAI monster, bool active)
    {
        if (monster != null)
            monster.gameObject.SetActive(active);
    }

    private void TeleportPlayer(Transform target)
    {
        if (target == null || xrOrigin == null) return;

        var cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null)
        {
            xrOrigin.transform.position = target.position;
        }
        else
        {
            // XR Origin 전체를 이동해 카메라 위치를 target에 맞춤
            var offset = target.position - cam.transform.position;
            xrOrigin.transform.position += offset;
            xrOrigin.transform.rotation = target.rotation;
        }

        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    private static void SetVisionMode(VisionMode mode)
    {
        var controller = FindFirstObjectByType<VisionEffectController>();
        controller?.SetVisionMode(mode);
    }
}