using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    // Public property
    public int maxScore;
    public int maxScoringPlayer;
    public ScoreBehavior scoreBehavior;

    private Dictionary<int, int> RoundScoreByPlayer;
    public  Dictionary<int, int> TotalPointsByPlayer;

    private string filePath = Application.dataPath + "/PlayerData/scores.txt";
    private int numTopScores = 6;

    // Start is called before the first frame update
    void Awake()
    {
        maxScore = 0;
        maxScoringPlayer = 0;
        RoundScoreByPlayer = new Dictionary<int, int>();
        TotalPointsByPlayer = new Dictionary<int, int>();
    }

    /// <summary>
    /// Add this round's points for a player
    /// </summary>
    /// <param name="player"> The player for whom the points are being added </param>
    /// <param name="value"> The amount of points to add</param>
    /// <returns>The new point total</returns>
    public int AddRoundPoints(int player, int value)
    {
        if (!RoundScoreByPlayer.ContainsKey(player))
        {
            RoundScoreByPlayer[player] = 0;
            TotalPointsByPlayer[player] = 0;
        }
        RoundScoreByPlayer[player] += value;
        TotalPointsByPlayer[player] += value;
        if (TotalPointsByPlayer[player] > maxScore)
        {
            maxScore = TotalPointsByPlayer[player];
            maxScoringPlayer = player; 
        }
        
        scoreBehavior.UpdateScores(player);
        return RoundScoreByPlayer[player];
    }

    /// <summary>
    /// Adds bonus points based off of an accuracy level
    /// </summary>
    /// <param name="player">The player for whom the points are being added</param>
    /// <param name="accuracy"></param>
    /// <returns>The new round point total</returns>
    public int AddBonusPoints(int player, float accuracy)
    {
        if (!RoundScoreByPlayer.ContainsKey(player))
        {
            RoundScoreByPlayer[player] = 0;
            TotalPointsByPlayer[player] = 0;
        }
        int numBonusPoints = Mathf.RoundToInt(RoundScoreByPlayer[player] * accuracy);
        
        scoreBehavior.UpdateScores(player);
        scoreBehavior.UpdateAccuracy(accuracy);
        Debug.Log("Updated accuracy: " + accuracy);
        return AddRoundPoints(player, numBonusPoints);
    }

    public void ResetRoundPoints()
    {
        foreach(int key in RoundScoreByPlayer.Keys.ToList())
        {
            RoundScoreByPlayer[key] = 0;
        }
    }

    public class UserRecord
    {
        public string initials;
        public int score;

        public UserRecord(string intitials, int score)
        {
            this.initials = intitials;
            this.score = score;
        }
    }

    /// <summary>
    /// Loads the scores from "scores.txt"
    /// </summary>
    /// <returns>A list of integers parsed from the data in "scores.txt"</returns>
    public List<UserRecord> LoadRecords()
    {
        if(File.Exists(filePath))
        {
            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);

            List<UserRecord> records = new List<UserRecord>();
            // parse each top line and add it to the top scores
            foreach(string line in lines)
            {
                string[] data = line.Split(":");
                if (int.TryParse(data[1], out int score))
                {
                    records.Add(new UserRecord(data[0], score));
                }
                else
                {
                    Debug.LogWarning("Failed to parse score: " + line);
                }
            }

            return records;
        }
        else
        {
            Debug.LogWarning("Scores file not found!");
            return new List<UserRecord> { };
        }
    }

    /// <summary>
    /// Saves the current overal score to "scores.txt"
    /// </summary>
    public void SaveScore()
    {
        // Load the saves file
        List<UserRecord> records = LoadRecords();
        // Sort the leaderboard based on score
        records.Sort((record1, record2) => record1.score.CompareTo(record2.score));
        string initials = PlayerData.activePlayers[maxScoringPlayer].initials;
        if (initials == null)
        {
            initials = "AAA";
        }

        // If file contains scores,
        // iterate through the records to compare new score
        // If the score is higher, insert it into the records list
        if(records.Count > 0)
        {
            for(int i = 0; i < records.Count; i++)
            {
                // Check if new score beats a score
                if(maxScore > records[i].score)
                {
                    // Remove score being replaced and insert new score
                    if(records.Count == numTopScores)
                        records.RemoveAt(i);
                    records.Insert(i, new UserRecord(initials, maxScore));
                    break;
                }
                else if(records.Count < numTopScores) // Just add if leaderboard isnt full
                {
                    records.Insert(i, new UserRecord(initials, maxScore));
                    break;
                }
            }
        }
        else // Adding the first record
        {
            records.Add(new UserRecord(initials, maxScore));
        }


        // Get the file directory
        string dirPath = Path.GetDirectoryName(filePath);

        // Create the file if it doesn't exist
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        // Sort the leaderboard based on score
        records.Sort((record1, record2) => record1.score.CompareTo(record2.score));

        // String of scores to add to the file
        string scores = "";
        for(int i = 0; i < numTopScores; i++)
        {
            // Early break if num scores doesnt hit the max
            if (i > records.Count - 1)
                break;

            // Stringify each record
            scores += $"{records[i].initials}:{records[i].score}";

            // If note last recorded score, add new line
            if(i < numTopScores - 1)
            {
                scores += "\n";
            }

            // Write text to the file
            File.WriteAllText(filePath, scores);
        }
    }
}
