using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using JahroConsole.Logging;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Data
{
    public class ConsoleStorageController
    {

        private static ConsoleStorageController _instance;

        private ConsoleStorage _storage;

        internal ConsoleStorage ConsoleStorage { get {return _storage;}}

        internal static ConsoleStorageController Instance
        {
            private set
            {
                _instance = value;
            }
            get 
            {
                if (_instance == null)
                {
                    _instance = new ConsoleStorageController();
                }
                return _instance;
            }
        }

        internal Action<ConsoleStorage> OnStorageLoad = delegate {};

        internal Action<ConsoleStorage> OnStorageSave = delegate {};

        private ConsoleStorageController(){}  

        private void ReadLocalSaves()
        {
            bool dataLoaded;
            var json = FileManager.ReadLocalSaveFromFile(out dataLoaded);
            if (dataLoaded)
            {
                try
                {
                    _storage = JsonUtility.FromJson<ConsoleStorage>(json);
                }
                catch
                {
                    Jahro.LogError(MessagesResource.LogSavesParsingError);
                    _storage = new ConsoleStorage();
                }
            }
            else
            {
                Jahro.LogDebug(MessagesResource.LogSavesLoadingError);
                _storage = new ConsoleStorage();
            }

            _storage.ProjectSettings = ReadOrCreateProjectSettings();            
            OnStorageLoad(_storage);
        }

        private void WriteLocalSaves()
        {
            OnStorageSave(_storage);
            
            string json = JsonUtility.ToJson(_storage, true);
            FileManager.SaveToLocalSavesFile(json);
        }

        public static ProjectSettings ReadOrCreateProjectSettings()
        {
            ProjectSettings settings = Resources.Load<ProjectSettings>(FileManager.ProjectSettingFile);
            
            if (settings == null)   
            {
                settings = ProjectSettings.CreateDefault();
            }

            FileManager.ValidateSettingsFile(settings);
            return settings;
        } 

        internal static void LoadState()
        { 
            Instance.ReadLocalSaves();
        }
        
        internal static void SaveState()
        {
            Instance.WriteLocalSaves();
        }

        internal static void Release()
        {
            Instance = null;
        }
    }
}