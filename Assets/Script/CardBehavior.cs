using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.Mathematics; // Tambahkan ini untuk menggunakan DOTween

public class CardBehavior : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public CardData cardData; // Referensi ke ScriptableObject kartu
    public TextMeshProUGUI valueText; // Tampilan nilai kartu
    public Image cardImage; // Tampilan gambar kartu
    public bool IsEnemy;
    public bool IsSetTarget;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;

    private bool isPlacedInSlot = false; // Flag untuk mengecek apakah kartu sudah ditempatkan di slot
    private Sequence idleAnimationSequence; // Sequence untuk animasi idle
    private bool isHolding = false; // Flag untuk mengecek apakah kartu sedang di-klik lama
    private int value;

    private void Start()
    {
        // Inisialisasi kartu dengan data dari ScriptableObject
        if (cardData != null)
        {
            if (cardData.kalkulasi == JenisKalkulasi.tambahkurang)
                valueText.text = cardData.value.ToString();

            else
                valueText.text = "X" + Mathf.Abs(cardData.value);

            cardImage.sprite = cardData.cardSprite;
        }

        if (!IsEnemy)
        {
            // Mulai animasi idle
            StartIdleAnimation();
        }

       if (IsSetTarget)
        {
            int min = 1, max = 11;

            // Hasil random range untuk nilai awal
            value = UnityEngine.Random.Range(min, max);

            // Tentukan apakah positif atau negatif
            int sign = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            value *= sign;

            // Tampilkan nilai ke teks
            valueText.text = value.ToString();
        }

    }

    private void StartIdleAnimation()
    {
        rectTransform = GetComponent<RectTransform>();

        // Hentikan animasi jika sedang berjalan
        if (idleAnimationSequence != null && idleAnimationSequence.IsActive())
        {
            idleAnimationSequence.Kill();
        }

        // Buat animasi idle
        idleAnimationSequence = DOTween.Sequence();

        // Tambahkan gerakan naik-turun kecil dengan posisi acak
        float moveAmount = UnityEngine.Random.Range(5f, 10f); // Jarak gerakan idle
        float duration = UnityEngine.Random.Range(1.5f, 3f); // Durasi animasi
        float delay = UnityEngine.Random.Range(0f, 1f); // Delay acak untuk setiap kartu

        idleAnimationSequence.Append(rectTransform.DOAnchorPosY(originalPosition.y + moveAmount, duration).SetEase(Ease.InOutSine))
                              .Append(rectTransform.DOAnchorPosY(originalPosition.y - moveAmount, duration).SetEase(Ease.InOutSine))
                              .SetDelay(delay)
                              .SetLoops(-1, LoopType.Yoyo); // Loop terus-menerus
    }

    public float GetCardValue()
    {
        // Pastikan nilai kartu tidak null dan masuk akal
        if (cardData == null)
        {
            Debug.LogError("Card data is null!");
            return 0; // Nilai default
        }

        return cardData.value;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition; // Simpan posisi awal
        originalParent = transform.parent; // Simpan parent awal
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlacedInSlot) return; // Jika sudah di slot, hentikan drag

        canvasGroup.blocksRaycasts = false; // Nonaktifkan raycast agar kartu bisa dilepas

        // Hentikan animasi idle saat mulai drag
        if (idleAnimationSequence != null)
        {
            idleAnimationSequence.Kill();
        }

        // Animasi skala menjadi 1 saat di-drag
        rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlacedInSlot) return; // Jika sudah di slot, hentikan drag

        rectTransform.anchoredPosition += eventData.delta; // Pindahkan kartu sesuai gerakan drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlacedInSlot) return; // Jika sudah di slot, hentikan drag

        canvasGroup.alpha = 1f; // Pulihkan transparansi
        canvasGroup.blocksRaycasts = true; // Aktifkan raycasting

        // Kembalikan skala ke 0.8 saat drag berakhir
        rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

        // Cari slot tempat kartu dilepaskan
        GameObject droppedSlot = GetDroppedSlot(eventData);
        GameObject targetNumObj = GetTargetNumber(eventData);
            

        if (droppedSlot != null && !IsSetTarget)
        {
            transform.SetParent(droppedSlot.transform); // Ubah parent ke slot
            rectTransform.anchoredPosition = Vector2.zero; // Reset posisi relatif ke slot

            // Tambahkan kartu ke slot
            Slot slot = droppedSlot.GetComponent<Slot>();
            if (slot != null)
            {
                slot.AddCard(this); // Tambahkan kartu ke slot
                isPlacedInSlot = true; // Tandai kartu sebagai ditempatkan

                // Informasikan ke GameManager bahwa kartu telah ditempatkan
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                gameManager.HandleCardPlacedInSlot(gameObject, slot.slotIndex);

                // Hancurkan kartu setelah ditempatkan
                Destroy(gameObject); // Hapus kartu dari scene
            }
        }
        else if (IsSetTarget && targetNumObj != null)
        {
            transform.SetParent(targetNumObj.transform);
            rectTransform.anchoredPosition = Vector2.zero;

            //Slot slot = droppedSlot.GetComponent<Slot>();
            gameObject.SetActive(false);
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            //gameManager.HandleCardPlacedInSlot(gameObject, slot.slotIndex);
            gameManager.EnemyTurn();
            AudioManager.Instance.PlaySFX(0);
            gameManager.targetScore += value;
            print(gameManager.targetScore);
            gameManager.targetNumTxt.text = gameManager.targetScore.ToString();
        }
        else
        {
            // Jika posisi drop tidak valid, kembalikan kartu ke posisi awal dengan efek animasi gelembung
            rectTransform.DOAnchorPos(originalPosition, 1f)
                .SetEase(Ease.InOutQuad) // Gunakan easing untuk efek lembut
                .OnStart(() =>
                {
                    AudioManager.Instance.PlaySFX(9);
                    // Tambahkan efek mengembang di awal animasi
                    rectTransform.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            // Kecilkan kembali ke ukuran normal
                            rectTransform.DOScale(1f, 0.3f).SetEase(Ease.InQuad);
                        });
                });

            // Mulai kembali animasi idle (jika ada)
            StartIdleAnimation();
        }

    }

    private GameObject GetDroppedSlot(PointerEventData eventData)
    {
        // Cari slot valid menggunakan raycast pada UI
        foreach (var raycastResult in eventData.hovered)
        {
            Slot slot = raycastResult.gameObject.GetComponent<Slot>();
            if (slot != null)
            {
                return slot.gameObject;
            }
        }
        return null;
    }
    
    private GameObject GetTargetNumber(PointerEventData eventData)
    {
        // Cari slot valid menggunakan raycast pada UI
        foreach (var raycastResult in eventData.hovered)
        {
            GameObject slot = raycastResult.gameObject;
            if ( slot.tag == "TargetNumber")
            {
                return slot.gameObject;
            }
        }
        return null;
    }

    // Menangani klik lama (hold)

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPlacedInSlot) // Hanya aktifkan jika kartu belum ditempatkan
        {
            isHolding = true; // Set flag hold
            rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack); // Animasi scale menjadi 1
            AudioManager.Instance.PlaySFX(5);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHolding) // Jika kartu di-klik lama
        {
            isHolding = false; // Reset flag hold
            rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack); // Kembalikan scale ke 0.8
        }
    }
}
