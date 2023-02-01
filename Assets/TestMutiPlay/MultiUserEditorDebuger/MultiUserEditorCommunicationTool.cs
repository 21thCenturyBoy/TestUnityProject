using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace TestMutiPlay
{

    public static class MultiUserEditorCommunicationTool
    {
        public class DllInvoke
        {
            //打开文件对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
            public static bool GetOFN([In, Out] OpenFileName ofn)
            {
                return GetOpenFileName(ofn);
            }

            //另存为对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
            public static bool GetSFN([In, Out] OpenFileName ofn)
            {
                return GetSaveFileName(ofn);
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetActiveWindow();

            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern bool CreateProcess(
            StringBuilder lpApplicationName, StringBuilder lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            StringBuilder lpEnvironment,
            StringBuilder lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
            );

            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int CloseHandle(IntPtr hObject);
        }

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public string lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public int lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public int wShowWindow;
            public int cbReserved2;
            public byte lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }


        //唤起指定目录exe文件进程
        public static void ProcesStart(string path, string command)
        {
            var currDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(path);
            MultiUserEditorCommunicationTool.PROCESS_INFORMATION pInfo = new();
            MultiUserEditorCommunicationTool.STARTUPINFO sInfo = new();
            if (!MultiUserEditorCommunicationTool.DllInvoke.CreateProcess(null, new StringBuilder(command), null, null, false, 0x08000000, null, null,
                    ref sInfo, ref pInfo))
            {
                Debug.LogError("ProcesStart faild :" + command);
            }

            Directory.SetCurrentDirectory(currDir);
            MultiUserEditorCommunicationTool.DllInvoke.WaitForSingleObject(pInfo.hProcess, 0xFFFFFFFF);
            MultiUserEditorCommunicationTool.DllInvoke.CloseHandle(pInfo.hProcess);
            MultiUserEditorCommunicationTool.DllInvoke.CloseHandle(pInfo.hThread);

        }

        public static void ProcesStartUnLock(string path, string command, ref int ProcessId)
        {

            var currDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(path);
            MultiUserEditorCommunicationTool.PROCESS_INFORMATION pInfo = new();
            MultiUserEditorCommunicationTool.STARTUPINFO sInfo = new();
            if (!MultiUserEditorCommunicationTool.DllInvoke.CreateProcess(null, new StringBuilder(command), null, null, false, 0x08000000, null, null,
                    ref sInfo, ref pInfo))
            {
                Debug.LogError("ProcesStart faild :" + command);
            }

            Directory.SetCurrentDirectory(currDir);
            ProcessId = pInfo.dwProcessId;

        }
    }
}