using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScratchFramework
{
    public partial class BlockSection : ScratchUIBehaviour, IScratchModifyLayout, IBlockScratch_Section
    {
        #region property

        private BlockLayout m_blockLayout;

        public BlockLayout BlockLayout
        {
            get
            {
                if (m_blockLayout == null && transform.parent != null)
                {
                    m_blockLayout = transform.parent.GetComponent<BlockLayout>();
                }

                return m_blockLayout;
            }
        }

        private BlockSectionHeader m_header;

        public BlockSectionHeader Header
        {
            get
            {
                if (m_header == null)
                {
                    m_header = GetComponentInChildren<BlockSectionHeader>();
                }

                return m_header;
            }
        }

        private BlockSectionBody m_body;

        public BlockSectionBody Body
        {
            get
            {
                if (m_body == null)
                {
                    m_body = GetComponentInChildren<BlockSectionBody>();
                }

                return m_body;
            }
        }

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

        public override Vector2 GetSize()
        {
            if (RectTrans == null) return Vector2.zero;
            if (Header != null)
            {
                Vector2 size = Vector2.zero;
                size.y = Header.GetSize().y;

                if (Body != null)
                {
                    size.y += Body.GetSize().y;
                }

                size.x = Header.GetSize().x;

                return size;
            }

            return RectTrans.sizeDelta;
        }


        public virtual void UpdateLayout()
        {
            Header?.OnUpdateLayout();
            Body?.OnUpdateLayout();

            SetSize(GetSize());
        }

        public virtual Vector2 SetSize(Vector2 size)
        {
            if (RectTrans == null) return Vector2.zero;
            RectTrans.sizeDelta = size;
            return size;
        }

        public IBlockSectionData GetData()
        {
            BlockSectionData data = new BlockSectionData();

            if (Header != null)
            {
                data.BlockHeadTreeList = new IBlockHeadData[Header.AllHeadSerializeDatas.Count];
                for (int i = 0; i < Header.AllHeadSerializeDatas.Count; i++)
                {
                    data.BlockHeadTreeList[i] = Header.AllHeadSerializeDatas[i].DataRef();
                }
            }

            if (Body != null)
            {
                data.BlockTreeRefList = new IBlockScratch_Block[Body.ChildBlocksArray.Count];
                for (int i = 0; i < Body.ChildBlocksArray.Count; i++)
                {
                    data.BlockTreeRefList[i] = Body.ChildBlocksArray[i];
                }
            }

            return data;
        }
    }
}