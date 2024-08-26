using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public interface IScratchLayout
    {
        Vector2 GetSize();
        void OnUpdateLayout();
    }

    public interface IScratchModifyLayout
    {
        void UpdateLayout();
        Vector2 SetSize(Vector2 size);
    }

    public interface IScratchSerializeData
    {
    }

    public interface IScratchBlockClick : IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
    }

    public abstract class ScratchUIBehaviour : ScratchBehaviour, IScratchLayout
    {
        public bool Active
        {
            set
            {
                if (!IsDestroying) gameObject.SetActive(value);
            }
            get => gameObject.activeSelf;
        }

        private bool m_visible;


        public bool Visible
        {
            set
            {
                if (!IsDestroying && m_visible != value)
                {
                    m_visible = value;
                    if (value) OnVisible();
                    else OnInVisible();
                }
            }
            get => m_visible;
        }

        private RectTransform m_rectTrans;

        public RectTransform RectTrans
        {
            get
            {
                if (m_rectTrans == null)
                {
                    m_rectTrans = GetComponent<RectTransform>();
                }

                return m_rectTrans;
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

        public ScratchUIBehaviour Parent { get; set; }


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

        public virtual Vector2 GetSize()
        {
            if (RectTrans == null) return Vector2.zero;
            return RectTrans.sizeDelta;
        }

        public virtual void OnUpdateLayout()
        {
        }

        public ScratchUIBehaviour Ancestors(ScratchUIBehaviour origin)
        {
            if (origin == null) return null;

            var parentUI = origin.Parent;
            if (parentUI != null)
            {
                return Ancestors(parentUI);
            }
            else return origin;
        }
    }


    public class ScratchUIBehaviour<T> : ScratchUIBehaviour where T : ScratchVMData, new()
    {
        public readonly VMContextComponent<T> ContextComponent = new VMContextComponent<T>();

        public T ContextData
        {
            get => ContextComponent.BindContext;
            set => ContextComponent.BindContext = value;
        }

        protected virtual void OnEnable()
        {
            if (ContextData == null) Initialize();
        }

        protected virtual void OnDisable()
        {
        }

        public virtual bool Initialize(T context = null)
        {
            if (base.Initialize())
            {
                if (context == null) ContextData = new T();
                else ContextData = context;
            }

            return Inited;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ContextComponent.Clear();
        }
    }
}