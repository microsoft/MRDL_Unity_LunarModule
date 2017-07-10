//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MRDL
{
    public class HighScoreDisplay : MonoBehaviour
    {
        private const string EmptyInitials = "___";
        private const string EmptyScore = "0";
        private const string SpacerCharacter = ".";
        private const int NumScoreCharacters = 15;

        [SerializeField]
        private GameObject scoreObjectPrefab;

        [SerializeField]
        private ObjectCollection scoreCollection;

        private void OnEnable() {
            CreateHighScores();
            scoreCollection.UpdateCollection();
        }

        private void CreateHighScores() {
            
            if (instantiatedHighScores == null) {
                // Instantiate all the high score objects
                instantiatedHighScores = new GameObject[HighScoreManager.MaxHighScores];
                for (int i = 0; i < HighScoreManager.MaxHighScores; i++) {
                    instantiatedHighScores [i] = GameObject.Instantiate(scoreObjectPrefab, scoreCollection.transform);
                }
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < HighScoreManager.MaxHighScores; i++) {
                HighScoreManager.HighScore highScore = HighScoreManager.Instance.GetHighScore(i);
                TextMesh textMesh = instantiatedHighScores[i].GetComponent<TextMesh>();
                string scoreString = EmptyScore;
                string initials = EmptyInitials;
                if (!highScore.IsEmpty) {
                    scoreString = highScore.Score.ToString();
                    initials = highScore.Initials;
                }
                stringBuilder.Append(initials);
                int numScoreChars = NumScoreCharacters - initials.Length - scoreString.Length;
                for (int j = 0; j < numScoreChars; j++) {
                    stringBuilder.Append(SpacerCharacter);
                }
                stringBuilder.Append(scoreString);
                textMesh.text = stringBuilder.ToString();
                // Reset the builder (.NET 2.0)
                stringBuilder.Length = 0;
                stringBuilder.Capacity = 0;
            }
        }

        private GameObject[] instantiatedHighScores;
    }
}
