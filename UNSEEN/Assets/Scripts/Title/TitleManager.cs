using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("=== UI ===")]
    public GameObject settingUI;

    // ── 씬 전환 / 앱 ──────────────────────────────

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnSettingButton()
    {
        settingUI.SetActive(true);
    }

    public void OnCloseSettingButton()
    {
        settingUI.SetActive(false);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}