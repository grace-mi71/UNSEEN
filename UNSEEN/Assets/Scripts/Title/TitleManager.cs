// Owner: Lee Haejun
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject settingUI;

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnSettingButton()
    {
        titleUI.SetActive(false);
        settingUI.SetActive(true);
    }

    public void OnCloseSettingButton()
    {
        settingUI.SetActive(false);
        titleUI.SetActive(true);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}