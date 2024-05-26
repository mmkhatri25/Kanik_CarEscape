using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderBoard : MonoBehaviour
{
    public GameObject entryPrefab;
    public Transform contentPanel;
    public string playerName;

    private List<PlayerScore> scores;
    private int lastHighScore;
    public static System.Action onUpdateScore;
    private void OnEnable()
    {
        onUpdateScore += UpdatePlayerScore;
    }
    private void OnDisable()
    {
        onUpdateScore -= UpdatePlayerScore;

    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateFakeScores();
        AddPlayerScore();

        lastHighScore = PlayerPrefs.GetInt("HighScore");
        DisplayLeaderboard();

    }

    // Update is called once per frame
    //void Update()
    //{
    //    int currentHighScore = PlayerPrefs.GetInt("HighScore");
    //    print("update - " + currentHighScore);

    //    if (currentHighScore > lastHighScore)
    //    {
    //        print("in update - "+ currentHighScore);
    //        lastHighScore = currentHighScore;
    //        UpdatePlayerScore();
    //        //DisplayLeaderboard();
    //    }
    //}

    void GenerateFakeScores()
    {
        // Initialize the list
        scores = new List<PlayerScore>();

        // Real names
        List<string> realNames = new List<string>
        {
            "Alice", "Bob", "Charlie", "David", "Eve",
            "Frank", "Grace", "Hannah", "Ivy", "Jack",
            "Kate", "Liam", "Mia", "Noah", "Olivia",
            "Parker", "Quinn", "Ryan", "Sophia", "Tyler",
            "Uma", "Vincent", "Willow", "Xander", "Yara",
            "Zoe",

            // Indian names
            "Aarav", "Aarohi", "Aryan", "Ananya", "Aisha",
            "Dev", "Diya", "Ishaan", "Jiya", "Kabir",
            "Kavya", "Neha", "Rohan", "Riya", "Sahil",
            "Sanya", "Tanisha", "Vivaan", "Yash", "Zara",

            // Pakistani names
            "Ahmed", "Ayesha", "Bilal", "Fariha", "Hassan",
            "Iqra", "Junaid", "Mahnoor", "Omar", "Sana",
            "Tariq", "Zainab", "Arham", "Bushra", "Farhan",
            "Ghazala", "Imran", "Javeria", "Khalid", "Nadia"
        };

        // Create fake player names and scores
        for (int i = 0; i < realNames.Count; i++)
        {
            int fakeScore = Random.Range(100, 5000);
            scores.Add(new PlayerScore(realNames[i], fakeScore));
        }
    }

    void AddPlayerScore()
    {
        // Add the real player's score
        scores.Add(new PlayerScore(playerName, PlayerPrefs.GetInt("HighScore")));

        // Sort the list by scores in descending order
        scores.Sort((a, b) => b.score.CompareTo(a.score));
    }

    void UpdatePlayerScore()
    {
        print("UpdatePlayerScore");
        // Remove the old player's score entry
        scores.RemoveAll(score => score.name == playerName);

        // Add the updated player's score
        scores.Add(new PlayerScore(playerName, PlayerPrefs.GetInt("HighScore")));

        // Sort the list by scores in descending order
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        DisplayLeaderboard();
    }
    public Text myHighscore;
    void DisplayLeaderboard()
    {
        // Clear previous entries
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Display the leaderboard
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject newEntry = Instantiate(entryPrefab, contentPanel);
            Text entryText = newEntry.GetComponentInChildren<Text>();
            entryText.text = (i + 1).ToString() + ". " + scores[i].name + " - " + scores[i].score.ToString();
            if (scores[i].name == playerName && scores[i].score == PlayerPrefs.GetInt("HighScore"))
            {
                myHighscore.text = entryText.text;
                entryText.color = Color.yellow; // Change this to any color you like
            }
            else
            {
                entryText.color = Color.white; // Default color for other entries
            }
        }
    }
}

public class PlayerScore
{
    public string name;
    public int score;

    public PlayerScore(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}
