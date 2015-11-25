﻿/*********************************************************************
 * This code is written by and belongs to DFT Games Ltd.
 * *******************************************************************
 * The license governing this asset id the Unity Asset Store EULA,
 * found here: http://unity3d.com/es/legal/as_terms hereby summarised:
 * *******************************************************************
 * Usage is granted to the licensee in conjunction with the
 * package that has been licensed and also in other final products
 * produced by the licensee and aimed to an end user.
 * It's forbidden to use this code as part of packages or assets
 * aimed to be used by developers other than the licensee
 * of the original package.
 * *******************************************************************
 *
 * Copyright 2010-2015 - DFT Games Ltd. - Version 3.0 (30 Oct. 2015)
 *
 * *******************************************************************
 */

using UnityEditor;
using UnityEngine;

namespace DFTGames.Tools.EditorTools
{
    [CustomEditor(typeof(FadeObstructors))]
    public class FadeObstructorsEditor : Editor
    {
        private FadeObstructors fader;
        private bool isDirty;
        private static Color backColor = Color.cyan;
        private static Color labelColor = Color.yellow;
        private Color backColorOriginal = Color.green;
        private Color labelColorOriginal = Color.green;

        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            fader = (FadeObstructors)target;
            Commons.DrawTexture(ResourceHelper.LogoFadeObstructors);
            backColorOriginal = GUI.backgroundColor;
            labelColorOriginal = GUI.contentColor;
            EditorGUILayout.Separator();
            Commons.SetColors(backColor, labelColor);

            EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
            bool tmpBool = EditorGUILayout.Toggle(new GUIContent("Ignore Triggers", "Do not take into account any trigger"), fader.ignoreTriggers);
            if (tmpBool != fader.ignoreTriggers)
            {
                isDirty = true;
                Undo.RecordObject(fader, "Ignore triggers");
                fader.ignoreTriggers = tmpBool;
            }
            Commons.SetColors(backColorOriginal, labelColor);
            Color tmpColor = EditorGUILayout.ColorField(new GUIContent("Fade Color", "Color shade and alpha to use during the fading"), fader.fadingColorToUse);
            if (tmpColor != fader.fadingColorToUse)
            {
                isDirty = true;
                Undo.RecordObject(fader, "Fading Color");
                fader.fadingColorToUse = tmpColor;
            }
            Commons.SetColors(backColor, labelColor);
            LayerMask tmpMask = Commons.LayerMaskField(new GUIContent("Layers to fade", "The layers we want to fade. The layers we want to fade. IF YOU USE MULTIPLE INSTANCES OF THIS SCRIPT ON THE SASME CAMERA MAKE SURE THAT EACH INSTANCE MANAGES ONLY ITS OWN LAYERS!"), fader.layersToFade);
            if (tmpMask != fader.layersToFade)
            {
                isDirty = true;
                Undo.RecordObject(fader, "Layers to fade");
                fader.layersToFade = tmpMask;
            }
            SerializedProperty playerT = serializedObject.FindProperty("playerTransform");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(playerT, new GUIContent("Player Transform", "The Transform of the Player"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Player Transform");
                serializedObject.ApplyModifiedProperties();
            }
            float tmpFloat = EditorGUILayout.Slider(new GUIContent("Raycast offset", "The distance off the target to avoid fading too much"), fader.offset, -2f, 0f);
            if (tmpFloat != fader.offset)
            {
                isDirty = true;
                Undo.RecordObject(fader, "Raycast offset");
                fader.offset = tmpFloat;
            }
            string tmpString = EditorGUILayout.TagField(new GUIContent("Player Tag", "Select the player's tag"), fader.playerTag);
            if (tmpString != fader.playerTag)
            {
                isDirty = true;
                Undo.RecordObject(fader, "Player Tag");
                fader.playerTag = tmpString;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            if (GUI.changed || isDirty)
            {
                EditorUtility.SetDirty(target); // or it won't save the data!!
            }
        }
    }
}