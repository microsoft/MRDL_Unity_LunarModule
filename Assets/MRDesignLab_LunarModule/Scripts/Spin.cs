//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Simple class to spin stuff, yo
    /// </summary>
    public class Spin : MonoBehaviour
    {
        public float SpinX;
        public float SpinY;
        public float SpinZ;

        void Update() {
            transform.Rotate(SpinX * Time.deltaTime, SpinY * Time.deltaTime, SpinZ * Time.deltaTime, Space.Self);
        }
    }
}