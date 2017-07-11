//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX;
using HUX.Interaction;
using HUX.Receivers;
using System;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Literally the ugliest thing I've ever written. This class should be burned.
    /// </summary>
    public class LanderTutorialDisplay : InteractionReceiver {
        /// <summary>
        /// How much thrust the player has to apply before moving on
        /// </summary>
        public float MinThrusterAmount = 1f;

        /// <summary>
        /// How much rotation the player has to apply before moving on
        /// </summary>
        public float MinRotationAmount = 1f;

        public Color ArrowColorNormal;
        public Color ArrowColorPressed;

        /// <summary>
        /// How long to hold the step before moving on
        /// </summary>
        public float MinStepLength = 1f;

        public Action OnDeactivate;

        public void Activate() {
            gameObject.SetActive(true);
            page = 0;
            SetPage(page);
            StartCoroutine(DoTutorialOverTime());
        }

        public void NextPage() {
            page++;

            if (page >= pages.Length)
                page = pages.Length - 1;

            progress = 0f;

            SetPage(page);
        }

        public string CurrentPageName {
            get {
                if (page >= 0 && page < pages.Length) {
                    return pages[page].name;
                }
                return string.Empty;
            }
        }

        public Vector3 LanderPosition {
            get {
                return landerPosition.position;
            }
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs) {
            base.OnTapped(obj, eventArgs);

            if (obj == null)
                return;

            switch (obj.name) {
                case "NextPage":
                    NextPage();
                    break;

                case "StartOver":
                    reachedEnd = true;
                    restart = true;
                    break;

                case "Close":
                    reachedEnd = true;
                    restart = false;
                    break;
            }
        }

        private void SetPage(int newPage) {
            page = newPage;
            for (int i = 0; i < pages.Length; i++) {
                pages[i].SetActive(i == page);
            }
            lastPageChangeTime = Time.time;

            //TODO refactor this, unbearably ugly
            switch (CurrentPageName) {
                case "TapAndDragForward":
                case "ControlRotationDone":
                    LanderInput.Instance.ResetInput();
                    break;

                default:
                    break;
            }
        } 

        private void Deactivate() {
            gameObject.SetActive(false);
            page = 0;
            SetPage(page);

            if (OnDeactivate != null)
                OnDeactivate();
        }

        #if UNITY_EDITOR
        protected override void OnDrawGizmos() {
            if (!Application.isPlaying) {
                SetPage(page);
            }
        }
        #endif

        private IEnumerator DoTutorialOverTime() {
            // Wait in case another tutorial is finishing out
            while (tutorialInProgress)
                yield return null;

            tutorialInProgress = true;
            // Start the lander gameplay tutorial so it thinks it's 'playing'
            LanderGameplay.Instance.TutorialStart(gameObject);
            // Wait for player to perform first action
            reachedEnd = false;

            // Wait for lander to set itself up
            // Then do a one-time rotation towards the player
            yield return null;
            Vector3 forward = Veil.Instance.HeadTransform.forward;
            forward.y = 0f;
            LanderInput.Instance.SetForward(-forward.normalized);

            float progressThisFrame = 0f;

            while (!reachedEnd) {

                LanderPhysics.Instance.LanderPosition = landerPosition.position;
                LanderEffects.Instance.ForceThrust = false;
                LanderEffects.Instance.ForceTorque = false;
                LanderPhysics.Instance.DemoRotation = Vector3.zero;
                LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.Hidden;
                LanderGameplay.Instance.Fuel = 1;

                // Check which page we're on
                switch (CurrentPageName) {
                    default:
                        break;

                    case "ThrustAndPropulsion":
                        progressBar.SetActive(false);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;
                        LanderEffects.Instance.ForceTorque = true;
                        LanderEffects.Instance.ForceThrust = true;
                        break;

                    case "HandTracking":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Both;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Both;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.Ready;
                        handCoach.LeftGesture = HandCoach.HandGestureEnum.Ready;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.None;
                        handCoach.LeftDirection = HandCoach.HandDirectionEnum.None;
                        if (handCoach.Tracking == HandCoach.HandVisibilityEnum.Both) {
                            progress += Time.deltaTime * 4;
                        }
                        if (progress >= MinStepLength) {
                            NextPage();
                        }
                        break;

                    case "ControlRotationDemo":
                        progressBar.SetActive(false);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.Both;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.MovementScale = 0.5f;
                        if (Time.time > demoDirectionTime + 1f) {
                            demoDirectionTime = Time.time;
                            demoDirectionIndex++;
                            if (demoDirectionIndex >= demoDirections.Length) {
                                demoDirectionIndex = 0;
                            }
                        }
                        handCoach.RightDirection = demoDirections[demoDirectionIndex];
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHold;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Static;
                        LanderEffects.Instance.ForceTorque = false;
                        break;

                    case "ControlRotationGyro":
                        progressBar.SetActive(false);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.None;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHold;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Static;
                        float xRotation = Mathf.Sin(Time.time * 1.5f) * 0.25f;
                        LanderPhysics.Instance.DemoRotation = new Vector3(0f, 0f, xRotation);
                        LanderEffects.Instance.ShowGyro = true;
                        LanderEffects.Instance.ForceTorque = true;
                        break;

                    case "TapAndDragForward":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        LanderEffects.Instance.ShowGyro = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Front;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Clamp01(LanderInput.Instance.FrameRotation.x) / MinRotationAmount);
                        directionalArrows[0].material.color = Color.Lerp (directionalArrows[0].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed: ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "TapAndDragBackward":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Back;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Abs(Mathf.Clamp(LanderInput.Instance.FrameRotation.x, -1f, 0f)) / MinRotationAmount);
                        directionalArrows[1].material.color = Color.Lerp(directionalArrows[1].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed : ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "TapAndDragUp":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Up;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Clamp01(LanderInput.Instance.FrameRotation.y) / MinRotationAmount);
                        directionalArrows[2].material.color = Color.Lerp(directionalArrows[2].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed : ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "TapAndDragDown":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Down;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Abs(Mathf.Clamp(LanderInput.Instance.FrameRotation.y, -1, 0f)) / MinRotationAmount);
                        directionalArrows[3].material.color = Color.Lerp(directionalArrows[3].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed : ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "TapAndDragLeft":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Left;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Clamp01(LanderInput.Instance.FrameRotation.z) / MinRotationAmount);
                        directionalArrows[4].material.color = Color.Lerp(directionalArrows[4].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed : ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "TapAndDragRight":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Right;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Right;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.Right;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.TapHoldRelease;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Directional;
                        progressThisFrame = (Mathf.Abs(Mathf.Clamp(LanderInput.Instance.FrameRotation.z, -1f, 0f)) / MinRotationAmount);
                        directionalArrows[5].material.color = Color.Lerp(directionalArrows[5].material.color, (progressThisFrame > 0.01f) ? ArrowColorPressed : ArrowColorNormal, Time.deltaTime * 5);
                        progress += progressThisFrame;
                        if (progress > MinStepLength)
                        {
                            NextPage();
                        }
                        break;

                    case "ControlRotationDone":
                        progressBar.SetActive(false);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.None;
                        handCoach.RightGesture = HandCoach.HandGestureEnum.None;
                        handCoach.RightMovement = HandCoach.HandMovementEnum.Static;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.None;
                        LanderEffects.Instance.ForceTorque = true;
                        break;

                    case "ControlPropulsionDemo":
                        progressBar.SetActive(false);
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Left;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Left;
                        handCoach.LeftGesture = HandCoach.HandGestureEnum.TapHold;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.None;
                        handCoach.LeftDirection = HandCoach.HandDirectionEnum.Up;
                        handCoach.LeftMovement = HandCoach.HandMovementEnum.PingPong;
                        LanderInput.Instance.TargetThrust = handCoach.LeftMovementLoopTime;
                        LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.ForceVisible;
                        //LanderEffects.Instance.ForceThrust = true;
                        break;

                    case "ControlPropulsionTry":
                        progressBar.SetActive(true);
                        LanderInput.Instance.ApplyInput = true;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;// handCoach.InvertTracked;
                        handCoach.Ghosting = HandCoach.HandVisibilityEnum.Both;
                        handCoach.CheckTracking = HandCoach.HandVisibilityEnum.Left;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.Left;
                        handCoach.LeftGesture = HandCoach.HandGestureEnum.TapHold;
                        handCoach.RightDirection = HandCoach.HandDirectionEnum.None;
                        handCoach.LeftDirection = HandCoach.HandDirectionEnum.Up;
                        handCoach.LeftMovement = HandCoach.HandMovementEnum.PingPong;
                        // Wait for player to press thrust for min
                        LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.ForceVisible;
                        progress += (LanderInput.Instance.TargetThrust / MinThrusterAmount);
                        if (progress > MinStepLength) {
                            NextPage();
                        }
                        break;

                    case "FuelDemo":
                        progressBar.SetActive(false);
                        LanderEffects.Instance.ShowGyro = true;
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.None;
                        float normalizedFuel = Mathf.Clamp01 (Mathf.Sin(Time.time) + 0.5f);
                        LanderInput.Instance.TargetThrust = (normalizedFuel > 0) ? 0.5f : 0f;
                        LanderGameplay.Instance.Fuel = Mathf.FloorToInt (normalizedFuel * LanderGameplay.Instance.Settings.FuelOnStartup);
                        break;

                    case "ControlThrustDone":
                        progressBar.SetActive(false);
                        LanderEffects.Instance.ShowGyro = false;
                        LanderInput.Instance.ApplyInput = false;
                        handCoach.Visibility = HandCoach.HandVisibilityEnum.None;
                        handCoach.Highlight = HandCoach.HandVisibilityEnum.None;
                        break;
                }

                progressBarTransform.localScale = new Vector3(progress / MinStepLength, 1f, 1f);
                yield return null;
            }

            // If we're not restarting
            if (!restart) {
                Deactivate();
            } else {
                Activate();
            }

            tutorialInProgress = false;
            yield break;
        }

        [SerializeField]
        private Transform landerPosition;

        [SerializeField]
        private GameObject[] pages;

        [SerializeField]
        private HandCoach handCoach;

        [SerializeField]
        private Transform progressBarTransform;

        [SerializeField]
        private GameObject progressBar;

        [SerializeField]
        private HandCoach.HandDirectionEnum[] demoDirections;

        [SerializeField]
        [Range(0,15)]
        private int page;

        [SerializeField]
        private Renderer[] directionalArrows;
        
        private float demoDirectionTime;
        private int demoDirectionIndex;
        private Vector3 handDirection = Vector3.zero;
        private float progress = 0f;
        private float lastPageChangeTime = 0f;
        private bool tutorialInProgress = false;
        private bool reachedEnd = false;
        private bool restart = false;
    }
}