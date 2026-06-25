using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("═══════════════ NAZWA SCENY ═══════════════")]
    public string sceneName = "Programowanie";

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Ladowanie sceny: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Nazwa sceny nie jest ustawiona!");
        }
    }

    public void LoadSceneByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Debug.Log("Ladowanie sceny: " + name);
            SceneManager.LoadScene(name);
        }
    }

    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Przeladowanie sceny: " + currentScene);
        SceneManager.LoadScene(currentScene);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Ladowanie nastepnej sceny: " + nextIndex);
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("Brak nastepnej sceny!");
        }
    }
}