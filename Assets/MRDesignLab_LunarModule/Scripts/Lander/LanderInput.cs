//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX;
using HUX.Utility;
using UnityEngine;

namespace MRDL
{
    public class LanderInput : Singleton<LanderInput>
    {
        public enum JoystickAxisEnum
        {
            LeftStickX,
            LeftStickY,
            RightStickX,
            RightStickY,
            LeftRightStickX,
            LeftRightStickY
        }

        [Header ("Input mapping")]
        public JoystickAxisEnum GamepadYAxis;
        public JoystickAxisEnum GamepadXAxis;
        public JoystickAxisEnum GamepadZAxis;


        public bool GamepadYAxisInvert = false;
        public bool GamepadXAxisInvert = false;
        public bool GamepadZAxisInvert = false;

        [HideInInspector]
        public KeyCode EditorHorzAdd;
        [HideInInspector]
        public KeyCode EditorHorzSub;
        [HideInInspector]
        public KeyCode EditorVertAdd;
        [HideInInspector]
        public KeyCode EditorVertSub;
        [HideInInspector]
        public KeyCode EditorUpAdd;
        [HideInInspector]
        public KeyCode EditorUpSub;
        [HideInInspector]
        public KeyCode EditorThrustAdd;
        [HideInInspector]
        public KeyCode EditorThrustSub;
        [HideInInspector]

        public Vector3 FrameRotation {
            get {
                return frameRotation;
            }
        }

        public enum InputTypeEnum
        {
            Gamepad,
            Hololens,
            Oasis,
        }

        public enum ThrottleVisibilityEnum
        {
            Hidden,
            Normal,
            ForceVisible,
        }

        public InputTypeEnum InputType {
            get {
                return inputType;
            }
            set {
                inputType = value;
            }
        }

        [Header("Gameplay settings")]
        public bool ApplyInput = true;

        public ThrottleVisibilityEnum ThrottleVisibility = ThrottleVisibilityEnum.Normal;

        public bool MultipleInputSources {
            get {
                return multipleInputSources;
            }
        }

        public Quaternion TargetRotation {
            get {
                return targetRotation;
            }
        }

        public bool InputSourceLost {
            get {
                return inputSourceLost;
            }
        }

        public float TargetThrust {
            get {
                return ThrusterInputCurve.Evaluate(targetThrust);
            }
            set {
                targetThrust = value;
            }
        }

        public float HandTrackingSpeed = 15f;

        public float HandTrackingInertia = 100f;

        public float GamepadTrackingSpeed = 2f;

        public float GamepadTrackingInertia = 100f;

        public AnimationCurve ThrusterInputCurve;

        public void ResetInput() {
            // TODO
            // These functions are screwing up input
            // we need to sort this out before re-enabling

            // Reset x and z rotation in lander
            /*Vector3 forward = LanderPhysics.Instance.LanderTransform.forward;
            forward.y = 0f;
            LanderPhysics.Instance.LanderTransform.forward = forward;

            // Reset rotation and target thrust
            targetRotation = LanderPhysics.Instance.LanderTransform.rotation;
            frameRotation = Vector3.zero;
            arcball.rotation = Quaternion.identity;
            arcballParent.rotation = Quaternion.identity;
            targetRotationTransform.rotation = Quaternion.identity;
            targetThrust = 0f;*/
        }

        public void SetForward(Vector3 forward) {
            // TODO
            // These functions are screwing up input
            // we need to sort this out before re-enabling

            /*LanderPhysics.Instance.LanderTransform.forward = forward;
            targetRotation = LanderPhysics.Instance.LanderTransform.rotation;
            targetRotationTransform.rotation = targetRotation;*/
        }

        [SerializeField]
        private InputTypeEnum inputType = InputTypeEnum.Gamepad;

        [SerializeField]
        private LocalHandInput rightHandInput;

        [SerializeField]
        private LocalHandInput leftHandInput;

        [SerializeField]
        private LanderGyro gyro;

        [SerializeField]
        private ThrottleDisplay throttle;

        //ControllerInput controllerInput;

