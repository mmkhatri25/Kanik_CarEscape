using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System.Reflection;



public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TextMeshProUGUI coinEarnedText;
    public TextMeshProUGUI playerCoinsText;
    public TextMeshProUGUI level_text;

    public GameObject PedastrianUnlockedUI;
    public GameObject TwoLaneUnlockedUI;

    public Image SoundObj;

    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public bool isTestLevel;

    public event Action OnSoundClicked;

    public GameObject PausePopup;
    public GameObject persistentObject;
    public GameObject[] rootObjects;
    public GameObject mainMenu;
    public void OnPauseBtn()
    {
        HighScoreManager.OnAddNewHighscore?.Invoke();
        PausePopup.SetActive(true);
        Time.timeScale = 0;

    }
    public void OnClosePause()
    {
        Time.timeScale = 1;
        PausePopup.SetActive(false);
    }
    public void MainManuButton()
    {
        HighScoreManager.OnAddNewHighscore?.Invoke();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        //if (persistentObject != null)
        //{
        //    Destroy(persistentObject);
        //}
        //DestroyAllObjectsInHierarchy();
        //LoadNewScene(0);
        //SceneManager.LoadScene(0);
        //RestartAndroidApp();
        PausePopup.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void LoadNewScene(int sceneIndex)
    {
        // Destroy all objects in the current hierarchy
        DestroyAllObjectsInHierarchy();

        // Destroy objects marked with DontDestroyOnLoad
        DestroyAllDontDestroyOnLoadObjects();

        // Load the new scene
        SceneManager.LoadScene(sceneIndex);
    }

    // Helper method to destroy all objects in the current scene's hierarchy
    private void DestroyAllObjectsInHierarchy()
    {
        // Get an array of all root game objects in the current scene
        rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        // Loop through each root object and destroy it
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }
    }

    // Helper method to destroy all objects marked with DontDestroyOnLoad
    private void DestroyAllDontDestroyOnLoadObjects()
    {
        // Create a new temporary scene
        var tempScene = SceneManager.CreateScene("TempScene");

        // Move all objects from DontDestroyOnLoad to the temporary scene
        var dontDestroyOnLoadObjects = GetDontDestroyOnLoadObjects();
        foreach (var obj in dontDestroyOnLoadObjects)
        {
            SceneManager.MoveGameObjectToScene(obj, tempScene);
        }

        // Unload the temporary scene, destroying all objects in it
        SceneManager.UnloadSceneAsync(tempScene);
    }

    // Helper method to find all objects marked with DontDestroyOnLoad
    private GameObject[] GetDontDestroyOnLoadObjects()
    {
        // Find a temporary object to access the root objects of DontDestroyOnLoad
        GameObject temp = null;
        try
        {
            temp = new GameObject();
            DontDestroyOnLoad(temp);
            var dontDestroyOnLoadScene = temp.scene;

            // Get all root game objects in the DontDestroyOnLoad scene
            var rootObjects = dontDestroyOnLoadScene.GetRootGameObjects();
            return rootObjects;
        }
        finally
        {
            if (temp != null)
            {
                Destroy(temp);
            }
        }
    }
    private void RestartAndroidApp()
    {
        Debug.Log("1 RestartAndroidApp");

        // Obtain the UnityPlayer class and the current activity
        
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

            // Set the FLAG_ACTIVITY_CLEAR_TOP flag to clear the back stack
            launchIntent.Call<AndroidJavaObject>("addFlags", 0x20000000); // FLAG_ACTIVITY_CLEAR_TOP

            // Use a handler to restart the app
            AndroidJavaRunnable restartRunnable = new AndroidJavaRunnable(() =>
            {
                currentActivity.Call("startActivity", launchIntent);
                currentActivity.Call("finish");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            });

            currentActivity.Call("runOnUiThread", restartRunnable);
        }

        // Close the app
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(UIManager).Name;
                    instance = obj.AddComponent<UIManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    private void Start()
    {
        persistentObject = GameObject.Find("MANAGERS_AND_UI");

        GameManager.Instance.OnLevelFinished += OnLevelFinished;
        GameManager.Instance.CoinsAdded += CoinsAdded;
        GameManager.Instance.LevelLoaded += LevelLoaded;

        int Number = PlayerPrefs.GetInt("isSound");
        bool soundOn = Number == 0 ? false : true;
        GameManager.Instance.isSoundOn = soundOn;
        OnSoundClicked?.Invoke();
        SetSoundUI(soundOn);

    }

    private void LevelLoaded()
    {
        int currentlvl = GameManager.Instance.currentLevel;
        
        //Show unlocked pedastrian in correct level index
        if(currentlvl == 7)
        {
            //PedastrianUnlockedUI.SetActive(true); //my workd
        }

        //Show unlocked pedastrian in correct level index
        if (currentlvl == 9)
        {
            //TwoLaneUnlockedUI.SetActive(true);//my workd
        }

        level_text.text = "LEVEL " + currentlvl.ToString();
    }

    private void CoinsAdded()
    {
        playerCoinsText.text = GameManager.Instance.playerCoins.ToString();
    }

    private void OnLevelFinished()
    {
        coinEarnedText.text = GameManager.Instance.coinsEarnedInCurrentLevel.ToString();
    }

    public void SoundClick()
    {
        bool soundOn = !GameManager.Instance.isSoundOn;
        int Numberr = PlayerPrefs.GetInt("isSound");
        if (Numberr == 1)
            PlayerPrefs.SetInt("isSound", 0);
        else
            PlayerPrefs.SetInt("isSound", 1);


        GameManager.Instance.isSoundOn = soundOn;

        OnSoundClicked?.Invoke();

        SetSoundUI(soundOn);
    }

    void SetSoundUI(bool soundOn)
    {
        Debug.Log("gameplay SetSoundUI " + soundOn);
        if (soundOn)
        {
            SoundObj.sprite = soundOnSprite;
        }
        else
        {
            SoundObj.sprite = soundOffSprite;
        }
    }
    public void setTestLevel()
    {
        if(!isTestLevel)
             isTestLevel = true;
        else
            isTestLevel = false;

    }
}
