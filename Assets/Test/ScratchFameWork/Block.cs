using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public enum Type
    {
        Undefined,
        Event, //事件
        Action, //行为
        Control, //控制
        Condition, //条件，与或非
        GetValue, //取值
        Variable, //变量
        Operation, //表达式
    }

    public enum BlockType
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
    public class Block : ScratchUIBehaviour
    {
        [SerializeField] 
        private BlockType m_type;

        public BlockType Type
        {
            get => m_type;
            set => m_type = value;
        }

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

        public BlockSection ParentSection => GetParentSection();

        public void Start()
        {
            Initialize();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            BlockDirector.AddBlockDrag(this);
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
    }
}