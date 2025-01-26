using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyUI : MonoBehaviour
{
    [Header("Enemy Settings")]
    public RectTransform enemyTransform; // Referensi ke RectTransform enemy
    public Image enemyImage; // Referensi ke komponen Image
    public Sprite[] enemyExpressions; // Array untuk menyimpan sprite ekspresi enemy

    [Header("Popup Animation")]
    public float popupDuration = 0.5f; // Durasi animasi popup
    public float popupDelay = 0.2f; // Delay sebelum animasi popup dimulai
    public Ease popupEase = Ease.OutBack; // Efek easing untuk animasi popup

    [Header("Idle Movement")]
    public float idleMoveAmount = 10f; // Jarak gerakan idle
    public float idleDuration = 2f; // Durasi gerakan idle

    [Header("Side Movement")]
    public float sideMoveDistance = 200f; // Jarak gerakan ke kanan/kiri
    public float sideMoveDuration = 1f; // Durasi gerakan ke kanan/kiri
    public float sideMoveDelay = 2f; // Delay antar gerakan kanan/kiri

    private Vector3 originalPosition; // Posisi asli enemy
    private Sequence idleSequence; // Sequence untuk animasi idle

    private void Start()
    {
        // Atur skala awal menjadi 0 untuk animasi popup
        enemyTransform.localScale = Vector3.zero;

        // Animasi popup
        enemyTransform.DOScale(Vector3.one, popupDuration)
        .SetDelay(popupDelay) // Tambahkan delay sebelum animasi
        .SetEase(popupEase) // Tambahkan efek easing
        .OnStart(() =>
        {
            // Mainkan efek suara saat animasi popup dimulai
            AudioManager.Instance.PlaySFX(11);
        })
        .OnComplete(() =>
        {
            // Mulai animasi idle setelah popup selesai
            StartIdleMovement();

            // Mulai animasi gerakan ke samping
            InvokeRepeating(nameof(StartSideMovement), sideMoveDelay, sideMoveDelay + sideMoveDuration);
        });


        // Simpan posisi asli enemy
        originalPosition = enemyTransform.anchoredPosition;
    }

    private void StartIdleMovement()
    {
        // Buat animasi idle (naik-turun dan mengembang/mengempis seperti gelembung)
        idleSequence = DOTween.Sequence()
            .Append(enemyTransform.DOAnchorPosY(originalPosition.y + idleMoveAmount, idleDuration).SetEase(Ease.InOutSine))
            .Join(enemyTransform.DOScale(new Vector3(1f, 0.9f, 1), idleDuration).SetEase(Ease.InOutSine)) // Mengembang sedikit
            .Append(enemyTransform.DOAnchorPosY(originalPosition.y - idleMoveAmount, idleDuration).SetEase(Ease.InOutSine))
            .Join(enemyTransform.DOScale(new Vector3(0.9f, 1f, 1), idleDuration).SetEase(Ease.InOutSine)) // Mengempis sedikit
            .SetLoops(-1, LoopType.Yoyo); // Loop terus-menerus
    }

    private void StartSideMovement()
    {
        // Animasi bergerak ke kanan
        enemyTransform.DOAnchorPosX(originalPosition.x + sideMoveDistance, sideMoveDuration)
            .SetEase(Ease.InOutQuad)
            .OnStart(() =>
            {
                enemyTransform.localScale = new Vector3(1, 1, 1); // Scale X = 1 saat ke kanan
            });
    }

    public void ChangeExpression()
    {
        // Ubah sprite enemy menjadi ekspresi acak
        if (enemyExpressions.Length > 0)
        {
            Sprite randomSprite = enemyExpressions[Random.Range(0, enemyExpressions.Length)];
            enemyImage.sprite = randomSprite;
        }
    }
}
