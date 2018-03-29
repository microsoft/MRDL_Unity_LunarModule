//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HUX;
using System.Collections.Generic;
using UnityEngine;
using MixedRealityToolkit.Common;

namespace MRDL
{
    public class StartupPad : MonoBehaviour
    {
        public enum StartupPadStateEnum
        {
            PlacingInvalid,
            PlacingValid,
            PlacingHidden,
            Placed,
            Hidden,
        }

        public StartupPadStateEnum State {
            get {
                return state;
            }set {
                state = value;
            }
        }
        
        public Color ColorValid = Color.magenta;
        public Color ColorInvalid = Color.red;
        public float ColumnRotateSpeed = 1f;
        public Vector3 LanderPosition {
            get {
                return landerPositionTransform.position;
            }
        }

        public List<Collider> IntersectingColliders = new List<Collider>();

        private void OnEnable() {
            if (columnMat == null) {
                columnMat = columnRenderer.material;
                columnStartColor = columnMat.color;
            }

            ringRenderer.material.color = Color.black;
            glowRenderer.material.SetColor("_TintColor", Color.black);
            ringRenderer.material.SetFloat("_ZTest", 8);
            columnMat.color = Color.black;

            landerObject.transform.parent = null;

            state = StartupPadStateEnum.Hidden;
        }

        private void OnDisable() {
            IntersectingColliders.Clear();
        }

        private void Update() {

            Color glowTargetColor = ColorInvalid;
            Color columnTargetColor = columnStartColor;
            Vector3 targetScale = Vector3.one;
            bool setStartupPosition = false;

            switch (State)
            {
                case StartupPadStateEnum.PlacingValid:
                    setStartupPosition = true;
                    mainCollider.enabled = true;
                    glowTargetColor = ColorValid;
                    columnTargetColor = ColorValid;
                    landerObject.SetActive(true);
                    break;

                case StartupPadStateEnum.PlacingInvalid:
                    setStartupPosition = true;
                    mainCollider.enabled = true;
                    glowTargetColor = ColorInvalid;
                    columnTargetColor = ColorInvalid;
                    landerObject.SetActive(true);
                    break;

                case StartupPadStateEnum.PlacingHidden:
                    mainCollider.enabled = true;
                    glowTargetColor = Color.black;
                    columnTargetColor = Color.black;
                    landerObject.SetActive(false);
                    targetScale.x = 0.1f;
                    targetScale.z = 0.1f;
                    break;

                case StartupPadStateEnum.Placed:
                    glowTargetColor = ColorValid;
                    columnTargetColor = ColorValid;
                    landerObject.SetActive(true);
                    if (prevState != StartupPadStateEnum.Placed) {
                        dustPuff.Play();
                    }
                    IntersectingColliders.Clear();
                    break;

                case StartupPadStateEnum.Hidden:
                default:
                    mainCollider.enabled = false;
                    glowTargetColor = Color.black;
                    columnTargetColor = Color.black;
                    targetScale.x = 0.1f;
                    targetScale.z = 0.1f;
                    landerObject.SetActive(false);
                    dustPuff.Stop();
                    IntersectingColliders.Clear();
                    break;
            }

            if (setStartupPosition) {
                Vector3 landerPosition = landerPositionTransform.position;
                landerPosition.y = CameraCache.Main.transform.position.y;
                landerPositionTransform.position = landerPosition;
                landerObject.transform.position = landerPosition;
            }

            ringRenderer.material.color = Color.Lerp (ringRenderer.material.color, glowTargetColor, Time.deltaTime * 10);
            glowRenderer.material.SetColor("_TintColor", Color.Lerp (glowRenderer.material.GetColor ("_TintColor"), glowTargetColor, Time.deltaTime * 10));

            columnTargetColor.a = columnStartColor.a;
            columnMat.color = Color.Lerp(columnMat.color, columnTargetColor, Time.deltaTime * 10);
            columnRenderer.transform.Rotate(0f, ColumnRotateSpeed, 0f);

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10);

            for (int i = IntersectingColliders.Count - 1; i >= 0; i--) {
                if (IntersectingColliders [i] == null) {
                    IntersectingColliders.RemoveAt(i);
                }
            }

            prevState = state;
        }

        void OnTriggerEnter (Collider other) {
            IntersectingColliders.Add(other);
        }

        void OnTriggerExit (Collider other) {
            IntersectingColliders.Remove(other);
        }

        [SerializeField]
        private GameObject landerObject;
        [SerializeField]
        private Renderer columnRenderer;
        [SerializeField]
        private Renderer glowRenderer;
        [SerializeField]
        private Renderer ringRenderer;
        [SerializeField]
        public ParticleSystem dustPuff;
        [SerializeField]
        private Transform landerPositionTransform;
        [SerializeField]
        private Collider mainCollider;

        
        private StartupPadStateEnum state;
        private StartupPadStateEnum prevState;
        private Material columnMat;
        private Color columnStartColor;
        private float pointValueObjectScale;
    }
}