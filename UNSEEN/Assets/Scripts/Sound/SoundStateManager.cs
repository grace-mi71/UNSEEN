using System.Collections;
using UnityEngine;

public class SoundStateManager : MonoBehaviour
{
    public enum SoundState
    {
        OutsideBraille,   // 보도블럭 밖
        OnBraille,        // 보도블럭 위
        Climbing,         // 사다리 오르는 중
        InsideVent        // 벤트 안
    }

    [Header("=== Audio Clips ===")]
    [Tooltip("A: 보도블럭 위에 있는 동안")]
    [SerializeField] private AudioClip soundA;
    [Tooltip("B: 보도블럭 밖에 있을 때")]
    [SerializeField] private AudioClip soundB;
    [Tooltip("C: 사다리 오르는 중")]
    [SerializeField] private AudioClip soundC;
    [Tooltip("D: 벤트 안에 있을 때")]
    [SerializeField] private AudioClip soundD;

    [Header("=== Interval Settings ===")]
    [Tooltip("사운드 A 반복 간격 (초)")]
    [SerializeField, Range(0.1f, 5f)] private float intervalA = 1f;
    [Tooltip("사운드 B 반복 간격 (초)")]
    [SerializeField, Range(0.1f, 5f)] private float intervalB = 2f;
    [Tooltip("사운드 C 반복 간격 (초)")]
    [SerializeField, Range(0.1f, 5f)] private float intervalC = 0.5f;
    [Tooltip("사운드 D 반복 간격 (초)")]
    [SerializeField, Range(0.1f, 5f)] private float intervalD = 1.5f;

    [Header("=== Volume ===")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    // ─────────────────────────────────────────
    //  싱글톤
    // ─────────────────────────────────────────

    public static SoundStateManager Instance { get; private set; }
    public SoundState CurrentState => currentState;

    // ─────────────────────────────────────────
    //  내부 상태
    // ─────────────────────────────────────────

    private SoundState currentState = SoundState.OutsideBraille;
    private AudioSource audioSource;
    private float timer = 0f;

    // 우선순위: InsideVent > Climbing > OnBraille > OutsideBraille
    private bool isOnBraille  = false;
    private bool isClimbing   = false;
    private bool isInsideVent = false;

    // ─────────────────────────────────────────
    //  Unity 생명주기
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드 (플레이어 자신의 소리)
    }

    private void Update()
    {
        var newState = EvaluateState();
        if (newState != currentState)
        {
            currentState = newState;
            timer = 0f;
            // 상태 바뀌면 즉시 한 번 재생
            PlayCurrentSound();
        }

        timer += Time.deltaTime;
        if (timer >= GetCurrentInterval())
        {
            timer = 0f;
            PlayCurrentSound();
        }
    }

    // ─────────────────────────────────────────
    //  외부에서 상태 변경 (BrailleBlock / LadderVentTransition 에서 호출)
    // ─────────────────────────────────────────

    /// <summary>BrailleBlock이 호출 — 보도블럭 위 여부</summary>
    public void SetOnBraille(bool value)
    {
        isOnBraille = value;
    }

    /// <summary>LadderVentTransition이 호출 — 사다리 오르는 중 여부</summary>
    public void SetClimbing(bool value)
    {
        isClimbing = value;
    }

    /// <summary>LadderVentTransition이 호출 — 벤트 안 여부</summary>
    public void SetInsideVent(bool value)
    {
        isInsideVent = value;
    }

    // ─────────────────────────────────────────
    //  내부 헬퍼
    // ─────────────────────────────────────────

    /// <summary>우선순위에 따라 현재 상태 결정</summary>
    private SoundState EvaluateState()
    {
        if (isInsideVent) return SoundState.InsideVent;
        if (isClimbing)   return SoundState.Climbing;
        if (isOnBraille)  return SoundState.OnBraille;
        return SoundState.OutsideBraille;
    }

    private void PlayCurrentSound()
    {
        var clip = GetCurrentClip();
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    private AudioClip GetCurrentClip()
    {
        return currentState switch
        {
            SoundState.OnBraille      => soundA,
            SoundState.OutsideBraille => soundB,
            SoundState.Climbing       => soundC,
            SoundState.InsideVent     => soundD,
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
            _                         => 1f
        };
    }
}