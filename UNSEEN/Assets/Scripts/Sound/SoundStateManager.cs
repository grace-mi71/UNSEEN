using UnityEngine;

public class SoundStateManager : UnityEngine.MonoBehaviour
{
    public enum SoundState
    {
        OutsideBraille,   // 보도블럭 밖 → 사운드 B
        OnBraille,        // 보도블럭 위 → 사운드 A
        Climbing,         // 사다리 오르는 중 → 사운드 C
        InsideVent,       // 벤트 안 → 사운드 D
        InsideElevator    // 엘리베이터 안 → 사운드 E
    }

    [UnityEngine.Header("=== Audio Clips ===")]
    [UnityEngine.Tooltip("A: 보도블럭 위에 있는 동안")]
    [UnityEngine.SerializeField] private UnityEngine.AudioClip soundA;
    [UnityEngine.Tooltip("B: 보도블럭 밖에 있을 때")]
    [UnityEngine.SerializeField] private UnityEngine.AudioClip soundB;
    [UnityEngine.Tooltip("C: 사다리 오르는 중")]
    [UnityEngine.SerializeField] private UnityEngine.AudioClip soundC;
    [UnityEngine.Tooltip("D: 벤트 안에 있을 때")]
    [UnityEngine.SerializeField] private UnityEngine.AudioClip soundD;
    [UnityEngine.Tooltip("E: 엘리베이터 안에 있을 때")]
    [UnityEngine.SerializeField] private UnityEngine.AudioClip soundE;

    [UnityEngine.Header("=== Interval Settings ===")]
    [UnityEngine.Tooltip("사운드 A 반복 간격 (초)")]
    [UnityEngine.SerializeField, UnityEngine.Range(0.1f, 5f)] private float intervalA = 1f;
    [UnityEngine.Tooltip("사운드 B 반복 간격 (초)")]
    [UnityEngine.SerializeField, UnityEngine.Range(0.1f, 5f)] private float intervalB = 2f;
    [UnityEngine.Tooltip("사운드 C 반복 간격 (초)")]
    [UnityEngine.SerializeField, UnityEngine.Range(0.1f, 5f)] private float intervalC = 0.5f;
    [UnityEngine.Tooltip("사운드 D 반복 간격 (초)")]
    [UnityEngine.SerializeField, UnityEngine.Range(0.1f, 5f)] private float intervalD = 1.5f;
    [UnityEngine.Tooltip("사운드 E 반복 간격 (초)")]
    [UnityEngine.SerializeField, UnityEngine.Range(0.1f, 5f)] private float intervalE = 2f;

    [UnityEngine.Header("=== Volume ===")]
    [UnityEngine.SerializeField, UnityEngine.Range(0f, 1f)] private float volume = 1f;

    // ─────────────────────────────────────────
    //  싱글톤
    // ─────────────────────────────────────────

    public static SoundStateManager Instance { get; private set; }
    public SoundState CurrentState => currentState;

    // ─────────────────────────────────────────
    //  내부 상태
    // ─────────────────────────────────────────

    private SoundState currentState = SoundState.OutsideBraille;
    private UnityEngine.AudioSource audioSource;
    private float timer = 0f;

    // 우선순위: InsideElevator > InsideVent > Climbing > OnBraille > OutsideBraille
    private bool isOnBraille      = false;
    private bool isClimbing       = false;
    private bool isInsideVent     = false;
    private bool isInsideElevator = false;

    // ─────────────────────────────────────────
    //  Unity 생명주기
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSource = GetComponent<UnityEngine.AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<UnityEngine.AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        var newState = EvaluateState();
        if (newState != currentState)
        {
            currentState = newState;
            timer = 0f;
            PlayCurrentSound();
        }

        timer += UnityEngine.Time.deltaTime;
        if (timer >= GetCurrentInterval())
        {
            timer = 0f;
            PlayCurrentSound();
        }
    }

    // ─────────────────────────────────────────
    //  외부에서 상태 변경
    // ─────────────────────────────────────────

    /// <summary>BrailleBlock이 호출 — 보도블럭 위 여부</summary>
    public void SetOnBraille(bool value)
    {
        UnityEngine.Debug.Log("보도블럭 감지 상태: " + value);
        isOnBraille = value;
    }

    /// <summary>LadderVentTransition이 호출 — 사다리 오르는 중 여부</summary>
    public void SetClimbing(bool value) => isClimbing = value;

    /// <summary>LadderVentTransition이 호출 — 벤트 안 여부</summary>
    public void SetInsideVent(bool value) => isInsideVent = value;

    /// <summary>ElevatorBoardingZone이 호출 — 엘리베이터 안 여부</summary>
    public void SetInsideElevator(bool value) => isInsideElevator = value;

    // ─────────────────────────────────────────
    //  내부 헬퍼
    // ─────────────────────────────────────────

    /// <summary>우선순위에 따라 현재 상태 결정</summary>
    private SoundState EvaluateState()
    {
        if (isInsideElevator) return SoundState.InsideElevator;
        if (isInsideVent)     return SoundState.InsideVent;
        if (isClimbing)       return SoundState.Climbing;
        if (isOnBraille)      return SoundState.OnBraille;
        return SoundState.OutsideBraille;
    }

    private void PlayCurrentSound()
    {
        var clip = GetCurrentClip();

        // 재생할 클립이 없으면 현재 나고 있는 소리를 즉시 정지
        if (clip == null)
        {
            audioSource.Stop();
            return;
        }

        // PlayOneShot 대신, 오디오 소스에 클립을 직접 넣고 Play()를 호출
        // 이렇게 하면 상태가 바뀌거나 인터벌이 돌 때 이전 소리가 뚝 끊기고 새 소리가 납니다.
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private UnityEngine.AudioClip GetCurrentClip()
    {
        return currentState switch
        {
            SoundState.OnBraille      => soundA,
            SoundState.OutsideBraille => soundB,
            SoundState.Climbing       => soundC,
            SoundState.InsideVent     => soundD,
            SoundState.InsideElevator => soundE,
            _                         => null
        };
    }

    private float GetCurrentInterval()
    {
        return currentState switch
        {
            SoundState.OnBraille      => intervalA,
            SoundState.OutsideBraille => intervalB,
            SoundState.Climbing       => intervalC,
            SoundState.InsideVent     => intervalD,
            SoundState.InsideElevator => intervalE,
            _                         => 1f
        };
    }
}