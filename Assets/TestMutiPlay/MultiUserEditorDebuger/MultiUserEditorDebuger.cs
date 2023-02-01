using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TestMutiPlay
{

    public interface IEditorProcess
    {
        public enum Type
        {
            UnityEditor,
            PackageEditor
        }

        string GetUserUin();
        int GetId();
        Type GetEditorType();
        void Start();
        void Run();
        void Stop();
        void Close();
    }
    public class UnityEditorProcess : IEditorProcess
    {
        private int m_id;
        private string m_uin;
        public UnityEditorProcess(int ptr, string uin)
        {
            m_id = ptr;
            m_uin = uin;
        }

        public string GetUserUin() => m_uin;

        public int GetId() => m_id;
        public IEditorProcess.Type GetEditorType() => IEditorProcess.Type.UnityEditor;
        public void Start()
        {
        }
        public void Run()
        {
        }
        public void Stop()
        {
        }
        public void Close()
        {
            var progress = Process.GetProcessById(m_id);
            progress?.Kill();
        }
    }
    public class PackageEditorProcess : IEditorProcess
    {
        private int m_id;
        private string m_uin;
        public PackageEditorProcess(int ptr, string uin)
        {
            m_id = ptr;
            m_uin = uin;
        }

        public string GetUserUin() => m_uin;

        public int GetId() => m_id;
        public IEditorProcess.Type GetEditorType() => IEditorProcess.Type.PackageEditor;
        public void Start()
        {
        }
        public void Run()
        {
        }
        public void Stop()
        {
        }
        public void Close()
        {
            var progress = Process.GetProcessById(m_id);
            progress?.Kill();
        }
    }

    /// <summary>
    /// 信息
    /// </summary>
    [Serializable]
    public class MultiUserData
    {
        public int MainId;
        public string Token;
    }

    /// <summary>
    /// 多人Editor
    /// </summary>
    public partial class MultiUserEditorDebuger : Singleton_Mono<MultiUserEditorDebuger>
    {
        private bool m_Inited = false;
        private MultiUserData m_data = new MultiUserData();

        public const string CommandLineTag = "-testUnity";

        private string[] m_CommandLineArgs;

        private Dictionary<string, IEditorProcess> m_ProcessPool = new Dictionary<string, IEditorProcess>();

        public Dictionary<string, IEditorProcess> ProcessPool => m_ProcessPool;

        /// <summary> 当前编辑器进程 </summary>
        public IEditorProcess.Type CurrentEnvironment { get; protected set; }
        public bool IsTestProgress { get; protected set; }
        public MultiUserData Data => m_data;

        public string CurrentEditorPath
        {
            get
            {
                if (!m_Inited) Initialize();
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.applicationPath;
#else
                return m_CommandLineArgs[0];
#endif
            }
        }
        public string CurrentEditorDir => Path.GetDirectoryName(CurrentEditorPath);
        public string CurrentEditorName => Path.GetFileName(CurrentEditorPath);


        /// <summary>
        /// 开启进程
        /// </summary>
        /// <param name="num">个数</param>
        /// <returns></returns>
        public int StudioProcessStart(int num)
        {
            if (!m_Inited) Initialize();

            try
            {
                for (int i = 0; i < num; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(CurrentEditorPath);

                    switch (CurrentEnvironment)
                    {
                        case IEditorProcess.Type.UnityEditor:
                            sb.Append($" -projectPath {GetTempProjectPath(i)}");
                            sb.Append($" -executeMethod \"MultiUserEditorDebuger_Editor.EditorRun\"");
                            break;
                        case IEditorProcess.Type.PackageEditor:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    sb.Append($" {CommandLineTag} \"{JsonUtility.ToJson(m_data)}\"");

                    string command = sb.ToString();
                    int progressId = 0;
                    MultiUserEditorTool.ProcesStartUnLock(CurrentEditorDir, command, ref progressId);

                    IEditorProcess process = null;
                    switch (CurrentEnvironment)
                    {
                        case IEditorProcess.Type.UnityEditor:
                            process = new UnityEditorProcess(progressId, num.ToString());
                            break;
                        case IEditorProcess.Type.PackageEditor:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    m_ProcessPool.Add(process.GetUserUin(), process);
                    sb.Clear();
                }


                return num;
            }
            catch (Exception e)
            {
                Debug.LogError("StudioProcessStart Error:" + e.ToString());
                return 0;
            }

        }
        public void KillStudioProcess(string uin)
        {
            if (!m_Inited) Initialize();

            try
            {
                m_ProcessPool[uin].Close();
                m_ProcessPool.Remove(uin);
            }
            catch (Exception e)
            {
                Debug.LogError("DisposeProcessAbort Error:" + e.ToString());
            }
        }
        public void KillAllStudioProcess()
        {
            if (!m_Inited) Initialize();

            try
            {
                foreach (var key in m_ProcessPool.Keys)
                {
                    m_ProcessPool[key].Close();
                }
                m_ProcessPool.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError("DisposeProcessAbort Error:" + e.ToString());
            }
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (m_Inited) return;

            m_CommandLineArgs = Environment.GetCommandLineArgs();


            for (int i = 0; i < m_CommandLineArgs.Length; i++)
            {
                if (m_CommandLineArgs[i].Equals(CommandLineTag, StringComparison.OrdinalIgnoreCase))
                {
                    m_data = JsonUtility.FromJson<MultiUserData>(m_CommandLineArgs[i + 1]);
                    IsTestProgress = true;
                    break;
                }
            }

            if (!IsTestProgress)
            {
                //配置数据
                m_data.MainId = Process.GetCurrentProcess().Id;
                m_data.Token = "D:sadas sa/2313df sadas -dfsd!!!";
            }

#if UNITY_EDITOR
            CurrentEnvironment = IEditorProcess.Type.UnityEditor;
#else
            CurrentEnvironment = IEditorProcess.Type.PackageEditor;
#endif

            m_Inited = true;
        }


        protected void OnApplicationQuit()
        {
            KillAllStudioProcess();
        }
    }
}



