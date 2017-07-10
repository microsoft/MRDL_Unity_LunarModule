//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections.Generic;
using UnityEngine;

namespace MRDL
{
    public class LandingPad : MonoBehaviour
    {
        public enum LandingPadStateEnum
        {
            PlacingHidden,
            PlacingInvalid,
            PlacingValid,
            PlacedUnoccupied,
            PlacedOccupied,
            SuccessfulLanding,
            FailedLanding,
        }

        public LandingPadStateEnum State {
            get {
                return state;
            } set {
                state = value;
            }
        }

        public bool ShowWarning = false;
        public float RadiusMultiplier = 0.85f;
        public float MinPointDisplayDistance = 0.25f;
        public Color ColorValid = Color.magenta;
        public Color ColorInvalid = Color.red;
        public Color ColorOccupied = Color.white;
        public Color[] ColorDanger;
        public Texture ColumnTexture;
        public Texture ColumnOccupiedTexture;
        public float DangerColorToggleTime = 0.5f;
        public float PointValueRotateSpeed = 1f;
        public float ColumnRotateSpeed = 1f;
        public float PlatformLightRotateSpeed = 1f;
        public float PlatformLightPulseOffset = 0.25f;
        public float PlatnformLightIntensity = 0.5f;
        public AnimationCurve PlatformLightPulsePattern;
        public AnimationCurve PointValueBobPattern;

        public List<Collider> IntersectingColliders = new List<Collider>();

        public float Radius
        {
            get
            {
                return mainCollider.bounds.extents.x * RadiusMultiplier;
            }
        }

        private void OnDisable() {
            IntersectingColliders.Clear();
        }

        private void OnEnable() {
            columnsTransform.localScale = new Vector3(1f, 0.001f, 1f);

            for (int i = 0; i < columnRenderers.Length; i++) {
                if (columnMat == null) {
                    columnMat = columnRenderers[i].material;
                    columnStartColor = columnMat.color;
                } else {
                    columnRenderers[i].sharedMaterial = columnMat;
                }
            }

            ringRenderer.material.color = Color.black;
            glowRenderer.material.SetColor("_TintColor", Color.black);
            columnMat.color = Color.black;
        }

