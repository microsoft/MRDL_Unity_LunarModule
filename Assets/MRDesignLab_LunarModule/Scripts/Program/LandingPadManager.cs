//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections;
using MixedRealityToolkit.Common;
using UnityEngine;

namespace MRDL
{
    public class LandingPadManager : Singleton<LandingPadManager>
    {
        public AudioClip PlaceClip;
        public AudioClip LandingAssistClip;

        [SerializeField]
        private GameObject landingPadPrefab;

        [SerializeField]
        private GameObject startupPadPrefab;

        [SerializeField]
        private AudioSource audio;

        public LandingPad LandingPad {
            get {
                if (landingPad == null) {
                    GameObject landingPadGo = GameObject.Instantiate(landingPadPrefab) as GameObject;
                    landingPad = landingPadGo.GetComponent<LandingPad>();
                    landingPad.transform.position = Vector3.left * 1000;
                }
                return landingPad;
            }
        }

        public StartupPad StartupPad {
            get {
                if (startupPad == null) {
                    GameObject startupPadGo = GameObject.Instantiate(startupPadPrefab) as GameObject;
                    startupPad = startupPadGo.GetComponent<StartupPad>();
                    startupPad.transform.position = Vector3.left * 1000;
                }
                return startupPad;
            }
        }

        public Vector3 LandingPadPosition
        {
            get
            {
                return landingPadPosition;
            }
        }

        public Vector3 LanderStartupPosition {
            get {
                return StartupPad.LanderPosition;
            }
        }

        public bool StartupPlacementConfirmed {
            get {
                return startupPlacementConfirmed;
            }
        }

        public bool StartupPlacementValid {
            get {
                return startupPlacementValid;
            }
        }

        public bool LandingPlacementConfirmed {
            get {
                return landingPlacementConfirmed;
            }
        }

        public bool LandingPlacementValid {
            get {
                return landingPlacementValid;
            }
        }

        public void ConfirmLandingPadPlacement() {
            landingPlacementConfirmed = true;
            landingPadPosition = landingPad.transform.position;
            audio.PlayOneShot(PlaceClip, 0.25f);
        }

        public void ConfirmStartupPadPlacement() {
            startupPlacementConfirmed = true;
            audio.PlayOneShot(PlaceClip, 0.25f);
        }

        public void PlaceLandingAndStartupPads() {
            StartCoroutine(PlaceLandingAndStartupPadsOverTime());
        }

        public void HideLandingPad() {
            LandingPad.gameObject.SetActive(false);
            landingPlacementConfirmed = false;
        }
        
        private IEnumerator PlaceLandingAndStartupPadsOverTime() {

            landingPlacementConfirmed = false;
            startupPlacementConfirmed = false;
            // Instantiate a landing pad
            // Place the landing pad in front of the player to begin with
            LandingPad.transform.position = CameraCache.Main.transform.position + CameraCache.Main.transform.forward;
            LandingPad.gameObject.SetActive(true);

            bool setFirstPosition = false;
            Vector3 surfacePoint = Vector3.zero;
            Vector3 surfaceNormal = Vector3.up;
            landingPlacementValid = false;

            // Now wait for the player to place the landing pad
            while (!landingPlacementConfirmed) {

                // Raycast gaze and see if we hit a floor plane
                bool hitMoonSurface = false;

                // Check our constant raycasting
                if (EnvironmentManager.Instance.EnvironmentHit.collider != null) {
                    surfaceNormal = EnvironmentManager.Instance.EnvironmentHit.normal;
                    if (EnvironmentManager.Instance.EnvironmentHit.collider.CompareTag(EnvironmentManager.MoonSurfaceTag)) {
                        hitMoonSurface = true;
                        if (setFirstPosition) {
                            surfacePoint = Vector3.Lerp(surfacePoint, EnvironmentManager.Instance.EnvironmentHit.point, 0.5f);
                        } else {
                            setFirstPosition = true;
                            surfacePoint = EnvironmentManager.Instance.EnvironmentHit.point;
                        }
                    }
                }

                float dot = Vector3.Dot(surfaceNormal, Vector3.up);
                landingPlacementValid = landingPad.IntersectingColliders.Count == 0 && dot > 0.9f;
                landingPlacementPossible = true;

                // Show / hide the landing pad based on whether we're looking at a valid point
                if (hitMoonSurface) {
                    LandingPad.transform.position = surfacePoint;
                } else {
                    landingPlacementPossible = false;
                    landingPlacementValid = false;
                    setFirstPosition = false;
                }

                // placementConfirmed will be called by the player clicking confirm
                yield return null;
            }

            setFirstPosition = false;
            while (!startupPlacementConfirmed) {
                // Raycast gaze and see if we hit a floor plane
                bool hitMoonSurface = false;

                // Check our constant raycasting
                if (EnvironmentManager.Instance.EnvironmentHit.collider != null) {
                    surfaceNormal = EnvironmentManager.Instance.EnvironmentHit.normal;
                    if (EnvironmentManager.Instance.EnvironmentHit.collider.CompareTag(EnvironmentManager.MoonSurfaceTag)) {
                        hitMoonSurface = true;
                        if (setFirstPosition) {
                            surfacePoint = Vector3.Lerp(surfacePoint, EnvironmentManager.Instance.EnvironmentHit.point, 0.5f);
                        } else {
                            setFirstPosition = true;
                            surfacePoint = EnvironmentManager.Instance.EnvironmentHit.point;
                        }
                    }
                }
                
                startupPlacementValid = StartupPad.IntersectingColliders.Count == 0;
                startupPlacementPossible = true;
                // Show / hide the startup pad based on whether we're looking at a valid point
                if (hitMoonSurface) {
                    StartupPad.transform.position = surfacePoint;
                    // Check to see if the startup pad is the min distance away from the landing pad
                    float distanceToLandingPad = Vector3.Distance(surfacePoint, landingPadPosition);
                    if (distanceToLandingPad < LandingPad.Radius + LanderGameplay.Instance.Settings.MinStartDistance) {
                        startupPlacementValid = false;
                    }
                } else {
                    startupPlacementPossible = false;
                    startupPlacementValid = false;
                    setFirstPosition = false;
                }
                // placementConfirmed will be called by the player clicking confirm
                yield return null;
            }
            yield break;
        }

