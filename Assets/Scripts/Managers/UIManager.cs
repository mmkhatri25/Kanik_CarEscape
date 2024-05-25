using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static UIManager instance;

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

    public void OnPauseBtn()
    {

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
        SceneManager.LoadScene(0);
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
        GameManager.Instance.OnLevelFinished += OnLevelFinished;
        GameManager.Instance.CoinsAdded += CoinsAdded;
        GameManager.Instance.LevelLoaded += LevelLoaded;
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
        GameManager.Instance.isSoundOn = soundOn;

        OnSoundClicked?.Invoke();

        SetSoundUI(soundOn);
    }

    void SetSoundUI(bool soundOn)
    {
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
