/*
 * Owner: Haejun Lee
 * Function of this code: Handles full-screen fade transitions for scene loading and in-place stage resets.
 * Additional notes: Requires a full-screen black UI Image assigned to fadeImage.
 */
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles scene transitions with fade-out and fade-in effects.
/// Place only one instance in each scene.
///
/// Setup:
///   1) Create a Screen Space Overlay Canvas with sort order 999.
///   2) Add a full-screen black Image with alpha 0 below the Canvas.
///   3) Assign that Image to fadeImage.
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
        // Always fade in when the scene starts.
        StartCoroutine(FadeIn());
    }

    // -----------------------------------------------------------------------------
    //  Public entry points
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Waits for the delay, fades out, reloads the current scene, and fades in.
    /// </summary>
    public void FadeAndRestartStage(float delay = 0f)
    {
        StartCoroutine(FadeOutThenLoad(delay, reloadScene: true, sceneName: null));
    }

    /// <summary>
    /// Waits for the delay, fades out, and loads the requested menu scene.
    /// </summary>
    public void FadeAndGoToMainMenu(float delay = 0f, string sceneName = "Main")
    {
        StartCoroutine(FadeOutThenLoad(delay, reloadScene: false, sceneName: sceneName));
    }

    public void FadeAndResetCurrentStage(float delay = 0f)
    {
        StartCoroutine(FadeOutThenAction(delay, () => GameFlowManager.Instance?.RestartCurrentStage()));
    }

    // -----------------------------------------------------------------------------
    //  Coroutines
    // -----------------------------------------------------------------------------

    private IEnumerator FadeIn()
    {
        // Transition from black to transparent.
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

        // Transition from transparent to black.
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Load after the screen is fully black. The next FadeManager fades in automatically.
        if (reloadScene)
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOutThenAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        action?.Invoke();
        yield return null;
        yield return FadeIn();
    }
}
