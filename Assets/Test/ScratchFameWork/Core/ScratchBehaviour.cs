using UnityEngine;

namespace ScratchFramework
{
    public abstract class ScratchBehaviour : MonoBehaviour
    {
        private bool m_isDestroying;
        protected bool m_isInitialized;

        public bool IsDestroying => m_isDestroying;
        
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

    public class ScratchBehaviour<T> : ScratchBehaviour where T : ScratchVMData, new()
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
                    OnInitialize();
                }

                ViewModelProperty.Value = value;
            }
        }

        protected virtual void OnInitialize()
        {
            ViewModelProperty.OnValueChanged += OnBindingContextChanged;
        }

        public virtual void Initialize(T context = null)
        {
            if (context == null) BindingContext = new T();
            else BindingContext = context;
        }

        protected virtual void OnBindingContextChanged(T oldValue, T newValue)
        {
            Binder.Unbind(oldValue);
            Binder.Bind(newValue);
        }
    }
}