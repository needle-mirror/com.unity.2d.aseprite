using System.IO;
using System.Threading;
using UnityEngine;

namespace UnityEditor.U2D.Aseprite
{
    [InitializeOnLoad]
    internal static class BackgroundImport
    {
        static string k_AssetsPath = "Assets/";
        static bool s_HasChange = false;
        static bool s_LastSettingsValue = false;

        static FileSystemWatcher s_Watcher = null;

        static BackgroundImport()
        {
            EditorApplication.update += OnUpdate;
            if (ImportSettings.backgroundImport)
                SetupWatcher();

            s_LastSettingsValue = ImportSettings.backgroundImport;
        }

        static void OnUpdate()
        {
            if (EditorApplication.isCompiling) 
                return;
            if (EditorApplication.isUpdating) 
                return;

            CheckForSettingsUpdate();
            CheckForChange();
        }

        static void CheckForSettingsUpdate()
        {
            if (ImportSettings.backgroundImport == s_LastSettingsValue)
                return;

            if (ImportSettings.backgroundImport)
                SetupWatcher();
            else
                StopWatcher();
            
            s_LastSettingsValue = ImportSettings.backgroundImport;
        }
        
        static void SetupWatcher()
        {
            if (Application.isBatchMode)
                return;
            
            ThreadPool.QueueUserWorkItem(MonitorDirectory, k_AssetsPath);
        }

        static void MonitorDirectory(object obj)
        {
            var path = (string)obj;

            s_Watcher = new FileSystemWatcher();
            s_Watcher.Path = path;
            s_Watcher.IncludeSubdirectories = true;
            s_Watcher.Changed += OnChangeDetected;
            s_Watcher.Created += OnChangeDetected;
            s_Watcher.Renamed += OnChangeDetected;
            s_Watcher.Deleted += OnChangeDetected;
            s_Watcher.EnableRaisingEvents = true;
        }

        static void OnChangeDetected(object sender, FileSystemEventArgs e)
        {
            var extension = Path.GetExtension(e.FullPath);
            if (extension == ".meta" ||
                extension == ".cs")
                return;
            
            s_HasChange = true;
        }

        static void StopWatcher()
        {
            if (s_Watcher != null)
            {
                s_Watcher.Dispose();
                s_Watcher = null;
            }
        }

        static void CheckForChange()
        {
            if (!s_HasChange) 
                return;
            // If the editor is already focused, skip forced import.
            if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                s_HasChange = false;
                return;
            }
            if (Application.isPlaying)
                return;
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
            
            var assetGuids = AssetDatabase.FindAssets("", new[] { k_AssetsPath });
            var assetPaths = new string[assetGuids.Length];
            for (var i = 0; i < assetGuids.Length; ++i)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                assetPaths[i] = path;
            }
            
            AssetDatabase.ForceReserializeAssets(assetPaths, ForceReserializeAssetsOptions.ReserializeAssets);
            
            s_HasChange = false;
        }
    }
}