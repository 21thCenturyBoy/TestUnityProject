using UnityEngine;

namespace ScratchFramework
{
    public abstract class BlockSpot : ScratchUIBehaviour
    {
        #region property

        private Block m_block;

        public Block Block
        {
            get
            {
                if (m_block == null)
                {
                    m_block = GetComponentInParent<Block>();
                }

                return m_block;
            }
        }

        #endregion

        public Vector2 DropPosition => Position;

        protected override void OnEnable()
        {
            base.OnEnable();
            BlockDragUIManager.Instance.AddSpot(this);
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            BlockDragUIManager.Instance.RemoveSpot(this);
        }
    }
}