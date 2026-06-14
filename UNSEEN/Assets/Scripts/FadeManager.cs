using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 페이드 아웃/인 후 씬 전환을 담당합니다.
/// 씬에 하나만 배치하세요.
///
/// 셋업:
///   1) Canvas (Screen Space - Overlay, Sort Order 999) 생성
///   2) Canvas 아래 Image 추가 → 색상 검정, Alpha 0, 전체 화면 채우기
///      (Anchor: stretch-stretch, Left/Right/Top/Bottom 모두 0)
///   3) 해당 Image를 fadeImage 필드에 연결
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }

    private void Start()
    {
        // 씬 시작 시 항상 페이드 인
        StartCoroutine(FadeIn());
    }

    // ─────────────────────────────────────────
    //  외부 호출
    // ─────────────────────────────────────────

    /// <summary>
    /// 점프스케어 후 호출 — delay초 대기 → 페이드 아웃 → 현재 씬 재시작 → 페이드 인
    /// </summary>
    public void FadeAndRestartStage(float delay = 0f)
    {
        StartCoroutine(FadeOutThenLoad(delay, reloadScene: true, sceneName: null));
    }

    /// <summary>
    /// Stage4 클리어 후 호출 — delay초 대기 → 페이드 아웃 → 메인 메뉴로 이동
    /// </summary>
    public void FadeAndGoToMainMenu(float delay = 0f, string sceneName = "Main")
    {
        StartCoroutine(FadeOutThenLoad(delay, reloadScene: false, sceneName: sceneName));
    }

    // ─────────────────────────────────────────
    //  코루틴
    // ─────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        // 검정에서 투명으로
        float elapsed = 0f;
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }

    private IEnumerator FadeOutThenLoad(float delay, bool reloadScene, string sceneName)
    {
        yield return new WaitForSeconds(delay);

        // 투명 → 검정 (페이드 아웃)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 완전히 검게 된 후 씬 전환
        // 새 씬의 FadeManager.Start()에서 자동으로 페이드 인
        if (reloadScene)
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}