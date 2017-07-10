//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HoloToolkit.Unity;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Extends the functionality of Holotoolkit's HeadsUpDirectionIndicator
    /// </summary>
    public class HeadsUpDirectionIndicatorAnimation : MonoBehaviour
    {

        [Tooltip("Whether to show the indicator when the target is on screen")]
        public bool ShowWhenTargetIsOnScreen = true;
        public AnimationCurve ScalePulseCurve;
        public AnimationCurve PositionPulseCurve;

        private HeadsUpDirectionIndicator indicator;
        private float baseDepth;
        private float baseMarginPercent;
        private GameObject pointer;

        private IEnumerator Start()
        {
            // Wait one frame for the indicator to initialize
            yield return null;
            indicator = GetComponent<HeadsUpDirectionIndicator>();
            baseDepth = indicator.Depth;
            baseMarginPercent = indicator.IndicatorMarginPercent;
            pointer = transform.GetChild(0).gameObject;
            yield break;
        }

        private void Update()
        {
            if (indicator == null)
                return;

            indicator.Depth = ScalePulseCurve.Evaluate(Time.time) * baseDepth;
            indicator.IndicatorMarginPercent = PositionPulseCurve.Evaluate(Time.time) * baseMarginPercent;

            if (!ShowWhenTargetIsOnScreen)
            {
                if (indicator.TargetObject == null)
                {
                    pointer.SetActive(false);
                }
                else
                {
                    Vector3 screenPoint = Camera.main.WorldToViewportPoint(indicator.TargetObject.transform.position);
                    if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
                    {
                        pointer.SetActive(true);
                    }
                    else
                    {
                        pointer.SetActive(false);
                        return;
                    }
                }
            }
        }
    }
}