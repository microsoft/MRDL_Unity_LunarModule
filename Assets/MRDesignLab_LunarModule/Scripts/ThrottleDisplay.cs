//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX;
using HUX.Utility;
using UnityEngine;

namespace MRDL
{
    public class ThrottleDisplay : MonoBehaviour
    {
        public enum StateEnum
        {
            Hidden,
            Visible,
            Manipulating,
        }
        
        public float FadeOutTime;

        public AnimationCurve GradientCurve;

        public AnimationCurve PositionCurve;

        public Color MarkerVisibleColor;

        public Color MarkerManipulatingColor;

        [Range(0f, 1f)]
        public float ThrottleAmount;

        public StateEnum State = StateEnum.Hidden;

        [SerializeField]
        private Renderer markerRenderer;
        [SerializeField]
        private Renderer gradientRenderer;
        [SerializeField]
        private Renderer backgroundRenderer;
        [SerializeField]
        private Transform markerTransform;

        private void OnEnable() {
            State = StateEnum.Hidden;
            stateLastFrame = StateEnum.Hidden;
            opacity = 0f;
            stateChangeTime = Time.time;
            markerRenderer.enabled = false;
            gradientRenderer.enabled = false;
            backgroundRenderer.enabled = false;
            backgroundColor = backgroundRenderer.material.color;
        }

        private void Update() {
            if (State != stateLastFrame) {
                stateChangeTime = Time.time;
            }

            ThrottleAmount = Mathf.Clamp01(ThrottleAmount);

            Color markerColor = MarkerVisibleColor;

            switch (State) {
                case StateEnum.Hidden:
                    opacity = 1f - Mathf.Clamp01((Time.time - stateChangeTime) / FadeOutTime);
                    if (opacity <= 0f) {
                        markerRenderer.enabled = false;
                        gradientRenderer.enabled = false;
                        backgroundRenderer.enabled = false;
                    }
                    break;

                case StateEnum.Manipulating:
                    markerRenderer.enabled = true;
                    gradientRenderer.enabled = true;
                    backgroundRenderer.enabled = true;
                    markerColor = MarkerManipulatingColor;
                    opacity = 1f;
                    break;

                case StateEnum.Visible:
                    markerRenderer.enabled = true;
                    gradientRenderer.enabled = true;
                    backgroundRenderer.enabled = true;
                    markerColor = MarkerVisibleColor;
                    if (opacity < 1f) {
                        opacity = Mathf.Clamp01((Time.time - stateChangeTime) / FadeOutTime);
                    }
                    break;
            }

            gradientRenderer.material.SetFloat("_CutOff", GradientCurve.Evaluate (ThrottleAmount));
            gradientRenderer.material.color = Color.Lerp(Color.black, Color.white, opacity);
            backgroundRenderer.material.color = Color.Lerp(Color.black, backgroundColor, opacity);
            markerRenderer.material.color = Color.Lerp(Color.black, markerColor, opacity);
            markerTransform.localPosition = new Vector3(0f, PositionCurve.Evaluate (ThrottleAmount), 0f);

            stateLastFrame = State;
        }

        private float opacity = 1f;
        private float stateChangeTime = 0f;
        private StateEnum stateLastFrame = StateEnum.Hidden;
        private Color backgroundColor;

    }
}
