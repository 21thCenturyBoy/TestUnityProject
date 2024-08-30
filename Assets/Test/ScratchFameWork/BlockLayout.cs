using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [ExecuteAlways]
    public class BlockLayout : ScratchUIBehaviour, IScratchModifyLayout
    {
        [SerializeField] private Color m_blockColor = Color.white;

        protected BlockSection[] m_sectionsArray;

        public BlockSection[] SectionsArray
        {
            get
            {
                if (m_sectionsArray == null)
                {
                    m_sectionsArray = FindSectionsArray();
                }

                return m_sectionsArray;
            }
        }

        public Color Color
        {
            get => m_blockColor;
            set => m_blockColor = value;
        }


        BlockSection[] FindSectionsArray()
        {
            int childCount = transform.childCount;
            List<BlockSection> sections = new List<BlockSection>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                BlockSection section = transform.GetChild(i).GetComponent<BlockSection>();
                if (section != null)
                {
                    sections.Add(section);
                }
            }

            return sections.ToArray();
        }

        public override Vector2 GetSize()
        {
            if (RectTrans == null) return Vector2.zero;

            Vector2 size = Vector2.zero;

            int sectionsLength = SectionsArray.Length;
            for (int i = 0; i < sectionsLength; i++)
            {
                var sectionSize = SectionsArray[i].GetSize();

                size.y += sectionSize.y;
                if (sectionSize.x > size.x)
                {
                    size.x = sectionSize.x;
                }
            }

            return size;
        }

        private void Start()
        {
            RectTrans.pivot = new Vector2(0, 1);
        }

        public override void OnUpdateLayout()
        {
            base.OnUpdateLayout();

            SetSize(GetSize());


            int sectionsLength = SectionsArray.Length;
            for (int i = 0; i < sectionsLength; i++)
            {
                SectionsArray[i].UpdateLayout();
            }
        }

        public void LateUpdate()
        {
            UpdateLayout();
            LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
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