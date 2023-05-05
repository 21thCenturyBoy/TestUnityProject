using System.Collections.Generic;
using UnityEngine;

namespace Utils.GOPool
{
    public abstract class SingletonGoPool<TPool> : BaseGOPool where TPool : SingletonGoPool<TPool>
    {
        protected static TPool _instance;
        public static TPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TPool>(true);

                    if (FindObjectsOfType(typeof(TPool)).Length > 1)
                    {
                        return _instance;
                    }
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<TPool>();
                        _instance.CreatePool(100);
                        singleton.name = "[singleton]" + typeof(TPool).Name;
                    }
                }
                return _instance;
            }
        }
    }
    public class MultiGOPool : SingletonGoPool<MultiGOPool>
    {
        protected Dictionary<int, SingleGOPool> m_SingleGoPools = new Dictionary<int, SingleGOPool>();
        public override GameObject Spawn(GameObject prefab, Transform parent = null, bool createIfPoolEmpty = true)
        {
            SingleGOPool singlepool;
            int instId = prefab.GetInstanceID();
            if (!m_SingleGoPools.TryGetValue(instId, out singlepool))
            {
                singlepool = gameObject.AddComponent<SingleGOPool>();
                singlepool.CreatePool(m_maxCount);
                singlepool.OnSearchPrefab = () => prefab;
                m_SingleGoPools.Add(instId, singlepool);
            }

            var obj = singlepool.Spawn(parent, createIfPoolEmpty); ;
            return obj;
        }
        public override bool Recycle(GameObject obj)
        {
            if (obj == null) return false;
            var prefab = obj.GetComponent<IGamePrefab>();
            if (prefab == null)
            {
                Destroy(obj);
                return true;
            }
            return m_SingleGoPools[prefab.PrefabInstId].Recycle(obj);
        }
    }
}
