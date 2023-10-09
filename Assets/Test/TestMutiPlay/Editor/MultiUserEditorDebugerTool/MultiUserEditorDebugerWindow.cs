using System.Linq;
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
        private bool m_ProgressFoldoutIsShow = true;

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

            if (!EditorApplication.isPlaying) return;

            m_ProgressFoldoutIsShow = EditorGUILayout.Foldout(m_ProgressFoldoutIsShow, "Progress：");
            if (m_ProgressFoldoutIsShow)
            {
                GUILayout.Label($"IsTestProgress：{MultiUserEditorDebuger.Instance.IsTestProgress}");
                GUILayout.Label(JsonUtility.ToJson(MultiUserEditorDebuger.Instance.Data, true));

                MultiUserEditorDebuger.Instance.ForceUsePackageEditor = EditorGUILayout.BeginToggleGroup("ForceUsePackageEditor", MultiUserEditorDebuger.Instance.ForceUsePackageEditor);
                GUILayout.BeginHorizontal();
                GUILayout.Label("PackagePath：", GUILayout.MaxWidth(100));
                MultiUserEditorDebuger.Instance.PackageEditorPath = GUILayout.TextField(MultiUserEditorDebuger.Instance.PackageEditorPath);
                GUILayout.EndHorizontal();
                GUILayout.Label("Ex：D:/Program Files/Package.exe");
                EditorGUILayout.EndToggleGroup();
                EditorGUI.indentLevel++;
                var keys = MultiUserEditorDebuger.Instance.ProcessPool.Keys.ToArray();
                foreach (var p in keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"PID：{MultiUserEditorDebuger.Instance.ProcessPool[p].GetId()}");
                    if (GUILayout.Button("Kill", GUILayout.MaxWidth(100)))
                    {
                        MultiUserEditorDebuger.Instance.KillStudioProcess(p);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }


        }

    }
}