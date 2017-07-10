//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Focus;
using HUX.Interaction;
using UnityEngine;

namespace HUX.Utility
{
    /// <summary>
    /// Tracks hand position relative to head
    /// Useful for joystick, trackball and throttle-style controls
    /// </summary>
    public class LocalHandInput : InteractibleObject
    {
        public InputSourceHands.HandednessEnum Handedness = InputSourceHands.HandednessEnum.Right;

        public float MinConfidence = 0.5f;

        public Vector3 LocalPosition {
            get {
                return finalLocalPosition;
            }
        }

        public Vector3 LocalVelocity {
            get {
                return finalLocalVelocity;
            }
        }

        /// <summary>
        /// If true, won't update unless FocusManager is targeting this object
        /// </summary>
        public bool RequireFocus;

        public bool HasFocus {
            get {
                if (!RequireFocus)
                    return true;

                return hasFocus;
            }
        }

        /// <summary>
        /// True if active and the player has a finger down
        /// </summary>
        public bool Pressed {
            get {
                return pressed;
            }
        }

        /// <summary>
        /// If true, motion below the range defined in DeadZone will be ignored
        /// </summary>
        public bool UseDeadZones = true;

        /// <summary>
        /// Motion range to ignore for x,y,z velocity
        /// </summary>
        public Vector3 DeadZone = Vector3.one * 0.1f;

        /// <summary>
        /// Clamps local position to -1,1
        /// </summary>
        public bool ClampPosition = true;

        /// <summary>
        /// Clamps local velocity to -1, 1
        /// </summary>
        public bool ClampVelocity = true;

        /// <summary>
        /// Applies mouse-style acceleration to input
        /// </summary>
        public bool AcceleratePosition = true;
        public bool AccelerateVelocity = true;

