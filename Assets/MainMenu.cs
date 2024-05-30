using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public event Action OnSoundClicked;
    public Image SoundObj;

    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    void Start()
    {
        
        if (PlayerPrefs.HasKey("isSound"))
        {
            int Numberr = PlayerPrefs.GetInt("isSound");
            if (Numberr == 1)
            {
                SetSoundUI(true);
            }
            else
            {
                SetSoundUI(false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("isSound", 1);
            int Numberr = PlayerPrefs.GetInt("isSound");
            if (Numberr == 1)
            {
                SetSoundUI(true);
            }
            else
            {
                SetSoundUI(false);
            }
        }
    }

    public void GameplayButton()
    {
        SceneManager.LoadScene(1);
    }
    public void SoundClick()
    {
        if (PlayerPrefs.HasKey("isSound"))
        {
            int Numberr = PlayerPrefs.GetInt("isSound");
            if(Numberr == 1)
            {

                PlayerPrefs.SetInt("isSound", 0);
                Numberr = PlayerPrefs.GetInt("isSound");

                bool soundOn = Numberr == 0 ? false : true; // Default to sound on (1) if the key doesn't exist
                Debug.Log("soundOn " + 111 + soundOn);

                SetSoundUI(soundOn);
            }
            else
            {
         

                PlayerPrefs.SetInt("isSound", 1);
                Numberr = PlayerPrefs.GetInt("isSound");

                bool soundOn = Numberr == 0 ? false : true; // Default to sound on (1) if the key doesn't exist
                Debug.Log("soundOn " + 000 + soundOn);

                SetSoundUI(soundOn);
            }
        }
        else
        {
            
            PlayerPrefs.SetInt("isSound", 1);
            int Numberr = PlayerPrefs.GetInt("isSound");

            bool soundOn = Numberr == 0 ? false : true; // Default to sound on (1) if the key doesn't exist
            SetSoundUI(soundOn);
        }
        
    }
    void SetSoundUI(bool soundOn)
    {
        Debug.Log("SetSoundUI "+ soundOn);
        if (soundOn)
        {
            SoundObj.sprite = soundOnSprite;
        }
        else
        {
            SoundObj.sprite = soundOffSprite;
        }
    }
}
