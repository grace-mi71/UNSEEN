// Owner: Lee Haejun
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