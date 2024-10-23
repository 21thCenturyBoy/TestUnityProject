using System;
using System.ComponentModel;
using System.IO;
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>WIndows系统API 或者NET APT，WIndows平台可用</summary>
public static class WindowsSystemAPI
{
    /// <summary> 打开指定目录的文件浏览器 </summary>
    /// <param name="filepath"></param>
    public static void OpenFileExplorer(string filepath)
    {
        filepath = filepath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (Directory.Exists(filepath))
        {
            using (var process = System.Diagnostics.Process.Start("explorer.exe", @filepath))
            {
                Debug.Log($"{filepath}打开..");
            }
        }
        else Debug.LogWarning($"{filepath}不存在..");
    }

    public static void SetClipboard(string text)
    {
        try
        {
            TryOpenClipboard();
            EmptyClipboard();
            IntPtr hGlobal = default;
            try
            {
                var bytes = (text.Length + 1) * 2;
                hGlobal = Marshal.AllocHGlobal(bytes);

                if (hGlobal == default) ThrowWin32();
                var target = GlobalLock(hGlobal);
                if (target == default) ThrowWin32();
                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                }
                finally
                {
                    GlobalUnlock(target);
                }
                if (SetClipboardData(cfUnicodeText, hGlobal) == default)
                {
                    ThrowWin32();
                }
                hGlobal = default;
            }
            finally
            {
                if (hGlobal != default)
                {
                    Marshal.FreeHGlobal(hGlobal);
                }
                CloseClipboard();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    static void TryOpenClipboard()
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(default)) break;
            if (--num == 0) ThrowWin32();
            Thread.Sleep(100);
        }
    }
    public static string? GetText()
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText)) return null;
        TryOpenClipboard();

        return InnerGet();
    }

    static string? InnerGet()
    {
        IntPtr handle = default;

        IntPtr pointer = default;
        try
        {
            handle = GetClipboardData(cfUnicodeText);
            if (handle == default) return null;

            pointer = GlobalLock(handle);
            if (pointer == default) return null;

            var size = GlobalSize(handle);
            var buff = new byte[size];

            Marshal.Copy(pointer, buff, 0, size);
            return Encoding.Unicode.GetString(buff).TrimEnd('\0');
        }
        finally
        {
            if (pointer != default) GlobalUnlock(handle);

            CloseClipboard();
        }
    }

    const uint cfUnicodeText = 13;

    static void ThrowWin32()
    {
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("User32.dll", SetLastError = true)]
    static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll")]
    static extern bool EmptyClipboard();

    [DllImport("Kernel32.dll", SetLastError = true)]
    static extern int GlobalSize(IntPtr hMem);
}
#endif
