using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ReloadCurrentScene()
    {
        // Reload scene saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeScenes(string sceneName)
    {
        // Berpindah ke scene tertentu
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        // Keluar dari game
        #if UNITY_EDITOR
        // Jika di editor Unity, berhenti play mode
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Jika di build, keluar dari aplikasi
        Application.Quit();
        #endif
    }
}
