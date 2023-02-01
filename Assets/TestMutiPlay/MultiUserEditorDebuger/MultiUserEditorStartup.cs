#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TestMutiPlay
{
    [Serializable]
    public class TemporaryConfig
    {
        public string Description;
        public string OrginUnityProjectPath;
        public string OrginUnityProjectName;
        public List<string> TempUnityProjectPaths;
        public List<string> TempUnityProjectNames;
    }

    [InitializeOnLoad]
    public static class MultiUserEditorStartup
    {
        public static string[] m_CommandLineArgs;
        //public static string m_CommandLine;

        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        public static string ProjectName => Path.GetFileName(ProjectPath);
        public static bool IsTemporaryProject;

        private static TemporaryConfig m_Config;

        public static TemporaryConfig Config
        {
            get
            {
                if (m_Config == null) m_Config = ReadProjectConfig();
                return m_Config;
            }
        }
        static MultiUserEditorStartup()
        {
            UnityEngine.Debug.Log("MultiUserEditorData Startup!");
            UnityEngine.Debug.Log( EditorApplication.applicationPath);

            m_CommandLineArgs = Environment.GetCommandLineArgs();
            //StringBuilder sb = new StringBuilder();
            //StringBuilder sb2 = new StringBuilder();
            //foreach (string arg in m_CommandLineArgs)
            //{
            //    sb.Append(" " + arg);
            //}
            //UnityEngine.Debug.Log(sb.ToString());
            //m_CommandLine = sb2.ToString();

            //UnityEngine.Debug.Log(Process.GetCurrentProcess().ProcessName);
            //Process[] proList = Process.GetProcesses(".");//获得本机的进程
            //int k = proList.Length.ToString(); //当前进程数量
        }

        public static TemporaryConfig ReadProjectConfig()
        {
            TemporaryConfig config;
            string configPath = Path.Combine(ProjectPath, "..\\ReadMeDontModify.json");
            if (File.Exists(configPath))
            {
                string jsonStr = File.ReadAllText(configPath, Encoding.UTF8);
                config = JsonUtility.FromJson<TemporaryConfig>(jsonStr);

                IsTemporaryProject = true;

                Debug.Log("是临时工程！");
            }
            else
            {
                //检查主工程
                var projectname = Path.GetFileName(ProjectPath);
                string projectCopyFolder = $"{projectname}_Temporary";
                configPath = Path.Combine(ProjectPath, $"..\\{projectCopyFolder}\\ReadMeDontModify.json");

                if (File.Exists(configPath))
                {
                    string jsonStr = File.ReadAllText(configPath, Encoding.UTF8);
                    config = JsonUtility.FromJson<TemporaryConfig>(jsonStr);

                    Debug.Log("是主工程！");
                }
                else
                {
                    config = new TemporaryConfig();
                    config.Description = "多开缓存目录,硬盘不够可删！";
                    config.OrginUnityProjectPath = ProjectPath;
                    config.OrginUnityProjectName = projectname;
                    config.TempUnityProjectPaths = new List<string>();
                    config.TempUnityProjectNames = new List<string>();

                    string dir = Path.Combine(ProjectPath, $"..\\{projectCopyFolder}");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        Debug.Log(dir);
                    }
                    File.WriteAllText(configPath, JsonUtility.ToJson(config, true), Encoding.UTF8);
                }

                IsTemporaryProject = false;
            }

            return config;
        }
        public static void WriteProjectConfig(TemporaryConfig config)
        {
            var projectname = Path.GetFileName(ProjectPath);
            string projectCopyFolder = $"{projectname}_Temporary";
            string projectCopyDir = Path.Combine(ProjectPath, $"..\\{projectCopyFolder}");
            string configPath = Path.Combine(ProjectPath, $"..\\{projectCopyFolder}\\ReadMeDontModify.json");
            if (!Directory.Exists(projectCopyDir))
            {
                Directory.CreateDirectory(projectCopyDir);
                Debug.Log(projectCopyDir);
            }
            File.WriteAllText(configPath, JsonUtility.ToJson(config, true), Encoding.UTF8);
        }

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        static void OnEditorStartupInit()
        {

        }
        [MenuItem("Tools/MultiUserTools/生成8个临时项目")]
        public static void GenerateTemporary8()
        {
            if (Config == null) ReadProjectConfig();
            GenerateTemporary(8);
        }
        private static void CreateTemporaryProject(string orginpath, string targetpath)
        {
            if (Directory.Exists(targetpath))
            {
                Debug.LogWarning($"路径存在:{targetpath}");
                return;
            }

            Directory.CreateDirectory(targetpath);

            Process process = new Process(); //实例
            process.StartInfo.CreateNoWindow = true; //设定不显示窗口
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "cmd.exe"; //设定程序名
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出
            process.StartInfo.RedirectStandardError = true; //重定向错误输出
            process.Start();

            string[] linkFloder = new[] { "Assets", "Packages", "ProjectSettings" };

            for (int i = 0; i < linkFloder.Length; i++)
            {
                string cmdline = $"mklink /j \"{Path.Combine(targetpath, linkFloder[i])}\" \"{Path.Combine(orginpath, linkFloder[i])}\"";
                process.StandardInput.WriteLine(cmdline);
            }
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
        }

        private static void GenerateTemporary(int num = 1)
        {
            string path = MultiUserEditorStartup.m_CommandLineArgs[2];
            var projectDir = new DirectoryInfo(path).Parent?.FullName;
            var projectname = Path.GetFileName(path);

            string projectCopyFolder = $"{projectname}_Temporary";
            string projectCopyFullPath = Path.Combine(projectDir, projectCopyFolder);

            List<string> projectTempPaths = new List<string>();
            List<string> projectTempNames = new List<string>();
            for (int i = 1; i <= num; i++)
            {
                string tempname = $"{projectCopyFolder}_{i}";
                string tempPath = Path.Combine(projectCopyFullPath, tempname);
                projectTempPaths.Add(tempPath);
                projectTempNames.Add(tempname);
            }
            for (int i = 0; i < projectTempPaths.Count; i++)
            {
                CreateTemporaryProject(path, projectTempPaths[i]);
            }

            Config.TempUnityProjectNames = projectTempNames;
            Config.TempUnityProjectPaths = projectTempPaths;

            WriteProjectConfig(Config);

            WindowsSystemAPI.OpenFileExplorer(projectCopyFullPath);

            return;


        }
    }
}

#endif