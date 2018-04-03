//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HUX.Interaction;
using MixedRealityToolkit.InputModule.InputHandlers;
using MixedRealityToolkit.InputModule.EventData;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    public class PlacementScreen : GameScreen, IInputHandler
    {
        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);
            // TODO turn on all the bells & whistles etc
            LandingPadManager.Instance.PlaceLandingAndStartupPads();
            // Provide feedback for placement
            StartCoroutine(ShowPlacementFeedback());
        }

        public override void Deactivate() {
            base.Deactivate();
            confirmPlacementParent.SetActive(false);
        }

        public void OnInputDown(InputEventData eventData) { }
        public void OnInputUp(InputEventData eventData)
        {
            if (eventData.selectedObject != null)
            {
                // Let landing pad placement know that we've chosen our landing pad spot
                if (!LandingPadManager.Instance.LandingPlacementConfirmed)
                {
                    LandingPadManager.Instance.ConfirmLandingPadPlacement();
                }
                else
                {
                    LandingPadManager.Instance.ConfirmStartupPadPlacement();
                }
            }
        }

        //**** Replacing OnTapped with IInputHandler
        //protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        //{
        //    base.OnTapped(obj, eventArgs);

        //    switch (obj.name)
        //    {
        //        case "ConfirmPlacement":
        //            // Let landing pad placement know that we've chosen our landing pad spot
        //            if (!LandingPadManager.Instance.LandingPlacementConfirmed) {
        //                LandingPadManager.Instance.ConfirmLandingPadPlacement();
        //            } else {
        //                LandingPadManager.Instance.ConfirmStartupPadPlacement();
        //            }
        //            break;

        //        default:
        //            Debug.LogError("Unknown button choice in " + name + ": " + obj.name);
        //            break;
        //    }
        //}

        protected IEnumerator ShowPlacementFeedback () {
            landingPadIntructions.SetActive(true);
            startupPadIntructions.SetActive(false);
            confirmPlacementParent.SetActive(true);
            while (!LandingPadManager.Instance.LandingPlacementConfirmed) {
                if (LandingPadManager.Instance.LandingPlacementValid) {
                    confirmPlacementButton.SetActive(true);
                } else {
                    confirmPlacementButton.SetActive(false);
                }
                yield return null;
            }

            confirmPlacementParent.SetActive(true);
            landingPadIntructions.SetActive(false);
            startupPadIntructions.SetActive(true);
            while (!LandingPadManager.Instance.StartupPlacementConfirmed) {
                if (LandingPadManager.Instance.StartupPlacementValid) {
                    confirmPlacementButton.SetActive(true);
                } else {
                    confirmPlacementButton.SetActive(false);
                }
                yield return null;
            }
            Deactivate();
            yield break;
        }

        [SerializeField]
        private GameObject landingPadIntructions;
        [SerializeField]
        private GameObject startupPadIntructions;
        [SerializeField]
        private GameObject confirmPlacementParent;
        [SerializeField]
        private GameObject confirmPlacementButton;
    }
}
