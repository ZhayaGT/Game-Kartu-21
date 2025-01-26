using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Untuk TextMeshPro
using DG.Tweening;  // Untuk DOTween

public class Slot : MonoBehaviour
{
    public int slotIndex; // Index slot (untuk referensi di GameManager)
    public GameManager gameManager;
    private int currentScore = 0; // Skor total di slot ini
    private List<CardBehavior> cardsInSlot = new List<CardBehavior>(); // Daftar kartu dalam slot
    public TextMeshProUGUI slotScoreText; // TextMeshPro untuk menampilkan skor di slot

    public void AddCard(CardBehavior newCard)
    {
        if (newCard == null)
        {
            Debug.LogError("Card is null. Cannot add to slot.");
            return;
        }

        cardsInSlot.Add(newCard);
        newCard.transform.SetParent(this.transform);
        newCard.transform.localPosition = Vector3.zero;

        float newCardValue = newCard.GetCardValue();
        Debug.Log($"Adding card value: {newCardValue} to slot {slotIndex}");

        if (newCard.cardData.kalkulasi == JenisKalkulasi.tambahkurang)
            currentScore = Mathf.Max(0, (int)(currentScore + newCardValue)); // Hindari skor negatif
        else
            currentScore = Mathf.Max(0, (int)(currentScore * newCardValue)); // Hindari skor negatif

        gameManager.UpdateSlotScore(slotIndex, newCardValue);
        
        AudioManager.Instance.PlaySFX(0);
        UpdateSlotScoreText();
        AnimateScoreText();
        AnimateSlotScale(newCardValue);
    }

    public void RemoveCard(CardBehavior cardToRemove)
    {
        if (cardToRemove == null || !cardsInSlot.Contains(cardToRemove))
        {
            Debug.LogError("Card not found in slot. Cannot remove.");
            return;
        }

        cardsInSlot.Remove(cardToRemove);

        float cardValue = cardToRemove.GetCardValue();
        Debug.Log($"Removing card value: {cardValue} from slot {slotIndex}");

        currentScore = Mathf.Max(0, (int)(currentScore - cardValue)); // Hindari skor negatif
        gameManager.UpdateSlotScore(slotIndex, -cardValue);

        Destroy(cardToRemove.gameObject);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public List<CardBehavior> GetCardsInSlot()
    {
        return new List<CardBehavior>(cardsInSlot);
    }

    public void ClearSlot()
    {
        Debug.Log($"Clearing all cards from slot {slotIndex}");

        foreach (CardBehavior card in cardsInSlot)
        {
            float cardValue = card.GetCardValue();
            currentScore = Mathf.Max(0, (int)(currentScore - cardValue)); // Hindari skor negatif
            gameManager.UpdateSlotScore(slotIndex, -cardValue);
            Destroy(card.gameObject);
        }

        cardsInSlot.Clear();
    }

    private void UpdateSlotScoreText()
    {
        if (slotScoreText != null)
        {
            slotScoreText.text = currentScore.ToString();
        }
    }

    private void AnimateScoreText()
    {
        if (slotScoreText != null)
        {
            slotScoreText.transform.localScale = Vector3.zero;
            slotScoreText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }

    private void AnimateSlotScale(float cardValue)
    {
        float scaleMultiplier = cardValue * 0.01f;
        Vector3 currentScale = transform.localScale;
        Vector3 targetScale = new Vector3(currentScale.x + scaleMultiplier, currentScale.y + scaleMultiplier, currentScale.z);

        transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack);
    }
}
