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


        public static readonly bool BlockDragDebug = false;

        private bool m_IsDraging = false;
        public bool IsDraging => m_IsDraging;

        #region Pointer Click Drag lifecycle

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (BlockDragDebug) Debug.LogError(nameof(OnPointerDown));
            offsetPointerDown = transform.position - Input.mousePosition;

 
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (BlockDragDebug) Debug.LogError(nameof(OnBeginDrag));
            m_IsDraging = true;
            BlockDragManager.Instance.OnBlockBeginDrag(this);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition + offsetPointerDown;
            BlockDragManager.Instance.OnBlockDrag(this);
            if (BlockDragDebug) Debug.LogError(nameof(OnDrag));
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (BlockDragDebug) Debug.LogError(nameof(OnPointerUp));
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (BlockDragDebug) Debug.LogError(nameof(OnPointerClick));
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            m_IsDraging = false;
            
            BlockDragManager.Instance.OnBlockEndDrag(this);
            if (BlockDragDebug) Debug.LogError(nameof(OnEndDrag));
        }

        #endregion
    }
}