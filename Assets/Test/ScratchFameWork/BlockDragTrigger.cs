using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockDragTrigger : ScratchUIBehaviour, IScratchBlockClick, IScratchBlockDrag
    {
        // private 
        private Vector3 offsetPointerDown = Vector3.zero;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerDown));
            offsetPointerDown = transform.position - Input.mousePosition;

            BlockDragManager.Instance.OnDrag(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerUp));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerClick));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnBeginDrag));
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnDrag));
            transform.position = Input.mousePosition + offsetPointerDown;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnEndDrag));
            
            BlockDragManager.Instance.OnDrag(null);
        }
    }
}