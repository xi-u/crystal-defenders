using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The menu layer: a tinted start screen before the run, and a game over
/// screen after the crystal falls. All it does is show and hide panels and
/// tell the WaveDirector to begin. The UI never simulates anything itself.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [SerializeField] WaveDirector director;
    [SerializeField] GameObject startPanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button startButton;
    [SerializeField] Button restartButton;

    void Awake()
    {
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        startButton.onClick.AddListener(OnStartPressed);
        restartButton.onClick.AddListener(OnRestartPressed);
    }

    void Update()
    {
        if (director != null && director.IsGameOver && !gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true);
        }
    }

    void OnStartPressed()
    {
        startPanel.SetActive(false);
        director.BeginRun();
    }

    void OnRestartPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
