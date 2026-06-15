// Owner: Lee Haejun
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject settingUI;

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