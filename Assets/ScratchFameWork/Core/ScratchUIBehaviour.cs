using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    [Serializable]
    public enum DataType : byte
    {
        Undefined,
        Label,
        Input,
        Operation,
        VariableLabel,
        RenturnVariableLabel,
        Icon,
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

        private RectTransform m_Parent;

        public RectTransform ParentTrans
        {
            get
            {
                m_Parent = transform.parent.GetComponent<RectTransform>();
                return m_Parent;
            }
        }

        public Vector3 Position
        {
            get => RectTrans.position;
            set => RectTrans.position = value;
        }

        public Vector3 LocalPosition
        {
            get => RectTrans.localPosition;
            set => RectTrans.localPosition = value;
        }
        
        public Vector3 LocalEulerAngles
        {
            get => RectTrans.localEulerAngles;
            set => RectTrans.localEulerAngles = value;
        }

        public Vector3 ScreenPos => ScratchUtils.WorldPos2ScreenPos(Position);

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

        public override bool Initialize()
        {
            TryAddComponent<RectTransform>();

            var sourceRectTransform = TempCanvasManager.Instance.RectTrans;
            RectTrans.anchorMin = sourceRectTransform.anchorMin;
            RectTrans.anchorMax = sourceRectTransform.anchorMax;
            RectTrans.anchoredPosition = sourceRectTransform.anchoredPosition;
            RectTrans.sizeDelta = sourceRectTransform.sizeDelta;
            RectTrans.pivot = sourceRectTransform.pivot;
            RectTrans.localScale = sourceRectTransform.localScale;

            return base.Initialize();
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
                        if (!Application.isPlaying) return _instance;
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

    public abstract class ScratchUIBehaviour<T> : ScratchUIBehaviour where T : ScratchVMData, new()
    {
        public readonly VMContextComponent<T> ContextComponent = new VMContextComponent<T>();

        public T ContextData
        {
            get => ContextComponent.BindContext;
            set
            {
                if (ContextComponent.BindContext != value)
                {
                    T orginData = ContextComponent.BindContext;
                    
                    if (orginData!= null) orginData.PropertyChanged -= ContextDataOnPropertyChanged;
                    if (value != null) value.PropertyChanged += ContextDataOnPropertyChanged;

                    ContextComponent.BindContext = value;

                    OnContextDataChanged(orginData, value);
                }
            }
        }

        protected virtual void Start()
        {
            if (!Inited) Initialize();
        }

        public override bool Initialize()
        {
            base.Initialize();

            if (ContextData == null)
            {
                ContextData = new T();
                OnCreateContextData();
            }

            return Inited;
        }

        protected abstract void OnCreateContextData();

        protected virtual void OnContextDataChanged(T orginData,T newData)
        {
            
        }

        public virtual bool Initialize(T context)
        {
            ContextData = context;
            Initialize();

            return Inited;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            ContextComponent.Clear();
        }

        public abstract void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}