using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UINotification : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine currentCoroutine;

    public void Show(string message, float duration = 2.5f)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowCoroutine(message, duration));
    }

    private IEnumerator ShowCoroutine(string message, float duration)
    {
        notificationText.text = message;

        // µ­Èë
        yield return Fade(0, 1, fadeInDuration);

        // Í£Áô
        yield return new WaitForSeconds(duration);

        // µ­³ö
        yield return Fade(1, 0, fadeOutDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}