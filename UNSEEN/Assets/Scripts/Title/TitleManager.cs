/*
 * Owner: Haejun Lee
 * Function of this code: Connects title-screen buttons to starting the main scene or exiting the application.
 * Additional notes: Both UI Button references must be assigned in the Inspector.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUIManager : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Button exitBtn;

    void Start()
    {
        gameStartBtn.onClick.AddListener(OnGameStart);
        exitBtn.onClick.AddListener(OnExit);
    }

    private void OnGameStart()
    {
        SceneManager.LoadScene("Main");
    }

    private void OnExit()
    {
        Application.Quit();
    }
}
