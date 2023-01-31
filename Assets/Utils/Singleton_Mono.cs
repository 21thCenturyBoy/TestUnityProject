using UnityEngine;

/// <summary>单例Mono，没有的话创建（跳场景不销毁）</summary>
public class Singleton_Mono<T> : MonoBehaviour where T : Singleton_Mono<T>
{
    protected static T _instance;
    private static object _lock = new object();

    protected Singleton_Mono() { }

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit." + " Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (ReferenceEquals(_instance, null))
                {
                    _instance = (T)FindObjectOfType(typeof(T), true);

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " + " - there should never be more than 1 singleton!" + " Reopenning the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                        Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created with DontDestroyOnLoad.");
                    }
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
    }
    private static bool applicationIsQuitting = false;

    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}


