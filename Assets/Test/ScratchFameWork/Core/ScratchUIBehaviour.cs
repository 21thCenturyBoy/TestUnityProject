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

        public virtual void RefreshUI()
        {
        }

        protected override void OnVisible()
        {
            base.OnVisible();

            TryAddComponent<CanvasGroup>().alpha = 1;
        }

        protected override void OnInVisible()
        {
            base.OnInVisible();
            TryAddComponent<CanvasGroup>().alpha = 0;
        }

        public virtual Vector2 GetSize()
        {
            if (RectTrans == null) return Vector2.zero;
            return RectTrans.sizeDelta;
        }
        // public virtual Vector2 SetSize(Vector2 size)
        // {
        //     if (RectTrans == null) return Vector2.zero;
        //     RectTrans.sizeDelta = size;
        //     return size;
        // }
        //
        // public virtual void UpdateLayout()
        // {
        //     var parent = Ancestors(this);
        //     parent?.OnUpdateLayout();
        // }

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
        protected readonly PropertyBinder<T> Binder = new PropertyBinder<T>();
        public readonly BindableProperty<T> ViewModelProperty = new BindableProperty<T>();


        public bool Inited => m_isInitialized;

        public T BindingContext
        {
            get { return ViewModelProperty.Value; }
            set
            {
                if (!Inited)
                {
                    m_isInitialized = true;

                    ViewModelProperty.OnValueChanged += OnBindingContextChanged;
                    OnInitialize();
                }

                ViewModelProperty.Value = value;
            }
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnRelease()
        {
        }

        protected virtual void OnEnable()
        {
            if (BindingContext == null) Initialize();
        }

        protected virtual void OnDisable()
        {
        }


        public virtual void Initialize(T context = null)
        {
            if (context == null) BindingContext = new T();
            else BindingContext = context;
        }

        protected virtual void OnDestroy()
        {
            base.OnDestroy();

            m_isInitialized = false;
            ViewModelProperty.OnValueChanged = null;

            OnRelease();

            ViewModelProperty.Value = null;
        }

        protected virtual void OnBindingContextChanged(T oldValue, T newValue)
        {
            Binder.Unbind(oldValue);
            Binder.Bind(newValue);
        }
    }
}