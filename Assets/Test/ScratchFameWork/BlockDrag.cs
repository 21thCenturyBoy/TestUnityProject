using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public abstract class BlockDrag : ScratchUIBehaviour, IScratchBlockClick, IScratchBlockDrag
    {
        private Block m_Block;

        public Block Block
        {
            get
            {
                if (m_Block == null)
                {
                    m_Block = GetComponent<Block>();
                }
                
                return m_Block;
            }
        }

        protected Vector3 offsetPointerDown = Vector3.zero;

        #region Pointer Click Drag

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerDown));
            offsetPointerDown = transform.position - Input.mousePosition;

            BlockDragManager.Instance.OnBlockBeginDrag(this);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerUp));
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnPointerClick));
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Debug.LogError(nameof(OnBeginDrag));
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition + offsetPointerDown;
            BlockDragManager.Instance.OnBlockDrag(this);
            Debug.LogError(nameof(OnDrag));
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            BlockDragManager.Instance.OnBlockEndDrag(this);
            Debug.LogError(nameof(OnEndDrag));
        }

        #endregion
    }
}