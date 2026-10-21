using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Sequence")]
    public string[] levelScenes = new string[] { "Level1", "level2", "Level3", "Level4", "Level5" };
    public float winTransitionDelay = 1.5f;
    public float finalWinTransitionDelay = 5.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsFinalLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (levelScenes == null || levelScenes.Length == 0) return false;
        return string.Equals(levelScenes[levelScenes.Length - 1], currentScene, System.StringComparison.OrdinalIgnoreCase);
    }

    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = -1;
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (string.Equals(levelScenes[i], currentScene, System.StringComparison.OrdinalIgnoreCase))
            {
                currentIndex = i;
                break;
            }
        }

        int completedLevelNum = (currentIndex >= 0) ? (currentIndex + 1) : 1;
        string nextScene = "";

        if (currentIndex >= 0 && currentIndex + 1 < levelScenes.Length)
        {
            nextScene = levelScenes[currentIndex + 1];
        }
        else
        {
            Debug.Log("<color=yellow>All levels completed! Looping back to TitleScreen.</color>");
            nextScene = "TitleScreen";
        }

        PlayerPrefs.SetInt("CompletedLevelNumber", completedLevelNum);
        PlayerPrefs.SetString("NextLevelSceneName", nextScene);
        PlayerPrefs.SetInt($"LevelCompleted_{completedLevelNum}", 1);
        int highest = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
        if (completedLevelNum + 1 > highest)
        {
            PlayerPrefs.SetInt("HighestUnlockedLevel", completedLevelNum + 1);
        }
        PlayerPrefs.Save();

        SceneManager.LoadScene("LevelComplete");
    }

    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
