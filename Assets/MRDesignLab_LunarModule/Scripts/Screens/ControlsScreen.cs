//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using HUX.Focus;
using HUX.Interaction;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Displays a dialog with a control scheme based on the current device(s)
    /// </summary>
    public class ControlsScreen : GameScreen
    {
        [SerializeField]
        private GameObject tutorialDisplayParent;
        [SerializeField]
        private GameObject tutorialObjectGamepad;
        [SerializeField]
        private GameObject tutorialObjectHololens;
        [SerializeField]
        private GameObject tutorialObjectOasis;
        [SerializeField]
        private GameObject selectControlsMenu;
        [SerializeField]
        private GameObject selectTutorialMenu;

        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);
            // TEMP - show tutorial first, then input selection
            selectTutorialMenu.SetActive(true);

            // TEMP - disable normal flow
            /*if (LanderInput.Instance.MultipleInputSources) {
                selectControlsMenu.SetActive(true);
            } else {
                selectTutorialMenu.SetActive(true);
            }*/
        }

        public override void Deactivate() {
            base.Deactivate();
            tutorialDisplayParent.SetActive(false);
            tutorialObjectGamepad.SetActive(false);
            tutorialObjectHololens.SetActive(false);
            tutorialObjectOasis.SetActive(false);
            selectControlsMenu.SetActive(false);
            selectTutorialMenu.SetActive(false);
        }

        protected override void OnFocusEnter(GameObject obj, FocusArgs args) {
            base.OnFocusEnter(obj, args);

            switch (obj.name) {

                case "GamepadInput":
                    selectControlsMenu.GetComponent<SimpleMenuCollection>().Subtitle = "Use a gamepad to control the lander";
                    break;

                case "HololensInput":
                    selectControlsMenu.GetComponent<SimpleMenuCollection>().Subtitle = "Use fingers to control the lander";
                    break;

                default:
                    // Do nothing
                    break;
            }
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            base.OnTapped(obj, eventArgs);

            if (obj == null)
                return;

            switch (obj.name)
            {
                case "TutorialYes":
                    ShowTutorial();
                    break;

                case "TutorialNo":
                    // TEMP - show control menu
                    selectTutorialMenu.SetActive(false);
                    selectControlsMenu.SetActive(true);

                    // TEMP - disable normal flow
                    //Deactivate();
                    break;

                case "Close":
                    // NOTE - this tapped event will come from a button in a tutorial displays
                    // the 'ControlScreen' object has been added to their 'Targets' arrays
                    Deactivate();
                    break;

                case "GamepadInput":
                    Debug.Log("Selected gamepad!");
                    LanderInput.Instance.InputType = LanderInput.InputTypeEnum.Gamepad;
                    selectControlsMenu.SetActive(false);
                    // TEMP - show a single-screen gamepad tutorial with a 'close' button
                    tutorialDisplayParent.SetActive(true);
                    tutorialObjectGamepad.SetActive(true);

                    // TEMP - disable normal flow
                    //selectTutorialMenu.SetActive(true);
                    break;

                case "HololensInput":
                    LanderInput.Instance.InputType = LanderInput.InputTypeEnum.Hololens;
                    selectControlsMenu.SetActive(false);
                    // TEMP - deactivate screen
                    Deactivate();

                    // TEMP - disable normal flow
                    //selectTutorialMenu.SetActive(true);
                    break;

                default:
                    Debug.LogError("Unknown button choice in " + name + ": " + obj.name);
                    break;
            }
        }

        private void ShowControlChoice () {
            selectControlsMenu.SetActive(true);
        }

        private void ShowTutorial() {
            selectTutorialMenu.SetActive(false);
            tutorialDisplayParent.SetActive(true);

            // TEMP - show hand tutorial
            // after tutorial, show control selection
            // if gamepad is selected, show single-screen gamepad tutorial
            LanderTutorialDisplay display = tutorialObjectHololens.GetComponent<LanderTutorialDisplay>(); ;
            display.OnDeactivate += ShowControlChoice;
            display.Activate();

            // TEMP - disable normal flow
            // Show the appropriate dialog based on our control scheme
            /*LanderTutorialDisplay display = null;
            switch (LanderInput.Instance.InputType) {
                case LanderInput.InputTypeEnum.Gamepad:
                    display = tutorialObjectHololens.GetComponent<LanderTutorialDisplay>();
                    break;

                case LanderInput.InputTypeEnum.Hololens:
                    display = tutorialObjectHololens.GetComponent<LanderTutorialDisplay>();
                    break;

                case LanderInput.InputTypeEnum.Oasis:
                    display = tutorialObjectOasis.GetComponent<LanderTutorialDisplay>();
                    break;
            }
            //display.OnDeactivate += Deactivate;
            display.Activate();*/
        }
    }
}