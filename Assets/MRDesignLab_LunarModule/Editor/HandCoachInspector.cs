//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Interaction;
using UnityEditor;
using UnityEngine;

namespace HUX
{
    [CustomEditor(typeof(HandCoach))]
    public class HandCoachInspector : Editor
    {
        public override void OnInspectorGUI() {
            HandCoach coach = (HandCoach)target;

            //DrawDefaultInspector();

            HUXEditorUtils.BeginSectionBox("Material settings");

            coach.HandMaterial = (Material)EditorGUILayout.ObjectField("Material", coach.HandMaterial, typeof(Material), false);
            coach.HighlightColor = EditorGUILayout.ColorField("Highlight color", coach.HighlightColor);
            coach.NormalColor = EditorGUILayout.ColorField("Normal color", coach.NormalColor);
            coach.TrackedColor = EditorGUILayout.ColorField("Tracked color", coach.TrackedColor);
            coach.TrackingLostColor = EditorGUILayout.ColorField("Tracking lost color", coach.TrackingLostColor);

            coach.HighlightColorProperty = HUXEditorUtils.MaterialPropertyName(coach.HighlightColorProperty, coach.HandMaterial, ShaderUtil.ShaderPropertyType.Color, false, "_Color", "Highlight");
            coach.TrackingColorProperty = HUXEditorUtils.MaterialPropertyName(coach.TrackingColorProperty, coach.HandMaterial, ShaderUtil.ShaderPropertyType.Color, false, "_Color", "Tracking");
            coach.MaterialTransparencyIsFloat = EditorGUILayout.Toggle("Use float property for transparency", coach.MaterialTransparencyIsFloat);
            if (coach.MaterialTransparencyIsFloat) {
                coach.MaterialTransparencyProperty = HUXEditorUtils.MaterialPropertyName(coach.MaterialTransparencyProperty, coach.HandMaterial, ShaderUtil.ShaderPropertyType.Range, false, "_Color", "Transparency");
            } else {
                coach.MaterialTransparencyProperty = HUXEditorUtils.MaterialPropertyName(coach.MaterialTransparencyProperty, coach.HandMaterial, ShaderUtil.ShaderPropertyType.Color, false, "_Color", "Transparency");
            }

            coach.Highlight = (HandCoach.HandVisibilityEnum)EditorGUILayout.EnumPopup("Hands to highlight", coach.Highlight);

            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Tracking settings");
            
            coach.CheckTracking = (HandCoach.HandVisibilityEnum)EditorGUILayout.EnumPopup("Check tracking", coach.CheckTracking);
            coach.AutoGhostLostTracking = EditorGUILayout.Toggle("Auto-ghost hands when tracking lost", coach.AutoGhostLostTracking);
            coach.Ghosting = (HandCoach.HandVisibilityEnum)EditorGUILayout.EnumPopup("Ghosting", coach.Ghosting);
            coach.Tracking = (HandCoach.HandVisibilityEnum)EditorGUILayout.EnumPopup("Tracking", coach.Tracking);

            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Gesture settings");
            coach.Visibility = (HandCoach.HandVisibilityEnum)EditorGUILayout.EnumPopup("Hands to show", coach.Visibility);
            coach.AutoLowerOnInvisible = EditorGUILayout.Toggle("Auto-lower on invisible", coach.AutoLowerOnInvisible);

            coach.RightGesture = (HandCoach.HandGestureEnum)EditorGUILayout.EnumPopup("Right gesture", coach.RightGesture);
            coach.LeftGesture = (HandCoach.HandGestureEnum)EditorGUILayout.EnumPopup("Left gesture", coach.LeftGesture);

            coach.RightMovement = (HandCoach.HandMovementEnum)EditorGUILayout.EnumPopup("Right movement", coach.RightMovement);
            coach.LeftMovement = (HandCoach.HandMovementEnum)EditorGUILayout.EnumPopup("Left movement", coach.LeftMovement);

            coach.RightDirection = (HandCoach.HandDirectionEnum)HUXEditorUtils.EnumCheckboxField<HandCoach.HandDirectionEnum>("Right hand direction", coach.RightDirection, "None", HandCoach.HandDirectionEnum.None);
            coach.LeftDirection = (HandCoach.HandDirectionEnum)HUXEditorUtils.EnumCheckboxField<HandCoach.HandDirectionEnum>("Left hand direction", coach.LeftDirection, "None", HandCoach.HandDirectionEnum.None);

            coach.StaticCurve = EditorGUILayout.CurveField("Static movement curve", coach.StaticCurve);
            coach.DirectionalCurve = EditorGUILayout.CurveField("Directional movement curve", coach.DirectionalCurve);
            coach.DirectionalTransparencyCurve = EditorGUILayout.CurveField("Directional movement transparency curve", coach.DirectionalTransparencyCurve);
            coach.PingPongCurve = EditorGUILayout.CurveField("Ping pong movement curve", coach.PingPongCurve);

            HUXEditorUtils.EndSectionBox();

            serializedObject.ApplyModifiedProperties();

            HUXEditorUtils.SaveChanges(target);
        }
    }
}