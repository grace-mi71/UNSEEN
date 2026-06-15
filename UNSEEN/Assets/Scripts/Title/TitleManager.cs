/*
 * Owner: Haejun Lee
 * Function of this code: Manages title screen UI interactions including
 * scene transitions, settings panel visibility, and application exit.
 */
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("=== UI ===")]
    [Tooltip("Reference to the settings UI panel GameObject.")]
    public GameObject settingUI;

    [Header("=== Audio ===")]
    [Tooltip("AudioSettingsManager 오브젝트 참조")]
    public AudioSettingsManager audioSettings;

    [SerializeField] private float volumeStep = 0.1f;

    public GameObject mainButtonGroup; // PlayBtn, SettingBtn, ExitBtn

    // ── Change Scene ──────────────────────────────

    public void OnPlayButton()
    {
        GameFlowManager.Instance?.StartGameFromTitle();
    }

    public void OnSettingButton()
    {
        mainButtonGroup.SetActive(false);
        settingUI.SetActive(true);
    }

    public void OnCloseSettingButton()
    {
        settingUI.SetActive(false);
        mainButtonGroup.SetActive(true);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    // ── BGM Volume ────────────────────────────────

    public void OnBgmVolumeUp()
    {
        if (audioSettings == null || audioSettings.bgmSlider == null) return;
        audioSettings.bgmSlider.value =
            Mathf.Clamp(audioSettings.bgmSlider.value + volumeStep, 0.0001f, 1f);
    }

    public void OnBgmVolumeDown()
    {
        if (audioSettings == null || audioSettings.bgmSlider == null) return;
        audioSettings.bgmSlider.value =
            Mathf.Clamp(audioSettings.bgmSlider.value - volumeStep, 0.0001f, 1f);
    }

    // ── SFX Volume ────────────────────────────────

    public void OnSfxVolumeUp()
    {
        if (audioSettings == null || audioSettings.sfxSlider == null) return;
        audioSettings.sfxSlider.value =
            Mathf.Clamp(audioSettings.sfxSlider.value + volumeStep, 0.0001f, 1f);
    }

    public void OnSfxVolumeDown()
    {
        if (audioSettings == null || audioSettings.sfxSlider == null) return;
        audioSettings.sfxSlider.value =
            Mathf.Clamp(audioSettings.sfxSlider.value - volumeStep, 0.0001f, 1f);
    }
}