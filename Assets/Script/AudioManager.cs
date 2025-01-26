using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; } // Singleton instance

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource; // AudioSource untuk SFX
    [SerializeField] private AudioSource musicSource; // AudioSource untuk musik

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] sfxClips; // Daftar AudioClip untuk SFX
    [SerializeField] private AudioClip[] musicClips; // Daftar AudioClip untuk Music

    private void Awake()
    {
        // Pastikan hanya ada satu instance AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Jangan hancurkan AudioManager saat berpindah scene
        }
        else
        {
            Destroy(gameObject); // Hancurkan instance baru jika sudah ada instance lain
        }
    }

    /// <summary>
    /// Memainkan efek suara (SFX) berdasarkan indeks.
    /// </summary>
    /// <param name="index">Indeks SFX dalam array.</param>
    public void PlaySFX(int index)
    {
        if (index >= 0 && index < sfxClips.Length && sfxSource != null)
        {
            sfxSource.PlayOneShot(sfxClips[index]); // Mainkan SFX sesuai indeks
        }
        else
        {
            Debug.LogWarning("SFX index out of range or SFX Source is not assigned.");
        }
    }

    /// <summary>
    /// Memainkan musik berdasarkan indeks dengan loop.
    /// </summary>
    /// <param name="index">Indeks musik dalam array.</param>
    public void PlayMusic(int index)
    {
        if (index >= 0 && index < musicClips.Length && musicSource != null)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop(); // Hentikan musik sebelumnya jika sedang bermain
            }

            musicSource.clip = musicClips[index]; // Set AudioClip ke musicSource
            musicSource.loop = true; // Pastikan musik di-loop
            musicSource.Play(); // Mainkan musik
        }
        else
        {
            Debug.LogWarning("Music index out of range or Music Source is not assigned.");
        }
    }

    /// <summary>
    /// Menghentikan musik yang sedang dimainkan.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
