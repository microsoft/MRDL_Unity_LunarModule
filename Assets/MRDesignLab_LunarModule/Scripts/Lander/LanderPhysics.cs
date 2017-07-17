//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Utility;
using System;
using UnityEngine;

namespace MRDL
{
    public class LanderPhysics : Singleton<LanderPhysics>
    {
        const string LandingPadTag = "LandingPad";
        const string MoonSurfaceTag = "MoonSurface";

        public Action<Vector3> OnLand;

        public float Gravity = -0.1f;

        public float SafeCeilingCollisionDot = 0.5f;
        public bool ApplyPhysics = true;
        public bool UseGravity
        {
            set
            {
                if (value)
                {
                    gravityTarget = Vector3.up * Gravity;
                }
                else
                {
                    Physics.gravity = Vector3.zero;
                    gravityTarget = Vector3.zero;
                }
            }
        }
        public float ForwardDampening = 0.5f;
        public float ForceMultiplier;
        public float TorqueMultiplier;

        public Vector3 DemoRotation;

        public Vector3 LanderPosition
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }

        public Quaternion LanderRotation
        {
            get
            {
                return transform.rotation;
            }
        }

        public Quaternion LanderTargetRotation
        {
            set
            {
                targetRotation = value;
            }
        }

        public Transform LanderTransform
        {
            get
            {
                return transform;
            }
        }

        public bool UnsafeCollision
        {
            get
            {
                return unsafeCollision;
            }
        }

        public float LanderDot
        {
            get
            {
                return landerDot;
            }
        }

        public float AxisLeftRight
        {
            get
            {
                return Mathf.Clamp(axisLeftRight, -1f, 1f);
            }
        }

        public float AxisFrontBack
        {
            get
            {
                return Mathf.Clamp(axisFrontBack, -1f, 1f);
            }
        }

        public float AxisUpDown
        {
            get
            {
                return Mathf.Clamp(axisUpDown, -1f, 1f);
            }
        }

        public void ForceLanding(Vector3 landPoint)
        {
            OnLand(landPoint);
        }

        private void Start()
        {
            LanderGameplay.Instance.OnGameplayStarted += OnGameplayStarted;
        }

        private void OnGameplayStarted()
        {
            // Reset
            collided = false;
            unsafeCollision = false;
            rotationLastFrame = Vector3.zero;
            transform.rotation = Quaternion.identity;
            Physics.gravity = Vector3.zero;
        }

        private void Update()
        {
            if (!LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused)
            {
                return;
            }

            // Calculate axis front/back left/right
            // Compare our rotation in world space
            // Do just a bit of smoothing
            Vector3 rotationThisFrame = LanderTransform.eulerAngles;
            axisLeftRight = Mathf.Lerp(axisLeftRight, -Mathf.DeltaAngle(rotationThisFrame.x, rotationLastFrame.x), 0.85f);
            axisFrontBack = Mathf.Lerp(axisFrontBack, -Mathf.DeltaAngle(rotationThisFrame.z, rotationLastFrame.z), 0.85f);
            axisUpDown = Mathf.Lerp(axisUpDown, -Mathf.DeltaAngle(rotationThisFrame.y, rotationLastFrame.y), 0.85f);
            rotationLastFrame = rotationThisFrame;

            //we turn off rigidbody collisions if the lander is above user eye height
            mainRigidbody.detectCollisions = LanderPosition.y <= LandingPadManager.Instance.LanderStartupPosition.y;

        }

