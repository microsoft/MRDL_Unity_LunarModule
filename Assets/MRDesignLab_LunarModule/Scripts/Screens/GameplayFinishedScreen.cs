//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HoloToolkit.UI.Keyboard;
using HUX.Dialogs;
using HUX.Interaction;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    public class GameplayFinishedScreen : GameScreen
    {
        public string[] SuccessPhrases;
        public string[] FailurePhrases;

        public int MaxCharsPerLine = 50;

        [SerializeField]
        private GameObject gameplayFinishedMenu;

        [SerializeField]
        private GameObject highScoreInputField;

        [SerializeField]
        private GameObject highScoreDisplay;

        [SerializeField]
        private GameObject highScoreDisplayParent;
        
        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);
            StartCoroutine(ShowGameplayFeedback());
        }

        public override void Deactivate() {
            base.Deactivate();
            highScoreDisplayParent.SetActive(false);
            gameplayFinishedMenu.SetActive(false);
            highScoreInputField.SetActive(false);
            highScoreDisplay.SetActive(false);
            // Hide the landing pad
            LandingPadManager.Instance.HideLandingPad();
            LanderEffects.Instance.HideLander();
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            base.OnTapped(obj, eventArgs);

            switch (obj.name)
            {
                case "TryAgain":
                    gameplayFinishedMenu.gameObject.SetActive(false);
                    Result = ProgramStateEnum.ChooseRoom;
                    Deactivate();
                    break;

                case "MainMenu":
                    gameplayFinishedMenu.gameObject.SetActive(false);
                    Result = ProgramStateEnum.StartupScreen;
                    Deactivate();
                    break;

                default:
                    Debug.LogError("Unknown button choice in " + name + ": " + obj.name);
                    break;
            }
        }

        protected IEnumerator ShowGameplayFeedback () {

            string userName = "Player";
            string result = string.Empty;           
            if (!LanderGameplay.Instance.HasCrashed) {
                // Landed successfully
                result = string.Format (SuccessPhrases[Random.Range(0, SuccessPhrases.Length)], userName);
            } else {
                // Failed
                result = string.Format(FailurePhrases[Random.Range(0, FailurePhrases.Length)], userName);
            }
            SimpleMenuCollection menu = this.gameplayFinishedMenu.GetComponent<SimpleMenuCollection>();
            menu.Title = SimpleDialogShell.WordWrap(result, MaxCharsPerLine);
            menu.Subtitle = "Your score: " + LanderGameplay.Instance.Score.ToString();

            // Set this true regardless
            highScoreDisplayParent.SetActive(true);
            // TEMP disable high score
            //highScoreDisplay.SetActive(true);

            // If we got a high score, show the leaderboard and wait for input
            // TEMP disable high score
            /*if (HighScoreManager.Instance.IsHighScore (LanderGameplay.Instance.Score)) {
                highScoreInputField.gameObject.SetActive(true);
                directionIndicator.TargetObject = highScoreInputField.gameObject;
                // Set the text to the last high score input
                Keyboard.Instance.m_InputField.text = highScoreText;
                Keyboard.Instance.onTextUpdated += OnTextUpdated;
                // TODO set focuser based on control scheme
                Keyboard.Instance.PresentKeyboard(HUX.Focus.FocusManager.Instance.GazeFocuser);
            }*/

            // Show the try again menu
            gameplayFinishedMenu.SetActive(true);
            directionIndicator.TargetObject = gameplayFinishedMenu.gameObject;

            yield break;
        }

        private void OnTextUpdated (string text) {
            highScoreText = text;
            if (highScoreText.Length >= HighScoreManager.MaxInitials) {
                Keyboard.Instance.onTextUpdated -= OnTextUpdated;
                Keyboard.Instance.Close();
            }
        }

        private string highScoreText;
    }
}
