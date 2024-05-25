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

        OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);




       
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
        Debug.Log("Worked");

        currentLevel = PlayerPrefs.GetInt("Level");
        if (currentLevel == 0)
        {
            currentLevel = 1;

            PlayerPrefs.SetInt("Level", currentLevel);
        }

        if (SceneExists("Level_" + currentLevel))
        {
            SceneManager.LoadScene("Level_" + currentLevel);
        }
        else
        {
            SceneManager.LoadScene("Level_" + UnityEngine.Random.Range(10, 30));
        }
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

    public void UpdateCoins(int amout)
    {
        Debug.Log("UpdateCoins " + amout);
        playerCoins = playerCoins+ amout;

        PlayerPrefs.SetInt("playercoins", playerCoins);
        Debug.Log("playerCoins " + playerCoins);


        CoinsAdded?.Invoke();
    }

    public void LevelFinished()
    {
        // show ads on level finished
        Debug.Log("Level Finished !");
        m_total_coin_text.transform.parent.gameObject.SetActive(false);
        UpdateCoins(GameManager.Instance.coinsEarnedInCurrentLevel/*PlayerPrefs.GetInt("playercoins")*/);

        //level_complete_UI.SetActive(true);
        //level_complete_UI.GetComponent<DOTweenAnimation>().DORestartAllById("LVL_COMPLETE");
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
        currentLevel++;

        PlayerPrefs.SetInt("Level", currentLevel);

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
            if (SceneExists("Level_" + currentLevel))
            {
                //currentLevel = PlayerPrefs.GetInt("Level");
                Debug.Log("Levelis "+ currentLevel);
                    SceneManager.LoadScene("Level_" + currentLevel);
                    //SceneManager.LoadScene("Level_" + 29);
                }
                else
                {
                    SceneManager.LoadScene("Level_" + UnityEngine.Random.Range(10, 30));
                }
            });
    }

    bool SceneExists(string sceneName)
    {

        // Iterate through all loaded scenes and check if the specified scene exists
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(i);
            string loadedScene = System.IO.Path.GetFileNameWithoutExtension(pathToScene);

            /*Debug.Log("LgCoreReloader: Reloading to scene(0): " + sceneName);

            Scene loadedScene = SceneManager.GetSceneByBuildIndex(i);
            Debug.LogWarning(loadedScene.name);
            */

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
            popup.SetActive(false); // Ensure the popup is hidden at th

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

        PlayerPrefs.SetInt("Level", 1); //reset level 0 do uncomment
        currentLevel = PlayerPrefs.GetInt("Level");
        PlayerPrefs.Save();
        Debug.Log("YOU LOST !" + PlayerPrefs.GetInt("Level"));
        //currentLevel = PlayerPrefs.GetInt("Level");
        OnLose?.Invoke();
        //SceneManager.LoadScene(0);
        //CarsInLevel.Clear();
    }
}
