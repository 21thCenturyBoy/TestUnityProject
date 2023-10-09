namespace TestCoreLib
{

    using System;


    /// <summary>
    /// 核心子系统,每个都是单例
    /// </summary>
    public abstract class CoreSubSystem<T> : ISubSystem, IDisposable where T : CoreSubSystem<T>
    {
        public abstract void AwakeSystem();
        public abstract void StartSystem();
        public abstract void UpdtaeSystem(float delta);
        public abstract void LateUpdtaeSystem(float delta);
        public abstract void FixedUpdtaeSystem(float delta);
        public abstract void DestroySystem();
        public abstract void Dispose();

        private static T m_Instance;
        private ISystem m_parent;
        public static T Instance => m_Instance;

        protected CoreSubSystem()
        {
            if (m_Instance != null)
            {
                Log.Warning("单例子系统被重复创建!");
                m_Instance.Dispose();
            }

            m_Instance = (T)this;
        }

        ISystem ISubSystem.Parent
        {
            get => m_parent;
            set
            {
                if (m_parent == value) return;
                m_parent = value;
                OnRegisterParent(m_parent);
            }
        }

        public abstract void OnRegisterParent(ISystem system);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CoreSubSystemAttribute : Attribute
    {
        public CoreSubSystemAttribute()
        {
        }
    }
}