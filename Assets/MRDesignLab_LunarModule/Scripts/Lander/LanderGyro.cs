//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL
{
    public class LanderGyro : MonoBehaviour
    {
        public Transform Target;
        public Vector3 PositionOffset = Vector3.zero;
        public Material RingMaterial;
        public Material FuelMaterial;
        public Color BaseColor = Color.white;
        public Color WarningColor = Color.red;
        public float Fuel = 1f;

        public AnimationCurve WarningColorCurve;

        [SerializeField]
        private Transform ringTransform;

        [SerializeField]
        private Renderer ringRenderer;

        [SerializeField]
        private Renderer fuelRenderer;

        private Material ringMat;
        private Material fuelMat;
        private float ringDot;
        private float minArrowScale = 0.675f;
        private float maxArrowScale = 1.25f;
        private float fuelOffset;

        private void OnEnable() {
            fuelOffset = -1;
        }

        private void Update()
        {
            if (Target == null || !Target.gameObject.activeSelf || LanderGameplay.Instance.HasLanded) {
                // Go invisible
                ringRenderer.enabled = false;
                fuelRenderer.enabled = false;
                return;
            }

            fuelOffset = Mathf.Clamp(fuelOffset + Time.deltaTime, -1f, 0f);

            if (ringMat == null)
            {
                ringMat = new Material(RingMaterial);
                fuelMat = new Material(FuelMaterial);
            }

            transform.position = Target.position + PositionOffset;
            ringTransform.rotation = Target.rotation;

            ringRenderer.enabled = true;
            ringRenderer.sharedMaterial = ringMat;

            fuelRenderer.enabled = true;
            fuelRenderer.sharedMaterial = fuelMat;

            ringDot = Vector3.Dot(Vector3.up, ringTransform.up);

            // If the dot is within the optimal range, show the optimal color
            if (ringDot > LanderGameplay.Instance.Settings.MinSafeCollisionDot) {
                ringMat.color = BaseColor;
            } else {
                // Otherwise show the regular color
                ringMat.color = WarningColor;// Color.Lerp(BaseColor, WarningColor, WarningColorCurve.Evaluate(ringDot));
            }
;
            fuelMat.SetFloat("_CutOff", Mathf.Clamp01 ((1f - Fuel) - fuelOffset));
        }

        private void OnDisable ()
        {
            if (!Application.isPlaying)
            {
                GameObject.DestroyImmediate(ringMat);
                GameObject.DestroyImmediate(fuelMat);
            }
        }
    }
}