        private void Update()
        {
            if (landingPlacementConfirmed) {
                if (startupPlacementConfirmed) {
                    StartupPad.State = (LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.HasLanded) ? StartupPad.StartupPadStateEnum.Hidden : StartupPad.StartupPadStateEnum.Placed;
                } else if (startupPlacementPossible) {
                    StartupPad.State = startupPlacementValid ? StartupPad.StartupPadStateEnum.PlacingValid : StartupPad.StartupPadStateEnum.PlacingInvalid;
                } else {
                    StartupPad.State = StartupPad.StartupPadStateEnum.PlacingHidden;
                }

                if (LanderGameplay.Instance.HasLanded) {
                    LandingPad.State = LanderGameplay.Instance.HasCrashed ? LandingPad.LandingPadStateEnum.FailedLanding : LandingPad.LandingPadStateEnum.SuccessfulLanding;
                } else {
                    LandingPad.ShowWarning = LanderGameplay.Instance.LandingAngleDanger || LanderGameplay.Instance.LandingSpeedDanger;
                    Vector3 landerPosition = LanderPhysics.Instance.LanderPosition;
                    landerPosition.y = 0f;
                    float distanceToPad = Vector3.Distance(new Vector3(landingPadPosition.x, 0f, landingPadPosition.z), landerPosition);
                    if (LanderGameplay.Instance.GameInProgress && distanceToPad < LandingPad.Radius) {
                        // Send a message that landing assist is engaged
                        if (LandingPad.State != LandingPad.LandingPadStateEnum.PlacedOccupied) {
                            if (LanderGameplay.Instance.Settings.AutoPilotOrient || LanderGameplay.Instance.Settings.AutoPilotLand) {
                                audio.PlayOneShot(LandingAssistClip, 0.5f);
                                GameplayMessage.Instance.DisplayMessage("Landing assistant engaged");
                            }
                        }
                        LandingPad.State = LandingPad.LandingPadStateEnum.PlacedOccupied;
                    } else {
                        LandingPad.State = LandingPad.LandingPadStateEnum.PlacedUnoccupied;
                    }
                }
            } else if (landingPlacementValid) {
                LandingPad.State = LandingPad.LandingPadStateEnum.PlacingValid;
            } else if (landingPlacementPossible) {
                LandingPad.State = LandingPad.LandingPadStateEnum.PlacingInvalid;
            } else {
                LandingPad.State = LandingPad.LandingPadStateEnum.PlacingHidden;
            }
        }

        private StartupPad startupPad;
        private LandingPad landingPad;

        private bool landingPlacementConfirmed = false;
        private bool landingPlacementValid = false;
        private bool landingPlacementPossible = false;

        private bool startupPlacementConfirmed = false;
        private bool startupPlacementValid = false;
        private bool startupPlacementPossible = false;

        private Vector3 landingPadPosition;
    }
}