//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Attaches to a custom prefab for the loading dialog
    /// </summary>
    public class MoonProgress : MonoBehaviour
    {
        public float MinLightRotate = 120f;
        public float MaxLightRotate = 0;
        public Transform LightTransform;

        void Update() {
            LightTransform.localEulerAngles = new Vector3(0f, Mathf.Lerp(MinLightRotate, MaxLightRotate, LoadingDialog.Instance.Progress / 100), 0f);
        }
    }
}
