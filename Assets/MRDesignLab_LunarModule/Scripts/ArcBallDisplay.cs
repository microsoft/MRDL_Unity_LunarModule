//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Utility;
using MRDL;
using UnityEngine;

namespace HUX.Interaction
{
    public class ArcBallDisplay : MonoBehaviour
    {
        [SerializeField]
        private Transform arrows;
        [SerializeField]
        private Transform hemisphere;
        [SerializeField]
        private Transform indicator;
        [SerializeField]
        private LocalHandInput handInput;

        public Color HemisphereColor;
        public Color ArrowsColor;
        public Color IndicatorColorFront;
        public Color IndicatorColorBack;

        public float MaxArrowRotation = 30f;
        public float MaxIndicatorRotation = 90f;

        private float horizontal;
        private float vertical;
        private Vector3 targetArrowAngles;
        private Vector3 targetIndicatorAngles;

        // Colors and materials modified in realtime
        private Material hemisphereMat;
        private Material arrowsMat;
        private Material[] indicatorMats;
        
        protected void OnEnable() {
            if (hemisphereMat == null) {
                hemisphereMat = hemisphere.GetComponent<Renderer>().material;
                arrowsMat = arrows.GetComponent<Renderer>().material;
                indicatorMats = indicator.GetComponent<Renderer>().materials;
            }
        }
        
        protected void Update()
        {
            float black = 1f;
            if (handInput.Pressed) {
                black = 0.1f;
            }
            hemisphereMat.color = Color.Lerp(HemisphereColor, Color.black, black);
            arrowsMat.color = Color.Lerp(ArrowsColor, Color.black, black);
            indicatorMats[0].color = Color.Lerp(IndicatorColorFront, Color.black, black);
            indicatorMats[1].color = Color.Lerp(IndicatorColorBack, Color.black, black);

            targetArrowAngles.y = 0f;
            targetArrowAngles.x = Mathf.Lerp(-MaxArrowRotation, MaxArrowRotation, (handInput.LocalPosition.x + 1f / 2));
            targetArrowAngles.z = Mathf.Lerp(MaxArrowRotation, -MaxArrowRotation, (handInput.LocalPosition.y + 1f / 2));

            targetIndicatorAngles.y = 0f;
            targetIndicatorAngles.x = targetArrowAngles.x; //ClampAngle(targetArrowAngles.x, -MaxIndicatorRotation, MaxIndicatorRotation);
            targetIndicatorAngles.z = targetArrowAngles.z; //ClampAngle(targetArrowAngles.z, -MaxIndicatorRotation, MaxIndicatorRotation);

            // Smooth out movement just slightly
            indicator.localRotation = Quaternion.Lerp (indicator.localRotation, Quaternion.Euler (targetIndicatorAngles), 0.5f);
            arrows.localRotation = Quaternion.Lerp (arrows.localRotation, Quaternion.Euler (targetArrowAngles), 0.5f);
        }

        protected float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        protected float ClampAngle(float angle, float min, float max)
        {

            angle = NormalizeAngle(angle);
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }

            min = NormalizeAngle(min);
            if (min > 180)
            {
                min -= 360;
            }
            else if (min < -180)
            {
                min += 360;
            }

            max = NormalizeAngle(max);
            if (max > 180)
            {
                max -= 360;
            }
            else if (max < -180)
            {
                max += 360;
            }
            
            return Mathf.Clamp(angle, min, max);
        }
    }
}