using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using JahroConsole.Core.Data;

namespace JahroConsole.View
{
    public class ConsoleOpenButton : MonoBehaviour
    {
        public ConsoleMainWindow MainWindow;

        private Vector2 _dragOffset;

        private RectTransform _holderTransform;

        private RectTransform _canvasTransform;

        void Awake()
        {
            GetComponentInChildren<Button>().onClick.AddListener(OnClick);

            _holderTransform = GetComponent<RectTransform>();
            _canvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();   
        }

        void Start()
        {
            ConsoleStorageController.Instance.OnStorageSave += OnStateSave; 
            
            MainWindow.SetOpenButton(this);
            LoadState(ConsoleStorageController.Instance.ConsoleStorage);

            var dragEventTrigger = GetComponent<EventTrigger>();
            dragEventTrigger.triggers[1].callback.AddListener(OnButtonPointerDrag);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnClick()
        {
            MainWindow.Show();
            Hide();
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.GeneralSettings.OpenButtonPosition = _holderTransform.anchoredPosition;
        }

        private void LoadState(ConsoleStorage storage)
        {
            _holderTransform.anchoredPosition = storage.GeneralSettings.OpenButtonPosition; 
        }

        private void OnButtonPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_holderTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

            _dragOffset.x = clickLocalPoint.x;
            _dragOffset.y = clickLocalPoint.y;
        }

        private void OnButtonPointerDrag(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            
            if ((pointerEventData.pressPosition - pointerEventData.position).magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }

            Vector2 dragLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, pointerEventData.position, pointerEventData.pressEventCamera, out dragLocalPoint);

            dragLocalPoint.x += _dragOffset.x;
            dragLocalPoint.y += _dragOffset.y;

            _holderTransform.anchoredPosition = dragLocalPoint;
        }
    }
}