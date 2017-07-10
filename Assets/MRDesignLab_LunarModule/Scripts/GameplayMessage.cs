//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX;
using HUX.Utility;
using UnityEngine;

namespace MRDL {
    public class GameplayMessage : Singleton<GameplayMessage> {

        public AnimationCurve AlphaCurve;
        public float MessageDuration = 2f;
        public float StartScale = 1f;
        public float EndScale = 1.5f;
        public Color MessageColor = Color.white;

        [SerializeField]
        private TextMesh textMesh;

        private float lastMessageDisplayed;

        public void DisplayMessage (string message) {
            textMesh.text = message;
            textMesh.color = MessageColor;
            textMesh.transform.localScale = Vector3.one * StartScale;
            lastMessageDisplayed = Time.time;
        }


        public void DisplayMessage(string message, Color color) {
            textMesh.text = message;
            textMesh.color = color;
            textMesh.transform.localScale = Vector3.one * StartScale;
            lastMessageDisplayed = Time.time;
        }

        private void OnEnable() {
            lastMessageDisplayed = Time.time - MessageDuration;
        }

        private void Update () {
            if (Time.time > lastMessageDisplayed + MessageDuration) {
                textMesh.gameObject.SetActive(false);
            } else {
                float normalizedTime = (Time.time - lastMessageDisplayed) / MessageDuration;
                textMesh.gameObject.SetActive(true);
                textMesh.transform.localScale = Vector3.Lerp(Vector3.one * StartScale, Vector3.one * EndScale, normalizedTime);
                Color color = textMesh.color;
                color.a = AlphaCurve.Evaluate(normalizedTime);
                textMesh.color = color;
            }
        }
    }
}
