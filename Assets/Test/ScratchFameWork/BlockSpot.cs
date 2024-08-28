using System.Collections;
using System.Collections.Generic;
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

        public Vector2 DropPosition => RectTrans.position;

        protected override void OnEnable()
        {
            base.OnEnable();
            BlockDragManager.Instance.AddSpot(this);
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            BlockDragManager.Instance.RemoveSpot(this);
        }
    }
}