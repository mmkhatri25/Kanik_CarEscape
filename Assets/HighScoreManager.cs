using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class HighScoreManager : MonoBehaviour
{
    public int highScore = 0;
    public int Score = 0;

    public int HighScore
    {
        get { return highScore; }
    }

    public TextMeshProUGUI CurrentScore; // Reference to the UI Text component


    // Define an action to be called when the high score increases
    public static Action<int> onHighScoreIncrease;
    public static Action OnResetScore;
    public static Action OnResetHighScore;
    public static Action OnAddNewHighscore;

    private void OnEnable()
    {
        onHighScoreIncrease += IncreaseHighScore;
        OnResetScore += ResetScore;
        OnResetHighScore += ResetHighScore;
        OnAddNewHighscore += SetNewHighScore;
    }
    private void OnDisable()
    {
        onHighScoreIncrease -= IncreaseHighScore;
        OnResetScore -= ResetScore;
        OnResetHighScore += ResetHighScore;
        OnAddNewHighscore -= SetNewHighScore;
    }
    void Start()
    {
        CurrentScore.text = highScore.ToString();
    }

    public void IncreaseHighScore(int amount)
    {
        highScore += amount;
        Score += amount;
        if (CurrentScore != null)
        {
            CurrentScore.text = Score.ToString();
        }
        if (highScore > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void ResetScore()
    {
        Score = 0;
        if (CurrentScore != null)
        {
            CurrentScore.text = Score.ToString();
        }
    }
    public void ResetHighScore()
    {
        highScore = 0;
    }

    public void SetNewHighScore()
    {
        Debug.Log("SetNewHighScore - "+ HighScore);
        LeaderBoard.OnAddNewHighScore?.Invoke(HighScore);
    }
}
