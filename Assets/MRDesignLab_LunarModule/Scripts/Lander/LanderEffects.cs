//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using MixedRealityToolkit.Common;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL
{
    public class LanderEffects : Singleton<LanderEffects>
    {
        public float ForceEmissionMultiplier;
        public float TorqueEmissionMultiplier;
        public float CrashExplosionForce = 10f;

        public bool EmitTrail = true;
        public bool ForceThrust = false;
        public bool ForceTorque = false;
        public bool ShowGyro = true;
        public float DustPuffDistance = 0.15f;

        [SerializeField]
        private LanderGyro gyro;

        [SerializeField]
        private GameObject[] landerObjects;

        [SerializeField]
        private ParticleSystem landParticles;
        [SerializeField]
        private ParticleSystem crashParticles;
        [SerializeField]
        private Transform upThruster;
        [SerializeField]
        private Transform[] pitchThrustersPositive;
        [SerializeField]
        private Transform[] rollThrustersPositive;
        [SerializeField]
        private Transform[] yawThrustersPositive;
        [SerializeField]
        private Transform[] pitchThrustersNegative;
        [SerializeField]
        private Transform[] rollThrustersNegative;
        [SerializeField]
        private Transform[] yawThrustersNegative;
        [SerializeField]
        private Transform gasTransform;
        [SerializeField]
        private Light thrusterLight;
        [SerializeField]
        private Projector shadowProjector;

        [SerializeField]
        private ParticleSystem trailParticles;

        private ParticleSystem.EmissionModule landParticleEmission;
        private float landParticlesEmissionRate;

        private void Start() {
            LanderGameplay.Instance.OnGameplayStarted += OnGameplayStarted;
            LanderGameplay.Instance.OnGameplayEnded += OnGameplayEnded;

            scrapPile = new GameObject("Scrap Pile").transform;

            // Turn off our lander
            for (int i = 0; i < landerObjects.Length; i++) {
                landerObjects[i].SetActive(false);
            }

            landParticleEmission = landParticles.emission;
            landParticlesEmissionRate = landParticleEmission.rateOverTime.constant;
        }

        private void OnGameplayEnded() {
            if (LanderGameplay.Instance.HasCrashed) {
                // Turn off our lander objects
                crashParticles.Play();
                ExplodePieces();
                shadowProjector.enabled = false;
            }
            trailParticles.Stop();
        }

        public void HideLander() {
            // Clean up our mess
            if (scrapPile != null && scrapPile.childCount > 0) {
                GameObject.Destroy(scrapPile.gameObject);
                scrapPile = new GameObject("Scrap Pile").transform;
            }
            // Hide our lander
            for (int i = 0; i < landerObjects.Length; i++) {
                landerObjects[i].SetActive(false);
            }
            shadowProjector.enabled = false;
        }

        public void ShowLander() {
            // Hide our lander
            for (int i = 0; i < landerObjects.Length; i++) {
                landerObjects[i].SetActive(true);
            }
            shadowProjector.enabled = true;
        }

        private void OnGameplayStarted() {
            // Clean up our mess
            if (scrapPile != null && scrapPile.childCount > 0) {
                GameObject.Destroy(scrapPile.gameObject);
                scrapPile = new GameObject("Scrap Pile").transform;
            }
            // Create the thing we're going to explode
            CreateExplosionPieces();
        }

        private void Update() {
            if (LanderGameplay.Instance.GameInProgress) {
                gyro.Fuel = (float)LanderGameplay.Instance.Fuel / LanderGameplay.Instance.Settings.FuelOnStartup;
            } else {
                gyro.Fuel = 1f;
            }

            if (LanderGameplay.Instance.GameInProgress && LandingPadManager.Instance.LandingPlacementConfirmed) {
                Vector3 particlePosition = LanderPhysics.Instance.LanderPosition;
                float distance = particlePosition.y - LandingPadManager.Instance.LandingPadPosition.y;
                particlePosition.y = LandingPadManager.Instance.LandingPadPosition.y;
                landParticles.transform.position = particlePosition;
                if (distance < DustPuffDistance) {
                    ParticleSystem.MinMaxCurve rate = landParticleEmission.rateOverTime;
                    rate.constant = (1f - (distance / DustPuffDistance)) * landParticlesEmissionRate;
                    landParticleEmission.rateOverTime = rate;
                    if (LanderGameplay.Instance.HasLanded) {
                        landParticles.Stop();
                    } else {
                        landParticles.Play();
                    }
                } else {
                    landParticles.Stop();
                }
            } else {
                landParticles.Stop();
            }

            // Get thrust and rotation from input / physics
            float thrust = LanderInput.Instance.TargetThrust;
            float axisLeftRight = LanderPhysics.Instance.AxisLeftRight;
            float axisFrontBack = LanderPhysics.Instance.AxisFrontBack;
            float axisUpDown = LanderPhysics.Instance.AxisUpDown;

            if (ForceThrust) {
                thrust = 0.5f;
            }

            if (ForceTorque) {
                // Generate random torque values for show
                axisLeftRight = Mathf.Lerp (-0.4f, 0.4f, Mathf.PingPong(Time.time * 2, 1f));
                axisFrontBack = Mathf.Lerp(-0.4f, 0.4f, Mathf.PingPong(Time.time * 2 + 0.333f, 1f));
                axisUpDown = Mathf.Lerp(-0.4f, 0.4f, Mathf.PingPong(Time.time * 2 + 0.666f, 1f));
            }

            upThruster.transform.localScale = Vector3.one * Random.Range(0.85f, 1.15f) * ForceEmissionMultiplier * Mathf.Clamp01(thrust);
            thrusterLight.intensity = LanderInput.Instance.TargetThrust * 2;

            if (axisLeftRight < 0f) {
                SetModuleValues(rollThrustersPositive, 0);
                SetModuleValues(rollThrustersNegative, TorqueEmissionMultiplier * Mathf.Abs(axisLeftRight));
            } else {
                SetModuleValues(rollThrustersPositive, TorqueEmissionMultiplier * axisLeftRight);
                SetModuleValues(rollThrustersNegative, 0);
            }
            if (axisFrontBack < 0f) {
                SetModuleValues(pitchThrustersPositive, 0);
                SetModuleValues(pitchThrustersNegative, TorqueEmissionMultiplier * Mathf.Abs(axisFrontBack));
            } else {
                SetModuleValues(pitchThrustersPositive, TorqueEmissionMultiplier * axisFrontBack);
                SetModuleValues(pitchThrustersNegative, 0);
            }
            if (axisUpDown < 0f) {
                SetModuleValues(yawThrustersPositive, TorqueEmissionMultiplier * Mathf.Abs(axisUpDown));
                SetModuleValues(yawThrustersNegative, 0);
            } else {
                SetModuleValues(yawThrustersPositive, TorqueEmissionMultiplier * axisUpDown);
                SetModuleValues(yawThrustersNegative, 0);
            }

            gasTransform.localScale = new Vector3(1f, (float)LanderGameplay.Instance.Fuel / LanderGameplay.Instance.Settings.FuelOnStartup, 1f);
            
            // Take care of our tutorial settings
            if (ShowGyro) {
                gyro.gameObject.SetActive(true);
            } else {
                gyro.gameObject.SetActive(false);
            }

            if (EmitTrail) {
                if (!trailParticles.isPlaying) {
                    trailParticles.Play();
                }
            } else if (trailParticles.isPlaying) {
                trailParticles.Stop();
            }

            // Project a shadow below us
            shadowProjector.transform.forward = Vector3.down;
        }

        private void SetModuleValues(Transform[] modules, float value) {
            for (int i = 0; i < modules.Length; i++) {
                modules[i].localScale = Vector3.one * Random.Range(0.45f, 0.65f) * value;
            }
        }

        private void CreateExplosionPieces() {
            // Create a clone of the lander object, but with each collider as a rigid body
            // Parent them under the scrapPile so we can delete them easily later
            scrapPile.transform.position = LanderPhysics.Instance.LanderPosition;
            scrapPile.transform.rotation = LanderPhysics.Instance.LanderRotation;
            scrapPile.gameObject.SetActive(false);
            List<Collider> allColliders = new List<Collider>();
            foreach (GameObject landerObject in landerObjects) {
                Collider[] colliders = landerObject.GetComponentsInChildren<Collider>();
                foreach (Collider pieceCollider in colliders) {
                    // Copy the piece
                    GameObject newPiece = GameObject.Instantiate(pieceCollider.gameObject, pieceCollider.transform.position, pieceCollider.transform.rotation, scrapPile);
                    newPiece.transform.localScale = pieceCollider.transform.lossyScale;
                    // Add a rigid body to the piece and make it explode
                    Rigidbody pieceRigidbody = newPiece.gameObject.AddComponent<Rigidbody>();
                    Collider newCollider = newPiece.GetComponent<Collider>();
                    allColliders.Add(newCollider);
                    // If it's a body piece make it heavier
                    if (newPiece.CompareTag("LanderBody")) {
                        pieceRigidbody.mass = 2f;
                    } else {
                        pieceRigidbody.mass = 0.25f;
                    }
                    pieceRigidbody.drag = 0.5f;
                    pieceRigidbody.angularDrag = 0.5f;
                    pieceRigidbody.AddExplosionForce(Random.Range(CrashExplosionForce * 0.5f, CrashExplosionForce), transform.position, 1f);
                    pieceRigidbody.AddTorque(Random.insideUnitSphere * CrashExplosionForce, ForceMode.Impulse);
                }
            }
        }

        private void ExplodePieces() {

            // Turn off our lander
            for (int i = 0; i < landerObjects.Length; i++) {
                landerObjects[i].SetActive(false);
            }

            scrapPile.transform.position = LanderPhysics.Instance.LanderPosition;
            scrapPile.transform.rotation = LanderPhysics.Instance.LanderRotation;
            // Turn on our exploded stuff
            scrapPile.gameObject.SetActive(true);
        }

        private Transform scrapPile;
    }
}