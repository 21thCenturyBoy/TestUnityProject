using System;
using UnityEditor;
using UnityEngine;

namespace TestMutiPlay
{

    public class MultiUserEditorDebugerWindow : EditorWindow
    {
        [MenuItem("Tools/MultiUserTools/Show Debuger Window")]
        static void OpenWindow()
        {
            var creater = GetWindow<MultiUserEditorDebugerWindow>();
            creater.Show();
        }

        public MultiUserEditorDebugerWindow()
        {
            titleContent.text = "MultiUserDebuger";
        }

        private string[] m_CommandLineArgs;

        //GUI
        private bool showFoldout;

        void OnGUI()
        {
            showFoldout = EditorGUILayout.Foldout(showFoldout, "CommandLine：");
            if (showFoldout)
            {
                EditorGUI.indentLevel++; //缩进级别
                for (int i = 0; i < m_CommandLineArgs.Length; i++)
                {
                    GUILayout.Label(m_CommandLineArgs[i]); //提示语句
                }

                EditorGUI.indentLevel--;
            }
            //#endregion
            //#region GUILayout.Button( 按钮
            //GUILayout.Label("按钮");
            //if (GUILayout.Button("按钮", GUILayout.Width(40), GUILayout.Height(40)))
            //{


            //}
        }

        void OnEnable()
        {
            m_CommandLineArgs = Environment.GetCommandLineArgs();

            if (!MultiUserEditorData.EditorStartup.IsTemporaryProject)
            {
                if (MultiUserEditorCommunication.PipelinePool.GetInstance != null)
                {
                    MultiUserEditorCommunication.PipelinePool.GetInstance.Dispose();
                }

                MultiUserEditorCommunication.PipelinePool.Instance.CreatePipeLineAsync();

            }

            //CompilationPipeline.compilationStarted += CompilationPipeline_compilationStarted;
            //CompilationPipeline.assemblyCompilationStarted += CompilationPipeline_compilationStarted;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private void CompilationPipeline_compilationStarted(object obj)
        {
            Close();
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            if (!MultiUserEditorData.EditorStartup.IsTemporaryProject)
            {
                if (MultiUserEditorCommunication.PipelinePool.GetInstance != null)
                {
                    MultiUserEditorCommunication.PipelinePool.GetInstance.Dispose();
                }

            }

            //CompilationPipeline.compilationStarted -= CompilationPipeline_compilationStarted;

        }

        public void OnBeforeAssemblyReload()
        {
            Debug.Log("Before Assembly Reload");

            Close();

            //if (!MultiUserEditorData.EditorStartup.IsTemporaryProject)
            //{
            //    if (MultiUserEditorCommunication.PipelinePool.GetInstance != null)
            //    {
            //        MultiUserEditorCommunication.PipelinePool.GetInstance.Dispose();
            //    }
            //}
        }

        public void OnAfterAssemblyReload()
        {

            Debug.Log("After Assembly Reload");

            //if (!MultiUserEditorData.EditorStartup.IsTemporaryProject)
            //{
            //    if (MultiUserEditorCommunication.PipelinePool.GetInstance != null)
            //    {
            //        MultiUserEditorCommunication.PipelinePool.GetInstance.Dispose();
            //    }
            //    MultiUserEditorCommunication.PipelinePool.Instance.CreatePipeLineAsync();
            //}
        }
    }
}