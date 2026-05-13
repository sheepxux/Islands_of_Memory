using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    private enum MenuMode
    {
        Start,
        Pause
    }

    [Header("Panels")]
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject controlsPanel;

    [Header("Buttons")]
    public Button startButton;
    public Button startControlsButton;
    public Button startQuitButton;
    public Button resumeButton;
    public Button pauseControlsButton;
    public Button pauseQuitButton;
    public Button backButton;
    public Button pauseHudButton;

    [Header("Audio")]
    public Slider volumeSlider;
    public TMP_Text volumeText;

    [Header("Player")]
    public MonoBehaviour playerMovement;

    private bool gameStarted;
    private bool paused;
    private bool movementWasEnabledBeforePause;
    private MenuMode controlsReturnMode = MenuMode.Start;

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ResolveReferences();
    }

    private void Awake()
    {
        ResolveReferences();
        HookButtons();

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        ShowStartMenu();
    }

    private void Update()
    {
        if (!gameStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (controlsPanel != null && controlsPanel.activeSelf)
            {
                CloseControls();
                return;
            }

            if (paused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        paused = false;
        Time.timeScale = 1f;

        SetActive(startMenuPanel, false);
        SetActive(controlsPanel, false);
        SetActive(pauseMenuPanel, false);
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, true);

        SetPlayerMovement(true);
    }

    public void PauseGame()
    {
        if (!gameStarted) return;

        paused = true;
        movementWasEnabledBeforePause = playerMovement != null && playerMovement.enabled;
        SetPlayerMovement(false);

        Time.timeScale = 0f;
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
        SetActive(controlsPanel, false);
        SetActive(pauseMenuPanel, true);
    }

    public void ResumeGame()
    {
        if (!gameStarted) return;

        paused = false;
        Time.timeScale = 1f;
        SetActive(controlsPanel, false);
        SetActive(pauseMenuPanel, false);
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, true);

        if (movementWasEnabledBeforePause)
            SetPlayerMovement(true);
    }

    public void OpenControlsFromStart()
    {
        controlsReturnMode = MenuMode.Start;
        SetActive(startMenuPanel, false);
        SetActive(pauseMenuPanel, false);
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
        SetActive(controlsPanel, true);
    }

    public void OpenControlsFromPause()
    {
        controlsReturnMode = MenuMode.Pause;
        SetActive(startMenuPanel, false);
        SetActive(pauseMenuPanel, false);
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
        SetActive(controlsPanel, true);
    }

    public void CloseControls()
    {
        SetActive(controlsPanel, false);

        if (controlsReturnMode == MenuMode.Pause)
        {
            SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
            SetActive(pauseMenuPanel, true);
        }
        else
        {
            SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
            SetActive(startMenuPanel, true);
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);

        if (volumeText != null)
            volumeText.text = "Volume";
    }

    private void ShowStartMenu()
    {
        gameStarted = false;
        paused = false;
        Time.timeScale = 0f;

        SetActive(startMenuPanel, true);
        SetActive(pauseMenuPanel, false);
        SetActive(controlsPanel, false);
        SetActive(pauseHudButton != null ? pauseHudButton.gameObject : null, false);
        SetPlayerMovement(false);
    }

    private void ResolveReferences()
    {
        if (startMenuPanel == null)
            startMenuPanel = FindChildObject("StartMenuPanel");

        if (pauseMenuPanel == null)
            pauseMenuPanel = FindChildObject("PauseMenuPanel");

        if (controlsPanel == null)
            controlsPanel = FindChildObject("ControlsPanel");

        if (startButton == null)
            startButton = FindChildComponent<Button>("StartButton");

        if (startControlsButton == null)
            startControlsButton = FindChildComponent<Button>("ControlsButton");

        if (startQuitButton == null)
            startQuitButton = FindChildComponent<Button>("QuitButton");

        if (resumeButton == null)
            resumeButton = FindChildComponent<Button>("ResumeButton");

        if (pauseControlsButton == null)
            pauseControlsButton = FindChildComponent<Button>("PauseControlsButton");

        if (pauseControlsButton == null)
            pauseControlsButton = FindChildComponent<Button>("PauseControlButton");

        if (pauseQuitButton == null)
            pauseQuitButton = FindChildComponent<Button>("PauseQuitButton");

        if (backButton == null)
            backButton = FindChildComponent<Button>("BackButton");

        if (pauseHudButton == null)
            pauseHudButton = FindChildComponent<Button>("PauseHudButton");

        if (pauseHudButton == null)
            pauseHudButton = FindChildComponent<Button>("PauseButton");

        if (volumeSlider == null)
            volumeSlider = FindChildComponent<Slider>("VolumeSlider");

        if (volumeText == null)
            volumeText = FindChildComponent<TMP_Text>("VolumeText");

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void HookButtons()
    {
        AddListener(startButton, StartGame);
        AddListener(startControlsButton, OpenControlsFromStart);
        AddListener(startQuitButton, QuitGame);
        AddListener(resumeButton, ResumeGame);
        AddListener(pauseControlsButton, OpenControlsFromPause);
        AddListener(pauseQuitButton, QuitGame);
        AddListener(backButton, CloseControls);
        AddListener(pauseHudButton, PauseGame);
    }

    private void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private GameObject FindChildObject(string childName)
    {
        Transform found = FindChildTransform(childName);
        return found != null ? found.gameObject : null;
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform found = FindChildTransform(childName);
        return found != null ? found.GetComponent<T>() : null;
    }

    private Transform FindChildTransform(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private void SetPlayerMovement(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}
