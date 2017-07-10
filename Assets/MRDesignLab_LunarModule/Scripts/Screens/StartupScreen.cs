//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using HUX.Focus;
using HUX.Interaction;
using HUX.Receivers;
using UnityEngine;

namespace MRDL
{
    public class StartupScreen : GameScreen
    {

        [SerializeField]
        private GameObject startupScreenMenuParent;

        [SerializeField]
        private GameObject startupMenu;

        [SerializeField]
        private GameObject difficultyMenu;

        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);
            startupScreenMenuParent.gameObject.SetActive(true);
            startupMenu.SetActive(true);
        }

        public override void Deactivate() {
            base.Deactivate();
            startupScreenMenuParent.SetActive(false);
            startupMenu.SetActive(false);
            difficultyMenu.SetActive(false);
        }

        protected override void OnFocusEnter(GameObject obj, FocusArgs args) {
            base.OnFocusEnter(obj, args);

            switch (obj.name) {
                case "Easy":
                    difficultyMenu.GetComponent<SimpleMenuCollection>().Subtitle = "Lots of fuel and a forgiving landing speed.";
                    break;

                case "Medium":
                    difficultyMenu.GetComponent<SimpleMenuCollection>().Subtitle = "Moderate fuel and a challenging landing speed.";
                    break;

                case "Hard":
                    difficultyMenu.GetComponent<SimpleMenuCollection>().Subtitle = "Very little fuel and no room for error!";
                    break;
            }
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            base.OnTapped(obj, eventArgs);

            switch (obj.name)
            {
                case "Start":
                    Result = ProgramStateEnum.ChooseRoom;
                    Deactivate();
                    break;

                case "Difficulty":
                    // switch to difficulty menu
                    startupMenu.SetActive(false);
                    difficultyMenu.SetActive(true);
                    break;

                case "Quit":
                    Result = ProgramStateEnum.Quitting;
                    Deactivate();
                    Application.Quit();
                    break;

                case "Easy":
                    // switch back to main menu
                    startupMenu.SetActive(true);
                    difficultyMenu.SetActive(false);
                    LanderGameplay.Instance.Difficulty = LanderGameplay.DifficultyEnum.Easy;
                    break;

                case "Medium":
                    // switch back to main menu
                    startupMenu.SetActive(true);
                    difficultyMenu.SetActive(false);
                    LanderGameplay.Instance.Difficulty = LanderGameplay.DifficultyEnum.Medium;
                    break;

                case "Hard":
                    // switch back to main menu
                    startupMenu.SetActive(true);
                    difficultyMenu.SetActive(false);
                    LanderGameplay.Instance.Difficulty = LanderGameplay.DifficultyEnum.Hard;
                    break;

                default:
                    Debug.LogError("Unknown button choice in " + name + ": " + obj.name);
                    break;
            }
        }
    }
}