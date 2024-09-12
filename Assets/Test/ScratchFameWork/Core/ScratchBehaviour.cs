using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScratchFramework
{
    public abstract class ScratchBehaviour : MonoBehaviour
    {
        private bool m_isDestroying;
        protected bool m_isInitialized;

        public bool Inited => m_isInitialized;
        public bool IsDestroying => m_isDestroying;

        public virtual bool Active
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

        public virtual bool Initialize()
        {
            if (m_isInitialized) return false;

            OnInitialize();

            m_isInitialized = true;

            return m_isInitialized;
        }

        protected virtual void OnEnable()
        {
            Visible = true;
        }

        protected virtual void OnDisable()
        {
            Visible = false;
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnVisible()
        {
        }

        protected virtual void OnInVisible()
        {
        }

        protected virtual void OnDestroy()
        {
            m_isDestroying = true;
        }

        public T TryAddComponent<T>() where T : Component
        {
            if (IsDestroying)
            {
                Debug.LogWarning($"{name} OnDestroy TryAdd!");
                return default;
            }

            T com = GetComponent<T>();
            if (com == null)
            {
                com = gameObject.AddComponent<T>();
            }

            return com;
        }
    }

    public class ScratchSingleton<T> : ScratchBehaviour where T : ScratchSingleton<T>
    {
        protected static T _instance;

        protected ScratchSingleton()
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

    public class VMContextComponent<T> where T : ScratchVMData, new()
    {
        public readonly PropertyBinder<T> Binder = new PropertyBinder<T>();
        public readonly BindableProperty<T> ViewModelProperty = new BindableProperty<T>();

        private bool m_IsDispose;
        public bool IsDisposed => m_IsDispose;

        public VMContextComponent()
        {
            ViewModelProperty.OnValueChanged += OnBindingContextChanged;
        }

        public T BindContext
        {
            get => ViewModelProperty.Value;
            set
            {
                if (IsDisposed)
                {
                    Debug.LogWarning("VMContext 已被释放！");
                    return;
                }

                ViewModelProperty.Value = value;
            }
        }

        public void BindData<TData>(string propertyName, IBindable<TData>.ValueChangedHandler onPropertyChanged)
        {
            Binder.Add<TData>(propertyName, onPropertyChanged);
        }

        private void OnBindingContextChanged(T oldValue, T newValue)
        {
            Binder.Unbind(oldValue);
            Binder.Bind(newValue);
        }

        public void Clear()
        {
            ViewModelProperty.OnValueChanged = null;
            ViewModelProperty.Value = null;

            m_IsDispose = true;
        }

        public VMContextComponent<T> Init(Action<T, T> callback = null)
        {
            ViewModelProperty.OnValueChanged += (value, newValue) => { callback?.Invoke(value, newValue); };
            return this;
        }
    }

    public partial class ScratchBehaviour<T> : ScratchBehaviour where T : ScratchVMData, new()
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