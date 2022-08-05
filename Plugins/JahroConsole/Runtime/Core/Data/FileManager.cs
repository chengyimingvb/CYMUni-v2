using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    internal static class FileManager
    {

        internal const string ProjectSettingFile = "jahro-settings";

        const string ProjectSettingPath = AssetsFolder + "/" + JahroConsoleAssetsFolder + "/" + "Resources";

        const string AssetsFolder = "Assets";

        const string JahroConsoleAssetsFolder = "JahroConsole";

        internal static void SaveToLocalSavesFile(string data)
        {
            string filePath = GetLocalSavesFilePath();
#if JAHRO_DEBUG
            Debug.Log("Saves: " + filePath);
#endif
            FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.Write(data);
            }
            stream.Close(); 
        }

        internal static string ReadLocalSaveFromFile(out bool dataLoaded)
        {
            string result = string.Empty;
            string path = GetLocalSavesFilePath();
            if (File.Exists(path) == false)
            {
                dataLoaded = false;
                return null;
            }
            FileStream stream = File.Open(path, FileMode.Open);
#if JAHRO_DEBUG
            Debug.Log("Saves load: " + path);
#endif
            using(StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
            stream.Close();
            dataLoaded = !string.IsNullOrEmpty(result);
            return result;
        }

        internal static void ValidateSettingsFile(ProjectSettings settings)
        {
#if UNITY_EDITOR
            var assetsResult =AssetDatabase.FindAssets(" t:ProjectSettings " + ProjectSettingFile);

            if (assetsResult == null || assetsResult.Length == 0)
            {
                if (AssetDatabase.IsValidFolder(ProjectSettingPath) == false)
                {
                    CreateAssetsFolder();
                }
                AssetDatabase.CreateAsset(settings, ProjectSettingPath + "/" + ProjectSettingFile + ".asset");
                AssetDatabase.SaveAssets();
            }      
#endif
        }

#if UNITY_EDITOR
        private static void CreateAssetsFolder()
        {
            if (AssetDatabase.IsValidFolder(AssetsFolder + "/" + JahroConsoleAssetsFolder) == false)
            {
                AssetDatabase.CreateFolder(AssetsFolder, JahroConsoleAssetsFolder);
                AssetDatabase.SaveAssets();
            }
            if (AssetDatabase.IsValidFolder(ProjectSettingPath) == false)
            {
                AssetDatabase.CreateFolder(AssetsFolder + "/" + JahroConsoleAssetsFolder, "Resources");
                AssetDatabase.SaveAssets();
            }
        }
#endif

        private static string GetLocalSavesFilePath()
        {
            string folderPath = Application.persistentDataPath;
            string filename = "state-save.dat";
            return folderPath + Path.DirectorySeparatorChar + filename;
        }
    }
}