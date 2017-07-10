//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Stores and retrieves high scores
    /// Currently stores in PlayerPrefs, but could be modified to use a leaderboard
    /// </summary>
    public class HighScoreManager : Singleton<HighScoreManager>
    {
        public const int MaxHighScores = 10;
        public const int MaxInitials = 3;

        /// <summary>
        /// Struct for keeping track of high scores in a player-prefs-friendly format
        /// </summary>
        [Serializable]
        public struct HighScore {
            public static HighScore Empty {
                get {
                    return new HighScore(-1, string.Empty, -1);
                }
            }
            public HighScore (int boardIndex, string initials, int score) {
                BoardIndex = boardIndex;
                Initials = initials;
                Score = score;
            }
            public int BoardIndex;
            public string Initials;
            public int Score;
            public bool IsEmpty {
                get {
                    return string.IsNullOrEmpty(Initials);
                }
            }
        }

        /// <summary>
        /// Returns a high score for an index on the leaderboard
        /// </summary>
        /// <param name="boardIndex"></param>
        /// <returns></returns>
        public HighScore GetHighScore (int boardIndex) {
            if (boardIndex < 0 || boardIndex >= MaxHighScores) {
                return HighScore.Empty;
            }
            // TEMP testing
            return new HighScore(boardIndex, "AAA", UnityEngine.Random.Range(0, 50000));
            //return highScores[boardIndex];
        }

        /// <summary>
        /// Sets a high score and saves the data to player prefs
        /// </summary>
        /// <param name="initials"></param>
        /// <param name="score"></param>
        public void SetHighScore (string initials, int score) {
            scoresModified = true;
            List<HighScore> scoresList = new List<HighScore>(highScores);
            scoresList.Add(new HighScore(0, initials, score));
            // Sort by score
            scoresList.Sort(
                delegate (HighScore s1, HighScore s2) {
                    if (s1.IsEmpty && s2.IsEmpty) {
                        return 0;
                    } else {
                        if (s1.IsEmpty) {
                            return 1;
                        } else if (s2.IsEmpty) {
                            return -1;
                        } else {
                            return s1.Score.CompareTo(s2.Score);
                        }
                    }
                });
            // Truncate to max length
            while (scoresList.Count > MaxHighScores) {
                scoresList.RemoveAt(scoresList.Count - 1);
            }
            highScores = scoresList.ToArray();
            // Set the index
            for (int i = 0; i < highScores.Length; i++) {
                highScores[i].BoardIndex = i;
            }

            SaveHighScores();
        }

        /// <summary>
        /// Whether a score is high enough to appear on the leaderboard
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool IsHighScore (int score) {
            bool hasOneHighScore = false;
            for (int i = 0; i < highScores.Length; i++) {
                if (!highScores[i].IsEmpty) {
                    if (score > highScores[i].Score) {
                        return true;
                    }
                    hasOneHighScore = true;
                }
            }

            if (!hasOneHighScore) {
                return true;
            }

            return false;
        }

        private void OnEnable() {
            LoadHighScores();
        }

        private void OnDisable() {
            if (scoresModified) {
                SaveHighScores();
            }
        }

        private void SaveHighScores () {
            string scoreKey = string.Empty;
            string initialsKey = string.Empty;
            for (int i = 0; i < highScores.Length; i++) {
                scoreKey = GetScoreKey(i);
                initialsKey = GetInitialsKey(i);
                PlayerPrefs.SetInt(scoreKey,  highScores[i].Score);
                PlayerPrefs.SetString(initialsKey, highScores[i].Initials);
            }
        }

        private void LoadHighScores () {
            string scoreKey = string.Empty;
            string initialsKey = string.Empty;
            int score = 0;
            string initials = string.Empty;
            for (int i = 0; i < MaxHighScores; i++) {
                highScores[i] = HighScore.Empty;
                highScores[i].BoardIndex = i;
                // Get player prefs keys for each index
                scoreKey = GetScoreKey(i);
                initialsKey = GetInitialsKey(i);
                // If the key exists, load the score
                if (PlayerPrefs.HasKey (scoreKey)) {
                    highScores[i].Score = PlayerPrefs.GetInt(scoreKey);
                    highScores[i].Initials = PlayerPrefs.GetString(initialsKey);
                }
            }
        }

        private string GetScoreKey (int boardIndex) {
            return ("LUNAR_MODULE_SCORE_" + boardIndex);
        }

        private string GetInitialsKey (int boardIndex) {
            return ("LUNAR_MODULE_INITIALS_" + boardIndex);
        }

        private HighScore[] highScores = new HighScore[MaxHighScores];
        private bool scoresModified = false;
    }
}