        private void Update() {

            Color glowTargetColor = ColorInvalid;
            Color lightColor = Color.white;
            Color columnTargetColor = columnStartColor;
            Vector3 columnTargetScale = Vector3.one;
            Texture columnTargetTexture = ColumnTexture;
            // Set the scale based on difficulty setting
            float landingPadScale = LanderGameplay.Instance.Settings.LandingPadScale;
            Vector3 targetScale = new Vector3(landingPadScale, 1f, landingPadScale);
            bool columnVisible = false;
            float ringZTest = 4;

            switch (State)
            {
                case LandingPadStateEnum.PlacingInvalid:
                default:
                    lightColor = ColorInvalid;
                    columnTargetColor = columnStartColor;
                    columnTargetScale.y = 0f;
                    ringZTest = 8;
                    break;

                case LandingPadStateEnum.PlacingValid:
                    glowTargetColor = ColorValid;
                    columnTargetColor = columnStartColor;
                    columnTargetScale.y = 0f;
                    ringZTest = 8;
                    break;

                case LandingPadStateEnum.PlacingHidden:
                    glowTargetColor = Color.black;
                    columnTargetColor = Color.black;
                    lightColor = Color.black;
                    columnTargetScale.y = 0f;
                    targetScale.x = 0.2f;
                    targetScale.z = 0.2f;
                    ringZTest = 8;
                    break;

                case LandingPadStateEnum.PlacedUnoccupied:
                    if (prevState == LandingPadStateEnum.PlacingValid) {
                        dustPuff.Play();
                    }
                    if (prevState != LandingPadStateEnum.PlacedUnoccupied) {
                        columnTargetColor = Color.Lerp(columnStartColor, Color.white, 0.15f);
                    }
                    columnVisible = true;
                    glowTargetColor = ColorValid;
                    break;

                case LandingPadStateEnum.PlacedOccupied:
                    if (prevState != LandingPadStateEnum.PlacedOccupied) {
                        columnTargetColor = Color.Lerp(columnStartColor, Color.white, 0.15f);
                    }
                    columnTargetTexture = ColumnOccupiedTexture;
                    columnVisible = true;
                    if (ShowWarning) {
                        if (Time.time > lastDangerToggleTime + DangerColorToggleTime) {
                            lastDangerToggleTime = Time.time;
                            lastDangerColorIndex++;
                            if (lastDangerColorIndex >= ColorDanger.Length) {
                                lastDangerColorIndex = 0;
                            }
                        }
                        glowTargetColor = ColorDanger[lastDangerColorIndex];
                    } else {
                        glowTargetColor = ColorOccupied;
                    }
                    break;
                    
                case LandingPadStateEnum.SuccessfulLanding:
                    if (prevState != LandingPadStateEnum.SuccessfulLanding) {
                        dustPuff.Play();
                    }
                    columnTargetScale.y = 0f;
                    glowTargetColor = ColorOccupied;
                    columnTargetColor = Color.black;
                    break;

                case LandingPadStateEnum.FailedLanding:
                    columnTargetScale.y = 0f;
                    glowTargetColor = ColorInvalid;
                    columnTargetColor = Color.black;
                    break;
            }

            // Rotate the lights and the point value number
            for (int i = 0; i < platformLights.Length; i++)
            {
                platformLights[i].color = lightColor;
                platformLights[i].intensity = PlatformLightPulsePattern.Evaluate(Time.time + ((i + 1) * PlatformLightPulseOffset)) * PlatnformLightIntensity;
            }

            ringRenderer.material.color = Color.Lerp(ringRenderer.material.color, glowTargetColor, Time.deltaTime * 10);
            glowRenderer.material.SetColor("_TintColor", Color.Lerp(glowRenderer.material.GetColor("_TintColor"), glowTargetColor, Time.deltaTime * 10));
            ringRenderer.material.SetFloat("_ZTest", ringZTest);

            columnMat.color = Color.Lerp(columnMat.color, columnTargetColor, Time.deltaTime);
            columnMat.mainTexture = columnTargetTexture;
            columnsTransform.localScale = Vector3.Lerp(columnsTransform.localScale, columnTargetScale, Time.deltaTime * 2);

            for (int i = 0; i < columnRenderers.Length; i++)
            {
                columnRenderers[i].enabled = columnVisible;
                columnRenderers[i].transform.Rotate(0f, (i % 2 == 0) ? ColumnRotateSpeed * i : -ColumnRotateSpeed * i, 0f);
            }

            for (int i = IntersectingColliders.Count - 1; i >= 0; i--) {
                if (IntersectingColliders[i] == null) {
                    IntersectingColliders.RemoveAt(i);
                }
            }

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10);

            prevState = state;
        }

        void OnTriggerEnter (Collider other) {
            if (other.gameObject.layer == EnvironmentManager.RoomSurfaceLayer) {
                IntersectingColliders.Add(other);
            }
        }

        void OnTriggerExit (Collider other) {
            IntersectingColliders.Remove(other);
        }

        [SerializeField]
        public ParticleSystem dustPuff;
        [SerializeField]
        private Transform scaleTransform;
        [SerializeField]
        private Collider mainCollider;
        [SerializeField]
        private Transform columnsTransform;
        [SerializeField]
        private Light[] platformLights;
        [SerializeField]
        private Renderer[] columnRenderers;
        [SerializeField]
        private Renderer glowRenderer;
        [SerializeField]
        private Renderer ringRenderer;

        private LandingPadStateEnum state;
        private LandingPadStateEnum prevState;
        private Material columnMat;
        private Color columnStartColor;
        private int lastDangerColorIndex;
        private float lastDangerToggleTime;
        private float pointValueObjectScale;
    }
}