using UnityEngine;
using DG.Tweening;

public class BubbleFloat : MonoBehaviour
{
    [SerializeField] private float floatHeight = 30f; // Tinggi pergerakan bubble
    [SerializeField] private float floatSpeed = 2f; // Kecepatan pergerakan vertikal
    [SerializeField] private float horizontalRange = 10f; // Jarak pergerakan horizontal
    [SerializeField] private float horizontalSpeed = 1f; // Kecepatan pergerakan horizontal
    [SerializeField] private float startDelay = 1f; // Delay sebelum animasi dimulai
    [SerializeField] private float popupDuration = 0.5f; // Durasi animasi popup
    [SerializeField] private float targetScale = 1f; // Durasi animasi popup
    [SerializeField] private bool isUsingSound = true; // Durasi animasi popup

    private Vector3 initialPosition;

    void Start()
    {
        // Simpan posisi awal objek
        initialPosition = transform.localPosition;

        // Atur skala awal ke 0 (tidak terlihat)
        transform.localScale = Vector3.zero;

        // Mainkan animasi popup setelah delay
        DOVirtual.DelayedCall(startDelay, () =>
        {
            PlayPopupAnimation();
        });
    }

    private void PlayPopupAnimation()
    {
        // Animasi popup menggunakan DOScale dari 0 ke 1
        transform.DOScale(targetScale, popupDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => StartFloatingAnimation()); // Setelah popup selesai, mulai animasi mengambang
            if (isUsingSound)
            {
               AudioManager.Instance.PlaySFX(7);
            }
    }

    private void StartFloatingAnimation()
    {
        // Tambahkan sedikit random untuk variasi animasi
        float randomDelay = Random.Range(0f, 0.5f);
        float randomSpeedMultiplier = Random.Range(0.8f, 1.2f);

        // Animasi mengambang (vertikal)
        transform.DOLocalMoveY(initialPosition.y + floatHeight, floatSpeed * randomSpeedMultiplier)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(randomDelay);

        // Animasi kiri-kanan (horizontal)
        transform.DOLocalMoveX(initialPosition.x + horizontalRange, horizontalSpeed * randomSpeedMultiplier)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(randomDelay);
    }
}
