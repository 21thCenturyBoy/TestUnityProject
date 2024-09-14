using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [ExecuteAlways]
    public class BlockSectionHeader : ScratchUIBehaviour, IScratchModifyLayout
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

        public float minHeight;

        public float minWidth = 150;
        public float paddingRight = 0;
        

        private List<ScratchUIBehaviour> m_HeadSerializeDatas = new List<ScratchUIBehaviour>();
        public List<ScratchUIBehaviour> HeadSerializeDatas => m_HeadSerializeDatas;
        public List<IBlockScratch_Head> AllHeadSerializeDatas { get;} = new List<IBlockScratch_Head>();

        public void UpdateHeadSerializeData()
        {
            m_HeadSerializeDatas.Clear();
            AllHeadSerializeDatas.Clear();
            
            int childCount = transform.childCount;

            m_HeadSerializeDatas.Capacity = childCount;

            for (int i = 0; i < childCount; i++)
            {
                IBlockScratch_Head dataBlock = transform.GetChild(i).GetComponent<IBlockScratch_Head>();

                if (dataBlock is ScratchUIBehaviour scratchUI)
                {
                    if (scratchUI != null && scratchUI.Active)
                    {
                        m_HeadSerializeDatas.Add(scratchUI);
                    }
                }
                AllHeadSerializeDatas.Add(dataBlock);
            }
        }


        public virtual void OnUpdateLayout()
        {
            base.OnUpdateLayout();
            
            UpdateHeadSerializeData();

            if (BlockLayout != null)
            {
                Image.SetColor(BlockLayout.Color);
            }


            if (Section != null)
            {
                if (Section.RectTrans.transform.GetSiblingIndex() == 0)
                {
                    float width = 0;
                    float height = minHeight - 40;
                    float h = 0;
                    int itemsLength = m_HeadSerializeDatas.Count;
                    for (int i = 0; i < itemsLength; i++)
                    {
                        ScratchUIBehaviour item = HeadSerializeDatas[i];
                        width += item.GetSize().x + 15;
                        if (item.GetSize().y > h)
                        {
                            h = item.GetSize().y;
                        }
                    }

                    width += 15 + paddingRight;

                    if (width < minWidth)
                        width = minWidth;

                    height += h;
                    if (height < minHeight)
                        height = minHeight;

                    SetSize(new Vector2(width, height));
                }
                else
                {
                    var width = BlockLayout.SectionsArray[0].Header.GetSize().x;
                    var height = GetSize().y;
                    SetSize(new Vector2(width, height));
                }
            }
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
    }
}