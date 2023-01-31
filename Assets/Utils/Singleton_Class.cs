using UnityEngine;

/// <summary>
/// 单例类，没有的话创建 </summary>
/// </summary>
public class Singleton_Class<T> where T : Singleton_Class<T>, new()
{
    private static T _instance;

    /// <summary> 不公开构造 </summary>
    protected Singleton_Class() { }

    public static T Instance
    {
        get
        {
            if (ReferenceEquals(_instance, null))
            {
                _instance = new T();
                Debug.Log("[Singleton] An instance of " + typeof(T) + " is Created.");
            }
            return _instance;
        }
    }
}
