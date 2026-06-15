/*
 * Owner: Haejun Lee
 * Function of this code: Manages title screen UI interactions including
 * scene transitions, settings panel visibility, and application exit.
 */
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("=== UI ===")]
    [Tooltip("Reference to the settings UI panel GameObject.")]
    public GameObject settingUI;

    // ── Change Scene ──────────────────────────────

    /// <summary>Loads the Main scene when the Play button is clicked.</summary>
    public void OnPlayButton()
    {
        SceneManager.LoadScene("Main");
    }

    /// <summary>Shows the settings UI panel when the Setting button is clicked.</summary>
    public void OnSettingButton()
    {
        settingUI.SetActive(true);
    }

    /// <summary>Hides the settings UI panel when the Close button is clicked.</summary>
    public void OnCloseSettingButton()
    {
        settingUI.SetActive(false);
    }

    /// <summary>Quits the application when the Exit button is clicked.</summary>
    public void OnExitButton()
    {
        Application.Quit();
    }
}