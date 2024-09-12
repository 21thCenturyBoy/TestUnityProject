using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Scratch/EditorWindow")]
        static void window()
        {
            ScratchEditorWindow mybianyi = GetWindow<ScratchEditorWindow>();
            mybianyi.Show();
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying) return;

            GUILayout.BeginVertical();

            foreach (ScratchVMData vmData in ScratchDataManager.Instance.DataDict.Values)
            {
                GUILayout.Label($"[{vmData.IdPtr}]{vmData}");
            }

            GUILayout.Label($"--------------------------");

            var inputs = GameObject.FindObjectsOfType<BlockHeaderItem_Input>();

            foreach (var input in inputs)
            {
                GUILayout.Label($"[{input.ContextData.IdPtr}][{input.ContextData.ChildOperation}]");
            }

            GUILayout.EndVertical();
        }
    }
}