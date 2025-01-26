using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public List<CardData> playerAllCards; // Daftar kartu milik player
    public List<CardData> enemyAllCards; // Daftar kartu milik enemy

    public Transform[] playerDeckSlots; // Slot kartu player
    public Transform[] middleSlots;
    public TextMeshProUGUI[] middleSlotScores; // Skor masing-masing slot tengah
    public Transform enemyDeckPosition; // Posisi awal untuk animasi kartu

    private Queue<CardData> preparedCards = new Queue<CardData>();

    private List<CardData> enemyDeck = new List<CardData>(); // Kartu musuh
    private List<GameObject> playerDeck = new List<GameObject>(); // Kartu pemain

    public GameObject cardPrefab; // Prefab kartu
    public GameObject EnemyCardPrefab; // Prefab kartu enemy
    public GameObject antiInteract; // Prefab Image
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI targetNumTxt;
    public int round = 0; // Untuk melacak round
    public int targetScore = 20;
    public EnemyUI enemyUI;

    public GameObject LoseUI;
    public GameObject WinUI;
    public RandomNumberPopup randomNumberPopup;
    public GameObject addTargetObject;
    public int spawnSetTarget = 8;

    public GameObject addTargetPrefab;
    public GameObject parentObject;

    private void Start()
    {
        InitializeGame();
        //addTargetObject.SetActive(false);

        //AddCardToDeck(playerDeckSlots.Length);
        //playerDeck.transform.parent.GetChild(playerDeck.transform.parent.childCount - 1);
    }

    private void InitializeGame()
    {
        StartCoroutine(InitializeGameWithDelay());
    }

    private IEnumerator InitializeGameWithDelay()
    {
        yield return new WaitForSeconds(3.7f);

        enemyUI.ChangeExpression();

        // Acak kartu untuk player
        for (int i = 0; i < playerDeckSlots.Length; i++)
        {
            AddCardToDeck(i);
        }

        // Simpan kartu untuk enemy
        for (int i = 0; i < 5; i++) // Misalnya musuh punya 5 kartu
        {
            CardData randomCard = enemyAllCards[Random.Range(0, enemyAllCards.Count)];
            enemyDeck.Add(randomCard);
        }

        // Reset skor di slot tengah
        foreach (var scoreText in middleSlotScores)
        {
            scoreText.text = "0";
        }

        // Siapkan kartu player
        PreparePlayerCards(100);
        // Siapkan kartu enemy
        PrepareEnemyCards(100);

        // Cek apakah parent tidak memiliki child
        if (parentObject.transform.childCount == 0)
        {
            // Spawn prefab sebagai child dari parentObject di posisi (0, 0, 0)
            GameObject newObject = Instantiate(addTargetPrefab, parentObject.transform);
            newObject.transform.localPosition = Vector3.zero;

            Debug.Log("Prefab berhasil di-spawn sebagai child dengan posisi (0, 0, 0).");
        }
        else
        {
            Debug.Log("Spawn gagal: Parent sudah memiliki child.");
        }

        targetScore = randomNumberPopup.targetNumber;
        Debug.Log($"Target:{targetScore}");
    }



    private void PreparePlayerCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData randomCard;
            if (i > 5)
                randomCard = playerAllCards[Random.Range(0, playerAllCards.Count)];

            else
                randomCard = playerAllCards[Random.Range(0, 10)];

            preparedCards.Enqueue(randomCard);
        }
    }

    private void PrepareEnemyCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData randomCard;
            if (i > 15)
                randomCard = enemyAllCards[Random.Range(0, enemyAllCards.Count)];

            else
                randomCard = enemyAllCards[Random.Range(0, 10)];

            enemyDeck.Add(randomCard);
        }
    }



    public void HandleCardPlacedInSlot(GameObject placedCard, int slotIndex)
    {
        // Tambahkan kartu baru ke deck player
        for (int i = 0; i < playerDeckSlots.Length; i++)
        {
            if (playerDeckSlots[i].childCount == 0)
            {
                AddCardToDeck(i);
                break;
            }
        }

        // Giliran musuh
        EnemyTurn();
    }

    private bool isPlayerDeckNegative = false;
    private bool isEnemyDeckNegative = false;


    // Sequence global untuk animasi kartu
    private Sequence deckAnimationSequence;

    private void AddCardToDeck(int slotIndex)
    {
        antiInteract.SetActive(true);

        if (deckAnimationSequence == null || !deckAnimationSequence.IsActive())
        {
            deckAnimationSequence = DOTween.Sequence();
        }

        if (preparedCards.Count == 0)
        {
            Debug.LogWarning("Prepared card list is empty. Generating new cards.");
            PreparePlayerCards(10); // Siapkan 10 kartu tambahan jika habis
        }

        CardData cardData = preparedCards.Dequeue();
        Debug.Log($"isPlayerDeckNegative {isPlayerDeckNegative}");
        if (isPlayerDeckNegative)
        {
            cardData.value = -Mathf.Abs(cardData.value); // Pastikan nilai negatif
        }
        else
        {
            cardData.value = Mathf.Abs(cardData.value); // Pastikan nilai positif
        }

        GameObject card = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        card.GetComponent<CardBehavior>().cardData = cardData;

        RectTransform cardRect = card.GetComponent<RectTransform>();
        RectTransform slotRect = playerDeckSlots[slotIndex].GetComponent<RectTransform>();

        cardRect.SetParent(slotRect, false);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;

        cardRect.localScale = Vector3.zero;

        deckAnimationSequence.Append(cardRect.DOScale(1f * 1.2f, 0.1f).SetEase(Ease.OutQuad))
        .AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(7); // Audio dimainkan persis di tengah animasi
        })
        .Append(cardRect.DOScale(1f, 0.1f).SetEase(Ease.InBounce))
        .AppendCallback(() =>
        {
            if (slotIndex == playerDeckSlots.Length - 1)
            {
                antiInteract.SetActive(false);
            }
        });


        deckAnimationSequence.AppendInterval(0.1f);

        playerDeck.Add(card);
    }




    // Fungsi untuk mengubah mode deck menjadi negatif
    public void SetPlayerDeckNegativeMode()
    {
        isPlayerDeckNegative = true;
    }

    // Fungsi untuk mengatur mode deck pemain ke positif
    public void SetPlayerDeckPositiveMode()
    {
        isPlayerDeckNegative = false;
    }


    // Fungsi mengubah semua nilai kartu di deck menjadi positif
    public void ChangeDeckValuesToPositive()
    {
        AudioManager.Instance.PlaySFX(10);
        SetPlayerDeckPositiveMode();
        foreach (GameObject card in playerDeck)
        {
            if (card == null) continue; // Pastikan kartu belum dihancurkan
            
            CardBehavior cardBehavior = card.GetComponent<CardBehavior>();
            if (cardBehavior == null) continue;

            int oldValue = (int) cardBehavior.cardData.value;
            if (oldValue < 0)
            {
                AnimateCardValueChange(card, Mathf.Abs(oldValue));
            }
        }
    }

    // Fungsi mengubah semua nilai kartu di deck menjadi negatif
    public void ChangeDeckValuesToNegative()
    {
        AudioManager.Instance.PlaySFX(10);
        SetPlayerDeckNegativeMode();
        foreach (GameObject card in playerDeck)
        {
            if (card == null) continue; // Pastikan kartu belum dihancurkan
            
            CardBehavior cardBehavior = card.GetComponent<CardBehavior>();
            if (cardBehavior == null) continue;

            int oldValue = (int) cardBehavior.cardData.value;
            if (oldValue > 0)
            {
                AnimateCardValueChange(card, -Mathf.Abs(oldValue));
            }
        }
    }

    

    // Fungsi animasi perubahan nilai kartu
    private void AnimateCardValueChange(GameObject card, int newValue)
    {
        if (card == null) return; // Pastikan kartu belum dihancurkan
        
        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect == null) return;

        // Animasi bubble burst (meledak)
        Sequence burstSequence = DOTween.Sequence();
        burstSequence.Append(cardRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
                    .OnComplete(() =>
                    {
                        // Pastikan objek kartu masih ada
                        if (card == null) return;

                        // Ubah nilai kartu
                        CardBehavior cardBehavior = card.GetComponent<CardBehavior>();
                        if (cardBehavior == null) return;

                        cardBehavior.cardData.value = newValue;

                        // Perbarui nama kartu sesuai dengan nilai
                        string newCardName = newValue < 0 ? "-" + cardBehavior.cardData.cardName : cardBehavior.cardData.cardName.TrimStart('-');
                        cardBehavior.cardData.cardName = newCardName;

                        // Update teks nilai kartu
                        if (cardBehavior.valueText != null)
                        {
                            cardBehavior.valueText.text = newValue.ToString();  // Mengubah teks pada TextMeshPro
                        }

                        // Animasi pop-out (muncul kembali)
                        cardRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                    });
    }


    public void UpdateSlotScore(int slotIndex, float value, bool tambahKurang=true)
    {
        // Validasi skor awal sebelum parsing
        int currentScore = 0;
        if (!string.IsNullOrEmpty(middleSlotScores[slotIndex].text))
        {
            currentScore = int.Parse(middleSlotScores[slotIndex].text);
        }

        // Tambahkan nilai
        if(tambahKurang)
            currentScore = (int)(currentScore + value);
        else
            currentScore = (int)(currentScore * value);

        // Pastikan skor minimal 0
        if (currentScore < 0)
        {
            currentScore = 0;
        }

        // Perbarui teks di slot
        middleSlotScores[slotIndex].text = currentScore.ToString();

        if (round < 11)
        {
            
            int playerPoints = int.Parse(middleSlotScores[1].text);
            int enemyPoints = int.Parse(middleSlotScores[0].text);

            if (playerPoints > targetScore)
            {
                Debug.Log("Player Lose");
                Debug.Log($"Target Score {targetScore}");
                LoseUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(12);
            }
            else if (enemyPoints > targetScore)
            {
                Debug.Log("Player Win");
                Debug.Log($"Target Score {targetScore}");
                WinUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(13);
            }

            Debug.Log($"[UpdateSlotScore] Slot {slotIndex}: Delta = {value}, New Score = {currentScore}");
        }

    }


    private void ChangeEnemyDeckValuesToPositive()
    {
        foreach (CardData card in enemyDeck)
        {
            if (card.value < 0)
            {
                card.value = Mathf.Abs(card.value); // Ubah nilai menjadi positif
            }
        }
    }

    

    private void ChangeEnemyDeckValuesToNegative()
    {
        foreach (CardData card in enemyDeck)
        {
            if (card.value > 0)
            {
                card.value = -Mathf.Abs(card.value); // Ubah nilai menjadi negatif
            }
        }
    }

    private int enemyLowerRounds = 0; // Hitung jumlah ronde musuh lebih rendah
    // Cari langkah terbaik untuk Enemy
    public void EnemyTurn()
    {
        antiInteract.SetActive(true);

        int enemyScore = int.Parse(middleSlotScores[0].text);
        int playerScore = int.Parse(middleSlotScores[1].text);
        int roundsLeft = 10 - round;

            // Cek apakah parent tidak memiliki child
            if (parentObject.transform.childCount == 0)
            {
                // Spawn prefab sebagai child dari parentObject di posisi (0, 0, 0)
                GameObject newObject = Instantiate(addTargetPrefab, parentObject.transform);
                newObject.transform.localPosition = Vector3.zero;

                Debug.Log("Prefab berhasil di-spawn sebagai child dengan posisi (0, 0, 0).");
            }
            else
            {
                Debug.Log("Spawn gagal: Parent sudah memiliki child.");
            }
        


        ChangeEnemyDeckValuesToPositive();

        Debug.Log($"[EnemyTurn] Ronde: {round}, EnemyScore: {enemyScore}, PlayerScore: {playerScore}, RoundsLeft: {roundsLeft}");

        if (enemyScore == 0)
        {
            foreach (CardData card in enemyDeck)
            {
                if (card.value > 0)
                {
                    PlaceBestCard(0, card); // Tambah poin ke slot enemy
                    Debug.Log($"[EnemyTurn] EnemyScore 0, menambahkan poin ke slot enemy dengan kartu {card.value}.");
                    antiInteract.SetActive(false);
                    return;
                }
            }
        }

         if (enemyScore < playerScore)
        {
            enemyLowerRounds++;
            if (enemyLowerRounds >= 3)
            {
                // Kurangi poin player sebanyak 10
                playerScore -= 10;
                playerScore = Mathf.Max(playerScore, 0); // Pastikan poin tidak negatif
                middleSlotScores[1].text = playerScore.ToString();
                Debug.Log("[EnemyTurn] Enemy kalah poin selama 3 ronde, mengurangi poin player sebanyak 10.");
                enemyLowerRounds = 0; // Reset hitungan
            }
        }
        else
        {
            enemyLowerRounds = 0; // Reset hitungan jika enemy tidak kalah poin
        }

        
        // 2. Periksa kartu di deck enemy untuk menyerang player
        foreach (CardData card in enemyDeck)
        {
            if (playerScore + card.value > targetScore)
            {
                PlaceBestCard(1, card); // Serang player
                Debug.Log($"[EnemyTurn] Memilih kartu {card.value} untuk menyerang player dan membuat skornya melebihi target.");
                antiInteract.SetActive(false);
                return;
            }
        }

        Debug.Log("[EnemyTurn] enemyScore " + enemyScore);

        // 1. Periksa apakah enemyScore + 20 melebihi target
        if (enemyScore + 15 > targetScore)
        {
            Debug.Log("[EnemyTurn] enemyScore + 8 melebihi target");
            if (enemyScore + 10 >= targetScore)
            {
                ChangeEnemyDeckValuesToNegative(); // Fokus mengurangi poin enemy
                Debug.Log("[EnemyTurn] Strategi: Mengubah kartu di deck enemy menjadi negatif untuk mengurangi poin.");

                // **Aksi nyata setelah perubahan nilai deck**
                CardData bestCard = null;
                int bestDifference = int.MaxValue;

                foreach (CardData card in enemyDeck)
                {
                    if (card.value < 0) // Pilih kartu negatif untuk dimainkan
                    {

                        int newEnemyScore;
                        // if (card.kalkulasi == JenisKalkulasi.tambahkurang) 
                            newEnemyScore = (int) (enemyScore + card.value);
                        // else
                        //     newEnemyScore = (int) (enemyScore * card.value);


                        int difference = Mathf.Abs(newEnemyScore - (targetScore - Random.Range(8,10))); // Cari kartu terbaik yang mendekati target - 5
                        if (newEnemyScore < targetScore && difference < bestDifference)
                        {
                            bestCard = card;
                            bestDifference = difference;
                        }
                    }
                }

                if (bestCard != null)
                {
                    PlaceBestCard(0, bestCard); // Mainkan kartu terbaik
                    Debug.Log($"[EnemyTurn] Mengurangi poin enemy dengan kartu terbaik {bestCard.value}.");
                    antiInteract.SetActive(false);
                    return;
                }
            }
        }
        else
        {
            if (enemyScore + 10 <= targetScore)
            {
                ChangeEnemyDeckValuesToPositive();
                Debug.Log("[EnemyTurn] Strategi: Mendekati target dengan aman.");

                // **Aksi nyata setelah perubahan nilai deck**
                CardData bestCard = null;
                int bestDifference = int.MaxValue;

                foreach (CardData card in enemyDeck)
                {
                    if (card.value > 0) // Pilih kartu positif untuk dimainkan
                    {
                        int newEnemyScore;
                        // if (card.kalkulasi == JenisKalkulasi.tambahkurang) 
                            newEnemyScore = (int)(enemyScore + card.value);
                        // else
                        //     newEnemyScore = (int)(enemyScore * card.value);

                        int difference = Mathf.Abs(playerScore - (targetScore + Random.Range(8,10))); // Cari kartu terbaik yang mendekati target + 8
                        if (newEnemyScore <= targetScore && difference < bestDifference)
                        {
                            bestCard = card;
                            bestDifference = difference;
                        }
                    }
                }

                if (bestCard != null)
                {
                    PlaceBestCard(0, bestCard); // Mainkan kartu terbaik
                    Debug.Log($"[EnemyTurn] Menambah poin enemy dengan kartu terbaik {bestCard.value}.");
                    antiInteract.SetActive(false);
                    return;
                }
            }
        }



        

        // 3. Jika poin enemy berada di posisi aman
        if (enemyScore + 10 <= targetScore && enemyScore >= targetScore / 2)
        {
            int randomChoice = Random.Range(0, 2); // Pilih salah satu strategi secara acak
            if (randomChoice == 0)
            {
                // Strategi: Membuat poin player mendekati/melebihi target
                foreach (CardData card in enemyDeck)
                {
                    if (card.value > 0)
                    {
                        PlaceBestCard(1, card); // Tambah poin player
                        Debug.Log($"[EnemyTurn] Menambah poin player dengan kartu {card.value}.");
                        antiInteract.SetActive(false);
                        return;
                    }
                }
            }
            else
            {
                // Strategi: Mengurangi poin player
                ChangeEnemyDeckValuesToNegative(); // Ubah kartu menjadi negatif
                foreach (CardData card in enemyDeck)
                {
                    if (card.value < 0)
                    {
                        PlaceBestCard(1, card); // Kurangi poin player
                        Debug.Log($"[EnemyTurn] Mengurangi poin player dengan kartu {card.value}.");
                        antiInteract.SetActive(false);
                        return;
                    }
                }
            }
        }

        // **Kondisi Baru**: Jika selisih poin antara player dan enemy lebih dari 10
        if (enemyScore < playerScore - 10)
        {
            int randomChoice = Random.Range(0, 2); // Pilih salah satu strategi secara acak
            if (randomChoice == 0)
            {
                // Strategi: Tambah poin player dengan kartu kecil (1-5) dari deck enemy
                foreach (CardData card in enemyDeck)
                {
                    if (card.value > 0 && card.value <= 5)
                    {
                        PlaceBestCard(1, card);
                        Debug.Log($"[EnemyTurn] Menambah poin player dengan kartu kecil {card.value}.");
                        antiInteract.SetActive(false);
                        return;
                    }
                }
            }
            else
            {
                // Strategi: Tambah poin ke slot enemy dengan kartu besar (6-10)
                foreach (CardData card in enemyDeck)
                {
                    if (card.value >= 6)
                    {
                        PlaceBestCard(0, card);
                        Debug.Log($"[EnemyTurn] Menambah poin ke slot enemy dengan kartu besar {card.value}.");
                        antiInteract.SetActive(false);
                        return;
                    }
                }
            }
        }
        else if (enemyScore > playerScore + 10 && enemyScore < targetScore)
        {
            // Jika enemy lebih dari 10 poin dibanding player dan belum mencapai target
            foreach (CardData card in enemyDeck)
            {
                if (card.value < 0)
                {
                    PlaceBestCard(1, card); // Fokus mengurangi poin player
                    Debug.Log($"[EnemyTurn] Enemy memiliki keunggulan, mengurangi poin player dengan kartu {card.value}.");
                    antiInteract.SetActive(false);
                    return;
                }
            }
        }

        // 4. Jika tidak ada pilihan terbaik, pilih langkah acak
        Debug.Log("[EnemyTurn] Tidak ada pilihan optimal, memilih langkah acak.");
        int randomSlot = Random.Range(0, 2); // 0 untuk Enemy, 1 untuk Player
        CardData randomCard = enemyDeck[Random.Range(0, enemyDeck.Count)];
        PlaceBestCard(randomSlot, randomCard);
        Debug.Log($"[EnemyTurn] Memilih kartu {randomCard.value} secara acak untuk slot {(randomSlot == 0 ? "Enemy" : "Player")}.");

        // Periksa poin setelah kartu ditempatkan
        

        antiInteract.SetActive(false);
    }







    private void PlaceBestCard(int slotIndex, CardData card)
    {
        int physicalSlot = slotIndex == 0 ? 0 : 1; // 0 untuk Enemy, 1 untuk Player
        Slot slotScript = middleSlots[physicalSlot].GetComponent<Slot>();

        if (slotScript != null)
        {
            GameObject enemyCard = Instantiate(EnemyCardPrefab, enemyDeckPosition.position, Quaternion.identity, enemyDeckPosition);
            CardBehavior cardBehavior = enemyCard.GetComponent<CardBehavior>();
            cardBehavior.cardData = card;

            enemyUI.ChangeExpression();

            RectTransform cardRect = enemyCard.GetComponent<RectTransform>();
            RectTransform slotRect = middleSlots[physicalSlot].GetComponent<RectTransform>();

            Sequence cardAnimation = DOTween.Sequence()
                .Append(cardRect.DOMove(slotRect.position, 0.5f).SetEase(Ease.InOutQuad))
                // .Join(cardRect.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBack))
                .OnComplete(() =>
                {
                    // Update ronde
                    round++;
                    roundText.text = $"Round: {round}";


                    cardRect.SetParent(slotRect, false);
                    cardRect.anchoredPosition = Vector2.zero;
                    cardRect.localRotation = Quaternion.identity;

                    slotScript.AddCard(cardBehavior);
                    enemyDeck.Remove(card);

                    CardData newCard = enemyAllCards[Random.Range(0, enemyAllCards.Count)];
                    enemyDeck.Add(newCard);

                    // Hancurkan objek kartu setelah animasi selesai
                    Destroy(enemyCard); // Hapus objek kartu dari scene, bukan hanya komponen CardBehavior
                    Destroy(cardBehavior); // Hapus objek kartu dari scene, bukan hanya komponen CardBehavior

                    Debug.Log($"Enemy placed card with value {card.value} in slot {physicalSlot}.");
                    ComparePoints(round, int.Parse(middleSlotScores[1].text), int.Parse(middleSlotScores[0].text));
                });
        }
    }

    private void ComparePoints(int round, int playerPoints, int enemyPoints)
    {
        // Debug.Log("Masuk Compare" + round + playerPoints + enemyPoints);
        // if (playerPoints > targetScore)
        //     {
        //         Debug.Log("Player Lose");
        //         Debug.Log($"Target Score {targetScore}");
        //         LoseUI.gameObject.SetActive(true);
        //         return;
        //     }
        //     else if (enemyPoints > targetScore)
        //     {
        //         Debug.Log("Player Win");
        //         Debug.Log($"Target Score {targetScore}");
        //         WinUI.gameObject.SetActive(true);
        //         return;
        //     }

        
        Debug.Log($"player point:{playerPoints} enemy{enemyPoints}");
        if (round == 10)
        {
            if (playerPoints > targetScore)
            {
                Debug.Log("Player Lose");
                Debug.Log($"Target Score {targetScore}");
                LoseUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(12);
                return;
            }
            else if (enemyPoints > targetScore)
            {
                Debug.Log("Player Win");
                Debug.Log($"Target Score {targetScore}");
                WinUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(13);
                return;
            }

            // Bandingkan jumlah poin player dan enemy
            if (enemyPoints > playerPoints)
            {
                Debug.Log("You Lose!");
                LoseUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(12);
            }
            else if (playerPoints > enemyPoints)
            {
                Debug.Log("You Win!");
                WinUI.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(13);
            }
            else
            {
                Debug.Log("It's a Draw!");
            }
        }
    }


}
