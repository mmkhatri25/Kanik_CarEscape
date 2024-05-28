using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Threading;

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    public GameObject coinPrefab;

    public int playerCoins;

    public int coinsEarnedInCurrentLevel;

    public int currentLevel;

    public bool isSoundOn = true;

    public bool isActive;

    public List<int> playedLevels = new List<int>();

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(GameManager).Name;
                    instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    public event Action OnCarHit;
    public event Action OnCarEscape;
    public event Action OnLevelFinished;
    public event Action OnLose;
    public event Action CoinsAdded;
    public event Action LevelLoaded;
    public event Action OnCarClick;

    public List<Car> CarsInLevel;

    public int carsInitialCount;

    public GameObject level_complete_UI, In_game_UI;
    public GameObject level_lose_UI;
    [Space]
    public TextMeshProUGUI m_total_coin_text;
    public TextMeshProUGUI higscoreText, TimeoutText;

    public CanvasGroup logoScreen;

    #region Variables

    public int carCount = 0; // Number of cars in the level
    public TextMeshProUGUI timerText; // UI Text to display the timer
    public GameObject popup; // The popup to show when the timer expires

    private float timeRemaining;
    private bool timerIsRunning = false;

    #endregion

    private void Awake()
    {
        //PlayerPrefs.SetInt("Level", 29);
    }

    void Start()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            Invoke("InitGame", 1.5f);
        }

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                // Time has run out
                timeRemaining = 0;
                timerIsRunning = false;
                if (!UIManager.Instance.isTestLevel)
                    ShowPopup();
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1; // To show time in a more user-friendly manner (optional)
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ShowPopup()
    {
        timeout = true;
        //popup.SetActive(true); // Show the popup
        LevelLose();
    }

    void InitGame()
    {
        Debug.Log("InitGame called");

        currentLevel = PlayerPrefs.GetInt("Level", 0);
        if (currentLevel == 0)
        {
            currentLevel = 1;
            PlayerPrefs.SetInt("Level", currentLevel);
            PlayerPrefs.Save();
        }

        Debug.Log("Starting Level: " + currentLevel);
        LoadNextLevel(currentLevel);
    }

    void LoadNextLevel(int currentLevel)
    {
        if (currentLevel <= 15)
        {
            int nextLevel = GetNextRandomLevelWithinFirst15();
            LoadLevel(nextLevel);
            Debug.Log("LoadNextLevel --1, next level: " + nextLevel);
        }
        else
        {
            // Load random levels from 16 to 100 without repetition until all levels are played
            int nextLevel = GetNextRandomLevel();
            LoadLevel(nextLevel);
        }
    }

    int GetNextRandomLevelWithinFirst15()
    {
        int nextLevel = 0;

        if (playedLevels.Count >= 15) // Reset if all 15 levels have been played
        {
            playedLevels.Clear();
        }

        do
        {
            nextLevel = UnityEngine.Random.Range(1, 16);
        } while (playedLevels.Contains(nextLevel));

        playedLevels.Add(nextLevel);

        return nextLevel;
    }

    int GetNextRandomLevel()
    {
        int nextLevel = 0;

        if (playedLevels.Count >= 85) // 100 - 15 = 85 levels to randomize
        {
            // If all levels from 16 to 100 have been played, clear the list and start over
            playedLevels.Clear();
        }

        do
        {
            nextLevel = UnityEngine.Random.Range(16, 101);
        } while (playedLevels.Contains(nextLevel));

        playedLevels.Add(nextLevel);

        return nextLevel;
    }


    void LoadLevel(int level)
    {
        string levelName = "Level_" + level;
        Debug.Log("Loading Level: " + levelName);

        if (IsSceneExists(levelName))
        {
            Debug.Log("IsSceneExists Loading Level: " + levelName + " , IsSceneExists - " + IsSceneExists(levelName));

            SceneManager.LoadScene(levelName);
            //PlayerPrefs.SetInt("Level", level);
            //PlayerPrefs.Save();
        }
        else
        {
            // Fallback in case the level scene does not exist
            int fallbackLevel = UnityEngine.Random.Range(10, 30);
            Debug.LogWarning("Level does not exist, loading fallback level: Level_" + fallbackLevel);
            SceneManager.LoadScene("Level_" + fallbackLevel);
        }
    }


    bool IsSceneExists(string name)
    {
        // Check if a scene exists in the build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == name)
            {
                return true;
            }
        }
        return false;
    }

    public void CarClicked()
    {
        OnCarClick?.Invoke();
    }

    public void CarHit()
    {
        OnCarHit?.Invoke();
    }

    GameObject carEscaped;
    private bool timeout;

    public void CarEscape(GameObject car)
    {
        carEscaped = car;
        HighScoreManager.onHighScoreIncrease?.Invoke(10);
        // spawn coin
        Instantiate(coinPrefab, car.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

        CarsInLevel.Remove(car.GetComponent<Car>());

        if (CarsInLevel.Count <= 0)
        {
            coinsEarnedInCurrentLevel = carsInitialCount * 2;
            Debug.Log("Here " + coinsEarnedInCurrentLevel);
            LevelFinished();

            return;
        }

        Invoke("DestroyCar", 0.4f);

        OnCarEscape?.Invoke();
    }

    void DestroyCar()
    {
        Destroy(carEscaped);
    }

    public void UpdateCoins(int amount)
    {
        Debug.Log("UpdateCoins " + amount);
        playerCoins = playerCoins + amount;

        PlayerPrefs.SetInt("playercoins", playerCoins);
        Debug.Log("playerCoins " + playerCoins);

        CoinsAdded?.Invoke();
    }

    public void LevelFinished()
    {
        // show ads on level finished
        Debug.Log("Level Finished !");
        m_total_coin_text.transform.parent.gameObject.SetActive(false);
        UpdateCoins(GameManager.Instance.coinsEarnedInCurrentLevel);

        LoadNextLevelDelay();
        OnLevelFinished?.Invoke();
    }

    public void RestartLevel()
    {
        ShowLogoScreen();
    }

    public void LoadNextLevelDelay()
    {
        Invoke("LoadNextLevel", .1f);
    }

    public void SkipLevel()
    {
        LoadNextLevel();
    }

    void LoadNextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("Level");
        currentLevel++;

        // Save the new current level
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.Save();

        // Load the next level
        //LoadNextLevel(currentLevel);

        ShowLogoScreen();
    }

    void ShowLogoScreen()
    {
        // Set the initial alpha to 0
        logoScreen.alpha = 0f;

        logoScreen.gameObject.SetActive(true);

        logoScreen.DOFade(1f, .2f)
            .OnComplete(() =>
            {
                int currentLevel = PlayerPrefs.GetInt("Level");
                LoadNextLevel(currentLevel);
                //if (SceneExists("Level_" + currentLevel))
                //{
                //    Debug.Log("Level is " + currentLevel);
                //    SceneManager.LoadScene("Level_" + currentLevel);
                //}
                //else
                //{
                //    SceneManager.LoadScene("Level_" + UnityEngine.Random.Range(10, 30));
                //}
            });
    }

    bool SceneExists(string sceneName)
    {
        // Iterate through all loaded scenes and check if the specified scene exists
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(i);
            string loadedScene = System.IO.Path.GetFileNameWithoutExtension(pathToScene);

            if (loadedScene == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the one you are interested in
        if (scene.name.Contains("Level"))
        {
            In_game_UI.SetActive(true);
            LevelLoaded?.Invoke();

            level_complete_UI.gameObject.SetActive(false);
            level_lose_UI.gameObject.SetActive(false);

            HideLogoScreen();

            CarsInLevel.Clear();

            Car[] cars = GameObject.FindObjectsOfType<Car>();
            foreach (Car car in cars)
            {
                CarsInLevel.Add(car);
            }

            Debug.Log("CarsInLevel  " + CarsInLevel.Count);

            carsInitialCount = CarsInLevel.Count;

            //======= Timer
            carCount = CarsInLevel.Count;
            // Calculate time based on the number of cars
            timeRemaining = carCount * 5f;
            timerIsRunning = true;
            popup.SetActive(false); // Ensure the popup is hidden at the start
            //======= Timer
        }
        m_total_coin_text.transform.parent.gameObject.SetActive(true);
        m_total_coin_text.text = PlayerPrefs.GetInt("playercoins").ToString("F0");
    }

    void HideLogoScreen()
    {
        logoScreen.DOFade(0f, 1.35f)
            .OnComplete(() => logoScreen.gameObject.SetActive(false));
    }

    public void LevelLose()
    {
        if (timeout)
            TimeoutText.gameObject.SetActive(true);
        else
            TimeoutText.gameObject.SetActive(false);
        timeout = false;

        HighScoreManager.OnResetScore?.Invoke();

        higscoreText.text = PlayerPrefs.GetInt("HighScore").ToString();

        level_lose_UI.GetComponent<DOTweenAnimation>().DORestartAllById("LVL_FAIL");
        level_lose_UI.SetActive(true);

        Debug.Log("YOU LOST !");
        playedLevels.Clear();
        PlayerPrefs.SetInt("Level", 1); // reset level to 1
        PlayerPrefs.Save();
        currentLevel = PlayerPrefs.GetInt("Level");
        Debug.Log("YOU LOST !" + currentLevel);
        OnLose?.Invoke();
    }
}
