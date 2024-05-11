using System;

namespace TestUnRedo
{
    /// <summary> 单例撤销还原模块 </summary>
    public class UnRedoModuleSingleton<T> : UnRedoModule where T : UnRedoModuleSingleton<T>, new()
    {
        private static T _instance;

        public override string GetModuleName()
        {
            return typeof(T).Name;
        }

        //不公开构造
        protected UnRedoModuleSingleton() : base()
        {
            IsSingleton = true;
        }

        public static T Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                {
                    _instance = UnRedoModuleFactory.Create<T>();
                    //LogService.Log("[Singleton] An instance of " + typeof(T) + " is Created.");
                }
                return _instance;
            }
        }

        public static bool CheckSingleton() => !ReferenceEquals(_instance, null);
    }
    

    /// <summary> 撤销还原模块工厂 </summary>
    public static class UnRedoModuleFactory
    {
        public static T Create<T>() where T : UnRedoModule, new()
        {
            T module = null;
            
            module = new T();
            module.Init();
            
            return module;
        }
    }
}