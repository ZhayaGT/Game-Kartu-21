using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WinLoseUI : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupUI; // UI utama untuk pop-up
    public float popupDelay = 3f; // Delay sebelum pop-up muncul
    public float popupDuration = 1f; // Durasi animasi pop-up

    [Header("Idle Bubble Effect")]
    public GameObject bubbleEffectObject; // Objek untuk efek idle bubble
    public float bubbleDuration = 2f; // Durasi animasi bubble (resize height & width)

    [Header("Important Indicator")]
    public GameObject importantIndicatorObject; // Objek untuk penanda penting
    public float importantPopupInterval = 3f; // Interval waktu untuk animasi popup

    private void Start()
    {
        // Animasi muncul pop-up dengan delay
        popupUI.transform.localScale = Vector3.zero;
        Invoke(nameof(ShowPopup), popupDelay);

        // Mulai animasi efek bubble jika ada objek yang diset
        if (bubbleEffectObject != null)
        {
            StartBubbleEffect();
        }

        // Mulai animasi penting jika ada objek yang diset
        if (importantIndicatorObject != null)
        {
            InvokeRepeating(nameof(AnimateImportantIndicator), 0f, importantPopupInterval);
        }
    }

    private void ShowPopup()
    {
        // Animasi pop-up scale
        popupUI.transform.DOScale(Vector3.one, popupDuration).SetEase(Ease.OutBounce);
    }

    private void StartBubbleEffect()
    {
        // Animasi idle bubble effect (width & height berubah seperti gelembung)
        RectTransform rectTransform = bubbleEffectObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            DOTween.Sequence()
                .Append(rectTransform.DOScale(new Vector3(1.1f, 0.9f, 1), bubbleDuration).SetEase(Ease.InOutSine))
                .Append(rectTransform.DOScale(new Vector3(0.9f, 1.1f, 1), bubbleDuration).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void AnimateImportantIndicator()
    {
        // Animasi popup untuk penanda penting
        RectTransform rectTransform = importantIndicatorObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        }
    }

    public void ReloadCurrentScene()
    {
        // Reload scene saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeScene(string sceneName)
    {
        // Berpindah ke scene tertentu
        SceneManager.LoadScene(sceneName);
    }
}
