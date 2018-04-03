//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HUX.Utility;
using MixedRealityToolkit.Common;
using System;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    public class LanderGameplay : Singleton<LanderGameplay>
    {
        const int SpatialUnitMultiplier = 8000;
        const int SpeedUnitMultiplier = 12000;

        #region classes & enums
        [Serializable]
        public class GameplaySession
        {
            public GameplaySession (float timeGameStarted, float fuelOnStartup) {
                TimeGameStarted = timeGameStarted;
                Fuel = fuelOnStartup;
            }

            public float Fuel = 0f;
            public float Altitude = Mathf.Infinity;
            public float HorizontalSpeed = 0f;
            public float VerticalSpeed = 0f;
            public int Score = 0;
            public float TimeGameStopped = 0f;
            public float TimeGameStarted = 0f;
            public bool HasLanded = false;
            public bool HasCrashed = false;
            public Vector3 LandPoint = Vector3.zero;
        }

        [Serializable]
        public class GameplaySettings
        {
            public float FuelOnStartup = 1000;
            public float TimeToLand = 100;
            public float ThrustFuelConsumption = 0.01f;
            public float TorqueFuelConsumption = 0.01f;

            public float StartDistanceFromPad = 1f;
            public float MinStartAltitude = 0.5f;
            public float PlacementCheckRadius = 0.05f;
            public float MinDistanceFromPlayer = 0.25f;

            public int MaxSafeVerticalSpeed = 5;
            public int MaxSafeHorizontalSpeed = 5;
            public float MinSafeCollisionDot = 0.995f;
            public float MinDangerDistanceToPad = 1.5f;
            public float MinStartDistance = 1f;

            public float OrientationCorrectionSpeed = 5f;
            public float PositionCorrectionSpeed = 0.25f;
            public bool AutoPilotLand = true;
            public bool AutoPilotOrient = true;

            public float LandingPadScale = 1f;
        }

        public enum DifficultyEnum
        {
            Easy,
            Medium,
            Hard
        }
        #endregion

        #region public
        public DifficultyEnum Difficulty = DifficultyEnum.Easy;

        public GameplaySettings Settings {
            get {
                return difficultySettings[(int)Difficulty];
            }
        }

        public Action OnGameplayStarted;

        public Action OnGameplayEnded;

        public bool GameInProgress {
            get {
                return gameInProgress;
            }
        }

        public bool Paused {
            get {
                return paused;
            }
        }

        public bool LandingSpeedDanger {
            get {
                if (!GameInProgress)
                    return false;

                if (currentSession.Altitude > Settings.MinDangerDistanceToPad)
                    return false;

                return VerticalSpeed > Settings.MaxSafeVerticalSpeed;
            }
        }

        public bool LandingAngleDanger {
            get {
                if (!GameInProgress)
                    return false;

                if (currentSession.Altitude > Settings.MinDangerDistanceToPad)
                    return false;

                return LanderPhysics.Instance.LanderDot <= Settings.MinSafeCollisionDot;
            }
        }

        public int Score {
            get {
                return currentSession.Score;
            }
        }

        public int GameTime {
            get {
                if (gameInProgress) {
                    return Mathf.FloorToInt(Mathf.Clamp(Settings.TimeToLand - (Time.time - currentSession.TimeGameStarted), 0f, float.MaxValue));
                } else {
                    return Mathf.FloorToInt(currentSession.TimeGameStarted - currentSession.TimeGameStopped);
                }
            }
        }

        public int Fuel {
            get {
                return Mathf.FloorToInt(currentSession.Fuel);
            }
            set {
                currentSession.Fuel = value;
            }
        }

        public int Altitude {
            get {
                return Mathf.FloorToInt(currentSession.Altitude * SpatialUnitMultiplier);
            }
        }

        public int HorizontalSpeed {
            get {
                return Mathf.FloorToInt(currentSession.HorizontalSpeed * SpeedUnitMultiplier);
            }
        }

        public int VerticalSpeed {
            get {
                return Mathf.FloorToInt(currentSession.VerticalSpeed * SpeedUnitMultiplier);
            }
        }

        public bool HasCrashed {
            get {
                return currentSession.HasCrashed;
            }
        }

        public bool HasLanded {
            get {
                return currentSession.HasLanded;
            }
        }

        public Vector3 LandPoint {
            get {
                return currentSession.LandPoint;
            }
        }
        #endregion

        #region private
        [SerializeField]
        private GameObject startButton;

        [SerializeField]
        private GameplaySettings[] difficultySettings = new GameplaySettings[3];

        private bool gameInProgress;
        private bool paused;
        private Vector3 positionLastFrame;
        private GameplaySettings currentSettings;
        private GameplaySession currentSession = new GameplaySession(0f, 0f);
        #endregion

        void Start() {
            LanderPhysics.Instance.OnLand += OnLand;
            startButton.SetActive(false);
        }

        public void TutorialStart(GameObject display) {
            StartCoroutine(TutorialStartOverTime(display));
        }

        public void Reset() {
            currentSession = new GameplaySession(Time.time, 1);
        }

        public void GameStart() {
            StartCoroutine(GameStartOverTime());
        }

        private IEnumerator TutorialStartOverTime(GameObject display) {
            // Start a 'fake' game so all the other components think we're playing
            gameInProgress = true;
            currentSession = new GameplaySession(Time.time, Settings.FuelOnStartup);

            // Turn off the lander physics so we don't fly about
            LanderPhysics.Instance.UseGravity = false;
            LanderPhysics.Instance.ApplyPhysics = false;
            LanderProximity.Instance.DetectObstacles = false;
            LanderEffects.Instance.EmitTrail = false;
            LanderEffects.Instance.ShowGyro = false;
            LanderEffects.Instance.ShowLander();
            LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.Hidden;

            // Let everyone know gameplay has started
            OnGameplayStarted();

            // Wait for the tutorial to finish
            while (display.gameObject.activeSelf) {
                yield return null;
            }

            // End fake gameplay
            gameInProgress = false;
            LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.Hidden;
            // Hide lander
            LanderEffects.Instance.HideLander();
            yield break;
        }       

        private IEnumerator GameStartOverTime() {

            currentSession = new GameplaySession(Time.time, Settings.FuelOnStartup);

            // Set up our lander for entry
            LanderEffects.Instance.ShowGyro = false;
            LanderEffects.Instance.EmitTrail = false;

            // Move the lander into position above the landing pad
            Vector3 landingPadPosition = LandingPadManager.Instance.LandingPad.transform.position;
            Vector3 landerStartPosition = LandingPadManager.Instance.LanderStartupPosition;

            // Get a random position around the room that doesn't collide with the walls
            /*bool foundRandomPosition = false;
            Vector3 landerStartPosition = Vector3.zero;
            while (!foundRandomPosition) {
                Vector3 randomPosition = (UnityEngine.Random.onUnitSphere * Settings.StartDistanceFromPad);
                randomPosition.y = Mathf.Abs(randomPosition.y) + Settings.MinStartAltitude;
                landerStartPosition = landingPadPosition + randomPosition;

                // If the position is too close to player, skip this position
                if (Vector3.Distance(landerStartPosition, CameraCache.Main.transform.position) < Settings.MinDistanceFromPlayer) {
                    yield return null;
                    continue;
                }

                // If the position is behind the player, skip this position
                Vector3 dir = landerStartPosition - CameraCache.Main.transform.position;
                dir.y = 0f;
                Vector3 forward = CameraCache.Main.transform.forward;
                forward.y = 0f;

                dir.Normalize();
                forward.Normalize();

                // If the position is behind the player, continue
                if (Vector3.Dot(dir, forward) < 0.5) {
                    yield return null;
                    continue;
                }

                // Make sure the lander doesn't intersect with any room stuff
                Collider[] colliders = Physics.OverlapSphere(landerStartPosition, Settings.PlacementCheckRadius, 1 << EnvironmentManager.RoomSurfaceLayer, QueryTriggerInteraction.Ignore);
                if (colliders.Length == 0) {
                    foundRandomPosition = true;
                }
                yield return null;
            }*/

            // Turn on our effects for entry
            LanderEffects.Instance.ForceThrust = true;
            LanderEffects.Instance.ShowLander();
            LanderAudio.Instance.ForceThrustVolume = 1f;
            LanderInput.Instance.SetForward(Vector3.forward);
            LanderInput.Instance.ResetInput();

            GameplayMessage.Instance.DisplayMessage("Module Incoming...");

            // Get the lander into position with the opening animation
            LanderOpening.Instance.DoLanderOpening(landerStartPosition);
            while (!LanderOpening.Instance.InPosition) {
                yield return null;
            }

            yield return new WaitForSeconds(0.25f);

            // Set everything up for user-controlled motion
            LanderEffects.Instance.ShowGyro = true;
            LanderEffects.Instance.ForceThrust = false;
            LanderEffects.Instance.EmitTrail = true;
            LanderPhysics.Instance.LanderPosition = landerStartPosition;
            LanderPhysics.Instance.UseGravity = true;
            LanderPhysics.Instance.ApplyPhysics = true;
            LanderProximity.Instance.DetectObstacles = true;
            LanderInput.Instance.ApplyInput = true;
            LanderAudio.Instance.ForceThrustVolume = 0f;
            LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.Normal;
            positionLastFrame = landerStartPosition;

            GameplayMessage.Instance.DisplayMessage("Begin!");

            // Start the game
            gameInProgress = true;

            // Let everyone know gameplay has started
            OnGameplayStarted();

            yield break;
        }     

        private void Update() {
            if (!GameInProgress) {
                paused = false;
                return;
            }

            paused = LanderInput.Instance.InputSourceLost;
            
            // Update fuel consumption
            float torqueFuelConsumption = Mathf.Abs(LanderPhysics.Instance.AxisLeftRight) + Mathf.Abs(LanderPhysics.Instance.AxisFrontBack) * Settings.TorqueFuelConsumption;
            float thrustFuelConsumption = LanderInput.Instance.TargetThrust * Settings.ThrustFuelConsumption;

            currentSession.Fuel = Mathf.Clamp(currentSession.Fuel - (torqueFuelConsumption + thrustFuelConsumption), 0f, float.MaxValue);

            // If we're out of gas physics can't move
            if (currentSession.Fuel <= 0) {
                LanderInput.Instance.ApplyInput = false;
            }
        }

        private void FixedUpdate() {
            if (!GameInProgress || Paused) {
                return;
            }

            // Check our altitude and speed
            float landingPadAltitude = Mathf.Infinity;
            if (LandingPadManager.Instance.LandingPlacementConfirmed) {
                landingPadAltitude = LandingPadManager.Instance.LandingPad.transform.position.y;
                currentSession.Altitude = Mathf.Abs(LanderPhysics.Instance.LanderPosition.y - landingPadAltitude);
            }

            // TODO make this similar to arcade version
            Vector3 movement = LanderPhysics.Instance.LanderPosition - positionLastFrame;
            currentSession.VerticalSpeed = Mathf.Abs(Mathf.Clamp (movement.y, Mathf.NegativeInfinity, 0f));
            movement.y = 0f;
            currentSession.HorizontalSpeed = movement.magnitude;

            positionLastFrame = LanderPhysics.Instance.LanderPosition;
        }

        private void OnLand(Vector3 landPoint) {

            currentSession.LandPoint = landPoint;
            if (LandingAngleDanger || LandingSpeedDanger || LanderPhysics.Instance.UnsafeCollision) {
                currentSession.HasCrashed = true;
            }

            EndGameplay();
        }

        private void EndGameplay() {

            currentSession.HasLanded = true;
            // TODO calculate real score
            currentSession.Score = currentSession.HasCrashed ? 0 : Mathf.Clamp(Fuel - GameTime, 0, int.MaxValue);
            currentSession.TimeGameStopped = Time.time;
            gameInProgress = false;
            LanderInput.Instance.ThrottleVisibility = LanderInput.ThrottleVisibilityEnum.Hidden;

            OnGameplayEnded();
        }
    }
}