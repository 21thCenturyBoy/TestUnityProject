using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        int GetId();
        Type GetEditorType();
        void Start();
        void Run();
        void Stop();
        void Close();
    }
    public class UnityEditorProcess : IEditorProcess
    {

        public int GetId() => 0;
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
        }
    }
    public class PackageEditorProcess : IEditorProcess
    {
        public int GetId() => 0;
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
        }
    }

    /// <summary>
    /// 多人Editor
    /// </summary>
    public partial class MultiUserEditorDebuger : Singleton<MultiUserEditorDebuger>
    {
        private bool m_Inited = false;

        private string[] m_CommandLineArgs;

        private Dictionary<int, IEditorProcess> m_ProcessPool = new Dictionary<int, IEditorProcess>();
        private Dictionary<int, Process> m_ClientProcessesPool = new Dictionary<int, Process>();

        /// <summary>
        /// 当前编辑器路径
        /// </summary>
        public string CurrentEditorPath
        {
            get
            {
                if (!m_Inited) Initialize();
                return m_CommandLineArgs[0];
            }
        }

        public int StudioProcessStart(string arguments)
        {
            if (!m_Inited) Initialize();

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = CurrentEditorPath;
                processStartInfo.Arguments = arguments;

                var p = Process.Start(processStartInfo);
                m_ClientProcessesPool.Add(p.Id, p);
                return p.Id;
            }
            catch (Exception e)
            {
                Debug.LogError("StudioProcessStart Error:" + e.ToString());
                return 0;
            }

        }
        public void DisposeProcessAbort(int id)
        {
            if (!m_Inited) Initialize();

            try
            {
                if (m_ClientProcessesPool.ContainsKey(id))
                {
                    var p = m_ClientProcessesPool[id];
                    p?.Dispose();
                    m_ClientProcessesPool.Remove(id);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("DisposeProcessAbort Error:" + e.ToString());
            }
        }
        public void Initialize()
        {
            if (m_Inited) return;

            m_CommandLineArgs = Environment.GetCommandLineArgs();


            m_Inited = true;
        }
    }
}



