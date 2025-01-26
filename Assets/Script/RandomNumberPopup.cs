using UnityEngine;
using TMPro;
using DG.Tweening;

public class RandomNumberPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textDisplay; // Komponen TextMeshPro untuk menampilkan angka
    [SerializeField] private float counterDuration = 3f; // Durasi animasi counter
    [SerializeField] private Vector2 numberRange = new Vector2(20, 30); // Rentang angka acak
    [SerializeField] private Transform popupTarget; // Transform untuk animasi popup
    [SerializeField] private float popupScale = 1.2f; // Skala animasi popup
    [SerializeField] private float popupDuration = 0.3f; // Durasi animasi popup
    public GameManager gameManager;

    public int targetNumber;

    private void Start()
    {
        // Mulai animasi secara otomatis saat game dimulai (Opsional)
        StartNumberAnimation();
        gameManager.targetScore = targetNumber;
    }

    /// <summary>
    /// Memulai animasi pemilihan angka dan animasi popup.
    /// </summary>
    public void StartNumberAnimation()
    {
        // Pilih angka acak dalam rentang
        targetNumber = Random.Range((int)numberRange.x, (int)numberRange.y + 1);
        // Jalankan animasi counter menggunakan DOTween
        DOTween.To(() => 0, x => textDisplay.text = x.ToString(), targetNumber, counterDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(PlayPopupAnimation); // Panggil animasi popup setelah selesai
    }

    /// <summary>
    /// Menjalankan animasi popup menggunakan DOScale.
    /// </summary>
    private void PlayPopupAnimation()
    {
        if (popupTarget == null) popupTarget = transform; // Gunakan transform ini jika popupTarget tidak diatur

        // Reset skala ke default
        popupTarget.localScale = Vector3.one;

        // Mainkan animasi DOScale untuk feedback popup
        popupTarget.DOScale(popupScale, popupDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => popupTarget.DOScale(1f, popupDuration * 0.5f).SetEase(Ease.InBack)); // Kembali ke skala semula
    }
}
