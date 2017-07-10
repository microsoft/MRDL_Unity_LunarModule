//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Utility;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Debugging utility for testing left / right hand input
    /// </summary>
    public class HandInputDisplay : Singleton<HandInputDisplay>
    {
        public enum VisibilityEnum
        {
            Always,
            TrackingPresent,
            TrackingLost,
        }

        public bool Show = false;

        public VisibilityEnum Visibility = VisibilityEnum.Always;

        public bool ShowInputLostMessage {
            get {
                return message.gameObject.activeSelf;
            } set {
                message.gameObject.SetActive(value);
            }
        }

        public bool FollowOnScreen = false;
        public Vector2 MaxRange = new Vector2(0.1f, 0.065f);
        public Vector2 MinRange = new Vector2(-0.1f, -0.05f);
        public Vector2 OffsetLeft = new Vector2(0.05f, 0.05f);
        public Vector2 OffsetRight = new Vector2(-0.05f, 0.05f);

        public float MinConfidence = 0.15f;

        public Texture HandAbsentTexture;
        public Texture HandPresentTexture;
        public Texture HandPressedTexture;

        public Color HandAbsentColor;
        public Color HandPresentColor;
        public Color HandPressedColor;

        [SerializeField]
        private TextMesh message;
        [SerializeField]
        private Renderer leftHand;
        [SerializeField]
        private Renderer rightHand;
        private Renderer messageRenderer;

        private void OnEnable() {
            messageRenderer = message.GetComponent<Renderer>();
        }

        private void Update() {
            if (Show) {
                messageRenderer.enabled = true;
                UpdateHand(leftHand, InputSources.Instance.hands.GetHandState(InputSourceHands.HandednessEnum.Left, MinConfidence), OffsetLeft);
                UpdateHand(rightHand, InputSources.Instance.hands.GetHandState(InputSourceHands.HandednessEnum.Right, MinConfidence), OffsetRight);
            } else {
                messageRenderer.enabled = false;
                leftHand.enabled = false;
                rightHand.enabled = false;
            }
        }

        private void UpdateHand (Renderer handRenderer, InputSourceHands.CurrentHandState handState, Vector2 handOffset) {
            if (handState == null) {
                switch (Visibility) {
                    case VisibilityEnum.Always:
                    case VisibilityEnum.TrackingLost:
                        handRenderer.enabled = true;
                        handRenderer.material.mainTexture = HandAbsentTexture;
                        handRenderer.material.color = HandAbsentColor;
                        break;

                    case VisibilityEnum.TrackingPresent:
                        handRenderer.enabled = false;
                        break;
                }
            } else {
                if (FollowOnScreen) {
                    // Get the world position of the billboarded indicator
                    Vector3 handPos = transform.InverseTransformPoint (handState.Position);
                    handPos.x += handOffset.x;
                    handPos.y += handOffset.y;
                    handPos.z = 0f;
                    handPos.x = Mathf.Clamp(handPos.x, MinRange.x, MaxRange.x);
                    handPos.y = Mathf.Clamp(handPos.y, MinRange.y, MaxRange.y);
                    handRenderer.transform.localPosition = handPos;
                 }

                if (handState.Pressed) {
                    switch (Visibility) {
                        case VisibilityEnum.Always:
                        case VisibilityEnum.TrackingPresent:
                            handRenderer.enabled = true;
                            handRenderer.material.mainTexture = HandPressedTexture;
                            handRenderer.material.color = HandPressedColor;
                            break;

                        case VisibilityEnum.TrackingLost:
                            handRenderer.enabled = false;
                            break;
                    }
                } else {
                    switch (Visibility) {
                        case VisibilityEnum.Always:
                        case VisibilityEnum.TrackingPresent:
                            handRenderer.enabled = true;
                            handRenderer.material.mainTexture = HandPresentTexture;
                            handRenderer.material.color = HandPresentColor;
                            break;

                        case VisibilityEnum.TrackingLost:
                            handRenderer.enabled = false;
                            break;
                    }
                }
            }
        }
    }
}