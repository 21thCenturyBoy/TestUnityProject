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

    public interface IScratchManager
    {
        bool Initialize();
        bool Active { get; set; }
        void OnUpdate();
        void OnLateUpdate();
        bool Clear();
    }


    public interface IScratchSerializeData
    {
    }

    public interface IScratchBlockClick : IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
    }

    public interface IScratchBlockDrag : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
    }

    public abstract class ScratchUIBehaviour : ScratchBehaviour, IScratchLayout
    {
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

        public virtual Vector2 GetSize()
        {
            if (RectTrans == null) return Vector2.zero;
            return RectTrans.sizeDelta;
        }

        public virtual void OnUpdateLayout()
        {
        }
    }

    public class ScratchUISingleton<T> : ScratchUIBehaviour where T : ScratchUISingleton<T>
    {
        private static T _instance;

        protected ScratchUISingleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                {
                    _instance = (T)FindObjectOfType(typeof(T), true);

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).Name.ToString();
                    }
                }

                return _instance;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _instance = null;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            if (ContextData == null) Initialize();
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