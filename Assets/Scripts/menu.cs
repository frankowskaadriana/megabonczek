using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("═══════════════ SCENY ═══════════════")]
    public string gameSceneName = "Programowanie";

    [Header("═══════════════ REFERENCJE UI ═══════════════")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle fullscreenToggle;

    [Header("═══════════════ DŹWIĘKI ═══════════════")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip gameStartSound;
    public AudioClip quitSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);

        AddButtonSound(playButton);
        AddButtonSound(settingsButton);
        AddButtonSound(quitButton);
        AddButtonSound(backButton);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Menu glowne zaladowane!");
    }

    void AddButtonSound(Button button)
    {
        if (button == null) return;
        button.onClick.AddListener(() => PlayClickSound());
    }

    public void PlayGame()
    {
        PlayClickSound();

        if (gameStartSound != null)
            AudioSource.PlayClipAtPoint(gameStartSound, Camera.main.transform.position, 0.8f);

        Debug.Log("Przenosze do sceny: " + gameSceneName);

        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        PlayClickSound();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Otwieram ustawienia");
    }

    public void CloseSettings()
    {
        PlayClickSound();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        Debug.Log("Zamykam ustawienia");
    }

    public void QuitGame()
    {
        PlayClickSound();

        if (quitSound != null)
            AudioSource.PlayClipAtPoint(quitSound, Camera.main.transform.position, 0.8f);

        Debug.Log("Zamykam gre...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);

        Debug.Log("Glosnosc glowna: " + value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);

        Debug.Log("Glosnosc SFX: " + value);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        Debug.Log("Glosnosc muzyki: " + value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Screen.fullScreen = isFullscreen;
        Debug.Log("Pelny ekran: " + isFullscreen);
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound, 0.5f);
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound, 0.3f);
    }

    public void OnButtonHover()
    {
        PlayHoverSound();
    }
}