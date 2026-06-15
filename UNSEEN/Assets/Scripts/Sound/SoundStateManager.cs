/*
 * Owner: Eunyeong Choi, Haejun Lee
 * Function of this code: Selects and repeats environmental guidance audio based on the player's current context.
 * Additional notes: State priority is elevator, vent, climbing, braille block, then outside braille.
 */
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundStateManager : UnityEngine.MonoBehaviour
{
    public enum SoundState
    {
        OutsideBraille,   // Sound B
        OnBraille,        // Sound A
        Climbing,         // Sound C
        InsideVent,       // Sound D
        InsideElevator    // Sound E
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

    [Header("=== Mixer ===")]
    [SerializeField] private AudioMixer mainMixer;         // Inspector 연결
    [SerializeField] private string mixerParam = "SFXVolume";

    // -----------------------------------------------------------------------------
    //  Singleton
    // -----------------------------------------------------------------------------

    public static SoundStateManager Instance { get; private set; }
    public SoundState CurrentState => currentState;

    // -----------------------------------------------------------------------------
    //  Internal state
    // -----------------------------------------------------------------------------

    private SoundState currentState = SoundState.OutsideBraille;
    private UnityEngine.AudioSource audioSource;
    private float timer = 0f;

    // Priority: InsideElevator > InsideVent > Climbing > OnBraille > OutsideBraille
    private bool isOnBraille      = false;
    private bool isClimbing       = false;
    private bool isInsideVent     = false;
    private bool isInsideElevator = false;

    // -----------------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSource = GetComponent<UnityEngine.AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<UnityEngine.AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // AudioMixer 그룹 연결
        if (mainMixer != null)
        {
            var groups = mainMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0)
                audioSource.outputAudioMixerGroup = groups[0];
        }
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

    // -----------------------------------------------------------------------------
    //  Public state updates
    // -----------------------------------------------------------------------------

    /// <summary>Updates whether the player is standing on a braille block.</summary>
    public void SetOnBraille(bool value)
    {
        UnityEngine.Debug.Log("보도블럭 감지 상태: " + value);
        isOnBraille = value;
    }

    /// <summary>Updates whether the player is climbing.</summary>
    public void SetClimbing(bool value) => isClimbing = value;

    /// <summary>Updates whether the player is inside a vent.</summary>
    public void SetInsideVent(bool value) => isInsideVent = value;

    /// <summary>Updates whether the player is inside an elevator.</summary>
    public void SetInsideElevator(bool value) => isInsideElevator = value;

    // -----------------------------------------------------------------------------
    //  Internal helpers
    // -----------------------------------------------------------------------------

    /// <summary>Chooses the current state according to the configured priority.</summary>
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

        // Stop immediately when the current state has no assigned clip.
        if (clip == null)
        {
            audioSource.Stop();
            return;
        }

        // Assign the clip directly so a state change immediately replaces the previous sound.
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