        /// <summary>
        /// Curve for accelerating input
        /// </summary>
        public AnimationCurve PositionAccelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 2f);
        public AnimationCurve VelocityAccelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 2f);

        /// <summary>
        /// If true, won't reset values until the next press
        /// </summary>
        public bool KeepValuesOnRelease = false;

        /// <summary>
        /// Time for smooth damp local position and velocity
        /// </summary>
        [Range(0.01f, 5f)]
        public float SmoothTime = 0.15f;

        /// <summary>
        /// How much to exaggerate hand motion
        /// </summary>
        [Range(0.1f, 10f)]
        public float InputScale = 4f;

        /// <summary>
        /// FocusManager SendMessage("FocusEnter") receiver.
        /// </summary>
        public void FocusEnter(FocusArgs args) {
            hasFocus = true;
        }

        /// <summary>
        /// FocusManager SendMessage("FocusExit") receiver.
        /// </summary>
        public void FocusExit(FocusArgs args) {
            hasFocus = false;
        }

        protected void OnEnable() {
            // Make sure curve wraps correctly
            PositionAccelerationCurve.postWrapMode = WrapMode.ClampForever;
            VelocityAccelerationCurve.postWrapMode = WrapMode.ClampForever;
        }

        protected void Update() {
            FindOrCreateTransformHelper();
            handPosThisFrame = Vector3.zero;

            if (RequireFocus && !hasFocus) {
                // Reset
                pressed = false;
                if (!KeepValuesOnRelease) {
                    localPosition = Vector3.zero;
                    localVelocity = Vector3.zero;
                }
            } else {
                // If we can see the hand...
                InputSourceHands.CurrentHandState handState = InputSources.Instance.hands.GetHandState(Handedness, MinConfidence);
                if (handState != null) {
                    // Check to see if the user is pressing
                    if (handState.Pressed) {
                        handPosThisFrame = handState.Position;
                        // If we're not pressing already...
                        if (!pressed) {
                            pressed = true;

                            // Zero everything out so we're starting from scratch
                            localPosition = Vector3.zero;
                            localVelocity = Vector3.zero;
                            smoothLocalPosition = Vector3.zero;
                            smoothLocalVelocity = Vector3.zero;
                            currentPositionVel = Vector3.zero;
                            currentVelocityVel = Vector3.zero;
                            finalLocalPosition = Vector3.zero;
                            finalLocalVelocity = Vector3.zero;                            
                            localPositionLastFrame = Vector3.zero;

                            // Store these for later
                            worldHeadPosOnPressed = Veil.Instance.HeadTransform.position;
                            worldHeadRotOnPressed = Veil.Instance.HeadTransform.eulerAngles;
                            worldHandPosOnPressed = handPosThisFrame;

                            transformHelper.eulerAngles = worldHandPosOnPressed;

                            timePressed = Time.time;
                        }
                    } else {
                        pressed = false;
                        if (!KeepValuesOnRelease) {
                            localPosition = Vector3.zero;
                            localVelocity = Vector3.zero;
                        }
                    }
                } else {
                    pressed = false;
                    if (!KeepValuesOnRelease) {
                        localPosition = Vector3.zero;
                        localVelocity = Vector3.zero;
                    }
                }
            }

            if (pressed) {
                // Get the current local position / velocity of the hand relative to our starting point
                transformHelper.position = Veil.Instance.HeadTransform.position;
                transformHelper.rotation = Veil.Instance.HeadTransform.rotation;

                Vector3 localHandPosNow = transformHelper.InverseTransformPoint(handPosThisFrame);
                transformHelper.rotation = Quaternion.Euler(worldHeadRotOnPressed);
                Vector3 localHandPosOnPressed = transformHelper.InverseTransformPoint(worldHandPosOnPressed);
                Vector3 localHeadPosNow = Vector3.zero;// transformHelper.InverseTransformPoint(Veil.Instance.HeadTransform.position);
                Vector3 localHeadPosOnPressed = transformHelper.InverseTransformPoint(worldHeadPosOnPressed);

                Vector3 newLocalPosition = ((localHandPosNow - localHandPosOnPressed) - (localHeadPosNow - localHeadPosOnPressed));
                Vector3 newLocalVelocity = newLocalPosition - localPositionLastFrame;

                if (UseDeadZones && Time.time > timePressed + deadZoneInterval) {

                    if (Mathf.Abs(newLocalVelocity.x) > DeadZone.x / InputScale) {
                        localPosition.x = newLocalPosition.x;
                        localVelocity.x = newLocalVelocity.x;
                    } else {
                        localPosition.x = localPositionLastFrame.x;
                        localVelocity.x = 0;
                    }

                    if (Mathf.Abs(newLocalVelocity.y) > DeadZone.y / InputScale) {
                        localPosition.y = newLocalPosition.y;
                        localVelocity.y = newLocalVelocity.y;
                    } else {
                        localPosition.y = localPositionLastFrame.y;
                        localVelocity.y = 0;
                    }

                    if (Mathf.Abs(newLocalVelocity.z) > DeadZone.z / InputScale) {
                        localPosition.z = newLocalPosition.z;
                        localVelocity.z = newLocalVelocity.z;
                    } else {
                        localPosition.z = localPositionLastFrame.z;
                        localVelocity.z = 0;
                    }

                } else {
                    localVelocity = newLocalVelocity;
                    localPosition = newLocalPosition;
                }

                localPositionLastFrame = localPosition;
            }

            // Smooth
            smoothLocalPosition = Vector3.SmoothDamp(smoothLocalPosition, localPosition, ref currentPositionVel, SmoothTime);
            smoothLocalVelocity = Vector3.SmoothDamp(smoothLocalVelocity, localVelocity, ref currentVelocityVel, SmoothTime);

            // Scale
            finalLocalPosition = smoothLocalPosition * InputScale;
            finalLocalVelocity = smoothLocalVelocity * InputScale;

            // Accelerate
            if (AcceleratePosition) {
                finalLocalPosition.x *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalPosition.x));
                finalLocalPosition.y *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalPosition.y));
                finalLocalPosition.z *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalPosition.z));
            }

            if (AccelerateVelocity) {
                finalLocalVelocity.x *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalVelocity.x));
                finalLocalVelocity.y *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalVelocity.y));
                finalLocalVelocity.z *= PositionAccelerationCurve.Evaluate(Mathf.Abs(finalLocalVelocity.z));
            }

            // Clamp
            if (ClampPosition) {
                finalLocalPosition.x = Mathf.Clamp(finalLocalPosition.x, -1f, 1f);
                finalLocalPosition.y = Mathf.Clamp(finalLocalPosition.y, -1f, 1f);
                finalLocalPosition.z = Mathf.Clamp(finalLocalPosition.z, -1f, 1f);
            }

            if (ClampVelocity) {
                finalLocalVelocity.x = Mathf.Clamp(finalLocalVelocity.x, -1f, 1f);
                finalLocalVelocity.y = Mathf.Clamp(finalLocalVelocity.y, -1f, 1f);
                finalLocalVelocity.z = Mathf.Clamp(finalLocalVelocity.z, -1f, 1f);
            }
        }

        protected void OnDisable() {
            if (transformHelper != null) {
                GameObject.Destroy(transformHelper.gameObject);
                transformHelper = null;
            }
            hasFocus = false;
        }

        private void FindOrCreateTransformHelper() {
            if (transformHelper == null) {
                transformHelper = new GameObject("Transform Helper").transform;
            }
        }

        // Raw values
        private Vector3 localPosition;
        private Vector3 localVelocity;
        private Vector3 handPosThisFrame;

        // Smoothed values
        private Vector3 smoothLocalVelocity;
        private Vector3 smoothLocalPosition;
        private Vector3 currentPositionVel;
        private Vector3 currentVelocityVel;

        // Scaled / clamped values
        private Vector3 finalLocalVelocity;
        private Vector3 finalLocalPosition;

        // Offset / transform helper values
        private Vector3 localPositionLastFrame;
        private Vector3 worldHeadPosOnPressed;
        private Vector3 worldHandPosOnPressed;
        private Vector3 worldHeadRotOnPressed;

        private bool pressed = false;
        private bool hasFocus = false;
        private float timePressed = 0f;
        private float deadZoneInterval = 0.5f;

        private static Transform transformHelper;
    }
}