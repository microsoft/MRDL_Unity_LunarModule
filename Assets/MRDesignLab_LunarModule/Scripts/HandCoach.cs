//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using UnityEngine;

namespace HUX.Interaction
{
    /// <summary>
    /// Class for displaying gesture tutorials
    /// </summary>
    public class HandCoach : MonoBehaviour
    {
        [Flags]
        public enum HandVisibilityEnum
        {
            None = 0,
            Left = 1,
            Right = 2,
            Both = Left | Right,
        }

        public enum HandMovementEnum
        {
            Static,             // Goes straight to target direction, no fades
            PingPong,           // Ping-pongs between zero and direction, no fades
            Directional,        // Loops towards target direction, fades between loops
            //Random,           // Moves around randomly betwen 0 and target directions, no fades
        }

        public enum HandGestureEnum
        {
            None,               // Neutral hand position
            Tap,                // Tap once, then release
            TapHold,            // Tap, then hold indefinitely
            TapHoldRelease,     // Tap, hold until movement is finished, then release
            Bloom,              // Bloom gesture repeatedly
            Ready,              // Held up with finger up
        }

        [Flags]
        public enum HandDirectionEnum
        {
            None = 0,
            Left = 1,
            Right = 2,
            Front = 4,
            Back = 8,
            Up = 16,
            Down = 32,
        }

        public Material HandMaterial;

        [Range(0.1f,2f)]
        public float MovementScale = 1f;

        /// <summary>
        /// The property the coach material should use to change hand color for highlights
        /// This must be a color property
        /// </summary>
        public string HighlightColorProperty = "_Color";

        /// <summary>
        /// The property the coach should use to change hand transparency
        /// This can be a color property, in which case the color's alpha will be used
        /// It can also be a float property, in which case a normalized range of 0-1 will be used
        /// </summary>
        public string MaterialTransparencyProperty = "_Color";

        /// The property the coach material should use to change hand color for lost tracking
        /// This must be a color property
        public string TrackingColorProperty = "_Color";

        /// <summary>
        /// If true, transparency property will be set with SetFloat instead of SetColor
        /// </summary>
        public bool MaterialTransparencyIsFloat = false;

        /// <summary>
        /// Automatically ghosts hands that have lost tracking
        /// </summary>
        public bool AutoGhostLostTracking = true;

        /// <summary>
        /// Automatically sets hand gesture to lowered when invisible
        /// </summary>
        public bool AutoLowerOnInvisible = true;

        /// <summary>
        /// The color of the hand when it's visible and not highlighted
        /// </summary>
        public Color NormalColor = Color.white;

        /// <summary>
        /// The color of the hand when it's visible and highlighted
        /// </summary>
        public Color HighlightColor = Color.white;

        /// <summary>
        /// This color will be used for the TrackingColorProperty when tracking is present
        /// </summary>
        public Color TrackedColor = Color.white;

        /// <summary>
        /// This color will be used for the TrackingColorProperty when tracking is lost
        /// </summary>
        public Color TrackingLostColor = Color.gray;

        /// <summary>
        /// When true, automatically sets transparency / color based on whether hands are tracked
        /// </summary>
        public HandVisibilityEnum CheckTracking = HandVisibilityEnum.Both;

        /// <summary>
        /// Curve used to drive static movement
        /// Curve must be from 0-1
        /// Is used with MovementLoopTime
        /// </summary>
        public AnimationCurve StaticCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Curve used to drive ping-pong movement
        /// Curve must be from 0-1
        /// Is used with MovementLoopTime
        /// </summary>
        public AnimationCurve PingPongCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Curve used to drive directional movement
        /// Curve must be from 0-1
        /// Is used with MovementLoopTime
        /// </summary>
        public AnimationCurve DirectionalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Curve used to drive transparency during directional movement
        /// Curve must be from 0-1
        /// Is used with MovementLoopTime
        /// </summary>
        public AnimationCurve DirectionalTransparencyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float LeftMovementLoopTime {
            get {
                return lMovementLoop;
            }
        }

        public float RightMovementLoopTime {
            get {
                return rMovementLoop;
            }
        }

        public HandVisibilityEnum InvertTracked {
            get {
                if (Application.isEditor) {
                    return HandVisibilityEnum.Both;
                } else {
                    switch (tracking) {
                        case HandVisibilityEnum.Both:
                        default:
                            return HandVisibilityEnum.None;

                        case HandVisibilityEnum.Left:
                            return HandVisibilityEnum.Right;

                        case HandVisibilityEnum.Right:
                            return HandVisibilityEnum.Left;

                        case HandVisibilityEnum.None:
                            return HandVisibilityEnum.Both;
                    }
                }
            }
        }

