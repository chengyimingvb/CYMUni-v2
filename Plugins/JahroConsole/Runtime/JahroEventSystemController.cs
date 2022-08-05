using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace JahroConsole
{
    public class JahroEventSystemController : MonoBehaviour
    {
        private EventSystem _localEventSystem;
        private List<EventSystem> _eventSystems = new List<EventSystem>();

        private void Awake()
        {
            ProcessEventSystems();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            ProcessEventSystems();
        }

        private void ProcessEventSystems()
        {
            _localEventSystem = GetComponentInChildren<EventSystem>(true);
            _eventSystems = FindObjectsOfType<EventSystem>().ToList();
            if (_localEventSystem != null)
            {
                _localEventSystem.gameObject.SetActive(_eventSystems.Count < 1);
            }
            _eventSystems = FindObjectsOfType<EventSystem>().ToList();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
