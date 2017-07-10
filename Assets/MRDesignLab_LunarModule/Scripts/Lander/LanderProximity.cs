//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX;
using HUX.Utility;
using System;
using UnityEngine;

namespace MRDL
{
    public class LanderProximity : Singleton<LanderProximity>
    {
        public bool DetectObstacles = true;

        public float SearchInterval = 0.5f;
        public float SearchRadius = 2f;
        public float MaxPingRange = 0.5f;
        public float MinPingRange = 0.005f;
        public float LowPingPitch = 1f;
        public float HighPingPitch = 2f;
        public AnimationCurve ProximityPingCurve;
        public AudioClip PingClip;
        public float LightFadeSpeed = 5f;

        [SerializeField]
        private Light pingLight;

        [SerializeField]
        private AudioSource pingAudio;

        private float lastPingTime;
        private float lastSearchTime;
        private float closestDistance = Mathf.Infinity;
        private float smoothClosestDistance = 0f;
        private Vector3 closestPoint;
        private Collider[] results = new Collider[50];
        private RaycastHit overlapHit;

        private void Update() {

            if (!DetectObstacles || !LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused || LanderGameplay.Instance.HasLanded) {
                pingLight.intensity = 0f;
                closestDistance = Mathf.Infinity;
                smoothClosestDistance = Mathf.Infinity;
                return;
            }

            pingLight.intensity = Mathf.Lerp(pingLight.intensity, 0f, Time.deltaTime * LightFadeSpeed);

            // If we're within a 'ping' range
            if (closestDistance < SearchRadius) {
                if (smoothClosestDistance == Mathf.Infinity)
                    smoothClosestDistance = closestDistance;

                smoothClosestDistance = Mathf.Lerp(smoothClosestDistance, closestDistance, Time.deltaTime * 3);
                float normalizedPingAmount = Mathf.Clamp01(smoothClosestDistance / MaxPingRange);
                // See how long our ping time is supposed to be
                float pingInterval = ProximityPingCurve.Evaluate(1f - normalizedPingAmount);
                if (Time.time > lastPingTime + pingInterval) {
                    lastPingTime = Time.time;
                    pingLight.intensity = 1f;
                    pingAudio.pitch = Mathf.Lerp(HighPingPitch, LowPingPitch, normalizedPingAmount);
                    pingAudio.clip = PingClip;
                    pingAudio.Play();
                }
            } else {
                smoothClosestDistance = Mathf.Infinity;
            }
        }

        private void FixedUpdate() {

            if (!LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused || LanderGameplay.Instance.HasLanded) {
                return;
            }

            if (Time.time > lastSearchTime + SearchInterval) {
                // Check all objects within our search radius
                // Find the closest one
                lastSearchTime = Time.time;
                closestDistance = Mathf.Infinity;
                closestPoint = Vector3.zero;
                int layerMask = 1 << EnvironmentManager.MoonSurfaceLayer | 1 << EnvironmentManager.RoomSurfaceLayer | 1 << EnvironmentManager.LandingPadLayer;
                Vector3 landerPosition = LanderPhysics.Instance.LanderPosition;
                Vector3 point = Vector3.zero;
                Vector3 direction = Vector3.zero;
                int numItems = Physics.OverlapSphereNonAlloc(landerPosition, SearchRadius, results, layerMask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < numItems; i++) {
                    direction = (landerPosition - results[i].bounds.center).normalized;
                    if (Physics.Raycast(landerPosition, direction, out overlapHit, SearchRadius * 2, layerMask, QueryTriggerInteraction.Ignore)) {
                        point = overlapHit.point;
                        float distance = Vector3.Distance(landerPosition, point);
                        if (distance < closestDistance) {
                            closestPoint = point;
                            closestDistance = distance;
                        }
                    }
                }
            }
        }
    }
}