        public HandVisibilityEnum Visibility
        {
            get
            {
                return visibility;
            } set
            {
                if (visibility != value)
                {
                    switch (value)
                    {
                        case HandVisibilityEnum.Both:
                            switch (visibility)
                            {
                                case HandVisibilityEnum.Left:
                                    rMovementChangeTime = Time.time;
                                    break;

                                case HandVisibilityEnum.Right:
                                    lMovementChangeTime = Time.time;
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case HandVisibilityEnum.Left:
                            switch (visibility)
                            {
                                case HandVisibilityEnum.Both:
                                    rMovementChangeTime = Time.time;
                                    break;

                                case HandVisibilityEnum.Right:
                                    lMovementChangeTime = Time.time;
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case HandVisibilityEnum.Right:
                            switch (visibility)
                            {
                                case HandVisibilityEnum.Both:
                                    lMovementChangeTime = Time.time;
                                    break;

                                case HandVisibilityEnum.Left:
                                    rMovementChangeTime = Time.time;
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case HandVisibilityEnum.None:
                        default:
                            break;
                    }
                    visibility = value;
                }
            }
        }

        public HandVisibilityEnum Highlight
        {
            get
            {
                return highlight;
            }
            set
            {
                highlight = value;
            }
        }

        public HandVisibilityEnum Tracking {
            get {
                return tracking;
            } set {
                if (CheckTracking == HandVisibilityEnum.None) {
                    tracking = value;
                }
            }
        }

        public HandVisibilityEnum Ghosting {
            get {
                return ghosting;
            }
            set {
                if (ghosting != value) {
                    ghosting = value;
                    ghostChangeTime = Time.time;
                }
            }
        }

        public HandDirectionEnum LeftDirection {
            get {
                return lDirection;
            } set {
                if (lDirection != value) {
                    lDirChangeTime = Time.time;
                    lDirection = value;
                }
            }
        }

        public HandDirectionEnum RightDirection {
            get {
                return rDirection;
            }
            set {
                if (rDirection != value) {
                    rDirChangeTime = Time.time;
                    rDirection = value;
                }
            }
        }

        public HandMovementEnum LeftMovement {
            get {
                return lMovement;
            } set {
                if (lMovement != value) {
                    lMovementChangeTime = Time.time;
                    lMovement = value;
                }
            }
        }

        public HandMovementEnum RightMovement {
            get {
                return rMovement;
            }
            set {
                if (rMovement != value) {
                    rMovementChangeTime = Time.time;
                    rMovement = value;
                }
            }
        }

        public HandGestureEnum RightGesture
        {
            get
            {
                return rGesture;
            }
            set
            {
                if (rGesture != value)
                {
                    rGesture = value;
                    rMovementChangeTime = Time.time;
                }
            }
        }

        public HandGestureEnum LeftGesture
        {
            get
            {
                return lGesture;
            }
            set
            {
                if (lGesture != value)
                {
                    lGesture = value;
                    lMovementChangeTime = Time.time;
                }
            }
        }

        [SerializeField]
        private HandVisibilityEnum visibility = HandVisibilityEnum.Both;
        [SerializeField]
        private HandVisibilityEnum highlight = HandVisibilityEnum.None;
        [SerializeField]
        private HandVisibilityEnum ghosting = HandVisibilityEnum.None;
        [SerializeField]
        private HandVisibilityEnum tracking = HandVisibilityEnum.Both;

        // Target direction
        [SerializeField]
        private HandDirectionEnum rDirection = HandDirectionEnum.None;
        [SerializeField]
        private HandDirectionEnum lDirection = HandDirectionEnum.None;

        // How to move towards the target direction
        [SerializeField]
        private HandMovementEnum lMovement = HandMovementEnum.Static;
        [SerializeField]
        private HandMovementEnum rMovement = HandMovementEnum.Static;

        // Which gesture to show
        [SerializeField]
        private HandGestureEnum lGesture = HandGestureEnum.None;
        [SerializeField]
        private HandGestureEnum rGesture = HandGestureEnum.None;

        [SerializeField]
        private SkinnedMeshRenderer rightRenderer;

        [SerializeField]
        private SkinnedMeshRenderer leftRenderer;

        [SerializeField]
        private Animator rightAnimator;

        [SerializeField]
        private Animator leftAnimator;

        private void OnEnable() {
            rMat = new Material(HandMaterial);
            lMat = new Material(HandMaterial);
            rightRenderer.sharedMaterial = rMat;
            leftRenderer.sharedMaterial = lMat;

            rightRenderer.updateWhenOffscreen = true;
            leftRenderer.updateWhenOffscreen = true;
            rightAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            leftAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            lTransparency = 0f;
            rTransparency = 0f;
            lHighlightColor = NormalColor;
            rHighlightColor = NormalColor;
        }

        private void Update() {

            // Check tracking

            bool rTracked = true;
            bool lTracked = true;
            if (CheckTracking == HandVisibilityEnum.Both || CheckTracking == HandVisibilityEnum.Right) {
                // If we're checking tracking, tracking is set from input source
                rTracked = InputSources.Instance.hands.IsHandVisible(InputSourceHands.HandednessEnum.Right);
            } else {
                // If we're not checking tracking, it's set from tracking value
                rTracked = (tracking == HandVisibilityEnum.Right || tracking == HandVisibilityEnum.Both);
            }

            if (CheckTracking == HandVisibilityEnum.Both || CheckTracking == HandVisibilityEnum.Left) {
                // If we're checking tracking, tracking is set from input source
                lTracked = InputSources.Instance.hands.IsHandVisible(InputSourceHands.HandednessEnum.Left);
            } else {
                // If we're not checking tracking, it's set from tracking value
                lTracked = (tracking == HandVisibilityEnum.Left || tracking == HandVisibilityEnum.Both);
            }

            if (rTracked && lTracked) {
                tracking = HandVisibilityEnum.Both;
            } else if (rTracked) {
                tracking = HandVisibilityEnum.Right;
            } else if (lTracked) {
                tracking = HandVisibilityEnum.Left;
            } else {
                tracking = HandVisibilityEnum.None;
            }

            // Do visibility and appearance
            Color lHighlightColorTarget = NormalColor;
            Color rHighlightColorTarget = NormalColor;
            Color rTrackingColorTarget = rTracked ? TrackedColor : TrackingLostColor;
            Color lTrackingColorTarget = lTracked ? TrackedColor : TrackingLostColor;
            float lTransTarget = 0f;
            float rTransTarget = 0f;
            bool rVisible = false;
            bool lVisible = false;

            rightAnimator.SetBool(LeftHandParam, false);
            leftAnimator.SetBool(LeftHandParam, true);

            AnimatorStateInfo rightState = rightAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo leftState = leftAnimator.GetCurrentAnimatorStateInfo(0);

            switch (visibility) {
                case HandVisibilityEnum.Both:
                    rTransTarget = 1f;
                    lTransTarget = 1f;
                    rVisible = true;
                    lVisible = true;
                    break;

                case HandVisibilityEnum.Right:
                    if (AutoLowerOnInvisible) {
                        leftAnimator.SetTrigger(LoweredParam);
                    } else {
                        leftAnimator.SetTrigger(NeutralParam);
                    }
                    rTransTarget = 1f;
                    lTransTarget = 0f;
                    rVisible = true;
                    break;

                case HandVisibilityEnum.Left:
                    if (AutoLowerOnInvisible) {
                        rightAnimator.SetTrigger(LoweredParam);
                    } else {
                        rightAnimator.SetTrigger(NeutralParam);
                    }
                    rTransTarget = 0f;
                    lTransTarget = 1f;
                    lVisible = true;
                    break;

                case HandVisibilityEnum.None:
                default:
                    if (AutoLowerOnInvisible) {
                        rightAnimator.SetTrigger(LoweredParam);
                        leftAnimator.SetTrigger(LoweredParam);
                    } else {
                        rightAnimator.SetTrigger(NeutralParam);
                        leftAnimator.SetTrigger(NeutralParam);
                    }
                    rTransTarget = 0f;
                    lTransTarget = 0f;
                    break;
            }

            switch (ghosting) {
                case HandVisibilityEnum.Both:
                    lTransTarget *= GhostAlpha;
                    rTransTarget *= GhostAlpha;
                    break;

                case HandVisibilityEnum.Left:
                    lTransTarget *= GhostAlpha;
                    if (AutoGhostLostTracking) {
                        lTransTarget *= lTracked ? 1 : GhostAlpha;
                    }
                    break;

                case HandVisibilityEnum.Right:
                    rTransTarget = GhostAlpha;
                    if (AutoGhostLostTracking) {
                        rTransTarget *= rTracked ? 1 : GhostAlpha;
                    }
                    break;

                case HandVisibilityEnum.None:
                default:
                    if (AutoGhostLostTracking) {
                        rTransTarget *= rTracked ? 1 : GhostAlpha;
                        lTransTarget *= lTracked ? 1 : GhostAlpha;
                    }
                    break;
            }

            switch (highlight) {
                case HandVisibilityEnum.None:
                    rHighlightColorTarget = NormalColor;
                    lHighlightColorTarget = NormalColor;
                    break;

                case HandVisibilityEnum.Left:
                    rHighlightColorTarget = NormalColor;
                    lHighlightColorTarget = HighlightColor;
                    break;

                case HandVisibilityEnum.Right:
                    rHighlightColorTarget = HighlightColor;
                    lHighlightColorTarget = NormalColor;
                    break;

                case HandVisibilityEnum.Both:
                    rHighlightColorTarget = HighlightColor;
                    lHighlightColorTarget = HighlightColor;
                    break;
            }

            lTransparency = Mathf.Lerp(lTransparency, lTransTarget, Time.deltaTime * MatAnimSpeed);
            rTransparency = Mathf.Lerp(rTransparency, rTransTarget, Time.deltaTime * MatAnimSpeed);
            lHighlightColor = Color.Lerp(lHighlightColor, lHighlightColorTarget, Time.deltaTime * MatAnimSpeed);
            rHighlightColor = Color.Lerp(rHighlightColor, rHighlightColorTarget, Time.deltaTime * MatAnimSpeed);
            rTrackingColor = Color.Lerp(rTrackingColor, rTrackingColorTarget, Time.deltaTime * MatAnimSpeed);
            lTrackingColor = Color.Lerp(lTrackingColor, lTrackingColorTarget, Time.deltaTime * MatAnimSpeed);

            // Get final transparency values from the movement direction
            float lFinalTrans = lTransparency;
            float rFinalTrans = rTransparency;

            // Do gestures and movement
            // See if we've looped
            bool rLooped = GetLoopedTime(ref rNormalizedLoop, ref rNormalizedLoopLastFrame, ref rMovementLoop, rMovementChangeTime, rMovement);
            bool lLooped = GetLoopedTime(ref lNormalizedLoop, ref lNormalizedLoopLastFrame, ref lMovementLoop, lMovementChangeTime, lMovement);

            SetHandGesture(rightAnimator, rightState.shortNameHash, rGesture, rVisible, rLooped);
            SetHandDirection(rDirection, ref rSmoothPosDir, ref rSmoothNegDir, rDirChangeTime, rVisible);
            SetHandMovement(rightAnimator, rDirection, rMovement, rSmoothPosDir, rSmoothNegDir, rMovementLoop, ref rFinalTrans);

            SetHandGesture(leftAnimator, leftState.shortNameHash, lGesture, lVisible, lLooped);
            SetHandDirection(lDirection, ref lSmoothPosDir, ref lSmoothNegDir, lDirChangeTime, lVisible);
            SetHandMovement(leftAnimator, lDirection, lMovement, lSmoothPosDir, lSmoothNegDir, lMovementLoop, ref lFinalTrans);

            // Set final material properties
            rMat.SetColor(HighlightColorProperty, rHighlightColor);
            lMat.SetColor(HighlightColorProperty, lHighlightColor);
            rMat.SetColor(TrackingColorProperty, rTrackingColor);
            lMat.SetColor(TrackingColorProperty, lTrackingColor);

            if (MaterialTransparencyIsFloat) {
                rMat.SetFloat(MaterialTransparencyProperty, rFinalTrans);
                lMat.SetFloat(MaterialTransparencyProperty, lFinalTrans);
            } else {
                // If the transparency property is a color
                // get the color first, then set its alpha
                rHighlightColor = rMat.GetColor(MaterialTransparencyProperty);
                lHighlightColor = lMat.GetColor(MaterialTransparencyProperty);
                rHighlightColor.a = rFinalTrans;
                lHighlightColor.a = lFinalTrans;
                rMat.SetColor(MaterialTransparencyProperty, rHighlightColor);
                lMat.SetColor(MaterialTransparencyProperty, lHighlightColor);
            }
        }

        private bool GetLoopedTime (ref float normalizedLoop, ref float normalizedLoopLastFrame, ref float movementLoop, float movementChangeTime, HandMovementEnum handMovement)
        {
            normalizedLoop = Mathf.Clamp01(Mathf.Repeat(Time.time - movementChangeTime, MovementLoopTime) / MovementLoopTime);

            switch (handMovement)
            {
                case HandMovementEnum.Directional:
                    float directionalLoop = Mathf.Repeat(Time.time - movementChangeTime, MovementLoopTime) / MovementLoopTime;
                    movementLoop = Mathf.Lerp(movementLoop, directionalLoop, Mathf.Clamp01(Time.time - movementChangeTime));
                    break;

                case HandMovementEnum.PingPong:
                    float pingPongLoop = Mathf.PingPong(Time.time - movementChangeTime, MovementLoopTime) / MovementLoopTime;
                    movementLoop = Mathf.Lerp(movementLoop, pingPongLoop, Mathf.Clamp01(Time.time - movementChangeTime));
                    break;

                case HandMovementEnum.Static:
                    float staticLoop = Mathf.Clamp(Time.time - movementChangeTime, 0f, MovementLoopTime) / MovementLoopTime;
                    movementLoop = Mathf.Lerp(movementLoop, staticLoop, Mathf.Clamp01(Time.time - movementChangeTime));
                    break;
            }

            bool looped = normalizedLoop < normalizedLoopLastFrame;
            normalizedLoopLastFrame = normalizedLoop;
            return looped;
        }

        private void SetHandGesture (Animator animator, int currentState, HandGestureEnum gesture, bool visible, bool looped) {
            if (visible) {
                switch (gesture) {
                    case HandGestureEnum.None:
                        if (currentState != neutralState)
                        {
                            animator.SetTrigger(NeutralParam);
                        }
                        break;

                    case HandGestureEnum.Ready:
                        if (currentState != readyState)
                        {
                            animator.SetTrigger(ReadyParam);
                        }
                        break;

                    case HandGestureEnum.Bloom:
                        if (looped)
                        {
                            animator.SetTrigger(BloomParam);
                        }
                        break;

                    case HandGestureEnum.Tap:
                        if (looped)
                        {
                            animator.SetTrigger(TapParam);
                        }
                        break;

                    case HandGestureEnum.TapHold:
                        if (currentState != tapHoldState)
                        {
                            animator.SetTrigger(TapHoldParam);
                        }
                        break;

                    case HandGestureEnum.TapHoldRelease:
                        if (looped)
                        {
                            animator.SetTrigger(TapHoldParam);
                        }
                        break;
                }
            }
        }

        private void SetHandDirection (HandDirectionEnum direction, ref Vector3 smoothPosDir, ref Vector3 smoothNegDir, float dirChangeTime, bool visible) {

            // Breaking these up into 2 vectors to make later modifications easier
            // TODO make this a little less bloated
            Vector3 posDir = Vector3.zero;
            Vector3 negDir = Vector3.zero;
            if (visible) {
                if ((direction & HandDirectionEnum.Front) == HandDirectionEnum.Front) {
                    posDir.z = 1;
                }
                if ((direction & HandDirectionEnum.Back) == HandDirectionEnum.Back) {
                    negDir.z = 1;
                }
                if ((direction & HandDirectionEnum.Left) == HandDirectionEnum.Left) {
                    posDir.x = 1;
                }
                if ((direction & HandDirectionEnum.Right) == HandDirectionEnum.Right) {
                    negDir.x = 1;
                }
                if ((direction & HandDirectionEnum.Up) == HandDirectionEnum.Up) {
                    posDir.y = 1;
                }
                if ((direction & HandDirectionEnum.Down) == HandDirectionEnum.Down) {
                    negDir.y = 1;
                }
            }

            smoothPosDir = Vector3.Slerp(smoothPosDir, posDir, (Time.time - dirChangeTime) * DirAnimSpeed);
            smoothNegDir = Vector3.Slerp(smoothNegDir, negDir, (Time.time - dirChangeTime) * DirAnimSpeed);
        }

        private void SetHandMovement(Animator animator, HandDirectionEnum direction, HandMovementEnum movement, Vector3 smoothPosDir, Vector3 smoothNegDir, float movementLoop, ref float transparency) {

            switch (movement) {
                case HandMovementEnum.Static:
                default:
                    float staticLoopMulitiplier = StaticCurve.Evaluate(movementLoop);
                    animator.SetFloat(ForwardParam, smoothPosDir.z * staticLoopMulitiplier * MovementScale);
                    animator.SetFloat(BackParam, smoothNegDir.z * staticLoopMulitiplier * MovementScale);
                    animator.SetFloat(LeftParam, smoothPosDir.x * staticLoopMulitiplier * MovementScale);
                    animator.SetFloat(RightParam, smoothNegDir.x * staticLoopMulitiplier * MovementScale);
                    animator.SetFloat(UpParam, smoothPosDir.y * staticLoopMulitiplier * MovementScale);
                    animator.SetFloat(DownParam, smoothNegDir.y * staticLoopMulitiplier * MovementScale);
                    break;

                case HandMovementEnum.PingPong:
                    float pingPongMultiplier = PingPongCurve.Evaluate(movementLoop);
                    animator.SetFloat(ForwardParam, smoothPosDir.z * pingPongMultiplier * MovementScale);
                    animator.SetFloat(BackParam, smoothNegDir.z * pingPongMultiplier * MovementScale);
                    animator.SetFloat(LeftParam, smoothPosDir.x * pingPongMultiplier * MovementScale);
                    animator.SetFloat(RightParam, smoothNegDir.x * pingPongMultiplier * MovementScale);
                    animator.SetFloat(UpParam, smoothPosDir.y * pingPongMultiplier * MovementScale);
                    animator.SetFloat(DownParam, smoothNegDir.y * pingPongMultiplier * MovementScale);
                    break;

                case HandMovementEnum.Directional:
                    float directionalMultiplier = DirectionalCurve.Evaluate(movementLoop);
                    animator.SetFloat(ForwardParam, smoothPosDir.z * directionalMultiplier * MovementScale);
                    animator.SetFloat(BackParam, smoothNegDir.z * directionalMultiplier * MovementScale);
                    animator.SetFloat(LeftParam, smoothPosDir.x * directionalMultiplier * MovementScale);
                    animator.SetFloat(RightParam, smoothNegDir.x * directionalMultiplier * MovementScale);
                    animator.SetFloat(UpParam, smoothPosDir.y * directionalMultiplier * MovementScale);
                    animator.SetFloat(DownParam, smoothNegDir.y * directionalMultiplier * MovementScale);
                    if (direction != HandDirectionEnum.None) {
                        transparency *= DirectionalTransparencyCurve.Evaluate(movementLoop);
                    }
                    break;

                /*case HandMovementEnum.Random:
                    break;*/
            }
        }

        private const float MatAnimSpeed = 1.5f;
        private const float DirAnimSpeed = 1.5f;
        private const float MovementLoopTime = 1.75f;
        private const float GhostAlpha = 0.5f;

        // Parameters used by hand animators
        private const string RaisedParam        = "Raised";
        private const string ReadyParam         = "Ready";
        private const string LoweredParam       = "Lowered";
        private const string NeutralParam       = "Neutral";
        private const string TapParam           = "Tap";
        private const string TapHoldParam       = "TapHold";
        private const string TapReleaseParam    = "TapRelease";
        private const string BloomParam         = "Bloom";
        private const string ForwardParam       = "Forward";
        private const string BackParam          = "Back";
        private const string LeftParam          = "Left";
        private const string RightParam         = "Right";
        private const string UpParam            = "Up";
        private const string DownParam          = "Down";
        private const string LeftHandParam      = "LeftHand";

        private int loweredState = Animator.StringToHash(LoweredParam);
        private int neutralState = Animator.StringToHash(NeutralParam);
        private int raisedState = Animator.StringToHash(RaisedParam);
        private int readyState = Animator.StringToHash(ReadyParam);
        private int tapState = Animator.StringToHash(TapParam);
        private int tapHoldState = Animator.StringToHash(TapHoldParam);
        private int tapReleaseState = Animator.StringToHash(TapReleaseParam);
        private int bloomState = Animator.StringToHash(BloomParam);
            
        // Direction
        private Vector3 rSmoothPosDir;
        private Vector3 lSmoothPosDir;
        private Vector3 rSmoothNegDir;
        private Vector3 lSmoothNegDir;

        // Material
        private float lTransparency;
        private float rTransparency;
        private Color lHighlightColor;
        private Color rHighlightColor;
        private Color lTrackingColor;
        private Color rTrackingColor;
        private Material rMat;
        private Material lMat;

        // Timing
        private float ghostChangeTime;
        private float lDirChangeTime;
        private float rDirChangeTime;
        private float lMovementChangeTime;
        private float rMovementChangeTime;
        private float lNormalizedLoop;
        private float rNormalizedLoop;
        private float lNormalizedLoopLastFrame;
        private float rNormalizedLoopLastFrame;
        private float rMovementLoop;
        private float lMovementLoop;
    }
}