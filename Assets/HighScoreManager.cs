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

    public TextMeshProUGUI highScoreText; // Reference to the UI Text component


    // Define an action to be called when the high score increases
    public static Action<int> onHighScoreIncrease; public static Action OnResetScore;

    private void OnEnable()
    {
        onHighScoreIncrease += IncreaseHighScore;
        OnResetScore += ResetHighScore;
    }
    private void OnDisable()
    {
        onHighScoreIncrease -= IncreaseHighScore;
        OnResetScore -= ResetHighScore;


    }
    private void Start()
    {
        highScoreText.text = "0";
    }

    public void IncreaseHighScore(int amount)
    {
        highScore += amount;
        Score += amount;
        // Update the UI Text component with the new high score
        if (highScoreText != null)
        {
            highScoreText.text = Score.ToString();
        }
        if (highScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            // Save the new high score to PlayerPrefs
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

    }

    public void ResetHighScore()
    {
        highScore = 0;
        Score = 0;

        // Update the UI Text component to reset the high score display
        if (highScoreText != null)
        {
            highScoreText.text = Score.ToString();
        }
    }
}
