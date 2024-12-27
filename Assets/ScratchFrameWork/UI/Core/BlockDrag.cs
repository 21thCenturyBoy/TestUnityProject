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
        public Vector3 OffsetPointerDown => offsetPointerDown;


        public virtual bool IsAllowDrag()
        {
            return true;
        }

        protected bool m_IsDraging = false;
        public bool IsDraging => m_IsDraging;

        #region Pointer Click Drag lifecycle

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // if (BlockDragDebug) Debug.LogError(nameof(OnPointerDown));
            offsetPointerDown = Position - this.ScreenPos2WorldPos(eventData.position);

            if (Input.GetMouseButtonDown(1))
            {
                MenuUIManager.Instance.RightPointerMenu_Block(Block);
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsAllowDrag()) return; //不允许拖动

            // if (BlockDragDebug) Debug.LogError(nameof(OnBeginDrag));
            m_IsDraging = true;
            BlockDragUIManager.Instance.OnBlockBeginDrag(this);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDraging) return;

            Position = this.ScreenPos2WorldPos(eventData.position) + offsetPointerDown;
            BlockDragUIManager.Instance.OnBlockDrag(this);
            // if (BlockDragDebug) Debug.LogError(nameof(OnDrag));
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            // if (BlockDragDebug) Debug.LogError(nameof(OnPointerUp));
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // if (BlockDragDebug) Debug.LogError(nameof(OnPointerClick));
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!m_IsDraging) return;
            
            m_IsDraging = false;

            BlockDragUIManager.Instance.OnBlockEndDrag(this);
            // if (BlockDragDebug) Debug.LogError(nameof(OnEndDrag));
        }

        #endregion
    }
}