        private Transform arcball;
        private Transform arcballParent;
        private Transform targetRotationTransform;
        private bool multipleInputSources;
        private bool inputSourceLost;
        private bool receivedInput = false;
        private Quaternion targetRotation;
        private Vector3 frameRotation;
        private float targetThrust;
        private float throttleOffset;

        private void Start() {

            inputType = InputTypeEnum.Hololens;
            // Create our arcball objects
            arcball = new GameObject("ArcBall").transform;
            arcballParent = new GameObject("ArcBallParent").transform;
            targetRotationTransform = new GameObject("TargetRotationTransform").transform;
            arcball.parent = arcballParent;

            // Check to see if we have a controller plugged in
            // TEMP make multiple input sources true by default
            //if (InputSources.Instance.hidGamepad.IsPresent()) {
                multipleInputSources = true;
            //}
        }

        private void Update() {

            // Reset these values every frame regardless
            targetThrust = Mathf.Clamp01(targetThrust);
            targetRotation = LanderPhysics.Instance.LanderTransform.rotation;
            inputSourceLost = false;
            receivedInput = false;

            if (LanderGameplay.Instance.HasLanded) {
                targetThrust = 0f;
            }

            // Orient the trackball towards the player on the y axis
            Vector3 eulerAngles = Veil.Instance.HeadTransform.eulerAngles;
            eulerAngles.x = 0f;
            eulerAngles.z = 0f;
            arcballParent.eulerAngles = eulerAngles;
            arcballParent.position = LanderPhysics.Instance.LanderPosition;

            switch (this.InputType) {
                case InputTypeEnum.Gamepad:
                case InputTypeEnum.Oasis:
                    switch (GamepadXAxis)
                    {
                        case JoystickAxisEnum.LeftStickX:
                        default:
                            frameRotation.x = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftStickY:
                            frameRotation.x = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickX:
                            frameRotation.x = InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickY:
                            frameRotation.x = InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftRightStickX:
                            frameRotation.x = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadXAxisInvert)
                            {
                                frameRotation.x -= InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.x += InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;

                        case JoystickAxisEnum.LeftRightStickY:
                            frameRotation.x = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadXAxisInvert)
                            {
                                frameRotation.x -= InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.x += InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;

                    }

                    switch (GamepadYAxis)
                    {
                        case JoystickAxisEnum.LeftStickX:
                        default:
                            frameRotation.y = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftStickY:
                            frameRotation.y = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickX:
                            frameRotation.y = InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickY:
                            frameRotation.y = InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftRightStickX:
                            frameRotation.y = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadYAxisInvert)
                            {
                                frameRotation.y -= InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.y += InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;

                        case JoystickAxisEnum.LeftRightStickY:
                            frameRotation.y = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadYAxisInvert)
                            {
                                frameRotation.y -= InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.y += InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;

                    }

                    switch (GamepadZAxis)
                    {
                        case JoystickAxisEnum.LeftStickX:
                        default:
                            frameRotation.z = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftStickY:
                            frameRotation.z = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickX:
                            frameRotation.z = InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.RightStickY:
                            frameRotation.z = InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            break;

                        case JoystickAxisEnum.LeftRightStickX:
                            frameRotation.z = InputSources.Instance.hidGamepad.leftJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadZAxisInvert)
                            {
                                frameRotation.z -= InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.z += InputSources.Instance.hidGamepad.rightJoyVector.x * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;

                        case JoystickAxisEnum.LeftRightStickY:
                            frameRotation.z = InputSources.Instance.hidGamepad.leftJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            if (GamepadZAxisInvert)
                            {
                                frameRotation.z -= InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            else
                            {
                                frameRotation.z += InputSources.Instance.hidGamepad.rightJoyVector.y * GamepadTrackingSpeed * Time.deltaTime;
                            }
                            break;
                            
                    }

                    frameRotation.x *= (GamepadXAxisInvert ? -1 : 1);
                    frameRotation.y *= (GamepadYAxisInvert ? -1 : 1);
                    frameRotation.z *= (GamepadZAxisInvert ? -1 : 1);

                    targetThrust = InputSources.Instance.hidGamepad.trigVector.y + InputSources.Instance.hidGamepad.trigVector.x;
                    break;

                case InputTypeEnum.Hololens:
                    if (LanderGameplay.Instance.GameInProgress) {
                        // If we don't have a thruster hand, wait for it to reappear
                        if (InputSources.Instance.hands.IsHandVisible(InputSourceHands.HandednessEnum.Left)) {
                            if (leftHandInput.Pressed) {
                                throttle.State = ThrottleDisplay.StateEnum.Manipulating;
                                targetThrust = leftHandInput.LocalPosition.y + throttleOffset;
                                receivedInput = true;
                            } else {
                                throttle.State = ThrottleDisplay.StateEnum.Visible;
                                throttleOffset = targetThrust;
                            }
                        } else {
                            throttle.State = ThrottleDisplay.StateEnum.Hidden;
                            throttleOffset = targetThrust;
                        }
                    } else {
                        throttle.State = ThrottleDisplay.StateEnum.Hidden;
                    }

                    // Override for tutorial
                    switch (ThrottleVisibility) {
                        case ThrottleVisibilityEnum.Normal:
                        default:
                            break;

                        case ThrottleVisibilityEnum.ForceVisible:
                            throttle.State = ThrottleDisplay.StateEnum.Visible;
                            break;

                        case ThrottleVisibilityEnum.Hidden:
                            throttle.State = ThrottleDisplay.StateEnum.Hidden;
                            break;
                    }

                    // Always apply target thrust
                    throttle.ThrottleAmount = targetThrust;

                    // Use the right hand for navigation
                    if (rightHandInput.Pressed) {
                        frameRotation.y = rightHandInput.LocalPosition.y * HandTrackingSpeed * Time.deltaTime;
                        frameRotation.x = rightHandInput.LocalPosition.z * HandTrackingSpeed * Time.deltaTime;
                        frameRotation.z = -rightHandInput.LocalPosition.x * HandTrackingSpeed * Time.deltaTime;
                        receivedInput = true;
                    }
                    break;
            }

            #if UNITY_EDITOR
            if (Input.GetKey(EditorVertAdd)) {
                frameRotation.x += 0.02f;
                receivedInput = true;
            }
            if (Input.GetKey(EditorVertSub)) {
                frameRotation.x += -0.02f;
                receivedInput = true;
            }
            if (Input.GetKey(EditorHorzAdd)) {
                frameRotation.z += 0.02f;
                receivedInput = true;
            }
            if (Input.GetKey(EditorHorzSub)) {
                frameRotation.z += -0.02f;
                receivedInput = true;
            }
            if (Input.GetKey(EditorUpAdd)) {
                frameRotation.y += 0.02f;
                receivedInput = true;
            }
            if (Input.GetKey(EditorUpSub)) {
                frameRotation.y += -0.02f;
                receivedInput = true;
            }
            if (Input.GetKeyDown(EditorThrustAdd)) {
                targetThrust += 0.15f;
                receivedInput = true;
            }
            if (Input.GetKeyDown(EditorThrustSub)) {
                targetThrust -= 0.15f;
                receivedInput = true;
            }
            #endif

            // If this is false it means we've run out of fuel
            // Don't bother to actually apply our input to the lander
            if (!ApplyInput) {
                targetThrust = 0f;
                return;
            }

            if (!inputSourceLost) {
                // Set the target transform to the lander's position and rotation
                targetRotationTransform.position = LanderPhysics.Instance.LanderPosition;
                targetRotationTransform.rotation = LanderPhysics.Instance.LanderRotation;
                targetRotationTransform.parent = arcball;
                // Transform the rotation to the trackball (XZ) in local space
                arcball.Rotate(frameRotation.x, 0f, frameRotation.z, Space.Self);
                // Rotate the target's Y in world space
                targetRotationTransform.Rotate(0f, frameRotation.y, 0f, Space.World);
                // Un-parent the target transform and get the resulting rotation
                targetRotationTransform.parent = null;
                targetRotation = targetRotationTransform.rotation;
                // TODO do this without a transform helper, it's not necessary

                if (!receivedInput) {
                    frameRotation = Vector3.Lerp(frameRotation, Vector3.zero, HandTrackingInertia * Time.deltaTime);
                }
            }
        }
    }
}