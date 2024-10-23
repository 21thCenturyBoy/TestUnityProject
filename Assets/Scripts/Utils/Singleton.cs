using UnityEngine;

/// <summary> 单例Mono，没有的话创建（跳场景销毁） </summary>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    protected static T _instance;

    protected Singleton() { }

    public static T Instance
    {
        get
        {
            if (_instance ==null)
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
                    Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created.");
                }
            }
            return _instance;
        }

    }
    protected virtual void OnDestroy()
    {
        _instance = null;
    }
}

