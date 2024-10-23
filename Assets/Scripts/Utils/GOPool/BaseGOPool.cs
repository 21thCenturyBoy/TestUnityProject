using System.Collections.Generic;
using UnityEngine;

namespace Utils.GOPool
{
    public interface IGamePrefab
    {
        public int PrefabInstId { get; set; }
        void OnPrefabAwake();

        void OnPrefabDestroy();
    }
    public abstract class BaseGOPool : MonoBehaviour
    {
        protected int m_maxCount = 100;
        public virtual void CreatePool(int maxcount)
        {
            m_maxCount = maxcount;
            m_Pool = new Stack<GameObject>(m_maxCount);
        }
        protected Stack<GameObject> m_Pool = new Stack<GameObject>();
        public virtual GameObject Spawn(GameObject prefab, Transform parent = null, bool createIfPoolEmpty = true)
        {
            if (m_Pool.Count > 0)
            {
                GameObject rtn = m_Pool.Pop();
                if (rtn == null)
                {
                    if (createIfPoolEmpty)
                    {
                        rtn = Instantiate(prefab, parent);
                        var prefabComs = rtn.GetComponents<IGamePrefab>();
                        if (prefabComs.Length != 0)
                        {
                            for (int i = 0; i < prefabComs.Length; i++)
                            {
                                prefabComs[i].PrefabInstId = prefab.GetInstanceID();
                            }
                        }
                    }
                }
                else
                {
                    var prefabComs = rtn.GetComponents<IGamePrefab>();
                    if (prefabComs.Length != 0)
                    {
                        for (int i = 0; i < prefabComs.Length; i++)
                        {
                            prefabComs[i].OnPrefabAwake();
                        }
                    }
                    rtn.transform.SetParent(parent);
                }
                rtn.gameObject.SetActive(true);
                return rtn;
            }
            else
            {
                if (createIfPoolEmpty)
                {
                    GameObject rtn = Instantiate(prefab, parent);
                    var prefabComs = rtn.GetComponents<IGamePrefab>();
                    if (prefabComs.Length != 0)
                    {
                        for (int i = 0; i < prefabComs.Length; i++)
                        {
                            prefabComs[i].PrefabInstId = prefab.GetInstanceID();
                        }
                    }
                    rtn.gameObject.SetActive(true);
                    return rtn;
                }
            }
            return null;
        }
        public virtual bool Recycle(GameObject obj)
        {
            if (obj == null) return false;
            if (m_Pool.Count >= m_maxCount && m_maxCount > 0)
            {
                Destroy(obj);
                return false;
            }

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform);

            var prefabComs = obj.GetComponents<IGamePrefab>();
            if (prefabComs.Length != 0)
            {
                for (int i = 0; i < prefabComs.Length; i++)
                {
                    prefabComs[i].OnPrefabDestroy();
                }
            }
            m_Pool.Push(obj);
            return true;
        }

        public virtual void Clear()
        {
            m_Pool.Clear();
            m_maxCount = 0;
        }
        public int Count => m_Pool.Count;
    }
}


