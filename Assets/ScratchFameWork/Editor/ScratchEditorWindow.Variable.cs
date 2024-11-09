using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class Variable : MenuTreeWindow<Variable>
        {
            private UnityEditor.Editor _editor;

            private bool ShowAll_VariableLabelRefDict =false;
            public override string GetMenuPath() => "Runtime/Variable";

            public override void ShowGUI()
            {
                if (!EditorApplication.isPlaying) return;
                
                ShowAll_VariableLabelRefDict = EditorGUILayout.Foldout(ShowAll_VariableLabelRefDict, "当前变量所有引用");
                if (ShowAll_VariableLabelRefDict)
                {
                    var labels = ScratchDataManager.Instance.VariableLabelRefDict;
                    foreach (var label in labels)
                    {
                        GUILayout.Label($"[{label.Key}][{label.Value.GetVariableData()}]");
                        GUILayout.Space(10);
                    }
                }
            }
        }
    }
}