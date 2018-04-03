//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections;
using UnityEngine;
using MixedRealityToolkit.Common;

namespace MRDL
{
    public class LanderOpening : MixedRealityToolkit.Common.Singleton<LanderOpening>
    {
        public float DistanceToPlayerOnSpawn;
        public float DistanceToPlayerOnPassHead;
        public float YPositionOnSpawn;
        public float TravelTime = 5f;
        public float FinalAdjustTime = 0.9f;
        public AnimationCurve TravelCurve;
        public AnimationCurve UprightCurve;
        public AnimationCurve YPositionCurve;

        public bool InPosition {
            get {
                return inPosition;
            }
        }

        public void DoLanderOpening (Vector3 targetPosition) {
            endPosition = targetPosition;
            StartCoroutine(DoLanderOpeningOverTime());
        }

        private IEnumerator DoLanderOpeningOverTime() {
            inPosition = false;

            Vector3 directionToPad = (CameraCache.Main.transform.position - LandingPadManager.Instance.LandingPad.transform.position).normalized;

            startPosition = CameraCache.Main.transform.position + (directionToPad * DistanceToPlayerOnSpawn);
            startPosition.y = YPositionOnSpawn;

            Vector3 positionThisFrame = startPosition;
            Vector3 positionLastFrame = startPosition;
            Vector3 direction = Vector3.zero;
            float timeStarted = Time.time;

            while (!inPosition) {
                float normalizedTime = ((Time.time - timeStarted) / TravelTime);
                if (normalizedTime >= 1) {
                    LanderPhysics.Instance.LanderPosition = endPosition;
                    LanderInput.Instance.ResetInput();
                    inPosition = true;
                } else {
                    // adjust the head position
                    headPosition = CameraCache.Main.transform.position + (CameraCache.Main.transform.right * DistanceToPlayerOnPassHead);
                    // move the lander along a bezier curve
                    positionThisFrame = GetPoint(startPosition, headPosition, endPosition, normalizedTime);
                    positionThisFrame.y += YPositionCurve.Evaluate(normalizedTime);
                    direction = (positionThisFrame - positionLastFrame).normalized;
                    positionLastFrame = positionThisFrame;
                    LanderPhysics.Instance.LanderTransform.position = positionThisFrame;
                    LanderPhysics.Instance.LanderTransform.up = Vector3.Lerp (direction, Vector3.up, UprightCurve.Evaluate (normalizedTime));
                }
                yield return null;
            }
            
            yield break;
        }

        // Where the lander will finally end up
        private Vector3 endPosition;

        // Where the lander will spawn
        private Vector3 startPosition;

        // The point near the player's head where the lander will zip by
        private Vector3 headPosition;

        private Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        private Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }

        private Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            float OneMinusT = 1f - t;
            return
                OneMinusT * OneMinusT * OneMinusT * p0 +
                3f * OneMinusT * OneMinusT * t * p1 +
                3f * OneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        private Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }

        private bool inPosition;
    }
}