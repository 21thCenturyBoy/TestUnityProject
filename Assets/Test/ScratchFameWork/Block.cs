using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ScratchFramework
{
    public enum FucType : byte
    {
        Undefined,
        Event, //事件
        Action, //行为
        Control, //控制
        Condition, //条件，与或非
        GetValue, //取值
        Variable, //变量
    }

    public enum BlockType : byte
    {
        none,
        trigger,
        simple,
        condition,
        loop,
        operation,
        define,
    }

    /// <summary>Block基类 </summary>
    public partial class Block : ScratchUIBehaviour, IBlockScratch_Block
    {
        private BlockLayout m_Layout;

        public BlockLayout Layout
        {
            get
            {
                if (m_Layout == null)
                {
                    m_Layout = GetComponent<BlockLayout>();
                }

                return m_Layout;
            }
        }

        private CanvasGroup m_CanvasGroup;

        public CanvasGroup CanvasUI
        {
            get
            {
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = TryAddComponent<CanvasGroup>();
                }

                return m_CanvasGroup;
            }
        }

        public BlockSection ParentSection => GetParentSection();

        public Guid BlockId { get; set; } = Guid.Empty;

        public void Start()
        {
            Initialize();

            BlockCanvasManager.Instance.AddBlock(this);
        }

        protected override void OnVisible()
        {
            base.OnVisible();
            if (IsDestroying) return;

            CanvasUI.alpha = 1;
        }

        protected override void OnInVisible()
        {
            base.OnInVisible();
            if (IsDestroying) return;

            CanvasUI.alpha = 0;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            BlockDirector.InitializeStruct(this);
        }

        BlockSection GetParentSection()
        {
            return GetComponentInParent<BlockSection>();
        }

        public void SetShadowActive(bool value)
        {
            if (Type != BlockType.operation)
            {
                foreach (var section in Layout.SectionsArray)
                {
                    if (section.Header != null && section.Header.Shadow)
                    {
                        section.Header.Shadow.Visible = value;
                    }

                    if (section.Body != null && section.Body.Shadow)
                    {
                        section.Body.Shadow.Visible = value;
                    }
                }
            }
        }

        [SerializeField] private FucType m_FucType;

        public virtual FucType BlockFucType
        {
            get => m_FucType;
            set => m_FucType = value;
        }

        [SerializeField] private BlockType m_type;

        public virtual BlockType Type
        {
            get => m_type;
            set => m_type = value;
        }

        private int m_Version;

        public int Version
        {
            get => m_Version;
            set => m_Version = value;
        }
        
        public List<BlockSection> GetChildSection()
        {
            List<BlockSection> sections = new List<BlockSection>();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                BlockSection childBlockSection = transform.GetChild(i).GetComponent<BlockSection>();
                if (childBlockSection != null)
                {
                    sections.Add(childBlockSection);
                }
            }

            return sections;
        }

        public IBlockData CopyData()
        {
            BlockData data = new BlockData();

            data.CopyData(this);

            return data;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            BlockCanvasManager.Instance.RemoveBlock(this);
        }
    }
}