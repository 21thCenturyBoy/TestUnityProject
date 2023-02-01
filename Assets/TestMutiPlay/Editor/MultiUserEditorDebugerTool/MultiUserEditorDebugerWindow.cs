using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TestMutiPlay
{

    public class MultiUserEditorDebugerWindow : EditorWindow
    {
        private static MultiUserEditorDebugerWindow m_window;
        [MenuItem("Tools/MultiUserTools/Show Debuger Window")]
        static void OpenWindow()
        {
            if (m_window == null) m_window = GetWindow<MultiUserEditorDebugerWindow>();
            m_window.Show();
        }

        public MultiUserEditorDebugerWindow()
        {
            titleContent.text = "MultiUserDebuger";
        }

        //GUI
        private bool m_CommandLineFoldoutIsShow;
        private bool m_ConfigFoldoutIsShow;

        void OnGUI()
        {
            m_CommandLineFoldoutIsShow = EditorGUILayout.Foldout(m_CommandLineFoldoutIsShow, "CommandLine：");
            if (m_CommandLineFoldoutIsShow)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < MultiUserEditorStartup.m_CommandLineArgs.Length; i++)
                {
                    GUILayout.Label($"[{i}]:{MultiUserEditorStartup.m_CommandLineArgs[i]}");
                }

                EditorGUI.indentLevel--;
            }
            m_ConfigFoldoutIsShow = EditorGUILayout.Foldout(m_ConfigFoldoutIsShow, "Config：");
            if (m_ConfigFoldoutIsShow)
            {
                EditorGUI.indentLevel++;
                GUILayout.Label(JsonUtility.ToJson(MultiUserEditorStartup.Config, true));
                EditorGUI.indentLevel--;
            }

        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

    }
}