/*
 * Owner: Haejun Lee
 * Function of this code: Connects title-screen buttons to starting the main scene or exiting the application.
 * Additional notes: Both UI Button references must be assigned in the Inspector.
 */
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