        private void FixedUpdate()
        {
            if (LanderGameplay.Instance.HasLanded && !LanderGameplay.Instance.HasCrashed)
            {
                // Shut off physics and lerp to the landing pad position
                mainRigidbody.isKinematic = true;
                mainRigidbody.MovePosition(Vector3.Lerp(mainRigidbody.position, LandingPadManager.Instance.LandingPadPosition, Time.deltaTime));
                axisUpDown = 0f;
                axisLeftRight = 0f;
                axisFrontBack = 0f;
                return;
            }

            if (!LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused)
            {
                mainRigidbody.isKinematic = true;
                axisUpDown = 0f;
                axisLeftRight = 0f;
                axisFrontBack = 0f;
                return;
            }

            Physics.gravity = Vector3.Lerp(Physics.gravity, gravityTarget, Time.deltaTime);

            // If we're not applying physics make the rigidbody kinematic so we don't fly about
            mainRigidbody.isKinematic = !ApplyPhysics;
            // Reset angular velocity to zero every frame
            mainRigidbody.angularVelocity = Vector3.Lerp(mainRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * ForwardDampening);

            // Make sure the rigidbody never falls asleep
            if (mainRigidbody.IsSleeping())
                mainRigidbody.WakeUp();

            if (LanderGameplay.Instance.GameInProgress)
            {
                // Rotate the lander to match the target rotation
                mainRigidbody.MoveRotation(LanderInput.Instance.TargetRotation);
                if (DemoRotation != Vector3.zero)
                {
                    LanderTransform.Rotate(DemoRotation, Space.Self);
                }
            }

            // Apply up/down force
            if (LanderInput.Instance.TargetThrust != 0)
            {
                Vector3 upForce = Vector3.up * LanderInput.Instance.TargetThrust * ForceMultiplier;
                mainRigidbody.AddRelativeForce(upForce, ForceMode.VelocityChange);
            }

            if (LandingPadManager.Instance.LandingPad.State == LandingPad.LandingPadStateEnum.PlacedOccupied)
            {
                if (LanderGameplay.Instance.Settings.AutoPilotOrient)
                {
                    Vector3 predictedUp = Quaternion.AngleAxis(
                        mainRigidbody.angularVelocity.magnitude * Mathf.Rad2Deg * 0.1f / LanderGameplay.Instance.Settings.OrientationCorrectionSpeed,
                        mainRigidbody.angularVelocity) * LanderTransform.up;

                    Vector3 correctionTorque = Vector3.Cross(predictedUp, Vector3.up);
                    mainRigidbody.AddTorque(correctionTorque * LanderGameplay.Instance.Settings.OrientationCorrectionSpeed, ForceMode.VelocityChange);
                }

                if (LanderGameplay.Instance.Settings.AutoPilotLand)
                {
                    Vector3 correctionPosition = LandingPadManager.Instance.LandingPadPosition - LanderTransform.position;
                    // Don't adjust thrust
                    correctionPosition.y = 0f;
                    mainRigidbody.AddForce(correctionPosition * LanderGameplay.Instance.Settings.PositionCorrectionSpeed, ForceMode.Force);
                }
            }

            landerDot = Vector3.Dot(Vector3.up, LanderTransform.up);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!ApplyPhysics)
                return;

            if (!LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused)
                return;

            if (collided)
                return;

            Vector3 landPoint = Vector3.zero;
            foreach (ContactPoint point in collision.contacts)
            {
                landPoint = point.point;
                if (point.otherCollider.CompareTag(MoonSurfaceTag))
                {
                    float collisionDot = Vector3.Dot(point.normal, Vector3.down);
                    // If it's not a ceiling
                    if (collisionDot < SafeCeilingCollisionDot)
                    {
                        // We're dead
                        collided = true;
                        unsafeCollision = true;
                        break;
                    }
                }
                else if (point.otherCollider.CompareTag(LandingPadTag))
                {
                    collided = true;
                    break;
                }
                else
                {// it's a blocking surface and we deserve to die
                    collided = true;
                    unsafeCollision = true;
                    break;
                }
            }
            OnLand(landPoint);
        }

        [SerializeField]
        private Rigidbody mainRigidbody;
        private Vector3 rotationLastFrame;
        private Vector3 gravityTarget;
        private bool collided = false;
        private bool unsafeCollision = false;
        private Quaternion targetRotation;
        private float axisLeftRight;
        private float axisFrontBack;
        private float axisUpDown;
        private float landerDot;
    }
}