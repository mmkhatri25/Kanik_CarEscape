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

    public float carTime;

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
    public TextMeshProUGUI ScoreFailed, TimeoutText;

    public CanvasGroup logoScreen;

    #region Variables

    public int carCount = 0; // Number of cars in the level
    public TextMeshProUGUI timerText; // UI Text to display the timer
    public GameObject popup; // The popup to show when the timer expires

    public float timeRemaining;
    private bool timerIsRunning = false;
    public int CurrentScore;
    public GameObject mainMenu, FadeScreen;
    GameObject carEscaped;
    public bool timeout, levelFinished;

    #endregion


    void Start()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    public void OnStartButton()
    {
        Time.timeScale = 1;
        isGameOver = false;
        timeout = false;
        UIManager.instance.isPause = false;

        HighScoreManager.OnResetHighScore?.Invoke();
        HighScoreManager.OnResetScore?.Invoke();
        mainMenu.SetActive(false);
        FadeScreen.SetActive(true);
        Invoke("InitGame", .3f);
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

        //if (UIManager.instance.isTestLevel)
        //    LoadNextLevel();
        //else
        LoadNextLevel(currentLevel);

    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0.5f)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                // Time has run out
                timeRemaining = 0;
                float minutes = Mathf.FloorToInt(timeRemaining / 60);
                float seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
        LevelLose();
    }

    void LoadNextLevel(int currentLevel)
    {

        //if (UIManager.instance.isTestLevel)
        //{
        //    LoadNextLevel();

        //}
        //else
        {
            Debug.Log("Starting Level: " + currentLevel);

            if (currentLevel <= 15)
            //if (currentLevel <= 3)

            {
                int nextLevel = GetNextRandomLevelWithinFirst15();
                LoadLevel(nextLevel);
                //Debug.Log("LoadNextLevel --1, next level: " + nextLevel);
            }
            else
            {
                // Load random levels from 16 to 100 without repetition until all levels are played
                int nextLevel = GetNextRandomLevel();
                LoadLevel(nextLevel);
            }
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
            SceneManager.LoadScene(levelName);
        }
        else
        {
            int fallbackLevel = UnityEngine.Random.Range(10, 99);
            SceneManager.LoadScene("Level_" + fallbackLevel);
        }
       UIManager.instance.level_text.text = "LEVEL " + level;
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

 

    public void CarEscape(GameObject car)
    {
        carEscaped = car;
        HighScoreManager.onHighScoreIncrease?.Invoke(10);
        Instantiate(coinPrefab, car.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

        CarsInLevel.Remove(car.GetComponent<Car>());

        if (CarsInLevel.Count <= 0)
        {
            levelFinished = true;
            Debug.Log("timeRemaining - "+ timeRemaining);
            HighScoreManager.onHighScoreIncrease?.Invoke((int) timeRemaining); 
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
        CurrentScore = 0;
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
    public void MainMenu()
    {
        levelFinished = false;
        isGameOver = false;
        timeout = false;
        UIManager.instance.isPause = false;
        HighScoreManager.OnResetHighScore?.Invoke();
        HighScoreManager.OnResetScore?.Invoke();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        mainMenu.SetActive(true);
    }

    public void LoadNextLevelDelay()
    {
        Invoke("LoadNextLevel", .1f);
    }

    void LoadNextLevel()
    {
        levelFinished = false;

        int currentLevel = PlayerPrefs.GetInt("Level");
        currentLevel++;
        Debug.Log("next level - "+ currentLevel);
        PlayerPrefs.SetInt("Level", currentLevel);
        LoadNextLevel(currentLevel);
        //LoadLevel(currentLevel);
    }
    void ShowLogoScreen1()
    {
        // Set the initial alpha to 0
        logoScreen.alpha = 0f;

        logoScreen.gameObject.SetActive(true);

        logoScreen.DOFade(1f, .2f)
            .OnComplete(() =>
            {
                mainMenu.SetActive(true);
                //RestartAndroidApp();
            });
    }
    
    void ShowLogoScreen()
    {
        logoScreen.alpha = 0f;

        logoScreen.gameObject.SetActive(true);

        logoScreen.DOFade(1f, .2f)
            .OnComplete(() =>
            {
                int currentLevel = PlayerPrefs.GetInt("Level");
                LoadNextLevel(currentLevel);
         
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
            timeRemaining = carCount * carTime; ;
            timerIsRunning = true;
            popup.SetActive(false); // Ensure the popup is hidden at the start
            //======= Timer
        }
        try
        {
            m_total_coin_text.transform.parent.gameObject.SetActive(true);
            m_total_coin_text.text = PlayerPrefs.GetInt("playercoins").ToString("F0");
        }
        catch (Exception ex)
        {
            //Debug.LogError("An error occurred while updating the total coin text: " + ex.Message);
            // Optionally, you can also log the stack
        }
        FadeScreen.SetActive(false);

    }

    void HideLogoScreen()
    {
        logoScreen.DOFade(0f, 1.35f)
            .OnComplete(() => logoScreen.gameObject.SetActive(false));
    }
    public bool isGameOver;

    public void LevelLose()
    {

        if (isGameOver || levelFinished)
            return;

        if (timeout && !isGameOver)
            TimeoutText.gameObject.SetActive(true);
        else
            TimeoutText.gameObject.SetActive(false);

        isGameOver = true;
        timeout = false;
        HighScoreManager.OnAddNewHighscore?.Invoke();
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
