using UnityEngine;
using TMPro;

public enum JenisKalkulasi { tambahkurang, kali};

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class CardData : ScriptableObject
{
    public float value; // Bisa positif atau negatif
    public Sprite cardSprite; // Gambar kartu
    public string cardName; // Nama kartu (opsional)
    public JenisKalkulasi kalkulasi;
}
