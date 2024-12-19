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
            public override string GetMenuPath() => "UI Runtime/Variable";

            public override void ShowGUI()
            {
                if (!EditorApplication.isPlaying) return;
                
                ShowAll_VariableLabelRefDict = EditorGUILayout.Foldout(ShowAll_VariableLabelRefDict, "当前变量所有引用");
                if (ShowAll_VariableLabelRefDict)
                {
                    var labels = GameObject.FindObjectsOfType<BlockHeaderItem_VariableLabel>();
                    foreach (var label in labels)
                    {
                        GUILayout.Label($"[{label.ContextData.Guid}][{label.ContextData.VariableRef}]");
                        GUILayout.Space(10);
                    }
                    var renturnLabels = GameObject.FindObjectsOfType<BlockHeaderItem_RenturnVariableLabel>();
                    foreach (var label in renturnLabels)
                    {
                        GUILayout.Label($"[{label.ContextData.Guid}][{label.ContextData.VariableRef}]");
                        GUILayout.Space(10);
                    }
                }
            }
        }
    }
}