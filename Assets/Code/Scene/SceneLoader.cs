using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("=== 场景配置 ===")]
    [SerializeField] private string mainSceneName = "DayScene";
    [SerializeField] private string crimeSceneName = "CrimeScene"; 

    [Header("=== 过渡设置 ===")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private GameObject transitionOverlay;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneName, true));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, bool useTransition)
    {
        isLoading = true;

        Debug.Log($"[SceneLoader] 开始加载场景：{sceneName}");

        // 淡出
        if (useTransition && transitionOverlay != null)
        {
            yield return StartCoroutine(FadeOut());
        }

        yield return new WaitForEndOfFrame();

        // 加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"[SceneLoader] 场景加载完成：{sceneName}");

        // 淡入
        if (useTransition && transitionOverlay != null)
        {
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(FadeIn());
        }

        isLoading = false;
    }

    private IEnumerator FadeOut()
    {
        if (transitionOverlay == null) yield break;

        CanvasGroup canvasGroup = transitionOverlay.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeIn()
    {
        if (transitionOverlay == null) yield break;

        CanvasGroup canvasGroup = transitionOverlay.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}