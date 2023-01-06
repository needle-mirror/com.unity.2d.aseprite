using System.IO;
using System.Threading;

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

        static void OnChangeDetected(object sender, FileSystemEventArgs e) => s_HasChange = true;

        static void StopWatcher()
        {
            s_Watcher.Dispose();
            s_Watcher = null;
        }

        static void CheckForChange()
        {
            if (!s_HasChange) 
                return;
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(k_AssetsPath, ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
            s_HasChange = false;
        }
    }
}