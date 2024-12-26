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

            public override bool IsFrameUpdate() => true;

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
            private bool m_CurrentCanvas = true;

            private Vector2 m_ScrollPosition;

            void DoTreeView()
            {
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                GUILayout.BeginVertical();

                m_GlobalCanvas = EditorGUILayout.Foldout(m_GlobalCanvas, "GlobalCanvas");

                if (m_GlobalCanvas)
                {
                    foreach (var globalCanvas in ScratchEngine.Instance.FileData.Global.Canvas)
                    {
                        GUILayout.Label($"[Global][{globalCanvas.Name}]");
                        foreach (KeyValuePair<int, IEngineBlockBaseData> valuePair in globalCanvas.BlockDataDicts)
                        {
                            GUILayout.Label($"[{valuePair.Key}]:{valuePair.Value.Type}");
                        }

                        foreach (var variableRef in globalCanvas.FragmentDataRefs)
                        {
                            GUILayout.Label($"[{variableRef.Key}](Ref:{variableRef.Value.Guid}):{variableRef.Value.CanvasPos}");
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

                        foreach (var variableRef in ScratchEngine.Instance.Current.FragmentDataRefs)
                        {
                            GUILayout.Label($"[{variableRef.Key}](Ref:{variableRef.Value.Guid}):{variableRef.Value.CanvasPos}");
                        }
                    }
                }


                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }
    }
}