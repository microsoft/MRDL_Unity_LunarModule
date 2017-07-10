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
    public class GameplayScreen : GameScreen
    {
        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);
            startupTargetObject = new GameObject("Startup target object");
            StartCoroutine(ShowGameplayFeedback());
        }

        public override void Deactivate() {
            base.Deactivate();
            if (startupTargetObject != null) {
                GameObject.Destroy(startupTargetObject);
            }
        }

        protected IEnumerator ShowGameplayFeedback () {

            // Point towards the place where the lander will arrive
            startupTargetObject.transform.position = LandingPadManager.Instance.LanderStartupPosition;
            directionIndicator.TargetObject = startupTargetObject;

            // Start the lander
            LanderGameplay.Instance.GameStart();

            // Wait for the lander gameplay to start
            while (!LanderGameplay.Instance.GameInProgress) {
                yield return null;
            }

            // Now point towards the lander itself
            directionIndicator.TargetObject = LanderPhysics.Instance.gameObject;

            // Wait for the lander gameplay to finish
            while (LanderGameplay.Instance.GameInProgress) {
                yield return null;
            }

            Deactivate();
            yield break;
        }

        private GameObject startupTargetObject;
    }
}
