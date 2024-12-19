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
        Trigger,
        Simple,
        Condition,
        Loop,
        Operation,
        Define,
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

        private BlockDrag m_BlockDrag;

        public BlockDrag BlockDrag
        {
            get
            {
                if (m_BlockDrag == null)
                {
                    m_BlockDrag = GetComponent<BlockDrag>();
                }

                return m_BlockDrag;
            }
        }

        private IBlockHeaderVariableLabel m_VariableLabel;

        public bool IsReturnValue { get; private set; }

        public IBlockHeaderVariableLabel VariableLabel
        {
            get
            {
                if (m_VariableLabel == null)
                {
                    TryGetVariableLabel(out m_VariableLabel);
                    IsReturnValue = m_VariableLabel is BlockHeaderItem_RenturnVariableLabel;
                }

                return m_VariableLabel;
            }
        }

        public BlockSection ParentSection => GetComponentInParent<BlockSection>();

        public bool IsRoot => ParentSection == null;

        public Guid BlockId { get; set; } = Guid.Empty;

        private void Start()
        {
            Initialize();
            
            lastSiblingIndex = transform.GetSiblingIndex();
            lastParent = transform.parent;

            InitKoalaData();

            Layout.UpdateLayout();
            
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


        /// <summary> 尝试通过UI方式获取IBlockHeaderVariableLabel (用VariableLabel) </summary>
        private bool TryGetVariableLabel(out IBlockHeaderVariableLabel label)
        {
            label = null;
            if (m_FucType == FucType.Variable)
            {
                label = Layout.SectionsArray[0].Header.GetComponentInChildren<IBlockHeaderVariableLabel>();
                if (label == null) return true;
            }

            return false;
        }
        /// <summary> 尝试通过UI方式获取Operation的Input </summary>
        public bool TryGetOperationInput(out BlockHeaderItem_Input input)
        {
            input = null;
            if (Type  == BlockType.Operation)
            {
                int index = transform.GetSiblingIndex();

                if (ParentTrans == null) return false;
                int len = ParentTrans.childCount;
                index++;
                if (index >= len) return false;
                var tempChild = ParentTrans.GetChild(index);
                if (tempChild == null) return false;
                //弟弟应该是BlockHeaderItem_Input
                input = tempChild.GetComponent<BlockHeaderItem_Input>();
                return input != null;
                
            }
            
            return false;
        }

        public void SetShadowActive(bool value)
        {
            if (Type != BlockType.Operation)
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

        void Update()
        {
            UpdateKoalaData();
        }

        public IBlockData GetDataRef()
        {
            BlockData data = new BlockData();

            data.GetData(this);

            return data;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestoryEngineData();
            
            BlockCanvasManager.Instance.RemoveBlock(this);
        }
    }
}