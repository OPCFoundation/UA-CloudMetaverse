using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Input
{
    [InitializeOnLoad]
    internal class Init
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern int AddDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryExW([MarshalAs(UnmanagedType.LPWStr)] string fileName, IntPtr fileHandle, uint flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr moduleHandle);

        const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
        const string MONO_SUPPORT = "MonoSupport.dll";
        static readonly string INIT_PATH = $"{typeof(Init).FullName}.cs";

        static Init()
        {
            IntPtr modulePtr = LoadLibraryExW($"{typeof(Init).Namespace}.dll", IntPtr.Zero, LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
            if (modulePtr != IntPtr.Zero)
            {
                // DLL search paths already configured in this process; nothing more to do.
                FreeLibrary(modulePtr);
                return;
            }

            List<string> rootFolders = new List<string>();
            rootFolders.Add(Application.dataPath);
            rootFolders.Add(Path.GetFullPath(Path.Combine("Library", "PackageCache")));

            string dllDirectory = string.Empty;
            for (int i = 0; i < rootFolders.Count; i++)
            {
                string[] filePathResults = Directory.GetFiles(rootFolders[i], INIT_PATH, SearchOption.AllDirectories);
                if (filePathResults.Length > 1)
                {
                    Debug.LogError($"Failed to find single file for {INIT_PATH}; found {filePathResults.Length} instead!");
                    return;
                }
                else if (filePathResults.Length == 1)
                {
                    dllDirectory = filePathResults[0];
                    break;
                }
            }

            if (string.IsNullOrEmpty(dllDirectory))
            {
                Debug.LogError($"Failed to find {INIT_PATH}.");
                return;
            }

            // Parse out the Editor folder and jump to the MonoSupport Windows/x64 folder.
            string dllDirectoryPlugins = Path.Combine(dllDirectory.Substring(0, dllDirectory.LastIndexOf("Editor")), Path.Combine("Plugins", Path.Combine("Windows", @"x64")));
            string dllDirectoryUnity = Path.Combine(dllDirectory.Substring(0, dllDirectory.LastIndexOf("Editor")), @"x64");

            if (Directory.Exists(dllDirectoryPlugins))
            {
                dllDirectory = dllDirectoryPlugins;
            }
            else if (Directory.Exists(dllDirectoryUnity))
            {
                dllDirectory = dllDirectoryUnity;
            }
            else
            {
                Debug.LogError($"Neither {dllDirectoryPlugins} nor {dllDirectoryUnity} exist.");
                return;
            }

            if (AddDllDirectory(dllDirectory) == 0)
            {
                Debug.LogError($"Failed to set DLL directory {dllDirectory}: Win32 error {Marshal.GetLastWin32Error()}");
                return;
            }

            Debug.Log(string.Format("Added DLL directory {0} to the user search path.", dllDirectory));
        }
    }
}