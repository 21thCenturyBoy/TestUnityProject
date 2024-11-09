using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class BlockSectionBody : ScratchUIBehaviour, IScratchModifyLayout, IScratchSectionChild
    {
        #region property

        private BlockSection m_section;

        public BlockSection Section
        {
            get
            {
                if (m_section == null && transform.parent != null)
                {
                    m_section = transform.parent.GetComponent<BlockSection>();
                }

                return m_section;
            }
        }

        private BlockLayout m_blockLayout;

        public BlockLayout BlockLayout
        {
            get
            {
                if (m_blockLayout == null && transform.parent != null && transform.parent.parent != null)
                {
                    m_blockLayout = transform.parent.parent.GetComponent<BlockLayout>();
                }

                return m_blockLayout;
            }
        }

        private BlockSpot_SectionBody m_SpotBody;

        public BlockSpot_SectionBody SpotBody
        {
            get
            {
                if (m_SpotBody == null)
                {
                    m_SpotBody = GetComponent<BlockSpot_SectionBody>();
                }

                return m_SpotBody;
            }
        }


        private BlockImage m_BlockImage;

        public BlockImage Image
        {
            get
            {
                if (m_BlockImage == null)
                {
                    m_BlockImage = TryAddComponent<BlockImage>();
                }

                return m_BlockImage;
            }
        }

        private BlockShadow m_BlockShadow;

        public BlockShadow Shadow
        {
            get
            {
                if (m_BlockShadow == null)
                {
                    m_BlockShadow = TryAddComponent<BlockShadow>();
                }

                return m_BlockShadow;
            }
        }

        #endregion

        private List<Block> m_ChildBlocksArray = new List<Block>();
        public List<Block> ChildBlocksArray => m_ChildBlocksArray;

        public int ChildBlocksCount { get; set; }

        public void UpdateChildBlocksList()
        {
            m_ChildBlocksArray.Clear();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Block childBlock = transform.GetChild(i).GetComponent<Block>();
                if (childBlock != null)
                {
                    m_ChildBlocksArray.Add(childBlock);
                }
            }

            ChildBlocksCount = m_ChildBlocksArray.Count;
        }

        public override void OnUpdateLayout()
        {
            base.OnUpdateLayout();

            if (BlockLayout != null)
            {
                Image.SetColor(BlockLayout.Color);
            }

            float minHeight = 50;
            if (Section.Block.Type == BlockType.trigger || Section.Block.Type == BlockType.define)
            {
                minHeight = 0;
            }

            float height = 0;

            UpdateChildBlocksList();
            int childsLength = ChildBlocksArray.Count;
            for (int i = 0; i < childsLength; i++)
            {
                height += ChildBlocksArray[i].Layout.GetSize().y - 10;
            }

            height -= 10;

            if (height < minHeight)
                height = minHeight;

            if (Section.transform.GetSiblingIndex() == Section.transform.parent.childCount - 2)
            {
                if (Section.Block.Type != BlockType.trigger && Section.Block.Type != BlockType.define)
                {
                    height += 50;
                }
            }

            SetSize(new Vector2(Section.GetSize().x, height));
        }

        public void UpdateLayout()
        {
            OnUpdateLayout();
        }

        public Vector2 SetSize(Vector2 size)
        {
            if (RectTrans == null) return Vector2.zero;
            RectTrans.sizeDelta = size;
            return size;
        }
        
        public IBlockScratch_Section GetSection() => Section;
    }
}