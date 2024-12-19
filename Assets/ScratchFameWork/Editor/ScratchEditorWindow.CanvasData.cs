using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class CanvasData : MenuTreeWindow<CanvasData>
        {
            public override string GetMenuPath() => "Engine/CanvasData";
            public bool Inited = false;

            public void Init()
            {
                Inited = true;
            }

            public override void ShowGUI()
            {
                if (EditorApplication.isPlaying)
                {
                    if (!Inited)
                    {
                        Init();
                    }
                }
                else
                {
                    return;
                }

                DoTreeView();
            }


            private bool m_GlobalCanvas = false;
            private bool m_RefGlobalCanvas = false;
            private bool m_CurrentCanvas = true;
            private bool m_RefCurrentCanvas = true;

            private Vector2 m_ScrollPosition;

            void DoTreeView()
            {
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                GUILayout.BeginVertical();

                m_GlobalCanvas = EditorGUILayout.Foldout(m_GlobalCanvas, "GlobalCanvas");
                if (m_GlobalCanvas)
                {
                    foreach (KeyValuePair<int, IEngineBlockBaseData> valuePair in ScratchEngine.Instance.CurrentGroup.GlobalCanvas.BlockDataDicts)
                    {
                        GUILayout.Label($"[{valuePair.Key}]:{valuePair.Value.Type}");
                    }

                    m_RefGlobalCanvas = EditorGUILayout.Foldout(m_RefGlobalCanvas, "RefGlobalCanvas");
                    if (m_RefGlobalCanvas)
                    {
                        foreach (var variableRef in ScratchEngine.Instance.CurrentGroup.GlobalCanvas.VariableRefs)
                        {
                            GUILayout.Label($"[{variableRef.GuidRef}]:{variableRef.CanvasPos}");
                        }
                    }
                }

                if (!ScratchEngine.Instance.CurrentIsGlobal)
                {
                    m_CurrentCanvas = EditorGUILayout.Foldout(m_CurrentCanvas, "CurrentCanvas");
                    if (m_CurrentCanvas)
                    {
                        foreach (KeyValuePair<int, IEngineBlockBaseData> valuePair in ScratchEngine.Instance.Current.BlockDataDicts)
                        {
                            GUILayout.Label($"[{valuePair.Key}]:{valuePair.Value.Type}");
                        }
                        m_RefCurrentCanvas = EditorGUILayout.Foldout(m_RefCurrentCanvas, "RefCurrentCanvas");
                        if (m_RefCurrentCanvas)
                        {
                            foreach (var variableRef in ScratchEngine.Instance.Current.VariableRefs)
                            {
                                GUILayout.Label($"[{variableRef.GuidRef}]:{variableRef.CanvasPos.x}");
                            }
                        }
             
                    }
                }


                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }
    }
}