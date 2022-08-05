using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JahroConsole.View
{
    internal class ItemSubmenu : MonoBehaviour, IPointerClickHandler
    {
        public RectTransform ViewArea;

        private JahroScrollItem _item;

        public void OnPointerClick(PointerEventData eventData)
        {
            var clickOnModalView = eventData.pointerPressRaycast.gameObject == this.gameObject;
            if (clickOnModalView)
            {
                Close();
            }
        }

        public void Open(JahroScrollItem item)
        {
            _item = item;
            UpdatePosition();

            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void OnCopyClick()
        {
            _item?.OnCopyButtonClick();
            Close();
        }

        public void OnShareClick()
        {
            _item?.OnShareButtonClick();
            Close();
        }

        public void UpdatePosition()
        {
            var parentTransform = transform.parent.GetComponent<RectTransform>();
            var itemTransform = _item.GetComponent<RectTransform>();
            var itemCornets = new Vector3[4];
            itemTransform.GetWorldCorners(itemCornets);
            ViewArea.position = itemCornets[3];

            float y = ViewArea.anchoredPosition.y + parentTransform.rect.height / 2f - 130;

            if (y < parentTransform.rect.yMin)
            {
                ViewArea.position = itemCornets[2] + Vector3.up * 110;
            }
        }
    }
}