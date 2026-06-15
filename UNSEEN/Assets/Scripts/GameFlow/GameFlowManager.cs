/*
 * Owner: Eunyeong Choi, Haejun Lee
 * Function of this code: Coordinates stage initialization, elevator transitions, monsters, vision modes, and XR player teleportation.
 * Additional notes: Stage references and spawn points must be assigned in the Inspector.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.XR.CoreUtils;
using Unseen.Interaction;
using Unseen.Vision;

[RequireComponent(typeof(AudioSource))]
public class GameFlowManager : MonoBehaviour
{
    public enum GameState { Title, Stage1, Stage2, Stage3, Stage4 }

    [Header("=== Current State ===")]
    [SerializeField] private GameState currentState = GameState.Title;

    [Header("=== Spawn Points ===")]
    [SerializeField] private Transform startPoint1;
    [SerializeField] private Transform startPoint2;
    [SerializeField] private Transform startPoint3;
    [SerializeField] private Transform startPoint4;

    [Header("=== Elevators ===")]
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
    [SerializeField] private float doorCloseDelay = 3f;
    [SerializeField] private float closeDuration = 1.5f;
    [SerializeField] private float monsterExitDoorCloseDelay = 3f;

    [Header("=== Stage4 Clear ===")]
    [Tooltip("메인 메뉴 씬 이름")]
    [SerializeField] private string mainMenuSceneName = "Title";
    public UnityEvent onStage4Clear;

    [Header("=== Title UI ===")]
    [SerializeField] private GameObject titleCanvas;

    [Header("=== Audio Settings ===")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip bgmClip;

    [Header("=== Mixer Settings ===")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup bgmMixerGroup;

    public static GameFlowManager Instance { get; private set; }
    public GameState CurrentState => currentState;

    private bool isTransitioning = false;
    private XROrigin xrOrigin;
    private Camera mainCamera;
    private AudioSource audioSource;
    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        xrOrigin = XRInteractionAutoInstaller.FindActiveSceneOrigin();
        mainCamera = xrOrigin != null && xrOrigin.Camera != null ? xrOrigin.Camera : Camera.main;

        audioSource = GetComponent<AudioSource>();
        if (sfxMixerGroup != null) audioSource.outputAudioMixerGroup = sfxMixerGroup;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        if (bgmMixerGroup != null) bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        SetMonsterActive(stage1Monster, false);
        SetMonsterActive(stage2Monster, false);
        SetMonsterActive(stage3Monster, false);
        SetMonsterActive(stage4Monster, false);
    }

    private void Start()
    {
        InitStage(currentState);
    }

    // -----------------------------------------------------------------------------
    //  Stage initialization
    // -----------------------------------------------------------------------------

    private void InitStage(GameState state)
    {
        currentState = state;

        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Stop();
            bgmSource.Play();
        }

        SetMonsterActive(stage1Monster, false);
        SetMonsterActive(stage2Monster, false);
        SetMonsterActive(stage3Monster, false);
        SetMonsterActive(stage4Monster, false);

        switch (state)
        {
            case GameState.Title:
                if (titleCanvas != null) titleCanvas.SetActive(true);
                TeleportPlayer(startPoint1, false);
                SetVisionMode(VisionMode.Normal);
                return;
            case GameState.Stage1:
                TeleportPlayer(startPoint1);
                SetVisionMode(VisionMode.Normal);
                StartCoroutine(SpawnSequence(stage1Elevator, stage1Monster));
                break;
            case GameState.Stage2:
                TeleportPlayer(startPoint2);
                SetVisionMode(VisionMode.TunnelVision);
                StartCoroutine(SpawnSequence(stage2Elevator, stage2Monster));
                break;
            case GameState.Stage3:
                TeleportPlayer(startPoint3);
                SetVisionMode(VisionMode.Cataract);
                StartCoroutine(SpawnSequence(stage3Elevator, stage3Monster));
                break;
            case GameState.Stage4:
                TeleportPlayer(startPoint4);
                SetVisionMode(VisionMode.Darkness);
                StartCoroutine(SpawnSequence(stage4Elevator, stage4Monster));
                break;
        }
    }

    public void StartGameFromTitle()
    {
        if (titleCanvas != null) titleCanvas.SetActive(false);
        InitStage(GameState.Stage1);
    }

    // -----------------------------------------------------------------------------
    //  Elevator boarding callback
    // -----------------------------------------------------------------------------

    public void OnPlayerBoardedElevator(GameState boardedStage)
    {
        if (boardedStage != currentState) return;
        if (isTransitioning) return;

        if (currentState == GameState.Stage4)
        {
            StartCoroutine(HandleStage4Clear());
            return;
        }

        StartCoroutine(TransitionToNextStage());
    }

    // -----------------------------------------------------------------------------
    //  Transition coroutines
    // -----------------------------------------------------------------------------

    private IEnumerator TransitionToNextStage()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(doorCloseDelay);
        GetCurrentElevator()?.CloseDoor();
        yield return new WaitForSeconds(closeDuration);
        isTransitioning = false;
        InitStage((GameState)((int)currentState + 1));
    }

    private IEnumerator HandleStage4Clear()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(doorCloseDelay);
        stage4Elevator?.CloseDoor();
        yield return new WaitForSeconds(closeDuration);
        isTransitioning = false;

        // Invoke optional completion events configured in the Inspector.
        onStage4Clear?.Invoke();

        // Fade out and move to the configured main-menu scene.
        InitStage(GameState.Title);
    }

    private IEnumerator SpawnSequence(ElevatorController elevator, MonsterAI monster)
    {
        elevator?.OpenDoor();
        SetMonsterActive(monster, true);
        yield return new WaitForSeconds(monsterExitDoorCloseDelay);
        elevator?.CloseDoor();
    }

    // -----------------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------------

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

    public void RestartCurrentStage()
    {
        StopAllCoroutines();
        isTransitioning = false;

        GetCurrentMonster()?.ResetForStageRestart();
        InitStage(currentState);
    }

    public void RealignPlayerToCurrentStage()
    {
        var target = currentState switch
        {
            GameState.Stage1 => startPoint1,
            GameState.Stage2 => startPoint2,
            GameState.Stage3 => startPoint3,
            GameState.Stage4 => startPoint4,
            _ => null
        };

        TeleportPlayer(target, false);
    }

    private static void SetMonsterActive(MonsterAI monster, bool active)
    {
        if (monster != null)
            monster.gameObject.SetActive(active);
    }

    private MonsterAI GetCurrentMonster()
    {
        return currentState switch
        {
            GameState.Stage1 => stage1Monster,
            GameState.Stage2 => stage2Monster,
            GameState.Stage3 => stage3Monster,
            GameState.Stage4 => stage4Monster,
            _ => null
        };
    }

    private void TeleportPlayer(Transform target, bool playSound = true)
    {
        if (target == null || xrOrigin == null) return;

        var cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null)
            xrOrigin.transform.position = target.position;
        else
        {
            // Never apply spawn-point pitch or roll to the XR rig. Head rotation must
            // remain fully controlled by the headset's TrackedPoseDriver.
            xrOrigin.transform.rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

            // Rotating the origin can move the tracked camera around the rig pivot,
            // so align its world position only after the yaw has been applied.
            var offset = target.position - cam.transform.position;
            xrOrigin.transform.position += offset;
        }

        if (playSound && teleportSound != null && audioSource != null)
            audioSource.PlayOneShot(teleportSound);
    }

    private static void SetVisionMode(VisionMode mode)
    {
        var controller = FindFirstObjectByType<VisionEffectController>();
        controller?.SetVisionMode(mode);
    }
}
