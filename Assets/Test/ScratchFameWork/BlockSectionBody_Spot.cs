using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class BlockSectionBody_Spot : ScratchUIBehaviour
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

        void OnEnable()
        {
            BlockDragManager.Instance.AddSpot(this);
        }

        void OnDisable()
        {
            BlockDragManager.Instance.RemoveSpot(this);
        }
    }
}
